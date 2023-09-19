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

    private void Awake()
    {
        _inputs = new NetworkInput();
        _inputs.Init(5);
    }

    private void Update()
    {
        CheckForCursorLock();
        //Set Input Tick

        MovementsInputs();
        JumpInputs();
        leftClickInputs();
    }
    public LocalInputs GetLocalInputs() { return _localInputs; }

    #region Interface
    public void OnInput(NetworkInput inputPayload, int tick)
    {
        inputPayload.tick = tick;
        inputPayload.forward = transform.forward;
        inputPayload.right = transform.right;
        ClientSend.SendInputData(inputPayload);
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
        _inputs.movements[0] = Input.GetKey(KeyCode.W);
        _inputs.movements[1] = Input.GetKey(KeyCode.S);
        _inputs.movements[2] = Input.GetKey(KeyCode.A);
        _inputs.movements[3] = Input.GetKey(KeyCode.D);
    }
    private void JumpInputs()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _inputs.movements[4] = true;
        }
        else
        if (Input.GetKeyUp(KeyCode.Space))
        {
            _inputs.movements[4] = false;
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
    private void CheckForCursorLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    #endregion InputsHandling

}
