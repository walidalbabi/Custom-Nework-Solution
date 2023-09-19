using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetworkInputs : MonoBehaviour, IInputNetwork
{
    private NetworkInput _inputs;
    public Queue<NetworkInput> inputsQueue = new Queue<NetworkInput>();

    private void Awake()
    {
        _inputs.Init(5);
    }

    #region Interface
    public void OnInput()
    {
        throw new System.NotImplementedException();
    }
    public void OnInput(NetworkInput input)
    {
        if (!_inputs.Equals(input))
        {
            //if (input.tick < _inputs.tick) return;
            this._inputs = input;
            inputsQueue.Enqueue(input);
        }
    }
    public NetworkInput GetNetworkInputs() { return _inputs; }
    #endregion Interface
}
