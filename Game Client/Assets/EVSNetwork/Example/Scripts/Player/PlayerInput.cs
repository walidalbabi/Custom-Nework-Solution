using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public struct LocalInputs
{
    public bool leftMouseClick;
}


public class PlayerInput : MonoBehaviour, IInputNetwork
{

    private NetworkInput _inputs;
    private LocalInputs _localInputs;

    private void Update()
    {
        //Set Input Tick
        _inputs.tick = NetworkManager.instance.timeManager.ClientTick;
        MovementsInputs();
        JumpInputs();
        leftClickInputs();
    }
    public LocalInputs GetLocalInputs() { return _localInputs; }

    #region Interface
    public void OnInput()
    {
        ClientSend.SendInputData(_inputs);
    }
    public void OnInput(NetworkInput input)
    {
        return; // this is Used by the server To Simulate The Input
    }
    public NetworkInput GetNetworkInputs() { return _inputs; }
    #endregion Interface

    #region InputsHandling
    private void MovementsInputs()
    {
        _inputs.horizontal = Input.GetAxis("Horizontal");
        _inputs.vertical = Input.GetAxis("Vertical");
    }
    private void JumpInputs()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _inputs.jump = true;
        }
        else
        if (Input.GetKeyUp(KeyCode.Space))
        {
            _inputs.jump = false;
        }
    }
    private void leftClickInputs()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _localInputs.leftMouseClick = true;
        }
        else
              if (Input.GetMouseButtonUp(0))
        {
            _localInputs.leftMouseClick = false;
        }
    }
    #endregion InputsHandling

}
