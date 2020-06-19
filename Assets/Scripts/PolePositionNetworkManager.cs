using Mirror;
using PolePosition.Player;
using PolePosition.UI;
using UnityEngine;

namespace PolePosition
{
    public class PolePositionNetworkManager : NetworkManager
    {
        public PolePositionManager.PolePositionManager _polePositionManager;

        public override void Awake()
        {
            if(_polePositionManager==null) _polePositionManager = FindObjectOfType<PolePositionManager.PolePositionManager>();
        }

        public override void OnStartServer()
        {
            // Configuration is in polePositionManager
            maxConnections = _polePositionManager.MaxNumPlayers;
            
            base.OnStartServer();
            NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreatePlayer);
        }

        void OnCreatePlayer(NetworkConnection connection, CreatePlayerMessage message)
        {
            Transform startPos = GetStartPosition();
            GameObject player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);
            
            PlayerInfo playerInfo = player.GetComponent<PlayerInfo>();
            playerInfo.ID = connection.connectionId;
            playerInfo.Name = message.Name;
            playerInfo.Color = message.Color;
            playerInfo.CurrentPosition = startPositionIndex;
            playerInfo.CurrentLap = 0;
            playerInfo.NumberOfLaps = _polePositionManager.NumberOfLaps; 

            NetworkServer.AddPlayerForConnection(connection, player);

            _polePositionManager.AddPlayer(playerInfo);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
            CreatePlayerMessage message = new CreatePlayerMessage()
            {
                Name = string.Format("Player {0}", conn.connectionId),
                Color = ColorPicker.GetRandomColor()
            };
            NetworkClient.Send(message);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            _polePositionManager.RemovePlayer(conn.identity.GetComponent<PlayerInfo>());   
            base.OnServerDisconnect(conn);
        }
    }
}