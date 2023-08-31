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
    }

    public override void Initialize(int id, string userName)
    {
        base.Initialize(id, userName);
    }
    public void OnInput(NetworkInput input)
    {
        _inputNetwork.OnInput(input);
    } 
    public void SendCommandShoot(Ray ray)
    {
        _playerController.Shoot(ray);
    }
}
