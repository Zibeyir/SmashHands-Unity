using UnityEngine;


public class XPOrb : Collectible
{
    public float xpAmount = 10f;


    void OnTriggerEnter2D(Collider2D other)
    {
        var e = other.GetComponent<Entity>();
        if (!e) return;
        e.AddXP(xpAmount);
        OnCollected(e);
    }
}