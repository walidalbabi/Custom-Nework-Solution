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
    class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }

        public static Dictionary<int, Client> clientsList = new Dictionary<int, Client>();
       
        public delegate void PacketHandler(int fromClient, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandler = new Dictionary<int, PacketHandler>();


        private static TcpListener tcpListener;
        private static UdpClient udpListener;


        public static void Start(int maxPlayers, int port)
        {
            MaxPlayers = maxPlayers;
            Port = port;

           Debug.Log($"Server: Starting ...");

            InitialiseServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

           Debug.Log($"Server: Started On {Port}");
        }

        private static void TCPConnectCallback(IAsyncResult result)
        {
            TcpClient client = tcpListener.EndAcceptTcpClient(result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

           Debug.Log($"Server: Incoming Connection from {client.Client.RemoteEndPoint} ...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if(clientsList[i].tcp.socket == null)
                {
                    clientsList[i].tcp.Connect(client);
                    return;
                }
            }
        }
        private static void UDPReceiveCallback(IAsyncResult result)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpListener.EndReceive(result, ref clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4)
                {
                    return;
                }

                using (Packet packet = new Packet(data))
                {
                    int clientID = packet.ReadInt();

                    if(clientID == 0)
                    {
                        return;
                    }

                    if(clientsList[clientID].udp.endPoint == null)
                    {
                        clientsList[clientID].udp.Connect(clientEndPoint);
                        return;
                    }

                    if (clientsList[clientID].udp.endPoint.ToString() == clientEndPoint.ToString())
                    {
                        clientsList[clientID].udp.HandleData(packet);
                    }
                }
            }
            catch (Exception e)
            {

               Debug.Log($"Server: Failed To Receive UDP Data {e}");
            }
        }
        public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
        {
            try
            {
                if(clientEndPoint != null)
                {
                    udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
                }
            }
            catch (Exception e)
            {

               Debug.Log($"Server: Failed To Send UDP Data To Client {clientEndPoint} error:  {e}");
            }
        }
        private static void InitialiseServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clientsList.Add(i, new Client(i));
            }


            packetHandler = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReveived },
                {(int)ClientPackets.ping, ServerHandle.PingRecieved },
                {(int)ClientPackets.inputs, ServerHandle.HandleInputs },
                {(int)ClientPackets.rotation, ServerHandle.PlayerRotation },
                {(int)ClientPackets.playerShoot, ServerHandle.PlayerShoot },
              
            };

           Debug.Log($"Server: Initialized Packets");
        }
        public static void Stop()
        {
            tcpListener.Stop();
            udpListener.Close();
        }

    }
}
