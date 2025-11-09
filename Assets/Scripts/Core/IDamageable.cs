using static UnityEngine.EventSystems.EventTrigger;

public interface IDamageable
{
    void TakeDamage(float amount, Entity source);
}