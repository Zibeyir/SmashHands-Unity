using UnityEngine;

public class XPOrb : Collectible
{
    public float baseXP = 10f;

    [Header("Random Settings")]
    Vector2 randomScaleRange = new Vector2(0.3f, 0.6f);
    public Color[] possibleColors;

    private float xpAmount;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // --- RANDOM SCALE ---
        float s = Random.Range(randomScaleRange.x, randomScaleRange.y);
        transform.localScale = new Vector3(s, s, 1);

        // --- SCALE → XP ---
        xpAmount = baseXP * (s+1);   // ölçü qədər XP artır

        // --- RANDOM COLOR ---
        if (sr != null && possibleColors != null && possibleColors.Length > 0)
        {
            sr.color = possibleColors[Random.Range(0, possibleColors.Length)];
        }
        else if (sr != null)
        {
            sr.color = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.9f, 1f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var e = other.GetComponent<Entity>();
        if (!e) return;

        e.AddXP(xpAmount);
        OnCollected(e);
    }
}
