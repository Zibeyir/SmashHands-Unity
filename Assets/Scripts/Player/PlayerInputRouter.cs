using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class PlayerInputRouter : MonoBehaviour
{
    public GameObject joystickObject;

    public Joystick joystick;
    public Button attackButton;
    bool _attackButtonPressed;

    public Vector2 MoveAxis
    {
        get
        {
            Vector2 a = Vector2.zero;
            a.x = Input.GetAxisRaw("Horizontal");
            a.y = Input.GetAxisRaw("Vertical");
            if (joystick) a += joystick.Direction;
            return Vector2.ClampMagnitude(a, 1f);
        }
    }
    void Start()
    {
        // Joystick və attack button setup
        if (attackButton)
            attackButton.onClick.AddListener(() => StartCoroutine(ButtonTap()));

        // ✅ Platform yoxlanışı
#if UNITY_ANDROID || UNITY_IOS
    // Mobil cihazda joystick aktiv olsun
    if (joystickObject)
        joystickObject.SetActive(true);
#else
        // WebGL və ya PC-də joystick gizlət
        if (joystickObject)
            joystickObject.SetActive(false);
#endif
    }


    IEnumerator ButtonTap()
    {
        _attackButtonPressed = true;
        yield return null;
        _attackButtonPressed = false;
    }

    public bool PunchPressed
    {
        get
        {
            return Input.GetMouseButtonDown(0) || _attackButtonPressed;
        }
    }



    public bool SpeedBoostPressed
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return UnityEngine.InputSystem.Mouse.current?.rightButton.wasPressedThisFrame == true;
#else
return Input.GetMouseButtonDown(0);
#endif
        }
    }
}