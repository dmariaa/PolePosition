using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private Text[] textPositions;

    [Header("Player Setup")] [SerializeField]
    private GameObject playerSetup;

    [SerializeField] private InputField _playerName;
    [SerializeField] private ColorPicker _colorPicker;
    [SerializeField] private Button _readyButton;


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

    public void UpdateSpeed(int speed)
    {
        textSpeed.text = "Speed " + speed + " Km/h";
    }

    public void UpdatePlayersPositions(PlayerInfo playerInfo)
    {
        int position = playerInfo.CurrentPosition - 1;
        if(position >= 0 && position < textPositions.Length)
        {
            Text text = textPositions[position];
            text.text = string.Format("{0} Lap{1}", playerInfo.Name, playerInfo.CurrentLap);
            text.color = playerInfo.Color;
        }
    }

    public void ClearPlayerPosition(PlayerInfo playerInfo)
    {
        int position = playerInfo.CurrentPosition - 1;
        if (position >= 0 && position < textPositions.Length)
        {
            Text text = textPositions[position];
            text.text = "";
        }
    }
    
    private void ActivateMainMenu()
    {
        mainMenu.SetActive(true);
        inGameHUD.SetActive(false);
        playerSetup.SetActive(false);
    }

    private void ActivateInGameHUD()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(true);
        playerSetup.SetActive(false);
    }

    private void ActivatePlayerSetup()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(false);
        playerSetup.SetActive(true);
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
    /// Delegate to change player name and color
    /// </summary>
    /// <param name="name"></param>
    /// <param name="color"></param>
    public delegate void OnPlayerReadyDelegate(string name, Color color);

    /// <summary>
    /// Event associated to delegate
    /// </summary>
    public event OnPlayerReadyDelegate OnPlayerReady;

    /// <summary>
    /// Called when user plays ready button
    /// </summary>
    private void PlayerReady()
    {
        if (OnPlayerReady != null)
        {
            Debug.LogFormat("PlayerReady button pressed");
            OnPlayerReady(_playerName.text, _colorPicker.SelectedColor);
        }
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
}