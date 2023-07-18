using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace LessonThree
{
    public class Server : MonoBehaviour
    {
        private const int MAX_CONNECTION = 10;

        private int port = 5805;

        private int hostID;
        private int reliableChannel;

        private bool isStarted = false;
        private byte error;

        List<int> connectionIDs = new List<int>();


        public void StartServer()
        {
            NetworkTransport.Init();

            ConnectionConfig cc = new ConnectionConfig();
            reliableChannel = cc.AddChannel(QosType.Reliable);
            HostTopology topology = new HostTopology(cc, MAX_CONNECTION);
            hostID = NetworkTransport.AddHost(topology, port);
            isStarted = true;
        }

        public void ShutDownServer()
        {
            if (!isStarted)
                return;

            NetworkTransport.RemoveHost(hostID);
            NetworkTransport.Shutdown();
            isStarted = false;
        }

        void Update()
        {
            if (!isStarted)
                return;

            int recHostId;
            int connectionId;
            int channelId;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;
            NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);

            while (recData != NetworkEventType.Nothing)
            {
                switch (recData)
                {
                    case NetworkEventType.Nothing:
                        break;
                    case NetworkEventType.ConnectEvent:
                        connectionIDs.Add(connectionId);
                        SendMessageToAll($"Player {connectionId} has connected.");
                        Debug.Log($"Player {connectionId} has connected.");
                        break;
                    case NetworkEventType.DataEvent:
                        string message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                        SendMessageToAll($"Player {connectionId}: {message}");
                        Debug.Log($"Player {connectionId}: {message}");
                        break;
                    case NetworkEventType.DisconnectEvent:
                        connectionIDs.Remove(connectionId);
                        SendMessageToAll($"Player {connectionId} has disconnected.");
                        Debug.Log($"Player {connectionId} has disconnected.");
                        break;
                    case NetworkEventType.BroadcastEvent:
                        break;
                }
                recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
            }
        }

        public void SendMessageToAll(string message)
        {
            for (int i = 0; i<connectionIDs.Count; i++)
            {
                SendMessage(message, connectionIDs[i]);
            }
        }

        public void SendMessage(string message, int connectionID)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(message);
            NetworkTransport.Send(hostID, connectionID, reliableChannel, buffer, message.Length *
            sizeof(char), out error);
            if ((NetworkError)error != NetworkError.Ok)
                Debug.Log((NetworkError)error);
        }
    }
}