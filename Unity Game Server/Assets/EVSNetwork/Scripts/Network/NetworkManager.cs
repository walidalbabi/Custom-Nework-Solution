using GameServer;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{

    public static NetworkManager instance;

    [SerializeField] private int _port = 5555;

    public GameObject playerPrefab;

    public static Dictionary<int, NetworkedObject> networkObjects = new Dictionary<int, NetworkedObject>();

    [HideInInspector] public TimeManager timeManager;

    private static int _networkedObjectId = 1000;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);


        timeManager = GetComponent<TimeManager>();
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
        Server.Start(50, _port);
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    public NetworkedClient InstantiatePlayer() => Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<NetworkedClient>();

    public static NetworkedObject InitializeNetworkObject(NetworkedObject obj)
    {
        _networkedObjectId++;
        obj.id = _networkedObjectId;
        obj.userName = "Server";
        networkObjects.Add(obj.id, obj);
        return obj;
    }
}
