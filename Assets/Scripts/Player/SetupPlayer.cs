using Mirror;
using System.Threading;
using UnityEngine;
using Random = System.Random;

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
            _playerInputController.enabled = false;

            if (isLocalPlayer)
            {
                ConfigureCamera();
            }
        }

        void OnSpeedChangeEventHandler(float speed)
        {
            SetSpeed(speed);
        }

        void ConfigureCamera()
        {
            if (Camera.main != null) Camera.main.gameObject.GetComponent<CameraController>().m_Focus = this.gameObject;
        }

        public void SetPosition(int position)
        {
            m_UIManager.UpdatePlayersPositions(m_PlayerInfo);
        }

        public void StartPlayerController()
        {    
            if (isLocalPlayer)
            {
                _playerInputController.enabled = true;
            }
        }

        public void SetSpeed(float speed)
        {
            m_UIManager.UpdateSpeed((int)speed * 5); // 5 for visualization purpose (km/h)
        }

        public void SetPlayerName(string name)
        {
            if (isLocalPlayer)
            {
                m_UIManager.SetConfigUIName(name);
            }
        }

        public void SetPlayerColor(Color color)
        {
            m_BodyMaterial.color = color;
            if (isLocalPlayer)
            {
                m_UIManager.SetConfigUIColor(color);
            }
        }
    }
}