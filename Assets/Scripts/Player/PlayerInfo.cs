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
        /// Player position in race
        /// </summary>
        [SyncVar(hook = nameof(OnChangePosition))] public int CurrentPosition;

        /// <summary>
        /// Player current race LAP
        /// </summary>
        [SyncVar] public int CurrentLap;

        /// <summary>
        /// Player current Color
        /// </summary>
        [SyncVar(hook = nameof(OnChangeColor))] public Color Color;

        /// <summary>
        /// Player current ArcInfo
        /// </summary>
        [SyncVar] public float ArcInfo;

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
            _setupPlayer.SetPosition(newPosition);
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