using Characters;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace Main
{
    public class SolarSystemNetworkManager : NetworkManager
    {
        [SerializeField] private string _playerName;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private int _count;
        private Dictionary<int, ShipController> _players = new Dictionary<int, ShipController>();


        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            Transform spawnTransform = GetStartPosition();
            GameObject player = Instantiate(playerPrefab, spawnTransform.position, spawnTransform.rotation);
            player.GetComponent<ShipController>().PlayerName = _inputField.text;
            _players.Add(conn.connectionId, player.GetComponent<ShipController>());

            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            NetworkServer.RegisterHandler(120, ReceiveName);
        }

        public class MessageLogin : MessageBase
        {
            public string Login;


            public override void Deserialize(NetworkReader reader)
            {
                Login = reader.ReadString();
            }

            public override void Serialize(NetworkWriter writer)
            {
                writer.Write(Login);
            }
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            MessageLogin login = new MessageLogin();
            login.Login = _inputField.text;
            conn.Send(120, login);
        }

        public void ReceiveName(NetworkMessage networkMessage)
        {
            _players[networkMessage.conn.connectionId].PlayerName = networkMessage.reader.ReadString();
            _players[networkMessage.conn.connectionId].gameObject.name = _players[networkMessage.conn.connectionId].
                PlayerName;
            Debug.Log(_players[networkMessage.conn.connectionId]);
        }
    }
}