using Mirror;
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
        private NetworkManager m_NetworkManager;
        private PlayerController m_PlayerController;
        private PlayerInfo m_PlayerInfo;
        private PolePositionManager m_PolePositionManager;
        private Material m_BodyMaterial;

        private PlayerInputController _playerInputController;
        
        private void Awake()
        {
            m_PlayerInfo = GetComponent<PlayerInfo>();
            m_PlayerController = GetComponent<PlayerController>();
            m_NetworkManager = FindObjectOfType<NetworkManager>();
            m_PolePositionManager = FindObjectOfType<PolePositionManager>();
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
            if (isLocalPlayer)
            {
                _playerInputController.enabled = true;
                ConfigureCamera();
            }
            else
            {
                _playerInputController.enabled = false;
            }
        }

        void ConfigureCamera()
        {
            if (Camera.main != null)
            {
                Camera.main.gameObject.GetComponent<CameraController>().m_Focus = this.gameObject;
            }
        }

        public void UpdatePosition()
        {
            m_UIManager.UpdatePlayersPositions(m_PlayerInfo);
        }

        public void SetSpeed(float speed)
        {
            if (isLocalPlayer)
            {
                m_UIManager.UpdateSpeed((int) speed * 5);
            }
        }

        public void SetPlayerName(string name)
        {
            if(isLocalPlayer)
            {
                m_UIManager.SetConfigUIName(name);
            }
        }

        public void SetPlayerColor(Color color)
        {
            if(isLocalPlayer)
            {
                m_UIManager.SetConfigUIColor(color);
            }
            
            m_BodyMaterial.color = color;
        }

        public void SetCurrentLap(int currentLap)
        {
            if(isLocalPlayer)
            {
                m_UIManager.SetCurrentLap(currentLap);
            }
        }

        public void SetCurrentLapTime(float currentLapTime)
        {
            if(isLocalPlayer)
            {
                m_UIManager.SetCurrentLapTime(currentLapTime);
            }
        }

        public void SetNumberOfLaps(int numberOfLaps)
        {
            if(isLocalPlayer)
            {
                m_UIManager.SetNumberOfLaps(numberOfLaps);
            }
        }
    }
}