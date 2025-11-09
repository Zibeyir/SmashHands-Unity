using UnityEngine;


public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public AudioSource sfx;
    public AudioSource music;


    public AudioClip punch;
    public AudioClip levelup;


    void Awake() { Instance = this; }


    public static void PlaySFX(string key)
    {
        if (!Instance) return;
        switch (key)
        {
            case "punch": Instance.sfx.PlayOneShot(Instance.punch); break;
            case "levelup": Instance.sfx.PlayOneShot(Instance.levelup); break;
        }
    }
}