using System;
using System.Collections.Generic;
using System.Linq;
using PolePosition.Player;
using UnityEngine;

namespace PolePosition.UI
{
    /// <summary>
    /// Lobby UI manager
    /// </summary>
    public class LobbyManager : MonoBehaviour
    {
        /// <summary>
        /// Prefab used in connection info elements
        /// </summary>
        [SerializeField] private GameObject ConnectionInfoPrefab = null;
        
        /// <summary>
        /// Players panel, connected players are created as childs 
        /// </summary>
        private Transform _playersPanel;
        
        /// <summary>
        /// Player settings UI 
        /// </summary>
        private PlayerSettingsManager _playerSettings;
        
        private void Awake()
        {
            _playersPanel = transform.Find("PlayersPanel");
            _playerSettings = transform.Find("PlayerSettings").GetComponent<PlayerSettingsManager>();
        }

        /// <summary>
        /// Adds a player to the lobby
        /// </summary>
        /// <param name="playerInfo">Player information</param>
        public void AddPlayer(PlayerInfo playerInfo)
        {
            if (ConnectionInfoPrefab != null)
            {
                GameObject connectionInfo = Instantiate(ConnectionInfoPrefab, _playersPanel);
                ConnectionInfoController controller = connectionInfo.GetComponent<ConnectionInfoController>();
                controller.CarID = playerInfo.ID;
                controller.PlayerName = playerInfo.Name;
                controller.CarColor = playerInfo.Color;
                controller.IsReady = false;
                
                if (playerInfo.isLocalPlayer)
                {
                    controller.EnableReadyButton(true);
                    
                    controller.OnPlayerNameClicked = () =>
                    {
                        _playerSettings.gameObject.SetActive(true);
                        _playerSettings.PlayerName = controller.PlayerName;
                        _playerSettings.CarColor = controller.CarColor;
                        
                        _playerSettings.OnAcceptButtonClicked = (playerName, color) =>
                        {
                            Debug.LogFormat("LobbyManager {0}, {1}", playerName, color);
                           
                            // Update interface
                            controller.PlayerName = playerName;
                            controller.CarColor = color;
                            
                            // Notify server
                            playerInfo.CmdSetName(playerName);
                            playerInfo.CmdSetColor(color);
                        };
                    };

                    controller.OnReadyButtonClicked = () =>
                    {
                        playerInfo.CmdSetIsReady(controller.IsReady);      
                    };
                }
                else
                {
                    controller.EnableReadyButton(false);
                    playerInfo.OnChangeColorEvent += color => controller.CarColor = color;
                    playerInfo.OnChangeNameEvent += playerName => controller.PlayerName = playerName;
                    playerInfo.OnChangeIsReadyEvent += ready => controller.IsReady = ready;
                }
            }
        }

        public void RemovePlayer(PlayerInfo playerInfo)
        {
            foreach (var connectionInfoController in _playersPanel.GetComponentsInChildren<ConnectionInfoController>())
            {
                if (connectionInfoController.CarID == playerInfo.ID)
                {
                    Destroy(connectionInfoController.gameObject);
                }
            }
        }
    }
}