using Mirror;
using UnityEngine;

namespace PolePosition.Manager
{
    /// <summary>
    /// State to manage race finished
    /// </summary>
    public class StateRaceFinished : PolePositionManagerState
    {
        private bool _qualifying;
        private float _timer;
        
        public StateRaceFinished(PolePositionManager polePositionManager, bool qualifying = false) : base(polePositionManager, "RaceFinished")
        {
            _qualifying = qualifying;
        }

        public override void Enter()
        {
            _polePositionManager.FinishRace();

            if (_qualifying)
            {
                _polePositionManager.RpcSetFinishTitle("Qualifying results", 65);
            }
            else
            {
                _polePositionManager.RpcSetFinishTitle("Race results", 65);
                _polePositionManager.RpcSetFinishSubtitle("Race is over", 36);
            }

            _timer = 0;
        }

        public override void Update()
        {
            _timer += Time.deltaTime;
            
            if (_qualifying)
            {
                if (_timer < 15.0f)
                {
                    _polePositionManager.RpcSetFinishSubtitle("RACE STARTS IN " + 
                            Utils.FormatSeconds(15.0f - _timer, false), 36);
                }
                else
                {
                    _polePositionManager.StateChange(new StateInRace(_polePositionManager));
                }
            }
        }

        public override void Exit()
        {
            _polePositionManager.RpcSetFinishTitle("", 65);
            _polePositionManager.RpcSetFinishSubtitle("", 36);
        }
    }
}