using static UnityEngine.EventSystems.EventTrigger;

public interface IBoost
{
    void Apply(Entity target);
}