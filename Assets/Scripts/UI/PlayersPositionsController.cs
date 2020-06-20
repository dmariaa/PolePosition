using System.Collections.Generic;
using PolePosition.Player;
using PolePosition.UI;
using UnityEngine;

namespace UI
{
    public class PlayersPositionsController : MonoBehaviour
    {
        [SerializeField] private PlayerInfoPanelController _playePositionPrefab;

        private readonly Dictionary<int, PlayerInfoPanelController> _playerInfoPanelControllers = new Dictionary<int, PlayerInfoPanelController>();

        public void UpdatePlayerInfo(PlayerInfo playerInfo)
        {
            PlayerInfoPanelController playerInfoPanelController;
            
            if(!_playerInfoPanelControllers.TryGetValue(playerInfo.ID, out playerInfoPanelController))
            {
                playerInfoPanelController = Instantiate(_playePositionPrefab, transform);
                _playerInfoPanelControllers.Add(playerInfo.ID, playerInfoPanelController);
            }

            playerInfoPanelController.UpdateName(playerInfo.Name);
            playerInfoPanelController.UpdateColor(playerInfo.Color);
            playerInfoPanelController.UpdateLaptime(playerInfo.TotalRaceTime);
            playerInfoPanelController.UpdatePosition(playerInfo.CurrentPosition);
            playerInfoPanelController.transform.SetSiblingIndex(playerInfo.CurrentPosition - 1);
        }

        public void RemovePlayerInfo(PlayerInfo playerInfo)
        {
            PlayerInfoPanelController playerInfoPanelController;

            if (_playerInfoPanelControllers.TryGetValue(playerInfo.ID, out playerInfoPanelController))
            {
                _playerInfoPanelControllers.Remove(playerInfo.ID);
                Destroy(playerInfoPanelController.gameObject);
            }
        }
    }
}