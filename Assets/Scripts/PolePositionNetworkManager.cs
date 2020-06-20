using PolePosition.Manager;
using Mirror;
using PolePosition.Player;
using PolePosition.UI;
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
            // NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreatePlayer);
        }

        public override void OnServerAddPlayer(NetworkConnection connection)
        {
            if (_polePositionManager.MaxNumPlayers == _polePositionManager.Players.Count ||
                _polePositionManager.InRace)
            {
                connection.Disconnect();
            }
            else
            {
                Transform startPos = GetStartPosition();
                GameObject player = startPos != null
                    ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                    : Instantiate(playerPrefab);
            
                PlayerInfo playerInfo = player.GetComponent<PlayerInfo>();
                playerInfo.ID = connection.connectionId;
                playerInfo.Name = string.Format("Player {0}", connection.connectionId);
                playerInfo.Color = ColorPicker.GetRandomColor();
                playerInfo.CurrentPosition = startPositionIndex;
                playerInfo.CurrentLap = 0;
                playerInfo.NumberOfLaps = _polePositionManager.NumberOfLaps; 
            
                NetworkServer.AddPlayerForConnection(connection, player);
            
                _polePositionManager.AddPlayer(playerInfo);
            }
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            if(conn.identity != null)
            {
                _polePositionManager.RemovePlayer(conn.identity.GetComponent<PlayerInfo>());
            }   
            
            base.OnServerDisconnect(conn);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            _polePositionManager.uiManager.ActivateMainMenu();
            _polePositionManager.uiManager.ShowMainMenuMessage("Disconnected from server", 18, 5f);
            _polePositionManager.uiManager.ClearPlayerResults();
            _polePositionManager.uiManager.Lobby.Clear();
            _polePositionManager.uiManager.ResetCamera();
        }

        // Old code used to generate players
        // before lobby implementation
        // void OnCreatePlayer(NetworkConnection connection, CreatePlayerMessage message)
        // {
        //     Transform startPos = GetStartPosition();
        //     GameObject player = startPos != null
        //         ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
        //         : Instantiate(playerPrefab);
        //     
        //     PlayerInfo playerInfo = player.GetComponent<PlayerInfo>();
        //     playerInfo.ID = connection.connectionId;
        //     playerInfo.Name = string.Format("Player {0}", connection.connectionId);
        //     playerInfo.Color = message.Color;
        //     playerInfo.CurrentPosition = startPositionIndex;
        //     playerInfo.CurrentLap = 0;
        //     playerInfo.NumberOfLaps = _polePositionManager.NumberOfLaps; 
        //
        //     NetworkServer.AddPlayerForConnection(connection, player);
        //
        //     _polePositionManager.AddPlayer(playerInfo);
        // }

        // public override void OnClientConnect(NetworkConnection conn)
        // {
        //     base.OnClientConnect(conn);
        //
        //     if (_polePositionManager.MaxNumPlayers == _polePositionManager.Players.Count)
        //     {
        //         conn.Disconnect();
        //     }
        //     
        //     int id = conn.connectionId;
        //     
        //     CreatePlayerMessage message = new CreatePlayerMessage()
        //     {
        //         Name = string.Format("Player {0}", id),
        //         Color = ColorPicker.GetRandomColor()
        //     };
        //     NetworkClient.Send(message);
        // }
    }
}