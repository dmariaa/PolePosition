using UnityEngine;

namespace PolePosition.StateMachine
{
    /// <summary>
    /// Simple state machine inspired by
    /// https://forum.unity.com/threads/c-proper-state-machine.380612/
    /// </summary>
    public class StateMachine
    {
        private IState _currentState;

        public void Update()
        {
            if (_currentState != null)
            {
                // Debug.LogFormat("Updating state {0}", _currentState);
                _currentState.Update();
            }
        }

        public void ChangeState(IState newState)
        {
            if (_currentState != null)
            {
                Debug.LogFormat("Exiting state {0}", _currentState);
                _currentState.Exit();
            }

            _currentState = newState;
            Debug.LogFormat("Entering state {0}", _currentState);
            _currentState.Enter();
        }
    }
}