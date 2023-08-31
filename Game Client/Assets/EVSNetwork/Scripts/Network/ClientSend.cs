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
            packet.Write("UserName");

            SendTCPData(packet);
        }
    }


    public static void SendInputData(NetworkInput inputs)
    {
        using (Packet packet = new Packet((int)ClientPackets.inputs))
        {
            foreach (var field in typeof(NetworkInput).GetFields(BindingFlags.Instance |
                                                  BindingFlags.NonPublic |
                                                  BindingFlags.Public))
            {
                object value = field.GetValue(inputs);
                if (value is int)
                {
                    packet.Write((int)value);
                }
                else if (value is float)
                {
                    packet.Write((float)value);
                }
                else if (value is bool)
                {
                    packet.Write((bool)value);
                }

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

    public static void PlayerShoot(Ray ray)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerShoot))
        {
            packet.Write(ray.origin);
            packet.Write(ray.direction);

            SendTCPData(packet);
        }
    }

}
