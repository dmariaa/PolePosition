using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using PolePosition.Player;
using PolePosition.StateMachine;
using PolePosition.UI;
using UnityEngine;
using NetworkBehaviour = Mirror.NetworkBehaviour;

namespace PolePosition.Manager
{
    public class PolePositionManager : NetworkBehaviour
    {
        [SyncVar (hook = nameof (OnChangeMaxNumPlayers))]public int MaxNumPlayers;
        private int _currentLobbyHostId = -1;
        [SyncVar(hook = nameof(OnChangeNumLaps))] public int NumberOfLaps = 4;
        [SyncVar(hook = nameof(OnChangeQualificationLap))] public bool QualificationLap = true;
        public UIManager uiManager;
        public CircuitController m_CircuitController;

        private Guid _globalGuid;
        public Guid GlobalGUID
        {
            get => _globalGuid;
        }

        /// <summary>
        /// Dictionary of ID -> playerInfos
        /// Used in server only
        /// </summary>
        private readonly Dictionary<int, PlayerInfo> _Players = new Dictionary<int, PlayerInfo>();
        public Dictionary<int, PlayerInfo> Players
        {
            get => _Players;
        }
        
        /// <summary>
        /// State machine to manage states and transitions
        /// Used in server only
        /// </summary>
        private StateMachine.StateMachine _stateMachine;
        
        /// <summary>
        /// Debugging spheres
        /// </summary>
        private readonly Dictionary<int, GameObject> _debuggingSpheres = new Dictionary<int, GameObject>();

        #region Callbacks
        private void Awake()
        {
            if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
            if (m_CircuitController == null) m_CircuitController = FindObjectOfType<CircuitController>();
            _globalGuid = Guid.NewGuid();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            uiManager.Lobby.OnUpdateNumDrivers = (int numDrivers) =>
                {
                    CmdUpdateNumPlayers(numDrivers);
                };
            uiManager.Lobby.OnUpdateNumLaps = (int numLaps) =>
            {
                CmdUpdateNumLaps(numLaps);
            };
            uiManager.Lobby.OnUpdateQualificationLap = (bool value) =>
            {
                CmdUpdateQualificationLap(value);
            };

        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            
            if(isServer)
            {
                // Initialize state machine
                _stateMachine = new StateMachine.StateMachine();
                _stateMachine.ChangeState(new StateInLobby(this));
            }
        }
        
        [ServerCallback]
        private void Update()
        {
            _stateMachine.Update();
        }
        #endregion
        
        #region State management
        /// <summary>
        /// Changes state in state machine
        /// Server side only
        /// </summary>
        /// <param name="newState">new State</param>
        [Server]
        public void StateChange(IState newState)
        {
            _stateMachine.ChangeState(newState);
        }
        #endregion

        #region Server side methods
        /// <summary>
        /// Starts the race
        /// Server side only
        /// </summary>
        [Server]
        public void StartRace()
        {
            foreach(var player in _Players)
            {
                player.Value.RpcLaunchPlayer();
            }
        }
        
        /// <summary>
        /// Finishes the race, shows results
        /// Server only
        /// </summary>
        [Server]
        public void FinishRace()
        {
            RpcClearResults();
            
            PlayerInfo[] playerInfos = _Players.Values.ToArray();
            Array.Sort(playerInfos, (one, two) => one.CurrentPosition > two.CurrentPosition ? 1 : -1);
            foreach (var playerInfo in playerInfos)
            {
                RpcAddPlayerResult(playerInfo.CurrentPosition, playerInfo.Color, playerInfo.Name, 
                    playerInfo.TotalRaceTime, playerInfo.BestLapTime);   
            }

            RpcShowResults();
        }

        /// <summary>
        /// Adds a player to it's list of players
        /// Server side only
        /// </summary>
        /// <param name="player"></param>
        [Server]
        public void AddPlayer(PlayerInfo player)
        {
            player.GetComponent<PlayerMatchChecker>().matchId = _globalGuid;
            if (_currentLobbyHostId == -1)
            {
                _currentLobbyHostId = player.ID;
                player.RpcActivateHostLobbyButtons(true, _currentLobbyHostId);
            }
            else
            {
                player.RpcActivateHostLobbyButtons(false, player.ID);
            }
            _Players.Add(player.ID, player);
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale = Vector3.one * 0.5f;
            sphere.GetComponent<SphereCollider>().enabled = false;
            _debuggingSpheres.Add(player.ID, sphere);
        }

        /// <summary>
        /// Removes a player from it's list of players
        /// Server side only
        /// </summary>
        /// <param name="player"></param>
        [Server]
        public void RemovePlayer(PlayerInfo player)
        {
            int id = player.ID;
            _Players.Remove(player.ID);
            if (id == _currentLobbyHostId && _Players.Count > 0)
            {
                _currentLobbyHostId= _Players.First().Value.ID;
                _Players.First().Value.RpcActivateHostLobbyButtons(true, _currentLobbyHostId);
            }
        }

        /// <summary>
        /// Disables collisions betweeen players
        /// </summary>
        [Server]
        public void DisablePlayersCollisions(bool ignore = true)
        {
            if (_Players.Count > 0)
            {
                int layer = LayerMask.NameToLayer("Cars");
                Physics.IgnoreLayerCollision(layer, layer, ignore);
            }
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
        public void UpdateRaceProgress(float raceTimer, out int playersFinished)
        {
            // Update car arc-lengths
            float circuitLength = m_CircuitController.CircuitLength;
            int i = 0;
            int finishedPlayers = 0;
            
            foreach (var player in _Players)
            {
                PlayerInfo playerInfo = player.Value;
                if (!playerInfo.AllLapsFinished)
                {
                    ComputeCarArcLength(ref playerInfo);
                
                    // Distance covered in circuit
                    float coveredCircuitDistance = playerInfo.CurrentLapCorrected==0 ?
                        playerInfo.ArcInfo - circuitLength:
                        circuitLength * (playerInfo.CurrentLapCorrected - 1) + playerInfo.ArcInfo;

                    playerInfo.TotalDistance = coveredCircuitDistance;
                    playerInfo.TotalRaceTime = raceTimer;

                    if (playerInfo.AllLapsFinished)
                    {
                        playerInfo.RpcShowHUDMessage("Race finished", 100);
                    }
                }
                else
                {
                    finishedPlayers++;
                }
            }

            playersFinished = finishedPlayers;
        }

        [Server]
        public void UpdatePlayersPositions()
        {
            PlayerInfo[] players = _Players.Values.ToArray();
            Array.Sort(players, (one, other) =>
            {
                if (one.AllLapsFinished && other.AllLapsFinished)
                {
                    return one.TotalRaceTime > other.TotalRaceTime ? 1 : -1;
                }

                if (one.AllLapsFinished && !other.AllLapsFinished)
                {
                    return -1;
                }

                if (!one.AllLapsFinished && other.AllLapsFinished)
                {
                    return 1;
                }

                return one.TotalDistance < other.TotalDistance ? 1 : -1;
            });
            
            int i = 1;
            foreach (var player in players)
            {
                PlayerInfo playerInfo = _Players[player.ID];
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

            // This call crashes if ComputeArcLength branded as [Server] or [ServerCallback]
            float minArcL =
                this.m_CircuitController.ComputeClosestPointArcLength(carPos, out carDirection, out segIdx, out carProj, out carDist);

            this._debuggingSpheres[ID].transform.position = carProj;
            
            // Has the player crossed finish line?
            // Debug.LogFormat("Current: {0}, New: {1}", player.CurrentSegmentIdx, segIdx);
            if(player.CurrentSegmentIdx==m_CircuitController.CircuitNumberOfSegments - 1 && segIdx==0)
            {
                Debug.LogFormat("Cambio de vuelta hacia delante");
                player.CrossedFinishLineForward();
            } else if (player.CurrentSegmentIdx == 0 && segIdx == m_CircuitController.CircuitNumberOfSegments - 1)
            {
                Debug.LogFormat("Cambio de vuelta hacia atras");
                player.CrossedFinishLineBackwards();
            }
            
            // Update player info
            player.CurrentCircuitPosition = carProj;
            player.LookAtPoint = carDirection;
            player.ArcInfo = minArcL;
            player.Direction = Vector3.Dot((carDirection - carProj).normalized, player.Speed.normalized);
            // Debug.Log(player.Direction);
            // Debug.DrawRay(player.transform.position, (carDirection - carProj).normalized * 3, Color.red);
            // Debug.DrawRay(player.transform.position, player.Speed.normalized * 3, Color.green);
            player.CurrentSegmentIdx = segIdx;
            return minArcL;
        }
        #endregion

        #region Client calls
        /// <summary>
        /// Updates client ui countdown
        /// Client side, called from server
        /// </summary>
        /// <param name="value">countdown value</param>
        [ClientRpc]
        public void RpcUpdateCountdown(int value)
        {
            int maxCountdown = 4;

            if (value > 0 && value < maxCountdown)
            {
                uiManager.UpdateUIMessage(""+value, 300);
            }
            else if (value == 0)
            {
                uiManager.UpdateUIMessage("GO", 300);
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
        /// Called to show player results in client
        /// </summary>
        [ClientRpc]
        public void RpcShowResults()
        {
            uiManager.ActivatePlayerResults();
        }

        [ClientRpc]
        public void RpcClearResults()
        {
            uiManager.ClearPlayerResults();
        }

        /// <summary>
        /// Called to show in game HUD in clients
        /// </summary>
        [ClientRpc]
        public void RpcShowInGameHUD(bool qualifying)
        {
            uiManager.ActivateInGameHUD();
            uiManager.ShowPanelPositions(!qualifying);
        }
        #endregion

        #region Hooks
        private void OnChangeMaxNumPlayers(int oldMaxNumPlayers, int newMaxNumPlayers)
        {
            uiManager.Lobby.UpdateNumDrivers(newMaxNumPlayers);
        }

        private void OnChangeNumLaps(int oldNumLaps, int newNumLaps)
        {
            uiManager.Lobby.UpdateNumLaps(newNumLaps);
        }

        private void OnChangeQualificationLap(bool oldValue, bool newValue)
        {
            uiManager.Lobby.UpdateQualificationLap(newValue);
        }
        

        #endregion

        [Command(ignoreAuthority = true)]
        void CmdUpdateNumPlayers(int numDrivers)
        {
            MaxNumPlayers = numDrivers;
        }

        [Command(ignoreAuthority = true)]
        void CmdUpdateNumLaps(int numLaps)
        {
            NumberOfLaps = numLaps;
            foreach(var player in _Players.Values)
            {
                player.NumberOfLaps = numLaps;
            }
        }

        [Command(ignoreAuthority = true)]
        void CmdUpdateQualificationLap(bool value)
        {
            QualificationLap = value;
        }
    }
}