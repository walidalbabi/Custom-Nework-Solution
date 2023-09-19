using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct NetworkInput
{
    public int tick;
    public Vector3 forward;
    public Vector3 right;
    public bool[] movements;


    public void Init(int movementsBtns)
    {
        movements = new bool[movementsBtns];
    }
}

public interface IInputNetwork
{
    public void OnInput(NetworkInput inputPayload,int tick);
    public void OnInput(NetworkInput input);
    public NetworkInput GetNetworkInputs();
}