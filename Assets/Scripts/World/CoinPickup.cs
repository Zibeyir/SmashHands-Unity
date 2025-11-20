using UnityEngine;


public class CoinPickup : Collectible
{
    public int amount = 1;


    void OnTriggerEnter2D(Collider2D other)
    {
        var e = other.GetComponent<Entity>();
        if (!e) return;
        e.stats.coins += amount;
        if (e.isPlayer) AudioManager.Instance.PlaySFX(SoundEnum.coin, transform);

        OnCollected(e);
    }
}