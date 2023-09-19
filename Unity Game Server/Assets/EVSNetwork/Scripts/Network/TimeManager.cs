using GameServer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public const int TICK_RATE = 64;

    [SerializeField] private int _currentTick;
    private float _minTimeBetweenTicks;
    private float _timer;
    private float _clientTime;
    /// <summary>
    /// On Tick is Processed return The currentTick
    /// </summary>
    public static Action<int> OnTick;


    public int currentTick { get { return _currentTick; } }
    public float delta { get { return _minTimeBetweenTicks; } }
    public float clientTime { get { return _clientTime; } }

    private void Awake()
    {
        //   Application.targetFrameRate = 60; // Limit to 60 FPS for example
        _minTimeBetweenTicks = 1f / TICK_RATE;
    }

    void Update()
    {
        _clientTime += Time.time;
        _timer += Time.deltaTime;

        while (_timer >= _minTimeBetweenTicks)
        {
            _timer -= _minTimeBetweenTicks;
            Tick();
            AdvanceTick();
        }
    }

    public void Tick()
    {
        OnTick?.Invoke(_currentTick);

        if (_currentTick % 3 == 0)
            ServerSend.SendServerTick(_currentTick);
    }

    private void AdvanceTick()
    {
        if(_currentTick == 32767)
        {
            _currentTick = 0;
            return;
        }

        _currentTick++;
    }

}