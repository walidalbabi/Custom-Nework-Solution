using GameServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet packet)
    {
        int tick = packet.ReadInt();
        float serverTime = packet.ReadFloat();
        string msg = packet.ReadString();
        int id = packet.ReadInt();

        Debug.Log($"Server:{ msg}");

        Client.instance.myID = id;
        ClientSend.WelcomeRecieved();
        NetworkManager.instance.timeManager.SetTick(tick);
        NetworkManager.instance.timeManager.SetClientTime(serverTime);

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }
    public static void PongRecieved(Packet packet)
    {
        float time = packet.ReadFloat();

        NetworkManager.instance.timeManager.SetPing(time);
    }
    public static void SpawnPlayer(Packet packet)
    {
        int id = packet.ReadInt();
        string username = packet.ReadString();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();

        NetworkManager.instance.SpawnPlayer(id, username, position, rotation);
    }
    public static void SetServerTick(Packet packet)
    {
        int tick = packet.ReadInt();
        NetworkManager.instance.timeManager.SetTick(tick);
    }
    public static void SetPlayerStatePayload(Packet packet)
    {
        int id = Client.instance.myID;
        Type type = Type.GetType(packet.ReadString());
        int tick = packet.ReadInt();
        Vector3 position = packet.ReadVector3();
        Vector3 velocity = packet.ReadVector3();

        //Changing to Object
        var obj = new StatePayload();
        obj.tick = tick;
        obj.position = position;
        obj.velocity = velocity;


        NetworkManager.players[id].SendAction(obj, type);
    }
    public static void ObjectPosition(Packet packet)
    {
        int id = packet.ReadInt();
        Vector3 pos = packet.ReadVector3();
        int tick = packet.ReadInt();

        PositionData data = new PositionData();
        data.Init(tick, pos);

        NetworkManager.networkObjects[id].SendAction(data, typeof(NetworkTransform));
    }
    public static void ObjectRotation(Packet packet)
    {
        int id = packet.ReadInt();
        Quaternion rot = packet.ReadQuaternion();
        int tick = packet.ReadInt();

        RotationData data = new RotationData();
        data.Init(tick, rot);

        NetworkManager.networkObjects[id].SendAction(data, typeof(NetworkTransform));
    }
    public static void WorldSnapsots(Packet packet)
    {
        Snapshots snapshot = new Snapshots();
        snapshot.tick = packet.ReadInt();
        snapshot.data = new List<PlayerSnapshostDataList>();

        int dataLength = packet.ReadInt();

        for (int i = 0; i < dataLength; i++)
        {
            int playerId = packet.ReadInt();
            Vector3 position = packet.ReadVector3();
            Quaternion rotation = packet.ReadQuaternion();

            if (playerId != Client.instance.myID)
            {
                snapshot.data.Add(new PlayerSnapshostDataList
                {
                    playerID = playerId,
                    position = position,
                    rotation = rotation
                });
            }
        }

        foreach (var playerData in snapshot.data)
        {
            PlayerSnapshot data = new PlayerSnapshot()
            {
                tick = snapshot.tick,
                position = playerData.position,
                rotation = playerData.rotation,
            };
            NetworkManager.networkObjects[playerData.playerID].SendAction(data, typeof(NetworkTransform));
        }
    }
    public static void PlayerDisconnect(Packet packet)
    {
        int id = packet.ReadInt();

        Destroy(NetworkManager.players[id].gameObject);
        NetworkManager.players.Remove(id);
    }
    public static void EntityHealth(Packet packet)
    {
        int id = packet.ReadInt();
        string typeName = packet.ReadString();
        Debug.Log(typeName);
        Type type = Type.GetType(typeName);
        float health = packet.ReadFloat();

        NetworkManager.networkObjects[id].SendAction(health, type);
    }
    public static void EntitySpawned(Packet packet)
    {
        int id = packet.ReadInt();
        Type type = Type.GetType(packet.ReadString());

        NetworkManager.networkObjects[id].SendAction("Respawn", type);
    }
}
