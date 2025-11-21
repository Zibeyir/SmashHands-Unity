using UnityEngine;


[CreateAssetMenu(menuName = "SmashIO/GameConfig", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Match")]
    public float matchDurationSeconds = 15 * 60f; // 15 minutes
    public int initialBotCount = 60;
    public Vector2 arenaSize = new Vector2(350, 350);
    public float gridSpacing = 50f;


    [Header("Player Base Stats")]
    public float baseHP = 300;
    public float baseSpeed = 12;
    public float baseMass = 20f;


    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject botPrefab;
    public GameObject xpOrbPrefab;
    public GameObject coinPrefab;
    public GameObject boostSpeedPrefab;
    public GameObject boostBeat2xPrefab;


    [Header("Spawning")]
    public int startXPOrbs = 2000;
    public int startCoins = 30;
    public int startBoosts = 20;

    [Header("Visuals (shared by all entities)")]
    public Sprite[] levelHandSprites;  // əl / silah şəkilləri (istəyə görə)

}