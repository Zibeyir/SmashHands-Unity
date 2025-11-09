using UnityEngine;
using System.Collections;
using System.Linq;


[RequireComponent(typeof(Entity))]
public class BotController : MonoBehaviour
{
    public Entity entity;


    public enum BotState { Wander, Hunt, Collect }
    public BotState _state;
    public Vector2 _target;


    void Reset() => entity = GetComponent<Entity>();


    void OnEnable() => StartCoroutine(ThinkLoop());
    void Start()
    {
        if (!isThinking)
        {
            isThinking = true;
            StartCoroutine(ThinkLoop());
        }
    }
    bool isThinking;


    IEnumerator ThinkLoop()
    {
        while (true)
        {
            if (!GameManager.Instance.GameRunning) { yield return null; continue; }


            Decide();
            yield return new WaitForSeconds(Random.Range(0.5f, 1f));
        }
    }


    void Decide()
    {
        // Priorities: near enemy -> hunt -> punch; else XP -> coin -> boost -> wander
        var enemy = GameManager.Instance.FindNearestEnemy(entity, maxDist: 5f * entity.Radius);
        if (enemy)
        {
            _state = BotState.Hunt;
            _target = enemy.position;
            //if (Vector2.Distance(transform.position, enemy.position) <= entity.Radius + 2f)
            //    entity.PerformPunch();
            return;
        }


        var xp = GameManager.Instance.FindNearestXP(transform.position, 5000f);
        if (xp)
        {
            _state = BotState.Collect;
            _target = xp.position;
            return;
        }
        var coin = GameManager.Instance.FindNearestCoin(transform.position, 5000f);
        if (coin)
        {
            _state = BotState.Collect;
            _target = coin.position;
            return;
        }
        var boost = GameManager.Instance.FindNearestBoost(transform.position, 5000f);
        if (boost)
        {
            _state = BotState.Collect;
            _target = boost.position;
            return;
        }


        _state = BotState.Wander;
        _target = (Vector2)transform.position + Random.insideUnitCircle * 600f;
    }


    void FixedUpdate()
    {
        if (!GameManager.Instance.GameRunning) return;
        Vector2 dir = (_target - (Vector2)transform.position);
        if (dir.sqrMagnitude > 4f)
            dir = dir.normalized;
        float speed = entity.stats.speed * (entity.boostSpeed2xActive ? 2f : 1f);
        Vector2 desiredVel = dir * speed;
        entity.rb.linearVelocity = Vector2.Lerp(entity.rb.linearVelocity, desiredVel, 0.5f);
        //Debug.Log("Bot State: " + gameObject.name + " Target: " + entity.rb.linearVelocity + " dir " + dir + " speed " + speed + " desiredVel " + desiredVel);
    }
}