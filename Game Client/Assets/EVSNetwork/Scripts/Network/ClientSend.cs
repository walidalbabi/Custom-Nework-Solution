using GameServer;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
   private static void SendTCPData(Packet packet)
    {
        packet.WriteLength();
        Client.instance.tcp.SendData(packet);
    }

    private static void SendUDPData(Packet packet)
    {
        packet.WriteLength();
        Client.instance.udp.SendData(packet);
    }

    public static void WelcomeRecieved()
    {
        using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            packet.Write(Client.instance.myID);
            packet.Write(Client.instance.myUserName);

            SendTCPData(packet);
        }
    }
    public static void SendPing()
    {
        using (Packet packet = new Packet((int)ClientPackets.ping))
        {
            packet.Write(Time.time);

            SendUDPData(packet);
        }
    }
    public static void SendInputData(NetworkInput inputs)
    {
        using (Packet packet = new Packet((int)ClientPackets.inputs))
        {
            packet.Write(inputs.tick);
            packet.Write(inputs.forward);
            packet.Write(inputs.right);
            packet.Write(inputs.movements.Length);
            for (int i = 0; i < inputs.movements.Length; i++)
            {
                packet.Write(inputs.movements[i]);
            }
            SendUDPData(packet);
        }
    }

    public static void SendRotationData(Quaternion rotation)
    {
        using (Packet packet = new Packet((int)ClientPackets.rotation))
        {
            packet.Write(rotation);

            SendUDPData(packet);
        }
    }

    public static void PlayerShoot(ShootPayload shootPayload)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerShoot))
        {
            packet.Write(shootPayload.tick);
            packet.Write(shootPayload.origine);
            packet.Write(shootPayload.direction);
            packet.Write(shootPayload.time);
            SendTCPData(packet);
        }
    }

}
