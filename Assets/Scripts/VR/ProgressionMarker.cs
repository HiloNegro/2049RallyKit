using UnityEngine;
using Vuforia;
using System.Collections;
using System;

public class ProgressionMarker : MonoBehaviour, ITrackableEventHandler
{
    /// <summary>
    /// 
    /// </summary>
    public RallyManager activityManager;

    /// <summary>
    /// 
    /// </summary>
    public ActivityStage[] activity;

    /// <summary>
    /// 
    /// </summary>
    public int eventId;

    /// <summary>
    /// 
    /// </summary>
    public GameObject child;

    /// <summary>
    /// 
    /// </summary>
    private TrackableBehaviour _trackableBehaviour;

    void Start()
    {
        _trackableBehaviour = GetComponent<TrackableBehaviour>();

        if (_trackableBehaviour)
            _trackableBehaviour.RegisterTrackableEventHandler(this);
    }

    public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
    {
        int currentActivity = ProgressTracker.Instance.ProgressState.GetCurrentActivityIndex();

        if (currentActivity == -1) return;

        if (!activityManager.CanDetectMarkers() || !activityManager.CanPlayEventForActivity(activity[currentActivity])) return;

        if ((newStatus == TrackableBehaviour.Status.DETECTED || newStatus == TrackableBehaviour.Status.TRACKED || newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED) && 
            (activity[currentActivity].CanPlayEvent(eventId) || activity[currentActivity].IsCurrentlyPlayingEvent(eventId)))
        {
            Debug.Log("Trackable " + _trackableBehaviour.TrackableName + " found");
            // Mark the event as found

                activity[currentActivity].SetEventFound(eventId);
                if (child != null)
                    child.SetActive(true);
        }
        else
        {
            Debug.Log("Trackable " + _trackableBehaviour.TrackableName + " lost");

            if (child != null)
                child.SetActive(false);
        }
    }
}