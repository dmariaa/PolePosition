﻿using System;
using Mirror;
using PolePosition;
using PolePosition.Player;
using UnityEngine;
using UnityEngine.UI;

namespace PolePosition.UI
{
    public class UIManager : MonoBehaviour
    {
        public bool showGUI = true;
        private NetworkManager m_NetworkManager;
        
        [Header("Main Menu")] [SerializeField] private GameObject mainMenu;
        [SerializeField] private Button buttonHost;
        [SerializeField] private Button buttonClient;
        [SerializeField] private Button buttonServer;
        [SerializeField] private InputField inputFieldIP;

        [Header("In-Game HUD")] [SerializeField]
        private GameObject inGameHUD;
        [SerializeField] private Text textSpeed;
        [SerializeField] private Text textLaps;
        [SerializeField] private Text textCurrentLap;
        [SerializeField] private Text textCurrentLapTime;
        [SerializeField] private Text textCountDown;
        [SerializeField] private PlayerInfoPanelController[] playerPositions;

        [Header("Player Results")] [SerializeField]
        private GameObject _playerResults;
        [SerializeField] private GameObject _rankingPanel;
        [SerializeField] private PlayerResultsPanelController _playerResultsPanelController;

        private GameObject _lobby;
        public LobbyManager Lobby
        {
           get => _lobby.GetComponent<LobbyManager>();  
        } 
        
        private void Awake()
        {
            m_NetworkManager = FindObjectOfType<NetworkManager>();
            _lobby = transform.Find("Canvas/Lobby").gameObject;
        }

        private void Start()
        {
            buttonHost.onClick.AddListener(() => StartHost());
            buttonClient.onClick.AddListener(() => StartClient());
            buttonServer.onClick.AddListener(() => StartServer());
            ActivateMainMenu();
        }
        
        public void ActivateMainMenu()
        {
            inGameHUD.SetActive(false);
            _lobby.SetActive(false);
            _playerResults.SetActive(false);
            mainMenu.SetActive(true);
        }

        public void ActivateInGameHUD()
        {
            mainMenu.SetActive(false);
            _lobby.SetActive(false);
            _playerResults.SetActive(false);
            inGameHUD.SetActive(true);
        }

        public void ActivateLobby()
        {
            mainMenu.SetActive(false);
            inGameHUD.SetActive(false);
            _playerResults.SetActive(false);
            _lobby.SetActive(true);
        }

        public void ActivatePlayerResults()
        {
            mainMenu.SetActive(false);
            inGameHUD.SetActive(false);
            _lobby.SetActive(false);
            _playerResults.SetActive(true);
        }

        private void StartHost()
        {
            m_NetworkManager.StartHost();
            ActivateLobby();
        }

        private void StartClient()
        {
            var text = inputFieldIP.text;
            m_NetworkManager.networkAddress = text==string.Empty ? "localhost" : text;
            m_NetworkManager.StartClient();        
            ActivateLobby();
        }

        private void StartServer()
        {
            m_NetworkManager.StartServer();
            ActivateInGameHUD();
        }
        
        public void SetNumberOfLaps(int numberOfLaps)
        {
            textLaps.text = "" + numberOfLaps;
        }

        public void SetCurrentLap(int currentLap)
        {
            textCurrentLap.text = "" + currentLap;
        }

        public void SetCurrentLapTime(float lapTime)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(lapTime * 1000);
            textCurrentLapTime.text = string.Format("{1:D2}:{2:D2}.{3:D4}", timeSpan.Hours, 
                timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        }
        
        public void UpdateSpeed(int speed)
        {
            textSpeed.text = "Speed " + speed + " Km/h";
        }
        
        public void UpdatePlayersPositions(PlayerInfo playerInfo)
        {
            // Debug.LogFormat("Setting player position {0}", playerInfo);
            int position = playerInfo.CurrentPosition - 1;
            if(position >= 0 && position < playerPositions.Length)
            {
                PlayerInfoPanelController playerInfoPanelController = playerPositions[position];
                playerInfoPanelController.UpdateData(playerInfo.CurrentPosition, 
                    playerInfo.Color,
                    playerInfo.Name,
                    0f,
                    playerInfo.TotalRaceTime);
            }
        }
        
        public void UpdateUIMessage(string message, int fontSize = 30, Color? color = null)
        {
            textCountDown.color = color ?? Color.red;
            textCountDown.fontSize = fontSize;
            textCountDown.text = message;
        }

        public void AddPlayerResult(int position, Color color, string playerName, float raceTime, float bestLapTime)
        {
            var panelTransform = _rankingPanel.transform;
            PlayerResultsPanelController panel = Instantiate(_playerResultsPanelController, panelTransform);
            panel.UpdateData(position, color, playerName,raceTime, bestLapTime);
        }
    }
}