using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using DG.Tweening;

public class MapLocation : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public RallyActivity activity;
    public ActivityStage activityStage;
    public CanvasGroup selectedLocationImage;
    public Animator selectedLocationAnimator;

    [Header("Events")]
    public CanvasGroup[] activityEvents;
    public PlayableDirector activitiesTimeline;

    private void Start()
    {
        selectedLocationImage.alpha = 0.0f;
        selectedLocationImage.transform.localScale = Vector2.one * 0.2f;

        for (int i = 0; i < activityEvents.Length; i++)
        {
            activityEvents[i].alpha = 0.0f;
            activityEvents[i].transform.localScale = Vector2.one;
        }

        ActivityProgress activityProgress = ProgressTracker.Instance.ProgressState.GetProgressForActivityId(activity.activityId);
        ActivityProgress currentActivityProgress = ProgressTracker.Instance.ProgressState.GetCurrentActivityProgress();
        ActivateLocation(currentActivityProgress != null && activityProgress != null && activityProgress.activityId == currentActivityProgress.activityId && activityProgress.currentSection >= 1);
        ActivateLocationEvents((activityProgress != null && activityProgress.audioRecorded) || (currentActivityProgress != null && activityProgress != null && activityProgress.currentSection >= 2));
    }

    public void ActivateLocation(bool selected)
    {
        selectedLocationAnimator.SetBool("Selected", selected);
    }

    public void ActivateLocationEvents(bool activate)
    {
        if (!activate)
        {
            activitiesTimeline.Stop();
            activitiesTimeline.time = activitiesTimeline.initialTime;
            activitiesTimeline.Evaluate();
        }

        if (activate)
        {
            if (activitiesTimeline.state != PlayState.Playing)
                activitiesTimeline.Stop();

            activitiesTimeline.Play();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ActivityProgress activityProgress = ProgressTracker.Instance.ProgressState.GetProgressForActivityId(activity.activityId);
        ActivityProgress currentActivityProgress = ProgressTracker.Instance.ProgressState.GetCurrentActivityProgress();

        //ActivateLocation(currentActivityProgress != null && activityProgress != null && activityProgress.activityId == currentActivityProgress.activityId && activityProgress.currentSection >= 1);
        //ActivateLocationEvents(currentActivityProgress != null && activityProgress != null && activityProgress.currentSection >= 2);

        if (activityProgress == null) return;

        if (activityProgress.activityCompleted)
        {
            activityStage.OpenCompletedActivityScreen(activity, activityProgress);
        }
        else if (activityProgress.activityId == currentActivityProgress.activityId && activityProgress.currentSection == 2)
        {
            activityStage.OpenCurrentActivityScreen();
        }
        else if (activityProgress.activityId == currentActivityProgress.activityId)
        {
            activityStage.ResumeActivity();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }
}