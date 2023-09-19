using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameServer
{
    class ServerHandle
    {
        public static void WelcomeReveived(int fromClient, Packet packet)
        {
            int clientID = packet.ReadInt();
            string userName = packet.ReadString();

           Debug.Log($"Server: Client {clientID} Got the message and his name is : {userName}");
            if(fromClient != clientID)
            {
               Debug.Log($"Player {userName} of {clientID} has assumed wrong ID {fromClient}");
            }

            Server.clientsList[fromClient].SendPlayerIntoGame(userName);
        }
        public static void PingRecieved(int fromClient, Packet packet)
        {
            float timefromClient = packet.ReadFloat();

            ServerSend.SendPong(fromClient, timefromClient);
        }
        public static void HandleInputs(int fromClient, Packet packet)
        {
            NetworkInput extractedInput = new NetworkInput();
            extractedInput.tick = packet.ReadInt();
            extractedInput.forward = packet.ReadVector3();
            extractedInput.right = packet.ReadVector3();
            extractedInput.movements = new bool[packet.ReadInt()];
            for (int i = 0; i < extractedInput.movements.Length; i++)
            {
                extractedInput.movements[i] = packet.ReadBool();
            }
            Server.clientsList[fromClient].networkedClient.OnInput(extractedInput);
        }
        public static void PlayerRotation(int fromClient, Packet packet)
        {
            Quaternion rot = packet.ReadQuaternion();
            Server.clientsList[fromClient].networkedClient.transform.rotation = rot;
        }
        public static void PlayerShoot(int fromClient, Packet packet)
        {
            ShootPayload shootPayload = new ShootPayload()
            {
                tick = packet.ReadInt(),
                origine = packet.ReadVector3(),
                direction = packet.ReadVector3(),
                time = packet.ReadFloat(),
            };

            Debug.Log($"Recieved A Shoot Command From {fromClient}, at Tick {shootPayload.tick}");
            Server.clientsList[fromClient].networkedClient.SendCommandShoot(shootPayload);
        }
    }
}
