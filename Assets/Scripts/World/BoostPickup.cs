using UnityEngine;


public class BoostPickup : Collectible
{
    public BoostData data;


    void OnTriggerEnter2D(Collider2D other)
    {
        var e = other.GetComponent<Entity>();
        if (!e) return;
        ApplyBoost(e);
        OnCollected(e);
    }


    void ApplyBoost(Entity target)
    {
        switch (data.type)
        {
            case BoostType.Speed:
                if (target.isPlayer) AudioManager.Instance.PlaySFX(SoundEnum.speed, transform);
                GameManager.Instance.StartCoroutine(GameManager.Instance.ApplyTimedSpeedBoost(target, data.durationSeconds));

                break;
            case BoostType.Beat2x:
                if (target.isPlayer) AudioManager.Instance.PlaySFX(SoundEnum.x2punch, transform);

                GameManager.Instance.StartCoroutine(GameManager.Instance.ApplyTimedDamageBoost(target, data.durationSeconds));
                break;
        }
    }
}