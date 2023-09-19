using GameServer;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public struct Snapshots
{
    public int tick;
    public Dictionary<int, PlayerSnapshotData> data;
}
public struct PlayerSnapshotData
{
    public int playerID;
    public Vector3 position;
    public Quaternion rotation;
}


public class NetworkManager : MonoBehaviour
{

    public static NetworkManager instance;

    [SerializeField] private int _port = 5555;

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform[] _spawnPoints;

    public static Dictionary<int, NetworkedObject> networkObjects = new Dictionary<int, NetworkedObject>();

    [HideInInspector] public TimeManager timeManager;

    private static int _networkedObjectId = 1000;

    //Snapshots
    int _bufferIndex = -1;
    private Snapshots[] _snapshotsBuffer;
    private Snapshots _currentTickSnapshot;
    private int _lastProcessedTick = -1;


    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);


        timeManager = GetComponent<TimeManager>();

        TimeManager.OnTick += OnTick;

        _snapshotsBuffer = new Snapshots[1024];

        for (int i = 0; i < _snapshotsBuffer.Length; i++)
        {
            _snapshotsBuffer[i].data = new Dictionary<int, PlayerSnapshotData>();
        }

        _currentTickSnapshot = new Snapshots();
    }

    private void OnDestroy()
    {
        TimeManager.OnTick -= OnTick;
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Server.Start(50, _port);
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    private void OnTick(int tick)
    {
        if (tick % 3 == 0)
            Sync(tick);
    }

    private void Sync(int tick)
    {
  
    }
    public NetworkedClient InstantiatePlayer() => Instantiate(_playerPrefab, GetARandomSpawnPoint().position, Quaternion.identity).GetComponent<NetworkedClient>();

    public Transform GetARandomSpawnPoint() { return _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)]; }

    public static NetworkedObject InitializeNetworkObject(NetworkedObject obj)
    {
        _networkedObjectId++;
        obj.id = _networkedObjectId;
        obj.userName = "Server";
        networkObjects.Add(obj.id, obj);
        return obj;
    }

    public void AddSnapshot(PlayerSnapshotData snapshotData, int tick, int entityId)
    {
        _bufferIndex = tick % 1024;

        if (_lastProcessedTick != tick)
        {
            // Clear the previous data
            _snapshotsBuffer[_bufferIndex].data.Clear();
            _lastProcessedTick = tick;
        }

        // Update the snapshot data for the entity. If the entity doesn't exist, it will be added.
        _snapshotsBuffer[_bufferIndex].data[entityId] = snapshotData;

        _snapshotsBuffer[_bufferIndex].tick = tick;


        if (tick % 3 == 0)
        {
            _currentTickSnapshot = _snapshotsBuffer[_bufferIndex];

            //Debug.Log($"{tick} + {_currentTickSnapshot.data[0].position}");
            //SendSnapshots
            ServerSend.WorldSnapshot(_currentTickSnapshot);
        }
         
    }

}
