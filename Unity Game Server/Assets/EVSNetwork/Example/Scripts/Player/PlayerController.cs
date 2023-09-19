using GameServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    private PlayerNetworkInputs _playerNetworkInputs;

    //Prediction
    private const int BUFFER_SIZE = 1024;
    private StatePayload[] _stateBuffer;
    private const int INPUT_BUFFER_SIZE = 1; // Stores the last 10 input packages
    private List<NetworkInput> _inputBuffer = new List<NetworkInput>();

    protected override void Awake()
    {
        base.Awake();

        TimeManager.OnTick += OnTick;

        _characterController = GetComponent<CharacterController>();
        _playerNetworkInputs = GetComponent<PlayerNetworkInputs>();

        _stateBuffer = new StatePayload[BUFFER_SIZE];
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

    private void OnTick(int tick)
    {
        // 1. Add incoming inputs to the buffer
        while (_playerNetworkInputs.inputsQueue.Count > 0)
        {
            NetworkInput inputPayload = _playerNetworkInputs.inputsQueue.Dequeue();
            _inputBuffer.Add(inputPayload);
        }

        // 2. Process the buffered inputs when they reach the buffer size
        if (_inputBuffer.Count >= INPUT_BUFFER_SIZE)
        {
            ProcessBufferedInputs(tick);
        }
    }

    private void ProcessBufferedInputs(int tick)
    {
        int lastBufferIndex = -1;

        foreach (var input in _inputBuffer)
        {
            int bufferIndex = input.tick % BUFFER_SIZE;
            StatePayload statePayload = ProcessMovements(input);
            _stateBuffer[bufferIndex] = statePayload;
            lastBufferIndex = bufferIndex;
        }

        // Clear the buffer once processed
        _inputBuffer.Clear();

        if (lastBufferIndex != -1)
        {
            //Sending Payload to the owner client to check for wrong simulation
            ServerSend.SendStatePayload(id, _stateBuffer[lastBufferIndex], this.GetType());
        }
    }
    private StatePayload ProcessMovements(NetworkInput input)
    {
        Vector2 movementsInputs = Vector2.zero;

        if (input.movements[0]) movementsInputs.y += 1;
        if (input.movements[1]) movementsInputs.y -= 1;
        if (input.movements[2]) movementsInputs.x += 1;
        if (input.movements[3]) movementsInputs.x -= 1;

        Debug.Log($"CLIENT:  {movementsInputs} on tick {input.tick}");

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
         //   velocity = velocity,
        };
    }

    public void Shoot(ShootPayload shootPayload)
    {
        Debug.DrawRay(shootPayload.origine, shootPayload.direction * 100f, Color.red, 5f);
        //RollBack
        // Calculate the time taken for the packet to travel from client to server
        float halfRTT = Time.time - shootPayload.time;
        float rollbackDurationInSeconds = Time.time - halfRTT - 0.1f;

        int rollbackTicks = Mathf.FloorToInt(rollbackDurationInSeconds / NetworkManager.instance.timeManager.delta);
        RollbackManager.intance.DoRollbackAll(id, rollbackTicks);

        //Raycast
        RaycastHit hit;
        if (Physics.Raycast(shootPayload.origine, shootPayload.direction, out hit, 100f))
        {
            if (hit.collider.GetComponent<Hitbox>())
            {
                hit.collider.GetComponent<Hitbox>().Damage(25f);
            }
        }
    }


    protected override void OnNewObjectAdded(object obj)
    {
        return;
    }
}

