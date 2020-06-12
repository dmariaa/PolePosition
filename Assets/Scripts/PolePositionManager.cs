using System;
using System.Collections.Generic;
using System.Threading;
using Mirror;
using PolePosition.Player;
using PolePosition.UI;
using UnityEngine;

namespace PolePosition
{
    public class PolePositionManager : NetworkBehaviour
    {
        private enum RaceStates
        {
            WAITING_FOR_PLAYERS,
            COUNT_DOWN,
            IN_RACE
        }

        public int MaxNumPlayers = 4;
        public int NumberOfLaps = 4;
        public UIManager uiManager;
        public CircuitController m_CircuitController;

        private readonly Dictionary<int, PlayerInfo> m_Players = new Dictionary<int, PlayerInfo>();
        private GameObject[] m_DebuggingSpheres;

        [SyncVar(hook = nameof(SetRaceSemaphore))] private int RaceSemaphore;
        private float timer;

        private RaceStates _raceState = RaceStates.WAITING_FOR_PLAYERS;

        private void SetRaceSemaphore(int oldRaceSemaphore, int newRaceSemaphore)
        {
            uiManager.UpdateCountdown(newRaceSemaphore);
        }

        private void Awake()
        {
            RaceSemaphore = 4;
            if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
            if (m_CircuitController == null) m_CircuitController = FindObjectOfType<CircuitController>();

            m_DebuggingSpheres = new GameObject[MaxNumPlayers];
            for (int i = 0; i < MaxNumPlayers; ++i)
            {
                m_DebuggingSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                m_DebuggingSpheres[i].GetComponent<SphereCollider>().enabled = false;
            }
        }

        [ServerCallback]
        private void Update()
        {
            if (m_Players.Count == 0)
                return;

            switch (_raceState)
            {
                case RaceStates.WAITING_FOR_PLAYERS:
                    if (m_Players.Count == MaxNumPlayers)
                    {
                        timer = 0;
                        _raceState = RaceStates.COUNT_DOWN;
                    }
                    break;

                case RaceStates.COUNT_DOWN:
                    timer += Time.deltaTime;
                    if (RaceSemaphore >= 0 && timer >= 1.0f)
                    {
                        RaceSemaphore--;
                        timer = 0.0f;
                    }
                    else if (RaceSemaphore <= 0)
                    {
                        StartRace();
                        _raceState = RaceStates.IN_RACE;
                    }
                    break;

                case RaceStates.IN_RACE:
                    timer += Time.deltaTime;
                    if (timer >= 1.0f && RaceSemaphore > -1)
                    {
                        RaceSemaphore--;
                    }
                    UpdateRaceProgress();
                    break;
            }   
        }

        private void StartRace()
        {
            foreach(var player in m_Players)
            {
                player.Value.RpcLaunchPlayer();
            }
        }


        public void AddPlayer(PlayerInfo player)
        {
            m_Players.Add(player.ID, player);
            // uiManager.UpdatePlayersPositions(player);
        }

        public void RemovePlayer(PlayerInfo player)
        {
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
                if (playerInfo.ArcInfo == 0)
                {
                    playerInfo.CurrentLap += 1;
                }
                arcLengths[i++] = new KeyValuePair<int, float>(playerInfo.ID, playerInfo.ArcInfo);
            }

            Array.Sort(arcLengths, (one, other) => one.Value < other.Value ? 1 : -1);
            i = 1;
            foreach (var arcLength in arcLengths)
            {
                PlayerInfo playerInfo = m_Players[arcLength.Key];
                playerInfo.CurrentPosition = i++;
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
            Vector3 carDirection;

            float minArcL =
                this.m_CircuitController.ComputeClosestPointArcLength(carPos, out carDirection, out segIdx, out carProj, out carDist);

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

            player.PosCentral = carProj;
            player.PuntoLookAt = carDirection;
            player.ArcInfo = minArcL;
            player.Direction = -Vector3.Cross(carProj, player.Speed).y;
            return minArcL;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            uiManager.UpdateCountdown(RaceSemaphore);
        }
    }
}