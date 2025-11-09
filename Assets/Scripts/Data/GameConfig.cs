using UnityEngine;


[CreateAssetMenu(menuName = "SmashIO/GameConfig", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Match")]
    public float matchDurationSeconds = 15 * 60f; // 15 minutes
    public int initialBotCount = 12;
    public Vector2 arenaSize = new Vector2(3000, 3000);
    public float gridSpacing = 80f;


    [Header("Player Base Stats")]
    public float baseHP = 100f;
    public float baseSpeed = 9f;
    public float baseMass = 20f;


    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject botPrefab;
    public GameObject xpOrbPrefab;
    public GameObject coinPrefab;
    public GameObject boostSpeedPrefab;
    public GameObject boostBeat2xPrefab;


    [Header("Spawning")]
    public int startXPOrbs = 80;
    public int startCoins = 30;
    public int startBoosts = 8;

    [Header("Visuals (shared by all entities)")]
    public Sprite[] levelHandSprites;  // əl / silah şəkilləri (istəyə görə)

}