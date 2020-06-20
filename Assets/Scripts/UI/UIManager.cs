using System;
using Mirror;
using PolePosition.Player;
using UI;
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
        [SerializeField] private Text messageText;

        [Header("In-Game HUD")] [SerializeField]
        private GameObject inGameHUD;
        [SerializeField] private Text textSpeed;
        [SerializeField] private Text textLaps;
        [SerializeField] private Text textCurrentLap;
        [SerializeField] private Text textCurrentLapTime;
        [SerializeField] private Text textCountDown;
        [SerializeField] private PlayersPositionsController panelPositions;

        [Header("Player Results")] [SerializeField]
        private GameObject _playerResults;

        [SerializeField] private Text _resultsTitleText;
        [SerializeField] private Text _resultsSubTitleText;
        [SerializeField] private GameObject _rankingPanel;
        [SerializeField] private PlayerResultsPanelController _playerResultsPanelController;

        private CameraController _camera;
        
        private GameObject _lobby;
        public LobbyManager Lobby
        {
           get => _lobby.GetComponent<LobbyManager>();  
        } 
        
        private void Awake()
        {
            m_NetworkManager = FindObjectOfType<NetworkManager>();
            _lobby = transform.Find("Canvas/Lobby").gameObject;
            _camera = FindObjectOfType<CameraController>();
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
        
        public void UpdatePlayerPosition(PlayerInfo playerInfo)
        {
            panelPositions.UpdatePlayerInfo(playerInfo);
        }

        public void RemovePlayerPosition(PlayerInfo playerInfo)
        {
            panelPositions.RemovePlayerInfo(playerInfo);
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

        public void ClearPlayerResults()
        {
            var panelTransform = _rankingPanel.transform;
            foreach (var childPlayerResultsPanelController in panelTransform.GetComponentsInChildren<PlayerResultsPanelController>())
            {
                Destroy(childPlayerResultsPanelController.gameObject);
            }
        }

        public void SetPlayerResultsTitle(string title, int size = 30)
        {
            if(_resultsTitleText != null) _resultsTitleText.text = title;
        }

        public void SetPlayerResultsSubtitle(string subtitle, int size = 30)
        {
            if(_resultsSubTitleText != null) _resultsSubTitleText.text = subtitle;
        }

        public void SetPlayerResultsTexts(string title, string subtitle, int titleSize = 30, int subTitleSize = 30)
        {
            SetPlayerResultsTitle(title, titleSize);
            SetPlayerResultsSubtitle(subtitle, subTitleSize);
        }

        public void ShowPanelPositions(bool show)
        {
            panelPositions.gameObject.SetActive(show);
        }

        public void ResetCamera()
        {
            _camera.Reset();
        }

        private float _mainMenuMessageTimer = 0f;
        private float _mainMenuMessageSeconds = 0f;

        public void ShowMainMenuMessage(string message, int fontSize, float seconds)
        {
            _mainMenuMessageSeconds = seconds;
            _mainMenuMessageTimer = Time.deltaTime;
            messageText.text = message;
            messageText.fontSize = fontSize;
            messageText.transform.parent.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (_mainMenuMessageTimer > 0f)
            {
                _mainMenuMessageTimer += Time.deltaTime;
                if (_mainMenuMessageTimer >= _mainMenuMessageSeconds)
                {
                    _mainMenuMessageTimer = 0f;
                    messageText.transform.parent.gameObject.SetActive(false);
                }
            }
        }
    }
}