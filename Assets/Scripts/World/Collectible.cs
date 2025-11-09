using Assets.Scripts.Core;
using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public abstract class Collectible : MonoBehaviour
{
    public float respawnDelay = 2f;
    protected bool _available = true;


    public virtual void OnCollected(Entity by)
    {
        if (!_available) return;
        _available = false;
        Debug.Log($"{by.stats.playerName} collected {gameObject.name}");
        gameObject.SetActive(false);
        Invoke(nameof(Respawn), respawnDelay);
    }


    public virtual void Respawn()
    {
        transform.position = SpawnManager.Instance.RandomSpawnPosition();
        _available = true;
        gameObject.SetActive(true);
    }
}