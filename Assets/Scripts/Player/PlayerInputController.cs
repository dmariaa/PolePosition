using Mirror;
using UnityEngine;

namespace PolePosition.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerInputController : NetworkBehaviour
    {
        private PlayerController _playerController;

        private void Awake()
        {
            if (_playerController == null)
            {
                _playerController = GetComponent<PlayerController>();
            }
        }

        [ClientCallback]
        private void Update()
        {
            if (isLocalPlayer)
            {
                CmdUpdateInputs(
                    Input.GetAxis("Vertical"),
                    Input.GetAxis("Horizontal"),
                    Input.GetAxis("Jump")
                );    
            }
        }

        [Command]
        private void CmdUpdateInputs(float acceleration, float steering, float brake)
        {
            _playerController.InputAcceleration = Mathf.Clamp(acceleration, -1, 1);
            _playerController.InputSteering = Mathf.Clamp(steering, -1, 1);
            _playerController.InputBrake = Mathf.Clamp(brake, 0, 1);
        }
    }
}
