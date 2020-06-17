using PolePosition.StateMachine;
using UnityEngine;

namespace PolePositionManager
{
    /// <summary>
    /// State to manage race
    /// </summary>
    public class StateInRace : PolePositionManagerState
    {
        private int _countDownTimer;
        private float _timer = 0f;
        private bool _carsRunning = false;
        private int _numberOfPlayersInRace = 0;

        public StateInRace(PolePositionManager polePositionManager) : base(polePositionManager, "InRace")
        {
        }

        public override void Enter()
        {
            _countDownTimer = 4;
            _polePositionManager.RpcShowInGameHUD();
            _numberOfPlayersInRace = _polePositionManager.Players.Count;
            foreach (var player in _polePositionManager.Players)
            {
                player.Value.RpcConfigureCamera();
            }
        }

        public override void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= 1f && _countDownTimer > -1)
            {
                _timer = 0f;
                _countDownTimer--;
                _polePositionManager.RpcUpdateCountdown(_countDownTimer);
            }

            if (_countDownTimer == 0)
            {
                _polePositionManager.StartRace();
                _carsRunning = true;
            }

            if (_carsRunning)
            {
                int finishedPlayers = 0;
                _polePositionManager.UpdateRaceProgress(out finishedPlayers);

                if (finishedPlayers == _numberOfPlayersInRace)
                {
                    _polePositionManager.StateChange(new StateRaceFinished(_polePositionManager));
                }
                else
                {
                    _polePositionManager.UpdatePlayersPositions();    
                }
            }        
        }

        public override void Exit()
        {
            // Nothing to do now
        }
    }
}