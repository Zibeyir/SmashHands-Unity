using UnityEngine;


public class TimeCounter
{
    public float Remaining { get; private set; }
    public bool Active { get; private set; }


    public TimeCounter(float seconds) { Remaining = seconds; Active = true; }


    public void Tick(float dt)
    {
        if (!Active) return;
        Remaining -= dt;
        if (Remaining <= 0f) { Remaining = 0f; Active = false; }
    }
}