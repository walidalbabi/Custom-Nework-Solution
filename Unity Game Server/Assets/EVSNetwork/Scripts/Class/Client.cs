using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using UnityEngine;

namespace GameServer
{
    class Client 
    {
        public static int defaultBufferSize = 4096;

        public int ID;
        public NetworkedClient networkedClient;
        public TCP tcp;
        public UDP udp;

        public Client(int id)
        {
            ID = id;
            tcp = new TCP(ID);
            udp = new UDP(ID);
        }




        /// <summary>
        /// TCP Client Base Class
        /// </summary>
        public class TCP
        {
            private readonly int ID;
            private NetworkStream _stream;
            private byte[] _receivedBuffer;
            private Packet _receivedData;
            public TcpClient socket;

            public TCP(int id)
            {
                ID = id;
            }

            public void Connect(TcpClient clientSocket)
            {
                socket = clientSocket;
                socket.ReceiveBufferSize = defaultBufferSize;
                socket.SendBufferSize = defaultBufferSize;

                _stream = clientSocket.GetStream();

                _receivedBuffer = new byte[defaultBufferSize];
                _receivedData = new Packet();

                _stream.BeginRead(_receivedBuffer, 0, defaultBufferSize, RecievedBufferCallback, null);


                ServerSend.Welcome(ID, "You are Connected to The Server");
            }
            public void SendData(Packet packet)
            {
                try
                {
                    if (socket != null)
                    {
                        _stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                   Debug.Log($"Error Sending Data To Player {ID} via TCP : {e}");
                }
            }
            private void RecievedBufferCallback(IAsyncResult result)
            {
                try
                {
                    int byteLenght = _stream.EndRead(result);
                    if (byteLenght <= 0)
                    {
                        //TODO// Disconnect
                       Debug.Log("Byte Lenght is 0");
                        Server.clientsList[ID].Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLenght];
                    Array.Copy(_receivedBuffer, data, byteLenght);

                    _receivedData.Reset(HandleData(data));
                    _stream.BeginRead(_receivedBuffer, 0, defaultBufferSize, RecievedBufferCallback, null);
                }
                catch (Exception e)
                {
                   Debug.Log($"Error Recieving TCP Packet : {e}");
                    Server.clientsList[ID].Disconnect();
                }
            }
            /// <summary>Prepares received data to be used by the appropriate packet handler methods.</summary>
            /// <param name="_data">The recieved data.</param>
            private bool HandleData(byte[] _data)
            {
                int _packetLength = 0;

                _receivedData.SetBytes(_data);

                if (_receivedData.UnreadLength() >= 4)
                {
                    // If client's received data contains a packet
                    _packetLength = _receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        // If packet contains no data
                        return true; // Reset receivedData instance to allow it to be reused
                    }
                }

                while (_packetLength > 0 && _packetLength <= _receivedData.UnreadLength())
                {
                    // While packet contains data AND packet data length doesn't exceed the length of the packet we're reading
                    byte[] _packetBytes = _receivedData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packetId = _packet.ReadInt();
                            Server.packetHandler[_packetId](ID,_packet); // Call appropriate method to handle the packet
                        }
                    });

                    _packetLength = 0; // Reset packet length
                    if (_receivedData.UnreadLength() >= 4)
                    {
                        // If client's received data contains another packet
                        _packetLength = _receivedData.ReadInt();
                        if (_packetLength <= 0)
                        {
                            // If packet contains no data
                            return true; // Reset receivedData instance to allow it to be reused
                        }
                    }
                }

                if (_packetLength <= 1)
                {
                    return true; // Reset receivedData instance to allow it to be reused
                }

                return false;
            }
            public void Disconnect() 
            {
                socket.Close();
                _stream = null;
                _receivedData = null;
                _receivedBuffer = null;
                socket = null;
            }

        }

        /// <summary>
        /// UDP Client Base Class
        /// </summary>
        public class UDP
        {

            public IPEndPoint endPoint;
            private int ID;

            public UDP(int id)
            {
                ID = id;
            }

            public void Connect(IPEndPoint endpoint)
            {
                endPoint = endpoint;
            }
            public void SendData(Packet packet)
            {
                Server.SendUDPData(endPoint, packet);
            }
            public void HandleData(Packet packetData)
            {
                int packetLenght = packetData.ReadInt();
                byte[] data = packetData.ReadBytes(packetLenght);

                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet packet = new Packet(data))
                    {
                        int packetID = packet.ReadInt();
                        Server.packetHandler[packetID](ID,packet);
                    }

                });
            }
            public void Disconnect()
            {
                endPoint = null;
            }

        }

        public void SendPlayerIntoGame(string playerName)
        {
            networkedClient = NetworkManager.instance.InstantiatePlayer();
            networkedClient.Initialize(ID, playerName);

            foreach (var client in Server.clientsList.Values)
            {
                if (client.networkedClient != null)
                {
                    if(client.ID != ID)
                    {
                        ServerSend.SpawnPlayer(ID, client.networkedClient);
                    }
                }
            }

            foreach (var client in Server.clientsList.Values)
            {
                if(client.networkedClient != null)
                {
                    ServerSend.SpawnPlayer(client.ID, networkedClient);
                }
            }
      
        }

        public void Disconnect()
        {
           Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has Disconnected");

            ThreadManager.ExecuteOnMainThread(() => {
                UnityEngine.Object.Destroy(networkedClient.gameObject);
                networkedClient = null;
            });

            tcp.Disconnect();
            udp.Disconnect();

            ServerSend.PlayerDisconnect(ID);
        }

    }

}


  

