using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PingDisplay : MonoBehaviour
{
    private TextMeshProUGUI _pingTxt;

    private void Awake()
    {
        _pingTxt = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        _pingTxt.text = "Ping:" + (int)NetworkManager.instance.timeManager.ping;
    }
}
