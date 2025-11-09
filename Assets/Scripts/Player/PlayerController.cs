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
            entity.TryAttack();

        // Optional speed boost
        if (input && input.SpeedBoostPressed)
            GameManager.Instance.TryActivateHeldSpeedBoost(entity);
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.GameRunning) return;
        if (entity.IsAttacking) return;
        Vector2 moveAxis = Vector2.zero;

        // 🖱️ Mouse-dan istiqamət
        if (mainCam)
        {
            Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 dir = (mouseWorld - transform.position);
            dir.z = 0;
            if (dir.magnitude > 0.2f)
                moveAxis = dir.normalized;
        }

        // 🎮 Joystick varsa, onu da əlavə et
        if (input && input.joystick)
            moveAxis += input.joystick.Direction;

        moveDir = moveAxis.normalized;

        float speed = entity.stats.speed * (entity.boostSpeed2xActive ? 2f : 1f);
        Vector2 desiredVel = moveDir * speed;
        entity.rb.linearVelocity = Vector2.Lerp(entity.rb.linearVelocity, desiredVel, 0.25f);

        // 🎯 Hərəkət istiqamətinə baxsın
        if (moveDir.sqrMagnitude > 0.05f)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            entity.transform.rotation = Quaternion.Lerp(entity.transform.rotation, Quaternion.Euler(0, 0, angle), 10f * Time.deltaTime);
        }
    }
}
