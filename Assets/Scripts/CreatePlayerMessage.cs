using Mirror;
using UnityEngine;

namespace PolePosition
{
    public class CreatePlayerMessage : MessageBase
    {
        public string Name;
        public Color32 Color;
    }
}