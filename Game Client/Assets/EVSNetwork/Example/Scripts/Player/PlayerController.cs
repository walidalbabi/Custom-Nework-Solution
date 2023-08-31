using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct StatePayload
{
    public int tick;
    public Vector3 position;
    public Vector3 velovity;
}

public class PlayerController : NetworkBehaviour
{

    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;

    private float _yVelocity = 0f;

    private CharacterController _characterController;
    private PlayerInput _playerInput;

    //Client Prediction
    private StatePayload[] _stateBuffer;
    private NetworkInput[] _inputBuffer;
    private StatePayload _lastServerStatePayload;
    private StatePayload _lastProccessedStatePayload;
    private int BUFFER_LENGHT = 1024;
    private int _bufferIndex = 0;

    //Reconciliation
    private int _tickToProcess;

    protected override void Awake()
    {
        base.Awake();

        TimeManager.OnTick += OnTick;

        _characterController = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();

        _stateBuffer = new StatePayload[BUFFER_LENGHT];
        _inputBuffer = new NetworkInput[BUFFER_LENGHT];
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        TimeManager.OnTick -= OnTick;
    }
    private void Start()
    {
    
        // float deltaTime = NetworkManager.instance.timeManager.deltaTick;
        float deltaTime = Time.fixedDeltaTime;
        gravity *= deltaTime * deltaTime;
        moveSpeed *= deltaTime;
        jumpSpeed *= deltaTime;
    }

    private void Update()
    {
        CheckForPlayerShoot();       
    }

    private void OnTick(int tick)
    {
        // Physics.Simulate(NetworkManager.instance.timeManager.deltaTick);

        //Send the current rotation, because its being handled localy
        ClientSend.SendRotationData(transform.rotation);

        //Check for Reconciliation
        ClientReconciliation(tick);

        _bufferIndex = tick % BUFFER_LENGHT;

        _inputBuffer[_bufferIndex].horizontal = Input.GetAxis("Horizontal");
        _inputBuffer[_bufferIndex].vertical = Input.GetAxis("Vertical");
        _inputBuffer[_bufferIndex].jump = _playerInput.GetNetworkInputs().jump;

        //Setting State Payload

        _stateBuffer[_bufferIndex] = Move(_inputBuffer[_bufferIndex], _inputBuffer[_bufferIndex].tick); ;

        //Send Player Inputs to Server
        _playerInput.OnInput();
    }

    private StatePayload Move(NetworkInput input, int tick)
    {
        Vector3 moveDirection = transform.right * input.horizontal + transform.forward * input.vertical;
        moveDirection *= moveSpeed;

        if (_characterController.isGrounded)
        {
            _yVelocity = 0f;
            if (input.jump)
            {
                _yVelocity = jumpSpeed;
            }
        }
        _yVelocity += gravity;

        moveDirection.y = _yVelocity;

        _characterController.Move(moveDirection);

        return SetStatePayload(tick);
    }

    [SerializeField]float lerpFactor = 0.1f;  // Adjust this value as needed for smoother results

    private void ClientReconciliation(int currrentTick)
    {
        if (_lastServerStatePayload.tick == _lastProccessedStatePayload.tick) return;

        _lastProccessedStatePayload = _lastServerStatePayload;

        int serverStateBufferIndex = _lastProccessedStatePayload.tick % BUFFER_LENGHT;
        float distanceError = Vector3.Distance(_stateBuffer[serverStateBufferIndex].position, _lastProccessedStatePayload.position);

        if (distanceError > 0.1f)
        {
            _characterController.enabled = false;
            Vector3 correctedPosition = Vector3.Lerp(transform.position, _lastProccessedStatePayload.position, lerpFactor);
            transform.position = _lastProccessedStatePayload.position;
            _characterController.enabled = true;

            _stateBuffer[serverStateBufferIndex] = _lastProccessedStatePayload;

            _tickToProcess = currrentTick - 1;

            //// Re-simulate the wrong states until the current tick
            //while (_tickToProcess < currrentTick)
            //{
            //    Debug.Log($"{_tickToProcess} {currrentTick}");
            //    int buffIndex = _tickToProcess % BUFFER_LENGHT;
            //    StatePayload statePayload = Move(_inputBuffer[buffIndex], _inputBuffer[buffIndex].tick);
            //    _stateBuffer[buffIndex] = statePayload;
            //    _tickToProcess++;
            //}
        }
    }


    private StatePayload SetStatePayload(int tick)
    {
        StatePayload state = new StatePayload
        {
            position = transform.position,
            velovity = _characterController.velocity,
            tick = tick
        };
        return state;
    }

    public void SetServerStatePayload(StatePayload payload)
    {
        _lastServerStatePayload = payload;
    }

    private void CheckForPlayerShoot()
    {
        if (_playerInput.GetLocalInputs().leftMouseClick)
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            ClientSend.PlayerShoot(ray);
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
