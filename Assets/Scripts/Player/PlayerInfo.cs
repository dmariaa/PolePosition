using System;
using Mirror;
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
        /// Current direction
        /// Positive the car is moving forward relative to the circuit
        /// Negative the car is moving backwards relative to the circuit
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
        /// Player Total Race Time
        /// </summary>
        [SyncVar(hook=nameof(OnChangeTotalRaceTime))] public float TotalRaceTime;

        /// <summary>
        /// Player Total number of laps
        /// (number of laps to complete in race
        /// </summary>
        [SyncVar(hook=nameof(OnChangeNumberOfLaps))] public int NumberOfLaps;
        
        /// <summary>
        /// Player current ??
        /// </summary>
        [SyncVar] public Vector3 PosCentral;

        /// <summary>
        /// Player current ??
        /// </summary>
        [SyncVar] public Vector3 PuntoLookAt;
        #endregion

        #region SyncVars Hooks
        private void OnChangeName(string oldName, string newName)
        {
            _setupPlayer.SetPlayerName(newName);
        }
        private void OnChangeColor(Color oldColor, Color newColor)
        {
            _setupPlayer.SetPlayerColor(newColor);
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

        [ClientRpc]
        public void RpcLaunchPlayer()
        {
            _setupPlayer.StartPlayerController();
        }
        #endregion

        #region Private props
        private SetupPlayer _setupPlayer;
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
    }
}