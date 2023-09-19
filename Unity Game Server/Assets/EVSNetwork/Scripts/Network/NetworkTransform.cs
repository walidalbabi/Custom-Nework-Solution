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

    private PlayerSnapshotData _currentTickSnapshot = new PlayerSnapshotData();

    protected override void Awake()
    {
        base.Awake();
        TimeManager.OnTick += OnTick;

        _currentTickSnapshot.playerID = id;
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
        Sync(tick);
    }

    private void Sync(int tick)
    {

        if (_syncType == SyncType.SyncTransform)
        {
            _currentTickSnapshot.position = transform.position;
            _currentTickSnapshot.rotation = transform.rotation;
        }
        else if (_syncType == SyncType.SyncPosition)
        {
            _currentTickSnapshot.position = transform.position;
        }
        else if (_syncType == SyncType.SyncRotatiom)
        {
            _currentTickSnapshot.rotation = transform.rotation;
        }

        NetworkManager.instance.AddSnapshot(_currentTickSnapshot, tick, _currentTickSnapshot.playerID);
    }
}
