using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct NetworkInput
{
    public int tick;
    public float horizontal;
    public float vertical;
    public bool jump;
}

public interface IInputNetwork
{
    public void OnInput();
    public void OnInput(NetworkInput input);
    public NetworkInput GetNetworkInputs();
}