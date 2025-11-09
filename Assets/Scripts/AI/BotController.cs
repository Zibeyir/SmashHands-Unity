using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Entity))]
public class BotController : MonoBehaviour
{
    public Entity entity;

    private Vector2 moveDir;
    private Vector2 targetPos;

    private float thinkTimer;
    private float attackTimer;
    private float randomDecisionTime;
    private float currentDecisionDuration;
    private bool isAttacking;

    // Tənzimlənən parametrlər (balans üçün)
    [Header("AI Settings")]
    private float visionRange = 3f;        // düşməni hiss etmə məsafəsi
    private float attackDistance = 2.8f;    // hücum məsafəsi
    private float decisionIntervalMin = 0.4f;
    private float decisionIntervalMax = 1.2f;
    private float randomMoveRadius = 6f;

    void Awake()
    {
        entity = GetComponent<Entity>();
        randomDecisionTime = Random.Range(decisionIntervalMin, decisionIntervalMax);
    }

    void OnEnable()
    {
        StartCoroutine(DecisionLoop());
    }

    IEnumerator DecisionLoop()
    {
        while (true)
        {
            if (!GameManager.Instance.GameRunning)
            {
                yield return null;
                continue;
            }

            DecideBehavior();
            randomDecisionTime = Random.Range(decisionIntervalMin, decisionIntervalMax);
            yield return new WaitForSeconds(randomDecisionTime);
        }
    }

    void DecideBehavior()
    {
        // Ən yaxın düşməni tap
        var enemy = GameManager.Instance.FindNearestEnemy(entity, visionRange * entity.Radius);
        bool doAttackDecision = Random.value > 0.5f; // hər dəfə vurmaya bilər

        if (enemy && Random.value > 0.25f) // 75% ehtimalla düşmənə fokus olur
        {
            float dist = Vector2.Distance(transform.position, enemy.position);
            Vector2 dir = (enemy.position - transform.position).normalized;

            // çox yaxındırsa bir az uzaqlaşsın
            if (dist < attackDistance * 0.6f)
            {
                moveDir = -dir;
                currentDecisionDuration = Random.Range(0.3f, 0.6f);
                return;
            }

            // hücum məsafəsindədirsə və təsadüfi qərar vursa
            if (dist <= attackDistance && doAttackDecision && Time.time > attackTimer)
            {
                entity.TryAttack();
                attackTimer = Time.time + Random.Range(1f, 2f); // hər dəfə vurmur
            }

            // yaxınlaşsın (bəzən az, bəzən çox)
            moveDir = dir;
            currentDecisionDuration = Random.Range(0.5f, 1.2f);
        }
        else
        {
            // heç kim yoxdursa və ya random qərarla başqa yerə getsin
            moveDir = Random.insideUnitCircle.normalized;
            currentDecisionDuration = Random.Range(1f, 2f);
            targetPos = (Vector2)transform.position + moveDir * randomMoveRadius;
        }
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.GameRunning) return;
        if (entity.IsAttacking) return;

        // Hərəkət
        Vector2 velocity = moveDir.normalized * entity.stats.speed * (entity.boostSpeed2xActive ? 2f : 1f);
        entity.rb.linearVelocity = Vector2.Lerp(entity.rb.linearVelocity, velocity, 0.3f);

        // Yaxın botlardan uzaqlaş
        AvoidOtherBots();

        //// Qabağa baxan istiqamətə dönsün (2D rotation)
        //if (moveDir.sqrMagnitude > 0.1f)
        //{
        //    float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg - 90f;
        //    entity.transform.rotation = Quaternion.Lerp(entity.transform.rotation, Quaternion.Euler(0, 0, angle), 10f * Time.deltaTime);
        //}// Botun getdiyi istiqamətə baxsın
        if (entity.rb.linearVelocity.sqrMagnitude > 0.05f)
        {
            Vector2 dir = entity.rb.linearVelocity.normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            entity.transform.rotation = Quaternion.Lerp(entity.transform.rotation, Quaternion.Euler(0, 0, angle), 10f * Time.deltaTime);
        }

    }

    void AvoidOtherBots()
    {
        Collider2D[] near = Physics2D.OverlapCircleAll(transform.position, 1.2f);
        Vector2 separation = Vector2.zero;
        foreach (var n in near)
        {
            if (n == null || n.transform == transform) continue;
            var other = n.GetComponent<Entity>();
            if (other && other != entity)
            {
                Vector2 away = (Vector2)transform.position - (Vector2)other.transform.position;
                if (away.sqrMagnitude < 1.44f) // məsafə 1.2f radiusda
                    separation += away.normalized / (away.magnitude + 0.1f);
            }
        }

        if (separation != Vector2.zero)
            entity.rb.AddForce(separation.normalized * entity.stats.speed * 0.5f);
    }
}
