using UnityEngine;


public struct StatePayload
{
    public int tick;
    public Vector3 position;
    public Vector3 velocity;
}
public struct ShootPayload
{
    public int tick;
    public Vector3 origine;
    public Vector3 direction;
    public float time;
}


public class PlayerController : NetworkBehaviour
{

    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;

    private float _yVelocity = 0f;

    private CharacterController _characterController;
    private PlayerInput _playerInput;

    [SerializeField] private Transform _serverModel;
    [SerializeField] private Interpolater _interpolator;
    //Prediction
    private const int BUFFER_SIZE = 1024;

    private StatePayload[] _stateBuffer;
    private NetworkInput[] _inputBuffer;
    private StatePayload latestServerState;
    private StatePayload lastProcessedState;

    NetworkInput inputPayload = new NetworkInput();

    //Reconciliation
    private int _tickToProcess;

    protected override void Awake()
    {
        base.Awake();

        TimeManager.OnTick += OnTick;

        _characterController = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();

        _stateBuffer = new StatePayload[BUFFER_SIZE];
        _inputBuffer = new NetworkInput[BUFFER_SIZE];

        for (int i = 0; i < _inputBuffer.Length; i++)
        {
            _inputBuffer[i].Init(5);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        TimeManager.OnTick -= OnTick;
    }
    private void Start()
    {

        float deltaTime = NetworkManager.instance.timeManager.delta;
        gravity *= deltaTime * deltaTime;
        moveSpeed *= deltaTime;
        jumpSpeed *= deltaTime;
    }

    [SerializeField] private float gunRate = 0.2f;
    private float shotTime;

    private void Update()
    {
        shotTime += Time.deltaTime;
        if (shotTime > gunRate)
        {
            CheckForPlayerShoot();
        }
    }

    private void OnTick(int tick)
    {
        //Send the current rotation, because its being handled localy
        ClientSend.SendRotationData(transform.rotation);

        if (tick % 3 == 0)
            ClientSend.SendPing();

        //Reconcilation
        if (!latestServerState.Equals(default(StatePayload)) && (lastProcessedState.Equals(default(StatePayload)) ||
            !latestServerState.Equals(lastProcessedState)))
        {
            HandleServerReconciliation(tick);
        }
        //Prediction
        int bufferIndex = tick % BUFFER_SIZE;

        inputPayload.Init(5);
        inputPayload = _playerInput.GetNetworkInputs();
        inputPayload.tick = tick;
        inputPayload.forward = transform.forward;
        inputPayload.right = transform.right;
        ClientSend.SendInputData(inputPayload);

        _inputBuffer[bufferIndex] = inputPayload;
        _stateBuffer[bufferIndex] = ProcessMovements(inputPayload);

        Interpolation interpolationState = new Interpolation {
            tick = _stateBuffer[bufferIndex].tick,
            position = _stateBuffer[bufferIndex].position,
            rotation = transform.rotation,
        };

        _interpolator.SetInterpolation(interpolationState);
    }

    private StatePayload ProcessMovements(NetworkInput input)
    {

        Vector2 movementsInputs = Vector2.zero;
        if (input.movements[0]) movementsInputs.y += 1;
        if (input.movements[1]) movementsInputs.y -= 1;
        if (input.movements[2]) movementsInputs.x += 1;
        if (input.movements[3]) movementsInputs.x -= 1;

      //  Debug.Log($"CLIENT:  {movementsInputs} on tick {input.tick}");

        Vector3 moveDirection = input.right * -movementsInputs.x + input.forward * movementsInputs.y;
        moveDirection *= moveSpeed;

        if (_characterController.isGrounded)
        {
            _yVelocity = 0f;

            if (input.movements[4])
            {
                _yVelocity = jumpSpeed;
            }
        }
        _yVelocity += gravity;

        moveDirection.y = _yVelocity;

        _characterController.Move(moveDirection);

        return new StatePayload
        {
            tick = input.tick,
            position = transform.position,
            velocity = _characterController.velocity,
        };

    }

    private void HandleServerReconciliation(int currentTick)
    {
        lastProcessedState = latestServerState;

        int serverStateBufferIndex = latestServerState.tick % BUFFER_SIZE;
        float positionError = Vector3.Distance(latestServerState.position, _stateBuffer[serverStateBufferIndex].position);

        if (positionError > 0.05f)
        {
            //    Debug.Log($"server tick {latestServerState.tick} Client tick {_stateBuffer[serverStateBufferIndex].tick} Distance Error = {positionError}");
            Debug.Log($"server tick {latestServerState.tick} Client tick {_stateBuffer[serverStateBufferIndex].tick} Distance Error = {positionError}");
            // Update buffer at index of latest server state
            _stateBuffer[serverStateBufferIndex] = latestServerState;

            _characterController.enabled = false;
            transform.position = _stateBuffer[serverStateBufferIndex].position;
            _characterController.enabled = true;

            // Now re-simulate the rest of the ticks up to the current tick on the client
            int tickToProcess = latestServerState.tick + 1;

            while (tickToProcess < currentTick)
            {
                int bufferIndex = tickToProcess % BUFFER_SIZE;

                // Process new movement with reconciled state
                StatePayload statePayload = ProcessMovements(_inputBuffer[bufferIndex]);

                // Update buffer with recalculated state
                _stateBuffer[bufferIndex] = statePayload;

                Interpolation interpolationState = new Interpolation
                {
                    tick = _stateBuffer[bufferIndex].tick,
                    position = _stateBuffer[bufferIndex].position,
                    rotation = transform.rotation,
                };

                //_interpolator.SetInterpolation(interpolationState);

                tickToProcess++;
            }
        }

    }

    public void SetServerStatePayload(StatePayload payload)
    {
        if (payload.tick <= latestServerState.tick) return;

        latestServerState = payload;
        _serverModel.position = payload.position;
    }


    private void CheckForPlayerShoot()
    {
        if (_playerInput.GetLocalInputs().leftMouseClick)
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            ShootPayload shootPayload = new ShootPayload() {
                tick = NetworkManager.instance.timeManager.currentTick,
                origine = ray.origin,
                direction = ray.direction,
                time = NetworkManager.instance.timeManager.clientTime
            };
            ClientSend.PlayerShoot(shootPayload);
            shotTime = 0;
        }
    }

    protected override void OnNewObjectAdded(object obj)
    {
        if (obj.GetType() == typeof(StatePayload))
        {
            StatePayload payload = (StatePayload)obj;
            SetServerStatePayload(payload);
        }
    }
}
