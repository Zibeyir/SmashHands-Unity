using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;


[RequireComponent(typeof(Rigidbody2D))]
public class Entity : MonoBehaviour, IDamageable
{
    [Header("Refs")]
    public Rigidbody2D rb;
    public CircleCollider2D bodyCollider;
    public Hitbox punchHitbox; // child object, disabled by default

    string GenerateName() => $"Bot_{Random.Range(100, 999)}";

    [Header("State")]
    public Stats stats = new Stats();
    public bool isPlayer;

    [Header("Attack Settings")]
    private float attackRange = 3f;
    private float attackCooldown = 1.2f;
    private float attackDashForce = 20;
    private float knockbackForce = 30f;
    private float _lastAttackTime;
    [HideInInspector] public bool IsAttacking=false;

    [Header("World UI")]
    public Slider attackBar;    // Prefab üzərindən bağlanacaq
    private Coroutine _attackRoutine;

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

    public void TryAttack()
    {
        if (Time.time < _lastAttackTime + attackCooldown) return;
        _lastAttackTime = Time.time;
        StartCoroutine(AttackRoutine());
        if (attackBar)
        {
            if (_attackRoutine != null)
                StopCoroutine(_attackRoutine);
            _attackRoutine = StartCoroutine(FillAttackBar());
        }
    }
    IEnumerator FillAttackBar()
    {
        attackBar.gameObject.SetActive(true);
        attackBar.value = 0f;

        float t = 0f;
        while (t < attackCooldown)
        {
            t += Time.deltaTime;
            attackBar.value = Mathf.Clamp01(t / attackCooldown);
            yield return null;
        }

        attackBar.value = 1f;
        yield return new WaitForSeconds(0.1f);
        attackBar.gameObject.SetActive(false);
    }

    IEnumerator AttackRoutine()
    {
        IsAttacking = true;
        // Ən yaxın düşməni tap
        var target = GameManager.Instance.FindNearestEnemy(this, attackRange);

        Vector2 dir = Vector2.zero;

        if (target)
        {
            dir = transform.right; // rotation yönü ilə eyni (sprite up istiqaməti)
        }
        else
        {
            // 🧭 Əgər düşmən yoxdursa, son baxdığı və ya hərəkət etdiyi istiqamətdə hücum etsin
            dir = transform.right; // rotation yönü ilə eyni (sprite up istiqaməti)
        }

        // 🔹 Hər halda bir az qabağa getsin (dash effekti)
        StartCoroutine(DashForward(dir));

        yield return new WaitForSeconds(0.05f); // zərbə anı

        // Əgər target var və məsafə uyğundursa, vur
        if (target)
        {
            bodyCollider.enabled = false;
            bodyCollider.enabled = true;
            //float dist = Vector2.Distance(transform.position, target.position);
            //if (dist <= attackRange)
            //{
            //    var other = target.GetComponent<Entity>();
            //    if (other && !GameManager.Instance.IsFriendly(this, other))
            //        ApplyHit(other, dir);
            //}
        }

        AudioManager.PlaySFX("punch");
        yield return new WaitForSeconds(0.6f); // qısa animasiya vaxtı
        IsAttacking = false;
    }

    IEnumerator DashForward(Vector2 dir)
    {
        float dashTime = 0.7f;         // nə qədər müddət irəli getsin
        float dashSpeed = attackDashForce;  // nə qədər güclü getsin
        float t = 0f;
        rb.AddForce(dir * dashSpeed * 2f, ForceMode2D.Impulse);
        Debug.Log("Attack "+(dir * dashSpeed * 2f));
        while (t < dashTime)
        {
            //rb.linearVelocity = dir * dashSpeed;
            t += Time.deltaTime;
            yield return null;
        }

        //rb.linearVelocity = Vector2.zero; // dayan
    }

    void ApplyHit(Entity other)
    {
        Vector2 dir = (other.gameObject.transform.position - transform.position).normalized;

        // Zərbə gücü səviyyəyə görə
        float dmg = 16f * Mathf.Sqrt(stats.mass) / 7f;
        float knock = knockbackForce * Mathf.Sqrt(stats.mass);

        // HP azaldır
        other.TakeDamage(dmg, this);

        // 🔹 Vurulanı geriyə at
        other.rb.AddForce(dir * knock*2f, ForceMode2D.Impulse);
    }

    public float Radius => 16f + Mathf.Sqrt(stats.mass) * 1.25f;
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!GameManager.Instance.GameRunning) return;
        if (collision.collider == null) return;
        if (!IsAttacking) return;
        var other = collision.collider.GetComponent<Entity>();
        if (other == null) return;

        ApplyHit(other);
        // Damage yalnız əgər zərbə cooldown bitibsə

    }


    void UpdateRadius()
    {
        //if (bodyCollider) bodyCollider.radius = Radius * 0.01f; // scale to pixels->meters if sprites are pixels
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


   


    public void TakeDamage(float amount, Entity source)
    {
        Debug.Log(gameObject.name + " took " + amount + " damage from " + (source ? source.gameObject.name : "unknown"));
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
            gameObject.SetActive(false);

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
        //Initialize(GenerateName(), stats.team, s.baseHP, s.baseSpeed, s.baseMass);
        //transform.position = SpawnManager.Instance.RandomSpawnPosition();
        //stats.hp = stats.maxHP;
        //gameObject.SetActive(true);
    }


}