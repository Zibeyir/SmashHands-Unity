using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Rigidbody2D))]
public class Entity : MonoBehaviour, IDamageable
{
    [Header("Refs")]
    public Rigidbody2D rb;
    public CircleCollider2D bodyCollider;
    public Hitbox punchHitbox; // child object, disabled by default


    [Header("State")]
    public Stats stats = new Stats();
    public bool isPlayer;


    [Header("Punch")]
    public float punchCooldown = 0.6f;
    public float punchRangeExtra = 40f; // range = radius + extra
    float _lastPunchTime;


    [Header("Runtime")]
    public bool boostDamage2xActive;
    public bool boostSpeed2xActive;

    public SpriteRenderer handRenderer;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<CircleCollider2D>();
        handRenderer = GetComponent<SpriteRenderer>();
    }
    void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<CircleCollider2D>();
    }
    public virtual void Initialize(string name, Team team, float baseHP, float baseSpeed, float baseMass)
    {
        stats.playerName = name;
        stats.team = team;
        stats.hp = baseHP;
        stats.maxHP = baseHP;
        stats.speed = baseSpeed;
        stats.mass = baseMass;
        stats.level = 1;
        stats.xp = 0f;
        UpdateRadius();
    }

    [SerializeField] float hitCooldown = 0.5f;
    float _lastHitTime;

    void ApplyHitTo(Entity other)
    {
        float dmg = 16f * Mathf.Sqrt(stats.mass) / 7f;
        float knock = 1280f * Mathf.Sqrt(stats.mass) / 7f;

        // Zərər vur
        other.TakeDamage(dmg, this);

        // Knockback istiqaməti
        Vector2 dir = (other.transform.position - transform.position).normalized;
        other.rb.AddForce(dir * knock, ForceMode2D.Impulse);

        // Sadə səs və vizual effekt
        AudioManager.PlaySFX("punch");
    }

    public float Radius => 16f + Mathf.Sqrt(stats.mass) * 1.25f;
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!GameManager.Instance.GameRunning) return;
        if (collision.collider == null) return;

        var other = collision.collider.GetComponent<Entity>();
        if (other == null) return;
        if (GameManager.Instance.IsFriendly(this, other)) return;
        if (other == this) return;

        // Damage yalnız əgər zərbə cooldown bitibsə
        if (Time.time < _lastHitTime + hitCooldown) return;
        _lastHitTime = Time.time;

        ApplyHitTo(other);
    }


    void UpdateRadius()
    {
        if (bodyCollider) bodyCollider.radius = Radius * 0.01f; // scale to pixels->meters if sprites are pixels
        transform.localScale = Vector3.one * (Radius / 16f);
    }


    public float RequiredXP() => stats.level * 100f;


    public void AddXP(float amount)
    {
        stats.xp += amount;
        while (stats.xp >= RequiredXP())
        {
            stats.xp -= RequiredXP();
            LevelUp();
        }
    }
    void LevelUp()
    {
        stats.level++;
        stats.maxHP += 20f + stats.level * 5f;
        stats.mass += 10f + stats.level * 2f;
        stats.hp = Mathf.Min(stats.maxHP, stats.hp + 20f);
        UpdateRadius();
        AudioManager.PlaySFX("levelup");
        UIManager.Instance?.OnLevelChanged(this);
        UpdateVisual();
    }
    void UpdateVisual()
    {
        var cfg = GameManager.Instance.Config;
        if (cfg == null || cfg.levelHandSprites == null || cfg.levelHandSprites.Length == 0)
            return;

        int idx = Mathf.Clamp(stats.level - 1, 0, cfg.levelHandSprites.Length - 1);

  

        // Əl sprite (əgər varsa)
        if (handRenderer && cfg.levelHandSprites != null && idx < cfg.levelHandSprites.Length && cfg.levelHandSprites[idx])
            handRenderer.sprite = cfg.levelHandSprites[idx];
    }


    public float PunchDamage()
    {
        float baseDmg = 16f * Mathf.Sqrt(stats.mass) / 7f;
        return boostDamage2xActive ? baseDmg * 2f : baseDmg;
    }


    public float PunchKnockback()
    {
        float baseKb = 1280f * Mathf.Sqrt(stats.mass) / 7f;
        return baseKb;
    }


    public bool CanPunch() => Time.time >= _lastPunchTime + punchCooldown;
    //public void PerformPunch()
    //{
    //    if (!CanPunch()) return;
    //    _lastPunchTime = Time.time;


    //    float range = Radius + punchRangeExtra;
    //    var hits = Physics2D.OverlapCircleAll(transform.position, range, LayerMask.GetMask("Characters"));
    //    foreach (var h in hits)
    //    {
    //        if (h.attachedRigidbody == rb) continue;
    //        var other = h.GetComponent<Entity>();
    //        if (!other) continue;
    //        if (GameManager.Instance.IsFriendly(this, other)) continue;


    //        // apply damage & knockback
    //        other.TakeDamage(PunchDamage(), this);
    //        Vector2 dir = (other.transform.position - transform.position).normalized;
    //        other.rb.AddForce(dir * PunchKnockback(), ForceMode2D.Impulse);
    //    }
    //    AudioManager.PlaySFX("punch");
    //    UIManager.Instance?.Shake();
    //}


    public void TakeDamage(float amount, Entity source)
    {
        stats.hp -= amount;
        if (stats.hp <= 0f) Die(source);
    }
    void Die(Entity killer)
    {
        Debug.Log($"{stats.playerName} died."+ gameObject.name);
        // rewards
        if (killer)
        {
            float xpGain = 50f + killer.stats.level * 20f;
            int coinsGain = Mathf.RoundToInt(5f + killer.stats.level * 1.5f);
            killer.AddXP(xpGain);
            killer.stats.coins += coinsGain;
        }


        // respawn or end
        if (isPlayer)
        {
            GameManager.Instance.OnPlayerDied(this);
        }
        else
        {
            StartCoroutine(RespawnBotRoutine());
        }
    }


    IEnumerator RespawnBotRoutine()
    {
        gameObject.SetActive(false);
        yield return new WaitForSeconds(2f);
        var s = GameManager.Instance.Config;
        Initialize(GenerateName(), stats.team, s.baseHP, s.baseSpeed, s.baseMass);
        transform.position = SpawnManager.Instance.RandomSpawnPosition();
        stats.hp = stats.maxHP;
        gameObject.SetActive(true);
    }


    string GenerateName() => $"Bot_{Random.Range(100, 999)}";
}