using UnityEngine;


public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }
    void Awake() => Instance = this;


    public Vector2 RandomSpawnPosition()
    {
        var s = GameManager.Instance.Config.arenaSize * 0.5f;
        return new Vector2(Random.Range(-s.x, s.x), Random.Range(-s.y, s.y));
    }
}