using System;
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

        [Header("Player Setup")] [SerializeField]
        private GameObject playerSetup;

        [SerializeField] private InputField _playerName;
        [SerializeField] private ColorPicker _colorPicker;
        [SerializeField] private Button _readyButton;

        [Header("Player Results")] [SerializeField]
        private GameObject _playerResults;

        [SerializeField] private GameObject _rankingPanel;
        [SerializeField] private PlayerResultsPanelController _playerResultsPanelController;

        private void Awake()
        {
            m_NetworkManager = FindObjectOfType<NetworkManager>();
        }

        private void Start()
        {
            buttonHost.onClick.AddListener(() => StartHost());
            buttonClient.onClick.AddListener(() => StartClient());
            buttonServer.onClick.AddListener(() => StartServer());
            _readyButton.onClick.AddListener(() => PlayerReady());
        
            ActivateMainMenu();
        }
        
        public void ActivateMainMenu()
        {
            inGameHUD.SetActive(false);
            playerSetup.SetActive(false);
            _playerResults.SetActive(false);
            mainMenu.SetActive(true);
        }

        public void ActivateInGameHUD()
        {
            mainMenu.SetActive(false);
            playerSetup.SetActive(false);
            _playerResults.SetActive(false);
            inGameHUD.SetActive(true);
        }

        public void ActivatePlayerSetup()
        {
            mainMenu.SetActive(false);
            inGameHUD.SetActive(false);
            _playerResults.SetActive(false);
            playerSetup.SetActive(true);
        }

        public void ActivatePlayerResults()
        {
            mainMenu.SetActive(false);
            inGameHUD.SetActive(false);
            playerSetup.SetActive(false);
            _playerResults.SetActive(true);
        }

        private void StartHost()
        {
            m_NetworkManager.StartHost();
            ActivatePlayerSetup();
        }

        private void StartClient()
        {
            var text = inputFieldIP.text;
            m_NetworkManager.networkAddress = text==string.Empty ? "localhost" : text;
            m_NetworkManager.StartClient();        
            ActivatePlayerSetup();
        }

        private void StartServer()
        {
            m_NetworkManager.StartServer();
            ActivateInGameHUD();
        }


        /// <summary>
        /// Called when user plays ready button
        /// </summary>
        private void PlayerReady()
        {
            CreatePlayerMessage message = new CreatePlayerMessage()
            {
                Name = _playerName.text,
                Color = _colorPicker.SelectedColor
            };

            NetworkClient.Send(message);
            ActivateInGameHUD();
        }

        public void SetConfigUIName(string playerName)
        {
            _playerName.text = playerName;
        }

        public void SetConfigUIColor(Color color)
        {
            _colorPicker.SelectedColor = color;
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
        
        public void UpdateCountdown(int countdown)
        {
            int maxCountdown = 4;
            if (countdown >= maxCountdown)
            {
                textCountDown.text = "Waiting for drivers...";
                textCountDown.fontSize = 62;
            }
            else if (countdown > 0 && countdown < maxCountdown)
            {
                textCountDown.fontSize = 300;
                textCountDown.text = "" + countdown;
            }
            else if (countdown == 0)
            {
                textCountDown.text = "GO";
            }
            else
            {
                textCountDown.text = "";
            }
        }

        public void AddPlayerResult(int position, Color color, string playerName, float raceTime, float bestLapTime)
        {
            var panelTransform = _rankingPanel.transform;
            PlayerResultsPanelController panel = Instantiate(_playerResultsPanelController, panelTransform);
            panel.UpdateData(position, color, playerName,raceTime, bestLapTime);
        }
    }
}