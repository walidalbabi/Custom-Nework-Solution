using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Snapshots
{
    public int tick;
    public List<PlayerSnapshostDataList> data;
}
public struct PlayerSnapshostDataList
{
    public int playerID;
    public Vector3 position;
    public Quaternion rotation;
}
public class NetworkManager : MonoBehaviour
{

    public static NetworkManager instance;

    public static Dictionary<int, NetworkedClient> players = new Dictionary<int, NetworkedClient>();
    public static Dictionary<int, NetworkedObject> networkObjects = new Dictionary<int, NetworkedObject>();

    public GameObject playerPrefLocal;
    public GameObject playerPref;

    [HideInInspector] public TimeManager timeManager;

    private static int _networkedObjectId = 1000;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        timeManager = GetComponent<TimeManager>();

    }

    public void SpawnPlayer(int id, string username, Vector3 position, Quaternion rotation)
    {
        GameObject player;
        if (id == Client.instance.myID)
        {
            player = Instantiate(playerPrefLocal, position, rotation);
        }
        else
        {
            player = Instantiate(playerPref, position, rotation);
        }

        var networkClient = player.GetComponent<NetworkedClient>();

        networkClient.Initialize(id, username);
        players.Add(id, networkClient);
        networkObjects.Add(id, networkClient);
    }

    public static NetworkedObject InitializeNetworkObject(NetworkedObject obj)
    {
        _networkedObjectId++;
        obj.id = _networkedObjectId;
        obj.userName = "Server";
        networkObjects.Add(obj.id, obj);
        return obj;
    }
}
