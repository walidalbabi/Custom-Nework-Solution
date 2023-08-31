using GameServer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private float SERVER_TICK_RATE = 60f;

    public int ClientTick;
    public int ServerTick;

    /// <summary>
    /// On Tick is Processed return The currentTick
    /// </summary>
    public static Action<int> OnTick;

    private float _timer;
    private int _currentTick;
    private float _minTimeBetweenTicks;

    private float tickDrift = 0; // The difference between the server and client ticks
    private float tickAdjustSpeed = 0.01f; // Speed at which to adjust tick drift

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

        // Calculate tick drift
        tickDrift = ServerTick - _currentTick;

        // Gradually adjust local tick to match server
        _currentTick += Mathf.RoundToInt(tickDrift * tickAdjustSpeed);

        while (_timer >= _minTimeBetweenTicks)
        {
            _timer -= _minTimeBetweenTicks;
            HandleTick();
            _currentTick++;

            ClientTick = _currentTick;
        }
    }

    private void HandleTick()
    {
        OnTick?.Invoke(_currentTick);
    }

    public void SetTick(int tick)
    {
        ServerTick = tick;
    }
}