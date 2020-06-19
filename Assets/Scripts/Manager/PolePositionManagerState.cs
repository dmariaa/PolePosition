using PolePosition.StateMachine;

namespace PolePosition.Manager
{
    public abstract class PolePositionManagerState : IState
    {
        /// <summary>
        /// Reference to current PolePositionManager
        /// </summary>
        protected PolePositionManager _polePositionManager;

        /// <summary>
        /// State name, for debugging purposes
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="polePositionManager">Reference to PolePositionManager</param>
        /// <param name="name">State name</param>
        protected PolePositionManagerState(PolePositionManager polePositionManager, string name)
        {
            _polePositionManager = polePositionManager;
            _name = name;
        }
        
        /// <summary>
        /// To string conversion
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[State => {0}]", _name);
        }
        
        /// <summary>
        /// Abstract enter state method
        /// </summary>
        public abstract void Enter();

        /// <summary>
        /// Abstract update state method
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Abstract exit state method
        /// </summary>
        public abstract void Exit();
    }
}