using System;
using Mirror;
using PolePosition.Player;
using UnityEngine;

namespace PolePosition.Manager
{
    /// <summary>
    /// State to manage race
    /// </summary>
    public class StateInRace : PolePositionManagerState
    {
        private int _countDown;
        private float _countDownTimer = 0f;
        private float _raceTimer = 0f;
        private bool _carsRunning = false;
        private int _numberOfPlayersInRace = 0;

        public StateInRace(PolePositionManager polePositionManager) : base(polePositionManager, "InRace")
        {
        }

        public override void Enter()
        {
            _polePositionManager.DisablePlayersCollisions(false);
            _polePositionManager.RpcShowInGameHUD(false);

            foreach (var player in _polePositionManager.Players.Values)
            {
                Transform startPosition = NetworkManager.startPositions[player.CurrentPosition - 1];
                player.NumberOfLaps = _polePositionManager.NumberOfLaps;
                player.CurrentLap = 0;
                player.TotalRaceTime = 0f;
                player.CurrentLapTime = 0f;
                player.BestLapTime = 0f;
                player.AllLapsFinished = false;
                player.CurrentSegmentIdx = _polePositionManager.m_CircuitController.CircuitNumberOfSegments - 1;
                player.GetComponent<SetupPlayer>().StartCar();
                player.GetComponent<SetupPlayer>().RelocateCar(startPosition.position, startPosition.rotation);
                player.GetComponent<PlayerMatchChecker>().matchId = _polePositionManager.GlobalGUID;
                
                player.RpcShowHUDMessage("ready to race!", 100);
                player.RpcConfigureCamera();
            }
            
            _carsRunning = false;
            _countDown = 4;
            _countDownTimer = 0f;
            _raceTimer = 0f;
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
                _raceTimer += Time.deltaTime;
                
                int finishedPlayers = 0;
                _polePositionManager.UpdateRaceProgress(_raceTimer, out finishedPlayers);

                if (finishedPlayers == _numberOfPlayersInRace || 
                    (_polePositionManager.TestMode && _numberOfPlayersInRace==1))
                {
                    _polePositionManager.StateChange(new StateRaceFinished(_polePositionManager, false));
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