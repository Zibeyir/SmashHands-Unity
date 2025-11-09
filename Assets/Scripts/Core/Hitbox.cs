using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;


[RequireComponent(typeof(Collider2D))]
public class Hitbox : MonoBehaviour
{
    public Entity owner;
}