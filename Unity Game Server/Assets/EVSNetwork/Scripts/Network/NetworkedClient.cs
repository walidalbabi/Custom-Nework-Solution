using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedClient : NetworkedObject
{
    private IInputNetwork _inputNetwork;
    private PlayerController _playerController;

    private void Awake()
    {
        _inputNetwork = GetComponent<IInputNetwork>();
        _playerController = GetComponent<PlayerController>();
    }

    public override void Initialize(int id, string userName)
    {
        base.Initialize(id, userName);
    }
    public void OnInput(NetworkInput input)
    {
        _inputNetwork.OnInput(input);
    } 
    public void SendCommandShoot(ShootPayload shootPayload)
    {
        _playerController.Shoot(shootPayload);
    }
}
