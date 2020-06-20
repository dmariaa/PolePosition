using System.Collections.Generic;
using PolePosition.Player;

namespace PolePosition.Manager
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
            if (_polePositionManager.MaxNumPlayers == _polePositionManager.Players.Count)
            {
                int numberOfReadyPlayers = 0;
            
                foreach (var player in _polePositionManager.Players)
                {
                    if (player.Value.IsReady)
                    {
                        numberOfReadyPlayers++;
                    }    
                }

                if (numberOfReadyPlayers >= _polePositionManager.MaxNumPlayers * 0.5)
                {
                    if (_polePositionManager.QualificationLap)
                    {
                        _polePositionManager.StateChange(new StateInQualificationRound(_polePositionManager));  
                    }
                    else
                    {
                        _polePositionManager.StateChange(new StateInRace(_polePositionManager));
                    }
                }
            }
        }

        public override void Exit()
        {
            _polePositionManager.InRace = true;
        }
    }
}