using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using GameServer;

public class Client : MonoBehaviour
{

    public static Client instance;
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 5555;
    public int myID = 0;
    public TCP tcp;
    public UDP udp;

    private bool _isConnected = false;
    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> _packetHandlers;


    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        tcp = new TCP();
        udp = new UDP();

        ConnectToServer();
    }
    private void OnApplicationQuit()
    {
        Disconnect();
    }
    public void ConnectToServer()
    {
        InitializeClientData();
        _isConnected = true;
        tcp.Connect();
    }

    #region Protocols
    public class TCP
    {
        public TcpClient socket;
        public NetworkStream stream;
        private Packet _receivedData;
        private byte[] _receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            _receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }
        public void SendData(Packet packet)
        {
            try
            {
                if(socket != null)
                {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null
                        , null);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error Sending TCP Data : {e}");
                throw;
            }
        }
        private void ConnectCallback(IAsyncResult result)
        {
            socket.EndConnect(result);

            if (!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();

            _receivedData = new Packet();

            stream.BeginRead(_receiveBuffer, 0, dataBufferSize, RecieveCallback, null);
        }
        private void RecieveCallback(IAsyncResult result)
        {
            try
            {
                int byteLenght = stream.EndRead(result);
                if (byteLenght <= 0)
                {
                    instance.Disconnect();
                    Debug.LogError("Byte lenght is "+ byteLenght);
                    return;
                }

                byte[] data = new byte[byteLenght];
                Array.Copy(_receiveBuffer, data, byteLenght);

                _receivedData.Reset(HandleData(data));
                stream.BeginRead(_receiveBuffer, 0, dataBufferSize, RecieveCallback, null);
            }
            catch (Exception e)
            {
                Disconnect();
                //ToDo : Disconnect
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
                        _packetHandlers[_packetId](_packet); // Call appropriate method to handle the packet
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
        private void Disconnect()
        {
            instance.Disconnect();

            stream = null;
            _receivedData = null;
            _receiveBuffer = null;
            socket = null;
        }

    }
    public class UDP
    {

        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect(int localPort)
        {
            socket = new UdpClient(localPort);
            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using (Packet packet = new Packet())
            {
                SendData(packet);
            }
        }
        public void SendData(Packet packet)
        {
            try
            {
                packet.InsertInt(instance.myID);

                if(socket != null)
                {
                    socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }
            }
            catch (Exception e)
            {

                Debug.Log($"Client: Failed To Send UDP Data {e}");
            }
        }
        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                byte[] data = socket.EndReceive(result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }

                HandleData(data);
            }
            catch (Exception e)
            {
                Disconnect();
            }
        }
        private void HandleData(byte[] data)
        {
            using(Packet packet = new Packet(data))
            {
                int packetLenght = packet.ReadInt();
                data = packet.ReadBytes(packetLenght);
            }

            ThreadManager.ExecuteOnMainThread(() => {
                using (Packet packet = new Packet(data))
                {
                    int packetID = packet.ReadInt();
                    _packetHandlers[packetID](packet);
                }

            });
        }
        private void Disconnect()
        {
            instance.Disconnect();

            endPoint = null;
            socket = null;
        }
    }

    #endregion Protocols

    private void InitializeClientData()
    {
        _packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ServerPackets.welcome, ClientHandle.Welcome },
                {(int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
                {(int)ServerPackets.serverTick, ClientHandle.SetServerTick },
                {(int)ServerPackets.objectPosition, ClientHandle.ObjectPosition },
                {(int)ServerPackets.objectRotation, ClientHandle.ObjectRotation },
                {(int)ServerPackets.playerDisconnect, ClientHandle.PlayerDisconnect },
                {(int)ServerPackets.statePayload, ClientHandle.SetPlayerStatePayload },
                {(int)ServerPackets.playerHealth, ClientHandle.PlayerHealth },
                {(int)ServerPackets.playerRespawned, ClientHandle.PlayerSpawned },

            };
        Debug.Log($"Client: Initialize Packet");
    }
    private void Disconnect()
    {
        _isConnected = false;
        tcp.socket.Close();
        udp.socket.Close();

        Debug.Log("Disconnected From Server.");
    }
}

