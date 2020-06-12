using Mirror;
using PolePosition.Player;
using UnityEngine;

namespace PolePosition
{
    public class PolePositionNetworkManager : NetworkManager
    {
        public PolePositionManager _polePositionManager;

        public override void Awake()
        {
            if(_polePositionManager==null) _polePositionManager = FindObjectOfType<PolePositionManager>();
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

            NetworkServer.AddPlayerForConnection(connection, player);
            
            PlayerInfo playerInfo = player.GetComponent<PlayerInfo>();
            playerInfo.ID = connection.connectionId;
            playerInfo.Name = string.IsNullOrEmpty(message.Name) ? string.Format("Player{0}", startPositionIndex) : message.Name;
            playerInfo.Color = message.Color;
            playerInfo.CurrentPosition = startPositionIndex;
            playerInfo.CurrentLap = 0;
            playerInfo.NumberOfLaps = _polePositionManager.NumberOfLaps; 
            
            _polePositionManager.AddPlayer(playerInfo);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            _polePositionManager.RemovePlayer(conn.identity.GetComponent<PlayerInfo>());   
            base.OnClientDisconnect(conn);
        }
    }
}