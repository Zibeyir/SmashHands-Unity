using UnityEngine;


public class BoostPickup : Collectible
{
    public BoostData data;


    void OnTriggerEnter2D(Collider2D other)
    {
        var e = other.GetComponent<Entity>();
        if (!e) return;
        Debug.Log($"{e.stats.playerName} triggered boost pickup: {data.type}");
        ApplyBoost(e);
        OnCollected(e);
    }


    void ApplyBoost(Entity target)
    {
        switch (data.type)
        {
            case BoostType.Speed:
                GameManager.Instance.StartCoroutine(GameManager.Instance.ApplyTimedSpeedBoost(target, data.durationSeconds));
                break;
            case BoostType.Beat2x:
                GameManager.Instance.StartCoroutine(GameManager.Instance.ApplyTimedDamageBoost(target, data.durationSeconds));
                break;
        }
    }
}