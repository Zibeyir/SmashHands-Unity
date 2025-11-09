using UnityEngine;


[RequireComponent(typeof(Entity))]
public class PlayerController : MonoBehaviour
{
    public Entity entity;
    public PlayerInputRouter input;


    void Reset()
    {
        entity = GetComponent<Entity>();
    }


    void Update()
    {
        if (!GameManager.Instance.GameRunning) return;


        // Punch
        //if (input && input.PunchPressed)
        //    entity.PerformPunch();


        // Manual speed boost (if any temporary system attached)
        if (input && input.SpeedBoostPressed)
            GameManager.Instance.TryActivateHeldSpeedBoost(entity);
    }


    void FixedUpdate()
    {
        if (!GameManager.Instance.GameRunning) return;
        if (!input) return;


        Vector2 move = input.MoveAxis;
        float speed = entity.stats.speed * (entity.boostSpeed2xActive ? 2f : 1f);
        Vector2 desiredVel = move * speed;
        Vector2 vel = Vector2.Lerp(entity.rb.linearVelocity, desiredVel, 0.25f);
        entity.rb.linearVelocity = vel;
    }
}