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
        public static void HandleInputs(int fromClient, Packet packet)
        {
            object inputObject = new NetworkInput();
            // Iterate through each field in the struct
            foreach (var field in inputObject.GetType().GetFields())
            {
                string fieldName = field.Name;
                object fieldValue = field.GetValue(inputObject);

                // Based on the type of the field, process the value accordingly
                if (field.FieldType == typeof(int))
                {
                    int intValue = packet.ReadInt();
                    field.SetValue(inputObject, intValue);
                }
                else if (field.FieldType == typeof(float))
                {
                    float floatValue = packet.ReadFloat();
                    field.SetValue(inputObject,floatValue);
                }
                else if (field.FieldType == typeof(bool))
                {
                    bool boolValue = packet.ReadBool();
                    field.SetValue(inputObject, boolValue);
                }
                // ... other types here If Needed
            }

            NetworkInput extractedInput = (NetworkInput)inputObject;
         
            Server.clientsList[fromClient].networkedClient.OnInput(extractedInput);
        }
        public static void PlayerRotation(int fromClient, Packet packet)
        {
            Quaternion rot = packet.ReadQuaternion();
            Server.clientsList[fromClient].networkedClient.transform.rotation = rot;
        }
        public static void PlayerShoot(int fromClient, Packet packet)
        {
            Vector3 origin = packet.ReadVector3();
            Vector3 direction = packet.ReadVector3();

            Ray ray = new Ray();
            ray.origin = origin;
            ray.direction = direction;

            Server.clientsList[fromClient].networkedClient.SendCommandShoot(ray);
        }
    }
}
