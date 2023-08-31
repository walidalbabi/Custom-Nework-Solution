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
        string msg = packet.ReadString();
        int id = packet.ReadInt();

        Debug.Log($"Server:{ msg}");

        Client.instance.myID = id;
        ClientSend.WelcomeRecieved();
        NetworkManager.instance.timeManager.SetTick(tick);

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
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
        obj.velovity = velocity;


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
    public static void PlayerDisconnect(Packet packet)
    {
        int id = packet.ReadInt();

        Destroy(NetworkManager.players[id].gameObject);
        NetworkManager.players.Remove(id);
    }
    public static void PlayerHealth(Packet packet)
    {
        int id = packet.ReadInt();
        float health = packet.ReadFloat();

     //   GameManager.players[id].SetHealth(health);
    }
    public static void PlayerSpawned(Packet packet)
    {
        int id = packet.ReadInt();

      //  GameManager.players[id].Respawned();
    }
}
