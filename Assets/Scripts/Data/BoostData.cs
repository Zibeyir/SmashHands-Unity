using UnityEngine;


public enum BoostType { Speed, Beat2x }


[CreateAssetMenu(menuName = "SmashIO/Boost", fileName = "Boost_")]
public class BoostData : ScriptableObject
{
    public BoostType type;
    public Color color = Color.white;
    public float durationSeconds = 5f;
    public float multiplier = 2f; // e.g., speed*2 or damage*2
}