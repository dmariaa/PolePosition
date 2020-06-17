namespace PolePositionManager
{
    /// <summary>
    /// State to manage race finished
    /// </summary>
    public class StateRaceFinished : PolePositionManagerState
    {
        public StateRaceFinished(PolePositionManager polePositionManager) : base(polePositionManager, "RaceFinished")
        {
        }

        public override void Enter()
        {
            _polePositionManager.FinishRace();
        }

        public override void Update()
        {
            // Nothing to do
        }

        public override void Exit()
        {
            // Nothing to do
        }
    }
}