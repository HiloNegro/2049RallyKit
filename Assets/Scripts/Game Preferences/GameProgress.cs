using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class where the progress of the game is recorded.
/// </summary>
[Serializable]
public class GameProgress
{
    private int activityCount = 3;

    public string username = "";
    public string password = "";

    public string entryId = "";
    public string sessionToken = "";

    public bool tutorialPlayed = false;
    public bool rallyCompleted = false;
    public bool activitiesCompleted = false;

    public List<int> activityOrder;
    public List<ActivityProgress> activityProgress;
    public int currentActivity = -1;

    public DateTime saveTime;

    public GameProgress()
    {
    }

    public void LoadDefaultValues()
    {
        activityOrder = new List<int>();
        for (int i = 0; i < activityCount; i++)
            activityOrder.Add(i);
        activityOrder.Shuffle();

        activityProgress = new List<ActivityProgress>();
        for (int i = 0; i < activityCount; i++)
            activityProgress.Add(new ActivityProgress());
    }

    public int GetCurrentActivityIndex()
    {
        if (currentActivity < 0 || currentActivity >= activityOrder.Count)
            return -1;

        return activityOrder[currentActivity];
    }

    public ActivityProgress[] GetAllActivityProgress()
    {
        return activityProgress.ToArray();
    }

    public ActivityProgress GetCurrentActivityProgress()
    {
        if (currentActivity < 0 || currentActivity >= activityProgress.Count) return null;

        return activityProgress[activityOrder[currentActivity]];
    }

    public ActivityProgress GetProgressForActivityId(int id)
    {
        for (int i = 0; i < activityProgress.Count; i++)
        {
            if (activityProgress[i].activityId == id)
            {
                return activityProgress[i];
            }
        }

        return null;
    }

    public void OnActivityCompleted()
    {
        currentActivity++;
    }
}