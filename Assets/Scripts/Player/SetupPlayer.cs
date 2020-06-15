﻿using Mirror;
using System.Threading;
using PolePosition.UI;
using UnityEngine;

/*
	Documentation: https://mirror-networking.com/docs/Guides/NetworkBehaviour.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

namespace PolePosition.Player
{
    public class SetupPlayer : NetworkBehaviour
    {
        private UIManager m_UIManager;
        private PlayerController m_PlayerController;
        private PlayerInfo m_PlayerInfo;
        private Material m_BodyMaterial;

        private PlayerInputController _playerInputController;

        private void Awake()
        {
            m_PlayerInfo = GetComponent<PlayerInfo>();
            m_PlayerController = GetComponent<PlayerController>();
            m_UIManager = FindObjectOfType<UIManager>();

            // Controller for player input
            _playerInputController = GetComponent<PlayerInputController>();

            // Gets the body material to update color
            Transform carBody = transform.Find("raceCar").Find("body");
            m_BodyMaterial = carBody.GetComponent<Renderer>().materials[1];
        }

        // Start is called before the first frame update
        void Start()
        {
            // Security measure, just in case for some reason 
            // player input controller is enabled on start
            _playerInputController.enabled = false;

            if (isLocalPlayer)
            {
                ConfigureCamera();
            }

            if (isServer)
            {
                m_PlayerController.enabled = true;
                m_PlayerController.enableRigidBody();
            }
            else
            {
                // No need for controller nor physics
                // in clients, server is fully authoritative
                m_PlayerController.enabled = false;
                m_PlayerController.disableRigidBody();
            }
        }

        /// <summary>
        /// Setups camera
        /// Client only
        /// </summary>
        [Client]
        void ConfigureCamera()
        {
            if (Camera.main != null)
            {
                Camera.main.gameObject.GetComponent<CameraController>().m_Focus = this.gameObject;
            }
        }

        /// <summary>
        /// Updates players positions on UI
        /// Client only
        /// </summary>
        [Client]
        public void UpdatePosition()
        {
            m_UIManager.UpdatePlayersPositions(m_PlayerInfo);
        }

        /// <summary>
        /// Sets current speed on UI
        /// Client only
        /// </summary>
        /// <param name="speed">Current car speed</param>
        [Client]
        public void SetSpeed(float speed)
        {
            if (isLocalPlayer)
            {
                m_UIManager.UpdateSpeed((int) speed * 5);
            }
        }

        /// <summary>
        /// Sets current player name
        /// Client only
        /// </summary>
        /// <param name="name">Player name</param>
        [Client]
        public void SetPlayerName(string name)
        {
            if(isLocalPlayer)
            {
                m_UIManager.SetConfigUIName(name);
            }
        }

        /// <summary>
        /// Sets car color
        /// Client only
        /// </summary>
        /// <param name="color">Car color</param>
        [Client]
        public void SetPlayerColor(Color color)
        {
            if(isLocalPlayer)
            {
                m_UIManager.SetConfigUIColor(color);
            }
            
            m_BodyMaterial.color = color;
        }

        /// <summary>
        /// Sets current lap on UI
        /// Client only
        /// </summary>
        /// <param name="currentLap">Current lap</param>
        [Client]
        public void SetCurrentLap(int currentLap)
        {
            if(isLocalPlayer)
            {
                m_UIManager.SetCurrentLap(currentLap);
            }
        }

        /// <summary>
        /// Sets current lap time on UI
        /// Client only
        /// </summary>
        /// <param name="currentLapTime">Current lap time</param>
        [Client] 
        public void SetCurrentLapTime(float currentLapTime)
        {
            if(isLocalPlayer)
            {
                m_UIManager.SetCurrentLapTime(currentLapTime);
            }
        }

        /// <summary>
        /// Sets number of laps on UI
        /// Client only
        /// </summary>
        /// <param name="numberOfLaps">Number of laps</param>
        [Client]
        public void SetNumberOfLaps(int numberOfLaps)
        {
            if(isLocalPlayer)
            {
                m_UIManager.SetNumberOfLaps(numberOfLaps);
            }
        }
        
        /// <summary>
        /// Enables player control of this car
        /// Client only
        /// </summary>
        [Client]
        public void StartPlayerController()
        {    
            if (isLocalPlayer)
            {
                _playerInputController.enabled = true;
            }
        }

        /// <summary>
        /// Disables player control of this car
        /// Client only
        /// </summary>
        [Client]
        public void StopPlayerController()
        {
            if (isLocalPlayer)
            {
                _playerInputController.enabled = false;
            }
        }
        
        /// <summary>
        /// Starts counting lap and total time
        /// Server only
        /// </summary>
        [Server]
        public void StartLapTime()
        {
            m_PlayerController.StartLapTime();            
        }

        /// <summary>
        /// Ends counting lap and total time
        /// Server only
        /// </summary>
        [Server]
        public void StopLapTime()
        {
            m_PlayerController.StopLapTime();
        }
        
        /// <summary>
        /// Stops the car
        /// Server only
        /// </summary>
        [Server]
        public void StopCar()
        {
            m_PlayerController.EngineStarted = false;
        }

        /// <summary>
        /// Change the text in the UI message
        /// </summary>
        public void SetUIText(string message, int fontSize = 30, Color? color = null)
        {
            m_UIManager.UpdateUIMessage(message, fontSize, color);
        }
    }
}