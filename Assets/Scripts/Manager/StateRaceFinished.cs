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
        }

        public override void Update()
        {
            if(_qualifying)
            {
                _timer += Time.deltaTime;
                
                if (_timer >= 5)
                {
                    _polePositionManager.StateChange(new StateInRace(_polePositionManager));
                }
            }
        }

        public override void Exit()
        {
            // Nothing to do
        }
    }
}