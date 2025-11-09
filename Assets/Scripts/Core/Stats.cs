using UnityEngine;


[System.Serializable]
public class Stats
{
    public string playerName = "Player";
    public Team team = Team.None;


    public float hp = 100f;
    public float maxHP = 100f;
    public float xp = 0f;
    public int level = 1;
    public float mass = 20f;
    public float speed = 9f;


    public int coins = 0;


    public bool IsDead => hp <= 0f;
}