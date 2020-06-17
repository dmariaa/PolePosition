using System.Security.Cryptography;
using PolePosition.StateMachine;
using UnityEngine;

namespace PolePositionManager
{
    public abstract class PolePositionManagerState : IState
    {
        protected PolePositionManager _polePositionManager;

        private string _name;
        public string Name
        {
            get => _name;
        }

        protected PolePositionManagerState(PolePositionManager polePositionManager, string name)
        {
            _polePositionManager = polePositionManager;
            _name = name;
        }
        
        public override string ToString()
        {
            return string.Format("[State => {0}]", _name);
        }
        //dskjfhsk
        public abstract void Enter();

        public abstract void Update();

        public abstract void Exit();
    }
}