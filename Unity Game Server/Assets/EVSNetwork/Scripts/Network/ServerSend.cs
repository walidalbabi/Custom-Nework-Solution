using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameServer
{
    class ServerSend
    {
        #region Send Protocols
        private static void SendTCPData(int clientID, Packet packet)
        {
            packet.WriteLength();
            Server.clientsList[clientID].tcp.SendData(packet);
        }
        private static void SendUDPData(int clientID, Packet packet)
        {
            packet.WriteLength();
            Server.clientsList[clientID].udp.SendData(packet);
        }
        private static void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clientsList[i].tcp.SendData(packet);
            }
        }
        private static void SendTCPDataToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != exceptClient)
                    Server.clientsList[i].tcp.SendData(packet);
            }
        }
        private static void SendUDPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clientsList[i].udp.SendData(packet);
            }
        }
        private static void SendUDPDataToAll(int exceptClient,Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != exceptClient)
                    Server.clientsList[i].udp.SendData(packet);
            }
        }
        #endregion Send Protocols

        #region Send Functions
        public static void Welcome(int clientID, string message)
        {
            using(Packet packet = new Packet((int)ServerPackets.welcome))
            {
                packet.Write(NetworkManager.instance.timeManager.GetTick());
                packet.Write(message);
                packet.Write(clientID);

                SendTCPData(clientID, packet);
            }
        }
        public static void SendServerTick(int tick)
        {
            using (Packet packet = new Packet((int)ServerPackets.serverTick))
            {
                packet.Write(tick);

                SendUDPDataToAll(packet);
            }
        }

        public static void SpawnPlayer(int toClient, NetworkedClient networkedClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                packet.Write(networkedClient.id);
                packet.Write(networkedClient.userName);
                packet.Write(networkedClient.transform.position);
                packet.Write(networkedClient.transform.rotation);

                SendTCPData(toClient, packet);
            }
        }
        public static void SpawnNetworkObject(int toClient, NetworkedObject networkObject)
        {
            using (Packet packet = new Packet((int)ServerPackets.spawnNetworkObject))
            {
                packet.Write(networkObject.id);
                packet.Write(networkObject.userName);
                packet.Write(networkObject.transform.position);
                packet.Write(networkObject.transform.rotation);

                SendTCPDataToAll(toClient, packet);
            }
        }
        public static void ObjectPosition(NetworkedObject networkedObject)
        {
            using (Packet packet = new Packet((int)ServerPackets.objectPosition))
            {
                packet.Write(networkedObject.id);
                packet.Write(networkedObject.transform.position);
                packet.Write(NetworkManager.instance.timeManager.GetTick());

                SendUDPDataToAll(networkedObject.id,packet);
            }
        }
        public static void ObjectRotation(NetworkedObject networkedObject)
        {
            using (Packet packet = new Packet((int)ServerPackets.objectRotation))
            {
                packet.Write(networkedObject.id);
                packet.Write(networkedObject.transform.rotation);
                packet.Write(NetworkManager.instance.timeManager.GetTick());

                SendUDPDataToAll(networkedObject.id, packet);
            }
        }
        public static void PlayerDisconnect(int playerID)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerDisconnect))
            {
                packet.Write(playerID);
                SendTCPDataToAll(packet);
            }
        }
        public static void SendStatePayload(int playerID, StatePayload payload, Type type)
        {
            using (Packet packet = new Packet((int)ServerPackets.statePayload))
            {
                packet.Write(type.AssemblyQualifiedName);
                packet.Write(payload.tick);
                packet.Write(payload.position);
                packet.Write(payload.velovity);

                SendUDPData(playerID, packet);
            }
        }
        public static void PlayerHealth(PlayerController player)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerHealth))
            {
        //        packet.Write(player.ID);
        //        packet.Write(player.health);

                SendTCPDataToAll(packet);
            }
        }
        public static void playerRespawned(PlayerController player)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerRespawned))
            {
          //      packet.Write(player.ID);

        //        SendTCPDataToAll(packet);
            }
        }
        #endregion Send Functions

    }
}
