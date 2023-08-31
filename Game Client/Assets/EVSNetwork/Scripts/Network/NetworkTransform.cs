using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public struct PositionData
{
    public int tick;
    public Vector3 position;
    public void Init(int tick, Vector3 position)
    {
        this.tick = tick;
        this.position = position;
    }
}

public struct RotationData
{
    public int tick;
    public Quaternion rotation;

    public void Init(int tick, Quaternion rotation)
    {
        this.tick = tick;
        this.rotation = rotation;
    }
}


[RequireComponent(typeof(NetworkedObject))]
public class NetworkTransform : NetworkBehaviour
{
    [SerializeField] private float _interpolationSpeed = 2f;

    private PositionData _lastServerPositionalData;
    private PositionData _lastProccessedPositionalData;

    private RotationData _lastServerRotationalData;
    private RotationData _lastProccessedRotationalData;

    private float _currentLerpTime = 0f;
    private Vector3 _newPosition;
    private Quaternion _newRotation;


    protected override void Awake()
    {
        base.Awake();
        TimeManager.OnTick += OnTick;
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        TimeManager.OnTick -= OnTick;
    }
    protected override void OnNewObjectAdded(object obj)
    {
        if (obj.GetType() == typeof(PositionData))
        {
            int tick = (int)obj.GetType().GetField("tick").GetValue(obj);

            if (tick <= _lastProccessedPositionalData.tick) return;

            _lastServerPositionalData = (PositionData)obj;

            // Reset the interpolation time whenever new data arrives
            _currentLerpTime = 0f;
        }
        else if (obj.GetType() == typeof(RotationData))
        {
            int tick = (int)obj.GetType().GetField("tick").GetValue(obj);

            if (tick <= _lastProccessedRotationalData.tick) return;

            _lastServerRotationalData = (RotationData)obj;

            // Reset the interpolation time whenever new data arrives
            _currentLerpTime = 0f;
        }

    }

    private void OnTick(int tick)
    {
        //_currentLerpTime += NetworkManager.instance.timeManager.deltaTick * _interpolationSpeed;

        //// Clamp the value to make sure it's between 0 and 1
        //_currentLerpTime = Mathf.Clamp(_currentLerpTime, 0f, 1f);

        //Interpolate();
    }

    private void Update()
    {
        _currentLerpTime += Time.deltaTime * _interpolationSpeed;

        // Clamp the value to make sure it's between 0 and 1
        _currentLerpTime = Mathf.Clamp(_currentLerpTime, 0f, 1f);

        Interpolate();
    }

    private void Interpolate()
    {
        if (!_lastServerPositionalData.Equals(_lastProccessedPositionalData))
        {
            _newPosition = Vector3.Lerp(transform.position, _lastServerPositionalData.position, _currentLerpTime);
            transform.position = _newPosition;
        }

        if (!_lastServerRotationalData.Equals(_lastProccessedRotationalData))
        {
            _newRotation = Quaternion.Lerp(transform.rotation, _lastServerRotationalData.rotation, _currentLerpTime);
            transform.rotation = _newRotation;
        }
    }
}