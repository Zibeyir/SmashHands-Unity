using UnityEngine;

public class SimpleSoundSwitcher : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] clips;

    int index = 0;

    void Update()
    {
        // Prev
        if (Input.GetKeyDown(KeyCode.A))
        {
            index--;
            if (index < 0) index = clips.Length - 1;
            audioSource.clip = clips[index];
        }

        // Next
        if (Input.GetKeyDown(KeyCode.D))
        {
            index++;
            if (index >= clips.Length) index = 0;
            audioSource.clip = clips[index];
        }

        // Play on mouse-left
        if (Input.GetMouseButtonDown(0))
        {
            if (audioSource.clip != null)
                audioSource.Play();
        }
    }
}
