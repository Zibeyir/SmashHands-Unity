using UnityEngine;


public class HUDBinder : MonoBehaviour
{
    void Update()
    {
        if (GameManager.Instance && GameManager.Instance.player)
        {
            // UI updated inside UIManager.UpdateHUD
        }
    }
}