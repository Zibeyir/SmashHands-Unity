using UnityEngine;
using UnityEngine.EventSystems;


public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public RectTransform baseRect;
    public RectTransform handle;
    public float radius = 64f;


    Vector2 _dir;


    public Vector2 Direction => _dir;


    public void OnPointerDown(PointerEventData e) => OnDrag(e);


    public void OnDrag(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect, e.position, e.pressEventCamera, out var local);
        local = Vector2.ClampMagnitude(local, radius);
        handle.anchoredPosition = local;
        _dir = local / radius;
    }


    public void OnPointerUp(PointerEventData e)
    {
        _dir = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}