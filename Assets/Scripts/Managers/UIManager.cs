using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    void Awake() { Instance = this; }


    [Header("Screens")]
    public GameObject startScreen;
    public GameObject hud;
    public GameObject endScreen;


    [Header("Start UI")]
    public TMP_InputField nameInput;
    public TMP_Dropdown mapDropdown; // 0: DM, 1: Team
    public TMP_Dropdown teamDropdown; // only visible if Team
    public TMP_Text totalCoinsText;


    [Header("HUD")]
    public TMP_Text timerText;
    public Slider hpBar;
    public Slider xpBar;
    public TMP_Text levelText;
    public TMP_Text coinsText;
    public TMP_Text leaderboardText;


    [Header("End")]
    public TMP_Text resultText;


    public void InitStart()
    {
        startScreen.SetActive(true);
        hud.SetActive(false);
        endScreen.SetActive(false);


        nameInput.text = SaveSystem.LoadName("Player");
        mapDropdown.value = SaveSystem.LoadMap(0);
        teamDropdown.value = SaveSystem.LoadTeam(0);
        totalCoinsText.text = $"Total Coins: {SaveSystem.LoadTotalCoins()}";
        teamDropdown.gameObject.SetActive(mapDropdown.value == 1);
    }


    public void OnMapChanged(int v)
    {
        teamDropdown.gameObject.SetActive(v == 1);
    }


    public void OnClickStart()
    {
        SaveSystem.SaveName(nameInput.text);
        SaveSystem.SaveMap(mapDropdown.value);
        SaveSystem.SaveTeam(teamDropdown.value);


        var gm = GameManager.Instance;
        gm.BeginMatch((GameMode)mapDropdown.value, (Team)teamDropdown.value, nameInput.text);


        startScreen.SetActive(false);
        hud.SetActive(true);
    }
    public void UpdateHUD(Entity player, float remainingSeconds)
    {
        if (!player) return;
        hpBar.value = player.stats.hp / Mathf.Max(1f, player.stats.maxHP);
        xpBar.value = player.stats.xp / Mathf.Max(1f, player.RequiredXP());
        levelText.text = $"Lv.{player.stats.level}";
        coinsText.text = $"Coins: {player.stats.coins}";


        var t = System.TimeSpan.FromSeconds(Mathf.CeilToInt(remainingSeconds));
        timerText.text = $"{t.Minutes:00}:{t.Seconds:00}";


        // leaderboard
        var list = GameManager.Instance.Leaderboard.Top(8);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        int rank = 1;
        foreach (var e in list)
        {
            bool isSelf = ReferenceEquals(e, player);
            sb.AppendLine($"#{rank} {(isSelf ? ">" : "")} {e.stats.playerName} — {Mathf.RoundToInt(e.stats.mass)}");
            rank++;
        }
        leaderboardText.text = sb.ToString();
    }


    public void OnLevelChanged(Entity e) { /* optional FX */ }


    public void ShowEnd(string summary)
    {
        hud.SetActive(false);
        endScreen.SetActive(true);
        resultText.text = summary;
    }


    public void OnClickPlayAgain() => GameManager.Instance.Restart();


    public void Shake() { /* add lightweight screenshake via animator */ }
}