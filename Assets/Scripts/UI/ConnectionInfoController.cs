using System;
using Mirror;
using PolePosition.Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PolePosition.UI
{
    /// <summary>
    /// ConnectionInfo prefab controller.
    /// Used in Lobby UI
    /// </summary>
    public class ConnectionInfoController : MonoBehaviour
    {
        /// <summary>
        /// Relevant UI elements
        /// </summary>
        private Image _carColorImage;
        private Text _playerNameText;
        private Text _playerReadyButtonText;
        private Button _playerReadyButton;
        
        /// <summary>
        /// Delegate called when player name is clicked
        /// </summary>
        public delegate void PlayerNameClicked();
        public PlayerNameClicked OnPlayerNameClicked;
        
        /// <summary>
        /// Delegate called when ready button clicked
        /// </summary>
        public delegate void ReadyButtonClicked();
        public ReadyButtonClicked OnReadyButtonClicked;

        public int CarID
        {
            get;
            set;
        }

        /// <summary>
        /// Car color
        /// </summary>
        public Color CarColor
        {
            get { return _carColorImage.color; }
            set { _carColorImage.color = value; }
        }

        /// <summary>
        /// Player name
        /// </summary>
        public string PlayerName
        {
            get { return _playerNameText.text;  }
            set { _playerNameText.text = value;  }
        }

        /// <summary>
        /// Is the player ready?
        /// </summary>
        private bool _isReady;
        public bool IsReady
        {
            get { return _isReady;  }
            set
            {
                _isReady = value;
                _playerReadyButtonText.text = _isReady ? "Ready" : "Not ready";
                _playerReadyButtonText.color = _isReady ? Color.green : Color.red;
            }
        }

        private void Awake()
        {
            _carColorImage = transform.Find("CarColorImage").GetComponent<Image>();

            Transform playerButton = transform.Find("PlayerReady"); 
            _playerReadyButtonText = playerButton.GetComponentInChildren<Text>();
            _playerReadyButton = playerButton.GetComponent<Button>();
            _playerReadyButton.onClick.AddListener(() =>
            {
                IsReady = !IsReady;
                if(OnReadyButtonClicked != null) OnReadyButtonClicked();
            });
            
            Transform playerName = transform.Find("PlayerName");
            _playerNameText = playerName.GetComponent<Text>();
            playerName.GetComponent<Button>().onClick.AddListener(() =>
            {
                if(OnPlayerNameClicked != null) OnPlayerNameClicked();
            });
        }

        /// <summary>
        /// Enables or disables ready button
        /// </summary>
        /// <param name="enable">enables if true, disables if false</param>
        public void EnableReadyButton(bool enable = true)
        {
            _playerReadyButton.interactable = enable;
        }
    }
}