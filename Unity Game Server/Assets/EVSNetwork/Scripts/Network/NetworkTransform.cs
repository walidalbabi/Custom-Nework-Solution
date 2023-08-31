using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SyncType
{
    SyncTransform,
    SyncPosition,
    SyncRotatiom
}
[RequireComponent(typeof(NetworkedObject))]
public class NetworkTransform : NetworkBehaviour
{
    [SerializeField] private SyncType _syncType;

    [Tooltip("This is used to specify how many ticks should be passed before we send the data")]
    [SerializeField][Min(1)] private int sendInterval = 1;  // Send every 3 ticks

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
        return;
    }

    private void OnTick(int tick)
    {
        if (tick % sendInterval == 0)
            Sync();
    }

    private void Sync()
    {
        if (_syncType == SyncType.SyncTransform)
        {
            GameServer.ServerSend.ObjectPosition(_networkObject);
            GameServer.ServerSend.ObjectRotation(_networkObject);
        }
        else if (_syncType == SyncType.SyncPosition)
        {
            GameServer.ServerSend.ObjectPosition(_networkObject);
        }
        else if (_syncType == SyncType.SyncRotatiom)
        {
            GameServer.ServerSend.ObjectRotation(_networkObject);
        }
    }
}
