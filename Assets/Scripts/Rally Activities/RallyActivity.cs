using UnityEngine;

public class RallyActivity : ScriptableObject
{
    [Header("General settings")]
    public string activityName;
    public int activityId;
    public Sprite activityBackground;
    public Sprite[] activityGif;

    [Header("Events")]
    public ActivityClue mainClue;
    public string[] intermissionAudios;
    public string onLocationArrivalAudio;
    public ActivityClue[] secondaryClues;
    public string questionText;
    public Sprite futureQrSprite;
    public string onActivityCompletedAudio;

    [Header("Location settings")]
    public float distanceToReachTarget;
    public double locationLatitude;
    public double locationLongitude;

    [Header("Future data")]
    public Sprite[] futureButtons;
}