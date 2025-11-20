using UnityEngine;

[RequireComponent(typeof(Entity))]
public class PlayerController : MonoBehaviour
{
    public Entity entity;
    public PlayerInputRouter input;

    private Camera mainCam;
    private Vector2 moveDir;

    private void Start()
    {
        entity = GetComponent<Entity>();
        input = GameManager.Instance.input;
        mainCam = Camera.main;
    }

    void Update()
    {
        if (!GameManager.Instance.GameRunning) return;

        // Attack input
        if (input && input.PunchPressed)
        {
            entity.TryAttack();

        }

        // Optional speed boost
        if (input && input.SpeedBoostPressed)
            GameManager.Instance.TryActivateHeldSpeedBoost(entity);
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.GameRunning) return;
        if (entity.IsAttacking) return;

        Vector2 moveAxis = Vector2.zero;

        // mouse ilə yön
        if (mainCam)
        {
            Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 dir = mouseWorld - transform.position;
            dir.z = 0;
            if (dir.magnitude > 0.2f)
                moveAxis = dir.normalized;
        }

        // joystick direction
        if (input && input.joystick)
            moveAxis += input.joystick.Direction;

        moveDir = moveAxis.normalized;

        // ---------------------------
        // 🔥 Speed Boost (Shift + mouse hərəkəti)
        // ---------------------------

        bool boosting = false;

        if (input && input.SpeedBoostPressed && moveDir.sqrMagnitude > 0.1f)
        {
            // XP varsa və 0 deyilsə → boost işləsin
            if (entity.stats.xp > 0f)
            {
                boosting = true;

                // XP yavaş-yavaş azalsın
                bool ok = entity.ConsumeXP(Time.deltaTime * 10f);    // saniyədə -10 XP
                if (!ok)
                    boosting = false; // xp qurtardı → boost dayanır
            }
        }

        // əgər boosting true-dursa speed 2x olur
        float speedMultiplier = boosting ? 2f : 1f;
        //float speed = entity.stats.speed * (entity.boostSpeed2xActive ? 2f : 1f);

        float speed = entity.stats.speed * speedMultiplier * (entity.boostSpeed2xActive ? 2f : 1f);
        Vector2 desiredVel = moveDir * speed;

        entity.rb.linearVelocity =
            Vector2.Lerp(entity.rb.linearVelocity, desiredVel, 0.25f);

        // rotation
        if (moveDir.sqrMagnitude > 0.05f)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            entity.transform.rotation = Quaternion.Lerp(
                entity.transform.rotation,
                Quaternion.Euler(0, 0, angle),
                10f * Time.deltaTime
            );
        }
    }

}
