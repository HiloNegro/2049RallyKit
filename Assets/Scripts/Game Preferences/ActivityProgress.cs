using UnityEngine;
using System.Collections;

[System.Serializable]
public class ActivityProgress
{
    public int activityId = -1;
    public int currentEventId = 0;
    public int currentSection = 0;
    public int currentIntermissionAudio = 0;

    public bool audioRecorded = false;
    public string audioName = "";
    public string audioUrl = "";

    public bool activityCompleted = false;

    /// <summary>
    /// Constructor used by the serializer
    /// </summary>
    public ActivityProgress()
    {

    }
}