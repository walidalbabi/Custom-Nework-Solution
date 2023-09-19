using GameServer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public const int TICK_RATE = 64;

    [SerializeField]private int _currentTick;
    private float _minTimeBetweenTicks;
    private float _timer;
    private float _clientTime;
    private float _ping;

    /// <summary>
    /// On Tick is Processed return The currentTick
    /// </summary>
    public static Action<int> OnTick;

    public int currentTick { get { return _currentTick; } }
    public float delta { get { return _minTimeBetweenTicks; } }
    public float clientTime { get { return _clientTime; } }
    public float ping { get { return _ping; } }

    private void Awake()
    {
        //  Application.targetFrameRate = 60; // Limit to 60 FPS for example
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
    }


    public void SetTick(int tick)
    {
        if (_currentTick < tick || _currentTick > tick)
            _currentTick = tick;
    }

    private void AdvanceTick()
    {
        if (_currentTick == 32767)
        {
            _currentTick = 0;
            return;
        }

        _currentTick++;
    }

    public void SetPing(float time)
    {
        _ping = (Time.time - time) * 1000;  // Convert to milliseconds if your time is in seconds
    }

    public void SetClientTime(float time)
    {
        _clientTime = time;
    }
}