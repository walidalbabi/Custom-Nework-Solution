using GameServer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private  float SERVER_TICK_RATE = 60f;

    public int ServerTick;

    /// <summary>
    /// On Tick is Processed return The currentTick
    /// </summary>
    public static Action<int> OnTick;

    private float _timer;
    private int _currentTick;
    private float _minTimeBetweenTicks;



    public float deltaTick { get { return _minTimeBetweenTicks; } }
    public int GetTick() { return _currentTick; }

    private void Awake()
    {
        _minTimeBetweenTicks = 1f / SERVER_TICK_RATE;
    }

    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime;

        while (_timer >= _minTimeBetweenTicks)
        {
            _timer -= _minTimeBetweenTicks;
            HandleTick();
            _currentTick++;

            ServerTick = _currentTick;
        }
    }

    private void HandleTick()
    {
        OnTick?.Invoke(_currentTick);

        ServerSend.SendServerTick(_currentTick);
    }


}
