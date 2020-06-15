using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using PolePosition.Player;
using PolePosition.UI;
using UnityEngine;
using NetworkBehaviour = Mirror.NetworkBehaviour;

namespace PolePosition
{
    public class PolePositionManager : NetworkBehaviour
    {
        private enum RaceStates
        {
            WAITING_FOR_PLAYERS,
            COUNT_DOWN,
            IN_RACE,
            RACE_FINISHED
        }
        
        private RaceStates _raceState = RaceStates.WAITING_FOR_PLAYERS;

        public int MaxNumPlayers = 4;
        public int NumberOfLaps = 4;
        public UIManager uiManager;
        public CircuitController m_CircuitController;

        private readonly Dictionary<int, PlayerInfo> m_Players = new Dictionary<int, PlayerInfo>();
        private GameObject[] m_DebuggingSpheres;

        private float timer;

        private int RaceSemaphore;

        private void Awake()
        {
            if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
            if (m_CircuitController == null) m_CircuitController = FindObjectOfType<CircuitController>();

            m_DebuggingSpheres = new GameObject[MaxNumPlayers];
            for (int i = 0; i < MaxNumPlayers; ++i)
            {
                m_DebuggingSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                m_DebuggingSpheres[i].GetComponent<SphereCollider>().enabled = false;
            }

            RaceSemaphore = 4;
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
                        RpcUpdateCountdown(RaceSemaphore);
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
                        RpcUpdateCountdown(RaceSemaphore);
                    }
                    UpdateRaceProgress();
                    break;
                case RaceStates.RACE_FINISHED:
                    break;
            }   
        }

        /// <summary>
        /// Starts the race
        /// Server side only
        /// </summary>
        [Server]
        private void StartRace()
        {
            foreach(var player in m_Players)
            {
                player.Value.RpcLaunchPlayer();
            }
        }
        
        /// <summary>
        /// Finishes the race, shows results
        /// Server only
        /// </summary>
        [Server]
        void FinishRace()
        {
            PlayerInfo[] playerInfos = m_Players.Values.ToArray();
            Array.Sort(playerInfos, (one, two) => one.CurrentPosition < two.CurrentPosition ? 1 : -1);
            foreach (var playerInfo in playerInfos)
            {
                RpcAddPlayerResult(playerInfo.CurrentPosition, playerInfo.Color, playerInfo.Name, 
                    playerInfo.TotalRaceTime, playerInfo.BestLapTime);   
            }

            RpcShowResults();
            _raceState = RaceStates.RACE_FINISHED;
        }

        /// <summary>
        /// Adds a player to it's list of players
        /// Server side only
        /// </summary>
        /// <param name="player"></param>
        [Server]
        public void AddPlayer(PlayerInfo player)
        {
            m_Players.Add(player.ID, player);
        }

        /// <summary>
        /// Removes a player from it's list of players
        /// Server side only
        /// </summary>
        /// <param name="player"></param>
        [Server]
        public void RemovePlayer(PlayerInfo player)
        {
            m_Players.Remove(player.ID);
        }

        /// <summary>
        /// Updates car race progress
        /// - Calculates players position in circuit
        /// - Calculates players relative position
        /// - Notifies players finish line crossed
        /// - Finishes the race when all players have finished
        /// Server side only
        /// TODO: Review, generates error when branded as [Server] or [ServerCallback]
        /// TODO: It has something to do with the call to the circuit what is a
        /// TODO: MonoBehabior not a NetworkBehavior
        /// </summary>
        public void UpdateRaceProgress()
        {
            // Update car arc-lengths
            float circuitLength = m_CircuitController.CircuitLength;
            KeyValuePair<int, float>[] arcLengths = new KeyValuePair<int, float>[m_Players.Count];
            int i = 0;
            int finishedPlayers = 0;
            
            foreach (var player in m_Players)
            {
                PlayerInfo playerInfo = player.Value;
                ComputeCarArcLength(ref playerInfo);
                int j = 0;
                
                // Distance covered in circuit
                float coveredCircuitDistance = playerInfo.CurrentLapCorrected==0 ?
                    playerInfo.ArcInfo - circuitLength:
                    circuitLength * (playerInfo.CurrentLapCorrected - 1) + playerInfo.ArcInfo;
                
                // Debug.LogFormat("ArcInfo: {0}, Covered length: {1}", playerInfo.ArcInfo, coveredCircuitDistance);
                arcLengths[i++] = new KeyValuePair<int, float>(playerInfo.ID, coveredCircuitDistance);

                if (playerInfo.AllLapsFinished)
                {
                    finishedPlayers++;
                }
            }

            // Race has finished
            if (finishedPlayers == m_Players.Count)
            {
                FinishRace();
                return;
            }

            Array.Sort(arcLengths, (one, other) => one.Value < other.Value ? 1 : -1);
            i = 1;
            foreach (var arcLength in arcLengths)
            {
                PlayerInfo playerInfo = m_Players[arcLength.Key];
                playerInfo.CurrentPosition = i++;
            }
        }

        /// <summary>
        /// Computes car position in circuit
        /// Server only 
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
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
            
            // Has the player crossed finish line?
            if(player.CurrentSegmentIdx==m_CircuitController.CircuitNumberOfSegments - 1 && segIdx==0)
            {
                player.CrossedFinishLineForward();
            } else if (player.CurrentSegmentIdx == 0 && segIdx == m_CircuitController.CircuitNumberOfSegments - 1)
            {
                player.CrossedFinishLineBackwards();
            }
            
            // Update player info
            player.CurrentCircuitPosition = carProj;
            player.LookAtPoint = carDirection;
            player.ArcInfo = minArcL;
            player.Direction = Vector3.Angle(player.LookAtPoint - carProj, player.Speed);
            player.CurrentSegmentIdx = segIdx;

            return minArcL;
        }
        
        /// <summary>
        /// Called when this object spawns on client
        /// </summary>
        public override void OnStartClient()
        {
            base.OnStartClient();
            uiManager.UpdateUIMessage("Waiting for players", 62);
        }

        /// <summary>
        /// Updates client ui countdown
        /// Client side, called from server
        /// </summary>
        /// <param name="value">countdown value</param>
        [ClientRpc]
        void RpcUpdateCountdown(int value)
        {
            int maxCountdown = 4;
            if (value > 0 && value < maxCountdown)
            {
                uiManager.UpdateUIMessage("" + value, 300);
            }else if (value == 0)
            {
                uiManager.UpdateUIMessage("GO!", 300);
            }
            else
            {
                uiManager.UpdateUIMessage("");
            }
        }

        /// <summary>
        /// Adds a player result to UI results table
        /// </summary>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <param name="playerName"></param>
        /// <param name="raceTime"></param>
        /// <param name="bestLapTime"></param>
        [ClientRpc]
        void RpcAddPlayerResult(int position, Color color, string playerName, float raceTime, float bestLapTime)
        {
            //Debug.LogFormat("Adding player info to results: {0} - {1} - {2} - {3} - {4}",
            //    position,color,playerName,raceTime,bestLapTime);
            uiManager.AddPlayerResult(position, color, playerName, raceTime, bestLapTime);
        }

        /// <summary>
        /// Shows results table
        /// </summary>
        [ClientRpc]
        void RpcShowResults()
        {
            uiManager.ActivatePlayerResults();
        }
    }
}