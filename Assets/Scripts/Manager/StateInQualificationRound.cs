using System;
using Mirror;
using PolePosition.Player;
using UnityEngine;

namespace PolePosition.Manager
{
    public class StateInQualificationRound : PolePositionManagerState
    {
        private int _countDown;
        private float _countDownTimer;
        private bool _carsRunning;
        private int _numberOfPlayersInRace = 0;
        
        public StateInQualificationRound(PolePositionManager polePositionManager) : base(polePositionManager, "InQualificationRound")
        {
        }

        public override void Enter()
        {
            // No players collisions for qualifying
            _polePositionManager.DisablePlayersCollisions();
            _polePositionManager.RpcShowInGameHUD(true);

            Transform startPosition = NetworkManager.startPositions[0];
   
            foreach (var player in _polePositionManager.Players.Values)
            {
                player.NumberOfLaps = 1;
                player.CurrentLap = 0;
                player.CurrentPosition = 1;
                player.TotalRaceTime = 0f;
                player.CurrentLapTime = 0f;
                player.BestLapTime = 0f;
                player.AllLapsFinished = false;
                player.CurrentSegmentIdx = _polePositionManager.m_CircuitController.CircuitNumberOfSegments - 1;
                player.GetComponent<SetupPlayer>().RelocateCar(startPosition.position, startPosition.rotation);
                player.GetComponent<PlayerMatchChecker>().matchId = Guid.NewGuid();
                
                player.RpcShowHUDMessage("qualification\nround", 100);
                player.RpcConfigureCamera();
            }

            _countDown = 4;
            _countDownTimer = 0f;
            _carsRunning = false;
        }

        public override void Update()
        {
            _countDownTimer += Time.deltaTime;
            _numberOfPlayersInRace = _polePositionManager.Players.Count;
            
            if (_countDownTimer >= 1f && _countDown > -1)
            {
                _countDownTimer = 0f;
                _countDown--;
                _polePositionManager.RpcUpdateCountdown(_countDown);
            }
            
            if (_countDown == 0)
            {
                _polePositionManager.StartRace();
                _carsRunning = true;
            }

            if (_carsRunning)
            {
                int finishedPlayers = 0;
                _polePositionManager.UpdateRaceProgress(0f, out finishedPlayers);
                
                if (finishedPlayers == _numberOfPlayersInRace || 
                    (!_polePositionManager.TestMode && _numberOfPlayersInRace==1))
                {
                    _polePositionManager.UpdatePlayersPositions(true);
                    if (_numberOfPlayersInRace > 1)
                    {
                        _polePositionManager.StateChange(new StateRaceFinished(_polePositionManager, true));    
                    }
                    else
                    {
                        _polePositionManager.StopAllCars();
                        _polePositionManager.StateChange(new StateRaceFinished(_polePositionManager, false));
                    }
                }
            }
        }

        public override void Exit()
        {
        }
    }
}