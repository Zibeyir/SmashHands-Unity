using System.Numerics;
using static UnityEngine.EventSystems.EventTrigger;

public interface IDamageable
{
    void TakeDamage(float amount,Vector2 force, Entity source);
}