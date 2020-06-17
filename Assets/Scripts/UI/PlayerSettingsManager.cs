using System;
using UnityEngine;
using UnityEngine.UI;

namespace PolePosition.UI
{
    /// <summary>
    /// Player Settings prefab manager
    /// </summary>
    public class PlayerSettingsManager : MonoBehaviour
    {
        /// <summary>
        /// UI relevant fields
        /// </summary>
        private InputField _playerNameText;
        private ColorPicker _colorPicker;
        private Button _acceptButton;

        /// <summary>
        /// Selected player name
        /// </summary>
        public string PlayerName
        {
            get => _playerNameText.text;
            set => _playerNameText.text = value;
        }

        /// <summary>
        /// Selected color
        /// </summary>
        public Color CarColor
        {
            get => _colorPicker.SelectedColor;
            set => _colorPicker.SelectedColor = value;
        }

        /// <summary>
        /// Delegate called when accept button is pressed
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="color"></param>
        public delegate void AcceptButtonClicked(string playerName, Color color);
        public AcceptButtonClicked OnAcceptButtonClicked;
        
        private void Awake()
        {
            _playerNameText = transform.Find("PanelSettings/Panel/PlayerName").GetComponent<InputField>();
            _colorPicker = transform.Find("PanelSettings/Panel/ColorPicker").GetComponent<ColorPicker>();
            _acceptButton = transform.Find("ButtonAccept").GetComponent<Button>();
            
            _acceptButton.onClick.AddListener(() =>
            {
                Debug.LogFormat("PlayerSettings {0}, {1}", PlayerName, CarColor);
                if(OnAcceptButtonClicked != null) OnAcceptButtonClicked(PlayerName, CarColor);
                gameObject.SetActive(false);
            });
        }
    }
}