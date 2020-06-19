using PolePosition.Manager;
using PolePosition.Player;
using UnityEngine;
using UnityEngine.UI;

namespace PolePosition.UI
{
    /// <summary>
    /// Lobby UI manager
    /// </summary>
    public class LobbyManager : MonoBehaviour
    {
        /// <summary>
        /// Current host that holds buttons
        /// </summary>
        private int _currentHostId=-1;

        /// <summary>
        /// 
        /// </summary>
        private int _localHostId = -1;
        
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

        /// <summary>
        /// Num drivers in the race
        /// </summary>
        private NumberInputController _numPlayersToDrive;

        private NumberInputController _numLaps;

        private Toggle _qualificationLapController;

        /// <summary>
        /// Delegate called when numDrivers is updated
        /// </summary> 
        public delegate void OnUpdateNumDriversDelegate(int numDrivers);
        public OnUpdateNumDriversDelegate OnUpdateNumDrivers;

        /// <summary>
        /// Delegate called when numLaps is updated
        /// </summary> 
        public delegate void OnUpdateNumLapsDelegate(int numLaps);
        public OnUpdateNumDriversDelegate OnUpdateNumLaps;

        /// <summary>
        /// Delegate called when qualificationRoundCheckBox is updated
        /// </summary> 
        public delegate void OnUpdateQualificationLapDelegate(bool value);
        public OnUpdateQualificationLapDelegate OnUpdateQualificationLap;

        private void Awake()
        {
            _playersPanel = transform.Find("PlayersPanel");
            _playerSettings = transform.Find("PlayerSettings").GetComponent<PlayerSettingsManager>();
            _numPlayersToDrive = transform.Find("NumPlayersToDrive").GetComponent<NumberInputController>();
            _numLaps = transform.Find("NumLaps").GetComponent<NumberInputController>();
            _qualificationLapController = transform.Find("QualificationRound").GetComponent<Toggle>();
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
                    _numPlayersToDrive.OnUpdateNumberInput = (int numDrivers) =>
                    {
                        OnUpdateNumDrivers?.Invoke(numDrivers);
                    };

                    _numPlayersToDrive.OnUpdateNumberValidate = (int numDrivers) =>
                    {
                        int currentPlayers = _playersPanel.GetComponentsInChildren<ConnectionInfoController>().Length;
                        return numDrivers >= currentPlayers && numDrivers <= 4;
                    };

                    _numLaps.OnUpdateNumberInput = (int numLaps) =>
                    {
                        OnUpdateNumLaps?.Invoke(numLaps);
                    };

                    _numLaps.OnUpdateNumberValidate = (int numLaps) =>
                    {
                        return numLaps >= 1 && numLaps <= 9;
                    };

                    _qualificationLapController.onValueChanged.AddListener((bool value) =>
                    {
                        OnUpdateQualificationLap?.Invoke(value);
                    });


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
            int id = playerInfo.ID;
            foreach (var connectionInfoController in _playersPanel.GetComponentsInChildren<ConnectionInfoController>())
            {
                if (connectionInfoController.CarID == playerInfo.ID)
                {
                    Destroy(connectionInfoController.gameObject);
                }
            }
        }
        
        public void UpdateNumDrivers(int drivers)
        {
            _numPlayersToDrive.Value = drivers;
        }

        public void UpdateNumLaps(int laps)
        {
            _numLaps.Value = laps;
        }

        public void UpdateQualificationLap(bool value)
        {
            _qualificationLapController.isOn = value;
        }

        public void ActivateButtons(bool activate)
        {
            _numPlayersToDrive.EnableNumberInputButtons(activate);
            _numLaps.EnableNumberInputButtons(activate);
            _qualificationLapController.interactable = activate;

        }
    }
}