using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Project.Scripts.Patterns;

public class SummaryHandler : Singleton<SummaryHandler>
{
    public float points = 0;
    public float kills = 0;

    private float timeStart = 0;
    public void ResetValues()
    {
        points = 0;
        kills = 0;
        timeStart = 0;
    }

    public void StartTimer()
    {
        timeStart = Time.time;
    }

    public float GetTimeSpent()
    {
        return Time.time - timeStart;
    }
}
