using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ParticlePool : MonoBehaviour
{
    public static ParticlePool Instance;

    [System.Serializable]
    public class FXEntry
    {
        public FXType type;
        public GameObject prefab;
    }

    [Header("Assign particle prefabs")]
    public FXEntry[] fxPrefabs;

    private Dictionary<FXType, GameObject> prefabMap = new Dictionary<FXType, GameObject>();
    private Dictionary<FXType, List<ParticleSystem>> pools = new Dictionary<FXType, List<ParticleSystem>>();

    public int initialCount = 3;

    private Camera mainCam;

    void Awake()
    {
        Instance = this;
        mainCam = Camera.main;

        // Prefab xəritəsi
        foreach (var e in fxPrefabs)
        {
            if (!prefabMap.ContainsKey(e.type))
                prefabMap.Add(e.type, e.prefab);
        }

        // Hər effekt üçün pool yarat
       
    }
    private void Start()
    {
        foreach (var kv in prefabMap)
            CreatePool(kv.Key);
    }
    // Pool yaratma
    void CreatePool(FXType type)
    {
        pools.Add(type, new List<ParticleSystem>());

        for (int i = 0; i < initialCount; i++)
        {
            ParticleSystem ps = InstantiateFX(type);
            ps.gameObject.SetActive(false);
            pools[type].Add(ps);
        }
    }

    // Instantiate helper
    ParticleSystem InstantiateFX(FXType type)
    {
        GameObject prefab = prefabMap[type];
        GameObject obj = Instantiate(prefab, transform);
        obj.transform.SetParent(GameManager.Instance.ParticleParents);

        return obj.GetComponent<ParticleSystem>();
    }

    // ===========================
    // STATIC CALLS
    // ===========================
    public static void Play(FXType type, Transform t, bool follow = false)
    {
        if (Instance == null) return;
        Instance.Spawn(type, t.position, t, follow);
    }

    public static void Play(FXType type, Vector3 worldPos, bool follow = false)
    {
        if (Instance == null) return;
        Instance.Spawn(type, worldPos, null, follow);
    }

    // ===========================
    // CORE SPAWN LOGIC
    // ===========================
    void Spawn(FXType type, Vector3 pos, Transform attachTarget, bool follow)
    {
        if (!prefabMap.ContainsKey(type)) return;

        // Kamera görmürsə effekt çalışmasın
        if (!IsVisible(pos)) return;

        ParticleSystem ps = GetAvailableFX(type);
        GameObject go = ps.gameObject;

        // Follow aktivdirsə → attach et
        if (follow && attachTarget != null)
        {
            go.transform.SetParent(attachTarget);
            go.transform.localPosition = Vector3.zero;
            Debug.Log("Attached FX to target "+type);
        }
        else
        {
            go.transform.SetParent(GameManager.Instance.ParticleParents);
            go.transform.position = pos;
        }

        go.SetActive(true);
        ps.Play();

        StartCoroutine(DisableFX(ps, ps.main.duration));
    }

    // Pool-dan boş obyekt tap
    ParticleSystem GetAvailableFX(FXType type)
    {
        List<ParticleSystem> list = pools[type];

        foreach (var fx in list)
        {
            if (!fx.gameObject.activeSelf)
                return fx;
        }

        // Hamısı doludursa → yenisini yarat
        ParticleSystem newFx = InstantiateFX(type);
        newFx.gameObject.SetActive(false);
        list.Add(newFx);
        return newFx;
    }

    // Effekt bitəndə geri qaytar
    IEnumerator DisableFX(ParticleSystem ps, float duration)
    {
        yield return new WaitForSeconds(duration);

        ps.Stop();
        ps.gameObject.SetActive(false);

        // Follow olunmuşdusa detach et
        //ps.transform.SetParent(transform);
    }

    // Kamera görürmü?
    bool IsVisible(Vector3 pos)
    {
        if (!mainCam) return true;

        var v = mainCam.WorldToViewportPoint(pos);
        return v.z > 0 && v.x >= 0 && v.x <= 1 && v.y >= 0 && v.y <= 1;
    }
}

public enum FXType
{
    None,
    Hit,
    LevelUp,
    Damage,
    Die
}
