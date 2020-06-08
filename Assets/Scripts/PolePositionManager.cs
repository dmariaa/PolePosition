using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PolePositionManager : NetworkBehaviour
{
    public int numPlayers;
    public NetworkManager networkManager;
    public UIManager uiManager;
    
    private Dictionary<int, PlayerInfo> m_Players = new Dictionary<int, PlayerInfo>();
    private CircuitController m_CircuitController;
    private GameObject[] m_DebuggingSpheres;
    
    private void Awake()
    {
        if (networkManager == null) networkManager = FindObjectOfType<NetworkManager>();
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (m_CircuitController == null) m_CircuitController = FindObjectOfType<CircuitController>();

        m_DebuggingSpheres = new GameObject[networkManager.maxConnections];
        for (int i = 0; i < networkManager.maxConnections; ++i)
        {
            m_DebuggingSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            m_DebuggingSpheres[i].GetComponent<SphereCollider>().enabled = false;
        }
    }

    private void Update()
    {
        if (m_Players.Count == 0)
            return;

        UpdateRaceProgress();
    }

    public void AddPlayer(PlayerInfo player)
    {
        m_Players.Add(player.ID, player);
    }

    public void RemovePlayer(PlayerInfo player)
    {
        uiManager.ClearPlayerPosition(player);
        m_Players.Remove(player.ID);
    }
    
    public void UpdateRaceProgress()
    {
        // Update car arc-lengths
        KeyValuePair<int, float>[] arcLengths = new KeyValuePair<int, float>[m_Players.Count];
        int i = 0;
        foreach (var player in m_Players)
        {
            PlayerInfo playerInfo = player.Value;
            ComputeCarArcLength(ref playerInfo);
            arcLengths[i++] = new KeyValuePair<int, float>(playerInfo.ID, playerInfo.ArcInfo);
        }
        
        Array.Sort(arcLengths, (one, other) => one.Value < other.Value ? 1 : -1);
        i = 1;
        foreach (var arcLength in arcLengths)
        {
            PlayerInfo playerInfo = m_Players[arcLength.Key];
            playerInfo.CurrentPosition = i++;
            uiManager.UpdatePlayersPositions(playerInfo);
        }
    }

    float ComputeCarArcLength(ref PlayerInfo player)
    {
        // Compute the projection of the car position to the closest circuit 
        // path segment and accumulate the arc-length along of the car along
        // the circuit.
        Vector3 carPos = player.transform.position;
        int ID = player.ID;

        int segIdx;
        float carDist;
        Vector3 carProj;

        float minArcL =
            this.m_CircuitController.ComputeClosestPointArcLength(carPos, out segIdx, out carProj, out carDist);

        this.m_DebuggingSpheres[ID].transform.position = carProj;

        if (this.m_Players[ID].CurrentLap == 0)
        {
            minArcL -= m_CircuitController.CircuitLength;
        }
        else
        {
            minArcL += m_CircuitController.CircuitLength *
                       (m_Players[ID].CurrentLap - 1);
        }

        player.ArcInfo = minArcL;
        return minArcL;
    }
}