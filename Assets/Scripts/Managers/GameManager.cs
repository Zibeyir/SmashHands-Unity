using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameConfig Config;


    public LeaderboardManager Leaderboard { get; private set; } = new LeaderboardManager();


    public bool GameRunning => _timer?.Active == true;


    [Header("Runtime")]
    public GameMode mode = GameMode.Deathmatch;
    public Entity player;
    public List<Entity> bots = new List<Entity>();
    public PlayerInputRouter input;

    [Header("Parents")]
    public Transform CollectibleParent;
    public Transform PlayersParents;
    public Transform ParticleParents;

    TimeCounter _timer;
    int _sessionCoinsAward;


    void Awake() { Instance = this; }


    void Start() { UIManager.Instance.InitStart(); }


    public void BeginMatch(GameMode gm, Team selectedTeam, string playerName)
    {
        mode = gm;
        bots.Clear();
        _sessionCoinsAward = 0;

        // 🔹 Hər matça başlayanda bot adlarını shuffle edirik
        BotNameDatabase.ResetForNewMatch();

        // Spawn player
        var pObj = Instantiate(Config.playerPrefab);
        player = pObj.GetComponent<Entity>();
        player.isPlayer = true;
        player.transform.position = SpawnManager.Instance.RandomSpawnPosition();
        player.Initialize(playerName, gm == GameMode.TeamArena ? selectedTeam : Team.None, Config.baseHP, Config.baseSpeed, Config.baseMass);
        Leaderboard.Register(player);

        var camFollow = Camera.main.GetComponent<CameraFollow>();
        if (camFollow) camFollow.target = player.transform;
        var pc = pObj.GetComponent<PlayerController>();
        if (!pc) pc = pObj.AddComponent<PlayerController>();

        // Spawn bots
        for (int i = 0; i < Config.initialBotCount; i++)
        {
            var bObj = Instantiate(Config.botPrefab);
            var e = bObj.GetComponent<Entity>();

            e.transform.position = SpawnManager.Instance.RandomSpawnPosition();

            Team t = Team.None;
            if (gm == GameMode.TeamArena)
                t = (i % 2 == 0) ? Team.Red : Team.Blue;

            // 🔹 Burada random, təkrarsız real ad veririk
            string botName = BotNameDatabase.GetNextName();
            bObj.gameObject.name = botName;

            e.Initialize(botName,
                         t,
                         Config.baseHP,
                         Config.baseSpeed * Random.Range(0.9f, 1.1f),
                         Config.baseMass);

            bots.Add(e);
            Leaderboard.Register(e);
        }

        // Spawn pickups...
        SpawnField(Config.xpOrbPrefab, Config.startXPOrbs);
        SpawnField(Config.coinPrefab, Config.startCoins);
        SpawnField(Config.boostSpeedPrefab, Config.startBoosts / 2);
        SpawnField(Config.boostBeat2xPrefab, Config.startBoosts / 2);

        _timer = new TimeCounter(Config.matchDurationSeconds);
    }

    void SpawnField(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject o = Instantiate(prefab);
            o.transform.position = SpawnManager.Instance.RandomSpawnPosition();
            o.transform.SetParent(CollectibleParent);
        }
    }


    void Update()
    {
        if (_timer == null) return;
        if (_timer.Active)
        {
            _timer.Tick(Time.deltaTime);
            UIManager.Instance.UpdateHUD(player, _timer.Remaining);
            if (!_timer.Active)
            {
                EndMatch();
            }
        }
    }
    public void EndMatch()
    {
        // Compute team or player ranking
        string summary = BuildSummary(true);


        // Save coins
        int total = SaveSystem.LoadTotalCoins();
        total += Mathf.Max(0, _sessionCoinsAward + player.stats.coins);
        SaveSystem.SaveTotalCoins(total);


        UIManager.Instance.ShowEnd(summary);
    }
    public void DieMatch()
    {
        // Compute team or player ranking
        string summary = BuildSummary();


        // Save coins
        int total = SaveSystem.LoadTotalCoins();
        total += Mathf.Max(0, _sessionCoinsAward + player.stats.coins);
        SaveSystem.SaveTotalCoins(total);


        UIManager.Instance.ShowDie(summary);
    }

    string BuildSummary(bool End = false)
    {
        var top = Leaderboard.Top(6);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("Results (by Mass):");
        int rank = 1;
        foreach (var e in top)
        {
            sb.AppendLine($"#{rank} {e.stats.playerName} — {Mathf.RoundToInt(e.stats.mass)}");
            rank++;
        }


        if (End && mode == GameMode.TeamArena)
        {
            float red = 0f, blue = 0f;
            foreach (var e in top)
            {
                if (e.stats.team == Team.Red) red += e.stats.mass;
                else if (e.stats.team == Team.Blue) blue += e.stats.mass;
            }
            sb.AppendLine($"Team Red Total: {Mathf.RoundToInt(red)}");
            sb.AppendLine($"Team Blue Total: {Mathf.RoundToInt(blue)}");
            sb.AppendLine(red > blue ? "Winner: RED" : (blue > red ? "Winner: BLUE" : "Draw"));
        }
        return sb.ToString();
    }


    public void Restart()
    {
        // Hard reset scene (simple way)
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }


    public void OnPlayerDied(Entity e)
    {
        // For v1.0, end immediately; alternative: respawn UI
        DieMatch();
    }

    public void ContinueGame()
    {
        player.gameObject.SetActive(true);
        player.Respawn();
    }
    public bool IsFriendly(Entity a, Entity b)
    {
        if (mode != GameMode.TeamArena) return false;
        return a.stats.team != Team.None && a.stats.team == b.stats.team;
    }


    // Query helpers for AI
    public Transform FindNearestEnemy(Entity seeker, float maxDist)
    {
        Transform best = null; float bestSqr = maxDist * maxDist;
        foreach (var e in bots)
        {
            if (!e || !e.gameObject.activeInHierarchy) continue;
            if(e == seeker) continue;
            if (IsFriendly(seeker, e)) continue;
            float d2 = (e.transform.position - seeker.transform.position).sqrMagnitude;
            if (d2 < bestSqr) { bestSqr = d2; best = e.transform; }
        }
        if (player && !IsFriendly(seeker, player))
        {
            float d2 = (player.transform.position - seeker.transform.position).sqrMagnitude;
            if (d2 < bestSqr) { bestSqr = d2; best = player.transform; }
        }
        return best;
    }
    public Transform FindNearestXP(Vector2 pos, float maxDist) => FindNearestWithTag(pos, maxDist, "XP");
    public Transform FindNearestCoin(Vector2 pos, float maxDist) => FindNearestWithTag(pos, maxDist, "Coin");
    public Transform FindNearestBoost(Vector2 pos, float maxDist) => FindNearestWithTag(pos, maxDist, "Pickup");


    Transform FindNearestWithTag(Vector2 pos, float maxDist, string tag)
    {
        var gos = GameObject.FindGameObjectsWithTag(tag);
        Transform best = null; float bestSqr = maxDist * maxDist;
        foreach (var g in gos)
        {
            float d2 = ((Vector2)g.transform.position - pos).sqrMagnitude;
            if (d2 < bestSqr) { bestSqr = d2; best = g.transform; }
        }
        return best;
    }


    // Boosts
    public System.Collections.IEnumerator ApplyTimedDamageBoost(Entity target, float seconds)
    {

        target.boostDamage2xActive = true;
        yield return new WaitForSeconds(seconds);
        target.boostDamage2xActive = false;
    }

    public void StartDieBot(Entity entity)
    {
        StartCoroutine(RespawnBotRoutine(entity));
    }
    IEnumerator RespawnBotRoutine(Entity entity)
    {
        entity.gameObject.SetActive(false);
        yield return new WaitForSeconds(2f);
        entity.gameObject.SetActive(true);
        Debug.Log("Respawning bot: " + entity.gameObject.name);
        entity.Respawn();
    }
    public IEnumerator ApplyTimedSpeedBoost(Entity target, float seconds)
    {

        target.boostSpeed2xActive = true;
        yield return new WaitForSeconds(seconds);
        target.boostSpeed2xActive = false;
    }
    public void TryActivateHeldSpeedBoost(Entity target)
    {
        // v1.0: placeholder hook (attach inventory later)
    }
}