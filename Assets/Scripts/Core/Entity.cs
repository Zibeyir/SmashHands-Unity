using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using TMPro;


[RequireComponent(typeof(Rigidbody2D))]
public class Entity : MonoBehaviour, IDamageable
{
    [Header("Refs")]
    public Rigidbody2D rb;
    public CircleCollider2D bodyCollider;
    public Hitbox punchHitbox; // child object, disabled by default
    public AttackTimeSliderFollow attackTimeSliderFollow;
    public GameObject attackTimeSliderFollowParent;
    string GenerateName() => $"Bot_{Random.Range(100, 999)}";

    [Header("State")]
    public Stats stats = new Stats();
    public bool isPlayer;

    [Header("Attack Settings")]
    private float attackRange = 3f;
    private float attackCooldown = 1.2f;
    private float attackDashForce = 20;
    private float knockbackForce = 15f;
    private float _lastAttackTime;
    public bool IsAttacking = false;
    public bool IsApplyHit = false;

    [Header("World UI")]
    public Slider attackBar;
    public TextMeshProUGUI nameText;    // Prefab üzərindən bağlanacaq
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
        nameText.text = name;
        stats.team = team;
        stats.hp = baseHP;
        stats.maxHP = baseHP;
        stats.speed = baseSpeed;
        stats.mass = baseMass;
        stats.level = 1;
        stats.xp = 0f;
        transform.SetParent(GameManager.Instance.PlayersParents);

        UpdateRadius();
        TeamNameColor();
    }

    private void TeamNameColor()
    {
        switch (stats.team)
        {
            case Team.None:
                nameText.color = Color.white;
                break;
            case Team.Red:
                nameText.color = Color.red;
                break;
            case Team.Blue:
                nameText.color = Color.blue;
                break;

            default:
                nameText.color = Color.white;
                break;
        }
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
        IsApplyHit = true;
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
        AudioManager.Instance.PlaySFX(SoundEnum.punch, transform);
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


        yield return new WaitForSeconds(0.6f); // qısa animasiya vaxtı
        IsAttacking = false;
        IsApplyHit = false;
    }

    IEnumerator DashForward(Vector2 dir)
    {
        float dashTime = 0.7f;         // nə qədər müddət irəli getsin
        float dashSpeed = attackDashForce;  // nə qədər güclü getsin
        float t = 0f;
        rb.AddForce(dir * dashSpeed * 70f, ForceMode2D.Force);
        //Debug.Log("Attack "+(dir * dashSpeed * 2f));
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
        IsApplyHit = false;

        // Zərbə gücü səviyyəyə görə
        float dmg = 16f * Mathf.Sqrt(stats.mass) / 7f;
        float knock = knockbackForce * Mathf.Sqrt(stats.mass);

        // HP azaldır
        other.TakeDamage(dmg, dir * knock, this);

        // 🔹 Vurulanı geriyə at
    }

    public float Radius => 16f + Mathf.Sqrt(stats.mass) * 1.25f;
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!GameManager.Instance.GameRunning) return;
        if (collision.collider == null) return;

        var other = collision.collider.GetComponent<Entity>();
        if (other == null) return;
        //Vector2 dir = (other.gameObject.transform.position - transform.position).normalized;
        //float knock = knockbackForce * Mathf.Sqrt(stats.mass);

        //rb.AddForce(-dir * attackDashForce * 80f, ForceMode2D.Force);

        if (!IsApplyHit) return;

        ApplyHit(other);
        //IsAttacking = false;
        // Damage yalnız əgər zərbə cooldown bitibsə

    }


    void UpdateRadius()
    {
        //if (bodyCollider) bodyCollider.radius = Radius * 0.01f; // scale to pixels->meters if sprites are pixels
        transform.localScale = Vector3.one * (Radius / 16f);
        attackTimeSliderFollow.GetOffset(transform.localScale.x);
        //bodyCollider.radius = transform.localScale.x;
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
        ParticlePool.Play(FXType.LevelUp, transform,true);

        if (isPlayer) AudioManager.Instance.PlaySFX(SoundEnum.levelup, transform);
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





    public void TakeDamage(float amount,Vector2 force, Entity source)
    {
        if (stats.team != Team.None && stats.team == source.stats.team) return; // dostu vurma
        if (IsApplyHit) return;
        rb.AddForce(force, ForceMode2D.Impulse);

        //Debug.Log(gameObject.name + " took " + amount + " damage from " + (source ? source.gameObject.name : "unknown"));
        stats.hp -= amount;
        if (stats.hp <= 0f)
        {
            AudioManager.Instance.PlaySFX(SoundEnum.die, transform);

            Die(source);
        }
        else
        {
            AudioManager.Instance.PlaySFX(SoundEnum.damage, transform);

            ParticlePool.Play(FXType.Damage, transform.position);

        }

    }
    void Die(Entity killer)
    {
        //Debug.Log($"{stats.playerName} died."+ gameObject.name);
        // rewards
        if (killer)
        {
            float xpGain = 50f + killer.stats.level * 20f;
            int coinsGain = Mathf.RoundToInt(5f + killer.stats.level * 1.5f);
            killer.AddXP(xpGain);
            killer.stats.coins += coinsGain;
            if(attackTimeSliderFollowParent) attackTimeSliderFollowParent.SetActive(false);
            ParticlePool.Play(FXType.Die, transform.position);
        }


        // respawn or end
        if (isPlayer)
        {
            GameManager.Instance.OnPlayerDied(this);
            gameObject.SetActive(false);

        }
        else
        {
            GameManager.Instance.StartDieBot(this);

        }
    }
    public bool ConsumeXP(float amount)
    {
        if (stats.xp <= 0f)
        {
            stats.xp = 0f;
            return false; // XP yoxdur → boost dayansın
        }

        stats.xp -= amount;
        if (stats.xp < 0f) stats.xp = 0f;
        return true;
    }


   
    public void Respawn()
    {
        transform.position = SpawnManager.Instance.RandomSpawnPosition();
        stats.hp = stats.maxHP;
        if (attackTimeSliderFollowParent)  attackTimeSliderFollowParent.SetActive(true);
        IsAttacking = false;
        Debug.Log($"{stats.playerName} respawned.");
    }

    public void TakeDamage(float amount, System.Numerics.Vector2 force, Entity source)
    {
        throw new System.NotImplementedException();
    }
}