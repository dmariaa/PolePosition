using Mirror;
using UnityEngine;

namespace DefaultNamespace
{
    public class PolePositionNetworkManager : NetworkManager
    {
        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            Transform startPos = GetStartPosition();
            GameObject player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            SetupPlayer setupPlayer = player.GetComponent<SetupPlayer>();
            setupPlayer.m_Position = startPositionIndex + 1;
            NetworkServer.AddPlayerForConnection(conn, player);
        }
    }
}