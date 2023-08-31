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
    public Vector3 velovity;
}

public class PlayerController : NetworkBehaviour
{

    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;
    public float maxHealth = 100f;

    private float _yVelocity = 0f;
    private float _health;

    private NetworkInput _inputs;


    private CharacterController _characterController;
    private PlayerNetworkInputs _playerNetworkInputs;

    //Client Prediction
    private StatePayload[] _stateBuffer;
    private int BUFFER_LENGHT = 1024;
    private int _bufferIndex = 0;

    public float health { get { return _health; } }


    protected override void Awake()
    {
        base.Awake();

        TimeManager.OnTick += OnTick;

        _characterController = GetComponent<CharacterController>();
        _playerNetworkInputs = GetComponent<PlayerNetworkInputs>();

        _stateBuffer = new StatePayload[BUFFER_LENGHT];
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        TimeManager.OnTick -= OnTick;
    }

    private void Start()
    {
        gravity *= NetworkManager.instance.timeManager.deltaTick * NetworkManager.instance.timeManager.deltaTick;
        moveSpeed *= NetworkManager.instance.timeManager.deltaTick;
        jumpSpeed *= NetworkManager.instance.timeManager.deltaTick;
    }

    public void Initialize(int id, string usrName)
    {
        _health = maxHealth;
    }

    private void OnTick(int tick)
    {
        //if (_health <= 0f)
        //{
        //    return;
        //}
        Physics.Simulate(NetworkManager.instance.timeManager.deltaTick);

        //if (_playerNetworkInputs.inputsQueue.Count > 0)
        //    _inputs = _playerNetworkInputs.inputsQueue.Dequeue();

        _inputs = _playerNetworkInputs.GetNetworkInputs();

        //Setting State Payload
        _bufferIndex = _inputs.tick % BUFFER_LENGHT;
        _stateBuffer[_bufferIndex] = Move(_inputs);

        //Sending Payload to the owner client to check of wrong simulation
        ServerSend.SendStatePayload(id, _stateBuffer[_bufferIndex], this.GetType());
    }
    private StatePayload Move(NetworkInput input)
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

        return SetStatePayload(input.tick);
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
    public void Shoot(Ray ray)
    {
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 5f);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.collider.GetComponent<PlayerController>())
            {
                hit.collider.GetComponent<PlayerController>().TakeDamage(50f);
            }
        }
    }


    public void TakeDamage(float damage)
    {
        if (_health <= 0f)
        {
            return;
        }

        _health -= damage;

        if (_health <= 0f)
        {
            _health = 0f;
            _characterController.enabled = false;
            transform.position = new Vector3(0f, 25f, 0f);
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        _health = maxHealth;
        _characterController.enabled = true;

        ServerSend.playerRespawned(this);
    }

    protected override void OnNewObjectAdded(object obj)
    {
        return;
    }
}

