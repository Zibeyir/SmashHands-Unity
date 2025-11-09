using UnityEngine;


public class PlayerInputRouter : MonoBehaviour
{
    public Joystick joystick;


    public Vector2 MoveAxis
    {
        get
        {
            Vector2 a = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            a.x = UnityEngine.InputSystem.Keyboard.current?.aKey.isPressed == true ? -1 : 0;
            a.x += UnityEngine.InputSystem.Keyboard.current?.dKey.isPressed == true ? 1 : 0;
            a.y = UnityEngine.InputSystem.Keyboard.current?.sKey.isPressed == true ? -1 : 0;
            a.y += UnityEngine.InputSystem.Keyboard.current?.wKey.isPressed == true ? 1 : 0;
#else
a.x = Input.GetAxisRaw("Horizontal");
a.y = Input.GetAxisRaw("Vertical");
#endif
            if (joystick) a += joystick.Direction;
            return Vector2.ClampMagnitude(a, 1f);
        }
    }


    public bool PunchPressed
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return UnityEngine.InputSystem.Mouse.current?.leftButton.wasPressedThisFrame == true;
#else
return Input.GetMouseButtonDown(0);
#endif
        }
    }


    public bool SpeedBoostPressed
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return UnityEngine.InputSystem.Mouse.current?.rightButton.wasPressedThisFrame == true;
#else
return Input.GetMouseButtonDown(1);
#endif
        }
    }
}