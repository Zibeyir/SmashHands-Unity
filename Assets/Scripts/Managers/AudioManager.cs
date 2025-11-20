using UnityEngine;


public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public AudioSource sfx;
    public AudioSource music;

    public float SoundDistance = 10f;
    public Transform PlayerT;   
    public AudioClip punch;
    public AudioClip levelup;
    public AudioClip damage;
    public AudioClip die;
    public AudioClip coin;
    public AudioClip speed;
    public AudioClip x2punch;


    void Awake() { Instance = this; }


    public void PlaySFX(SoundEnum key, Transform distanceT)
    {
        if (!Instance||(Vector3.Distance(PlayerT.position,distanceT.position)> SoundDistance)) return;
        switch (key)
        {
            case SoundEnum.punch: sfx.PlayOneShot(punch); break;
            case SoundEnum.damage: sfx.PlayOneShot(damage); break;
            case SoundEnum.die: sfx.PlayOneShot(die); break;
            case SoundEnum.coin: sfx.PlayOneShot(coin); break;
            case SoundEnum.speed: sfx.PlayOneShot(speed); break;
            case SoundEnum.x2punch: sfx.PlayOneShot(x2punch); break;

            case SoundEnum.levelup: sfx.PlayOneShot(levelup); break;

        }
    }
}
public enum SoundEnum
{
    punch,
    levelup,
    damage,
    die,
    coin,
    speed,
    x2punch

}