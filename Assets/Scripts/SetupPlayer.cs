using System;
using Mirror;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using Random = System.Random;

/*
	Documentation: https://mirror-networking.com/docs/Guides/NetworkBehaviour.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class SetupPlayer : NetworkBehaviour
{
    [SyncVar] private int m_ID;
    [SyncVar(hook = nameof(ChangePlayerName))] private string m_Name;
    [SyncVar(hook = nameof(ChangePlayerColor))] private Color m_Color;
    [SyncVar(hook = nameof(ChangePlayerCurrentLap))] private int m_CurrentLap;
    [SyncVar(hook = nameof(ChangePlayerPosition))] public int m_Position;
    
    private UIManager m_UIManager;
    private NetworkManager m_NetworkManager;
    private PlayerController m_PlayerController;
    private PlayerInfo m_PlayerInfo;
    private PolePositionManager m_PolePositionManager;
    private Material m_BodyMaterial;
    
    private PlayerInputController _playerInputController;

    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();
        m_ID = connectionToClient.connectionId;
        m_Name = "Player" + m_ID;
        m_Color = ColorPicker.Colors[new Random().Next(0, ColorPicker.Colors.Count)];
        m_CurrentLap = 0;
    }

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();
        m_PlayerInfo.ID = m_ID;
        m_PlayerInfo.Name = m_Name;
        m_PlayerInfo.Color = m_Color;
        m_PlayerInfo.CurrentPosition = m_Position;
        m_PlayerInfo.CurrentLap = m_CurrentLap;
        m_PolePositionManager.AddPlayer(m_PlayerInfo);
    }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer()
    {
    }

    public override void OnNetworkDestroy()
    {
        base.OnNetworkDestroy();
        m_PolePositionManager.RemovePlayer(m_PlayerInfo);
    }

    #endregion

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
            m_PlayerController.OnSpeedChangeEvent += OnSpeedChangeEventHandler;
            m_UIManager.OnPlayerReady += OnPlayerStartEventHandler;
            ConfigureCamera();
        }
    }

    void OnSpeedChangeEventHandler(float speed)
    {
        m_UIManager.UpdateSpeed((int) speed * 5); // 5 for visualization purpose (km/h)
    }

    /// <summary>
    /// Called from UI when player ready button pressed
    /// </summary>
    /// <param name="playerName"></param>
    /// <param name="color"></param>
    void OnPlayerStartEventHandler(string playerName, Color color)
    {
        CmdSetPlayerConfig(playerName, color);
        Debug.Log(string.Format("[{0}]: OnPlayerStartEventHandler called [name={1}, color={2}]", 
            m_ID, m_Name, m_Color));
    }
    
    void ConfigureCamera()
    {
        if (Camera.main != null) Camera.main.gameObject.GetComponent<CameraController>().m_Focus = this.gameObject;
    }

    /// <summary>
    /// Command to update color and name in server
    /// </summary>
    /// <param name="playerName"></param>
    /// <param name="color"></param>
    [Command]
    void CmdSetPlayerConfig(string playerName, Color color)
    {
        m_Name = playerName;
        m_Color = color;
    }

    [Command]
    public void CmdSetPlayerPosition(int position)
    {
        m_Position = position;
    }

    /// <summary>
    /// Hook called when server updates color
    /// </summary>
    /// <param name="oldColor"></param>
    /// <param name="color"></param>
    public void ChangePlayerColor(Color oldColor, Color color)
    {
        m_PlayerInfo.Color = color;
        m_BodyMaterial.color = color;
        m_UIManager.SetConfigUIColor(color);
    }

    /// <summary>
    /// Hook called when server updates name
    /// </summary>
    /// <param name="oldName"></param>
    /// <param name="newName"></param>
    public void ChangePlayerName(string oldName, string newName)
    {
        m_PlayerInfo.Name = newName;
    }
    
    /// <summary>
    /// Hook called when server updates current lap
    /// </summary>
    /// <param name="old"></param>
    /// <param name="newLap"></param>
    public void ChangePlayerCurrentLap(int old, int newLap)
    {
        m_PlayerInfo.CurrentLap = newLap;
    }

    /// <summary>
    /// Hook called when server updates position
    /// </summary>
    /// <param name="old"></param>
    /// <param name="newPosition"></param>
    public void ChangePlayerPosition(int old, int newPosition)
    {
        m_PlayerInfo.CurrentPosition = newPosition;
    }
}