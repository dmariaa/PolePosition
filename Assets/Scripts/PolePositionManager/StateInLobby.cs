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
            _numberOfPlayers = polePositionManager.MaxNumPlayers;
            _players = polePositionManager.Players;
        }
        
        public override void Enter()
        {
            // Nothing to do here
        }
        
        public override void Update()
        {
            int numberOfReadyPlayers = 0;
            
            foreach (var player in _players)
            {
                if (player.Value.IsReady)
                {
                    numberOfReadyPlayers++;
                }    
            }

            if (numberOfReadyPlayers == _numberOfPlayers)
            {
                _polePositionManager.StateChange(new StateInRace(_polePositionManager));
            }
        }

        public override void Exit()
        {
        }
    }
}