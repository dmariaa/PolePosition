using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public string Name { get; set; }

    public int ID { get; set; }

    public int CurrentPosition { get; set; }

    public int CurrentLap { get; set; }

    public Color Color { get; set; }
    
    public float ArcInfo { get; set; }

    public override string ToString()
    {
        return Name;
    }
}