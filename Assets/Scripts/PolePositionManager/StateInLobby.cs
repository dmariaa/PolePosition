using System.Collections.Generic;
using PolePosition.Player;

namespace PolePositionManager
{
    /// <summary>
    /// State to manage lobby
    /// </summary>
    public class StateInLobby : PolePositionManagerState
    {
        private int _numberOfPlayers;
        private Dictionary<int, PlayerInfo> _players;

        public StateInLobby(PolePositionManager polePositionManager) : base(polePositionManager, "InLobby")
        {
            
        }
        
        public override void Enter()
        {
            
        }
        
        public override void Update()
        {
            int numberOfReadyPlayers = 0;
            
            foreach (var player in _polePositionManager.Players)
            {
                if (player.Value.IsReady)
                {
                    numberOfReadyPlayers++;
                }    
            }

            if (numberOfReadyPlayers == _polePositionManager.MaxNumPlayers)
            {
                _polePositionManager.StateChange(new StateInRace(_polePositionManager));
            }
        }

        public override void Exit()
        {
        }
    }
}