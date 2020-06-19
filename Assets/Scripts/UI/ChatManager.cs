using Mirror;
using PolePosition.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    class ChatManager: MonoBehaviour
    {
        public InputField chatMessage;
        public Text chatHistory;
        public Scrollbar scrollbar;

        public void Awake()
        {
            PlayerInfo.OnMessage += OnPlayerMessage;
        }

        private void OnPlayerMessage(PlayerInfo player, string message)
        {
            string color ="#" + ColorUtility.ToHtmlStringRGBA(player.Color);
            /*string prettyMessage = player.isLocalPlayer ?
                $"<color=red>{player.Name}: </color> {message}" :
                $"<color=blue>{player.Name}: </color> {message}";*/
            string prettyMessage = string.Format("<color={0}>[ {1} ]: {2}</color>", color, player.Name, message);
            if (player.isLocalPlayer)
            {
                prettyMessage = "<b>" + prettyMessage + "</b>";
            }
            AppendMessage(prettyMessage);

            Debug.Log(message);
        }

        public void OnSend()
        {
            if (chatMessage.text.Trim() == "")
                return;

            // get our player
            PlayerInfo player = NetworkClient.connection.identity.GetComponent<PlayerInfo>();

            // send a message
            player.CmdSend(chatMessage.text.Trim());

            chatMessage.text = "";
        }

        internal void AppendMessage(string message)
        {
            StartCoroutine(AppendAndScroll(message));
        }

        IEnumerator AppendAndScroll(string message)
        {
            chatHistory.text += message + "\n";

            // it takes 2 frames for the UI to update ?!?!
            yield return null;
            yield return null;

            // slam the scrollbar down
            scrollbar.value = 0;
        }
    }
}
