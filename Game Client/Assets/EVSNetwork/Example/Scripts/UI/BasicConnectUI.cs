using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BasicConnectUI : MonoBehaviour
{
    [SerializeField] TMP_InputField _userNameField;
    [SerializeField] TMP_InputField _ipField;
    
    public void OnUsernameChange()
    {
        Client.instance.SetUsername(_userNameField.text);
    }
    public void OnIPChange()
    {
        Client.instance.SetIPAddress(_ipField.text);
    }
    public void Connect()
    {
        string username = _userNameField.text;
        string ip = _ipField.text;

        Client.instance.ConnectToServer();

        gameObject.SetActive(false);
    }
}
