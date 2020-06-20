using System;
using Mirror;
using PolePosition.UI;
using UnityEngine;

namespace PolePosition.Player
{
    public class PlayerInfo : NetworkBehaviour
    {
        private static string _printString = string.Join(Environment.NewLine, new string[]
        {
            "Player [{0}]: {{",
            "  Name: {1},",
            "  Color: {2},",
            "  CurrentPosition: {3}",
            "  CurrentLap: {4}",
            "}}"
        });

        #region SyncVars
        /// <summary>
        /// Network Object ID
        /// </summary>
        [SyncVar] public int ID;

        /// <summary>
        /// Player name
        /// </summary>
        [SyncVar(hook = nameof(OnChangeName))] public string Name;

        /// <summary>
        /// Current speed
        /// </summary>
        [SyncVar(hook = nameof(OnChangeSpeed))] public Vector3 Speed;

        /// <summary>
        /// Current direction,
        /// if positive the car is moving forward relative to the circuit,
        /// if negative the car is moving backwards relative to the circuit
        /// </summary>
        [SyncVar(hook = nameof(OnChangeDirection))] public float Direction;

        /// <summary>
        /// Player position in race
        /// </summary>
        [SyncVar(hook = nameof(OnChangePosition))] public int CurrentPosition;

        /// <summary>
        /// Player current race LAP
        /// </summary>
        [SyncVar(hook = nameof(OnChangeCurrentLap))] public int CurrentLap;

        /// <summary>
        /// Player current Color
        /// </summary>
        [SyncVar(hook = nameof(OnChangeColor))] public Color Color;

        /// <summary>
        /// Player current ArcInfo
        /// </summary>
        [SyncVar(hook = nameof(OnChangeArcInfo))] public float ArcInfo;
        
        /// <summary>
        /// Player Current Lap Time
        /// </summary>
        [SyncVar(hook=nameof(OnChangeCurrentLapTime))] public float CurrentLapTime;

        /// <summary>
        /// Best Lap time
        /// </summary>
        [SyncVar(hook=nameof(OnChangeBestLapTime))] public float BestLapTime;

        /// <summary>
        /// Player Total Race Time
        /// </summary>
        [SyncVar(hook=nameof(OnChangeTotalRaceTime))] public float TotalRaceTime;

        /// <summary>
        /// Player Total number of laps
        /// (number of laps to complete in race
        /// </summary>
        [SyncVar(hook=nameof(OnChangeNumberOfLaps))] public int NumberOfLaps;
        
        /// <summary>
        /// Player current position in circuit as a vector3
        /// </summary>
        [SyncVar] public Vector3 CurrentCircuitPosition;

        /// <summary>
        /// Forward looking vector in circuit
        /// </summary>
        [SyncVar] public Vector3 LookAtPoint;

        [SyncVar(hook = nameof(OnChangeIsReady))] public bool IsReady;
        #endregion

        #region Delegates and events
        public delegate void OnChangeNameDelegate(string playerName);
        public event OnChangeNameDelegate OnChangeNameEvent;

        public delegate void OnChangeColorDelegate(Color color);
        public event OnChangeColorDelegate OnChangeColorEvent;

        public delegate void OnChangeIsReadyDelegate(bool isReady);
        public event OnChangeIsReadyDelegate OnChangeIsReadyEvent;
        #endregion

        #region SyncVars Hooks
        private void OnChangeIsReady(bool oldIsReady, bool newIsReady)
        {
            if (OnChangeIsReadyEvent != null)
            {
                OnChangeIsReadyEvent(newIsReady);
            }
            
            _setupPlayer.UpdatePosition();
        }
        private void OnChangeName(string oldName, string newName)
        {
            if (OnChangeNameEvent != null)
            {
                OnChangeNameEvent(newName);
            }
            
            _setupPlayer.UpdatePosition();
        }
        private void OnChangeColor(Color oldColor, Color newColor)
        {
            _setupPlayer.SetPlayerColor(newColor);    
            _setupPlayer.UpdatePosition();        // TODO: Change UI to event subscription

            if (OnChangeColorEvent != null)
            {
                OnChangeColorEvent(newColor);
            }
        }

        private void OnChangePosition(int oldPosition, int newPosition)
        {
            _setupPlayer.UpdatePosition();
        }

        private void OnChangeSpeed(Vector3 oldSpeed, Vector3 newSpeed)
        {
            _setupPlayer.SetSpeed(newSpeed.magnitude);
        }

        private void OnChangeDirection(float oldDirection, float newDirection)
        {
            // Debug.LogFormat("Direction: {0}", newDirection);
        }

        private void OnChangeArcInfo(float oldArcInfo, float newArcInfo)
        {
            // Debug.LogFormat("ArcLength: {0}", newArcInfo);
        }

        private void OnChangeCurrentLapTime(float oldTime, float newTime)
        {
            _setupPlayer.SetCurrentLapTime(newTime);
        }

        private void OnChangeBestLapTime(float oldTime, float newTime)
        {
            Debug.LogFormat("Setting best lap time to {0}, current lap: {1}",
                Utils.FormatTime(BestLapTime),
                CurrentLap);
        }

        private void OnChangeTotalRaceTime(float oldTime, float newTime)
        {
            _setupPlayer.UpdatePosition();
        }

        private void OnChangeNumberOfLaps(int oldNumberOfLaps, int newNumberOfLaps)
        {
            _setupPlayer.SetNumberOfLaps(newNumberOfLaps);
        }

        private void OnChangeCurrentLap(int oldCurrentLap, int newCurrentLap)
        {
            _setupPlayer.SetCurrentLap(newCurrentLap);
        }
        #endregion

        #region Client RpcCalls and Commands
        [ClientRpc]
        public void RpcShowHUDMessage(string message, int size)
        {
            _setupPlayer.UpdateHUDMessage(message, size);
        }

        [ClientRpc]
        public void RpcActivateHostLobbyButtons(bool activate, int id)
        {
            if (ID == id && isLocalPlayer)
            {
                _setupPlayer.ActivateHostLobbyButtons(activate);
                /*_setupPlayer.m_UIManager.Lobby.OnUpdateNumDrivers = (int numDrivers) =>
                {
                    CmdUpdateNumPlayers(numDrivers);
                };*/
            }
        }
        [ClientRpc]
        public void RpcConfigureCamera()
        {
            if (isLocalPlayer)
            {
                _setupPlayer.ConfigureCamera();    
            }
            
        }
        
        /// <summary>
        /// Called from server to launch player control
        /// </summary>
        [ClientRpc]
        public void RpcLaunchPlayer()
        {
            _setupPlayer.StartPlayerController();
        }

        /// <summary>
        /// Called from server to stop player
        /// </summary>
        [ClientRpc]
        public void RpcStopPlayer()
        {
            _setupPlayer.StopPlayerController();
        }


        /// <summary>
        /// Chat receive message
        /// </summary>
        /// <param name="message"></param>
        [ClientRpc]
        public void RpcReceive(string message)
        {
            OnMessage?.Invoke(this, message);
        }

        /// <summary>
        /// Chat send message
        /// </summary>
        /// <param name="message"></param>
        [Command]
        public void CmdSend(string message)
        {
            if (message.Trim() != "")
                RpcReceive(message.Trim());
        }

        [Command]
        public void CmdSetName(string playerName)
        {
            Name = playerName;
        }

        [Command]
        public void CmdSetColor(Color color)
        {
            Color = color;
        }

        [Command]
        public void CmdSetIsReady(bool ready)
        {
            IsReady = ready;
        }

        [Command]
        void CmdUpdateNumPlayers(int numDrivers)
        {
            _setupPlayer._polePositionManager.MaxNumPlayers = numDrivers;
        }

        #endregion

        #region Private props
        /// <summary>
        /// Setup player component reference
        /// </summary>
        private SetupPlayer _setupPlayer;
        
        /// <summary>
        /// Total circuit distance covered
        /// </summary>
        public float TotalDistance = 0;
        
        /// <summary>
        /// Current lap value, used to measure distance
        /// in current lap... It turns down and up when you cross
        /// finish line forward or backward
        /// Used server only, not needed in client
        /// </summary>
        public float CurrentLapCorrected = 0;
        
        /// <summary>
        /// Current circuit segment the car is actually
        /// Used server only, not needed in client
        /// </summary>
        public int CurrentSegmentIdx = 0;
        
        /// <summary>
        /// true if last finish line crossing was made backwards,
        /// false if was made forward
        /// Used server only, not needed in client
        /// </summary>
        private bool _crossedFinishLineBack = false;

        /// <summary>
        /// true if all laps finished
        /// Used server only, not needed in client
        /// </summary>
        public bool AllLapsFinished = false;

        public static event Action<PlayerInfo, string> OnMessage;
        #endregion

        #region Method Overrides
        private void Awake()
        {
            _setupPlayer = GetComponent<SetupPlayer>();
        }
        
        public override string ToString()
        {
            return string.Format(_printString,
                ID,
                Name,
                Color.ToString(),
                CurrentPosition,
                CurrentLap
                );
        }
        #endregion

        #region Other Methods
        /// <summary>
        /// The player crossed the finish line in forward direction
        /// Called server side only
        /// </summary>
        [Server]
        public void CrossedFinishLineForward()
        {
            if (!_crossedFinishLineBack)
            {
                if (CurrentLap > 1)
                {
                    if (CurrentLapTime < BestLapTime)
                    {
                        BestLapTime = CurrentLapTime;
                    }    
                } else if (CurrentLap == 1)
                {
                    BestLapTime = CurrentLapTime;
                }

                CurrentLap += 1;
                
                if (CurrentLap <= NumberOfLaps)
                {
                    _setupPlayer.StartLapTime();
                }
                else
                {
                    StopCar();
                }
            }

            CurrentLapCorrected += 1;
            _crossedFinishLineBack = false;
        }

        /// <summary>
        /// Finishes race for this car
        /// </summary>
        [Server]
        public void StopCar()
        {
            AllLapsFinished = true;
            RpcStopPlayer();
            _setupPlayer.StopLapTime();
            _setupPlayer.StopCar();
        }

        /// <summary>
        /// The player crossed the finish line in wrong direction
        /// Called server side only
        /// </summary>
        [Server]
        public void CrossedFinishLineBackwards()
        {
            CurrentLapCorrected -= 1;
            _crossedFinishLineBack = true;
        }
        #endregion
    }
}