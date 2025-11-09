using UnityEngine;


public static class SaveSystem
{
    const string KEY_NAME = "playerName";
    const string KEY_COINS = "totalCoins";
    const string KEY_MAP = "lastSelectedMap";
    const string KEY_TEAM = "lastSelectedTeam";


    public static void SaveName(string name) => PlayerPrefs.SetString(KEY_NAME, name);
    public static string LoadName(string def = "Player") => PlayerPrefs.GetString(KEY_NAME, def);


    public static void SaveTotalCoins(int total) => PlayerPrefs.SetInt(KEY_COINS, total);
    public static int LoadTotalCoins(int def = 0) => PlayerPrefs.GetInt(KEY_COINS, def);


    public static void SaveMap(int mapIdx) => PlayerPrefs.SetInt(KEY_MAP, mapIdx);
    public static int LoadMap(int def = 0) => PlayerPrefs.GetInt(KEY_MAP, def);


    public static void SaveTeam(int team) => PlayerPrefs.SetInt(KEY_TEAM, team);
    public static int LoadTeam(int def = 0) => PlayerPrefs.GetInt(KEY_TEAM, def);
}