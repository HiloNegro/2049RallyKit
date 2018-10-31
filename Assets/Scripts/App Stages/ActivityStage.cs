using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class ActivityStage : AppStage
{
    [Header("General settings")]
    public RallyActivity activity;
    public RallyManager rallyManager;
    private ActivityProgress activityProgress;
    public CanvasGroup clueLabelGroup;
    private TextMeshProUGUI clueLabel;
    public ActivityScreen activityScreen;
    public Image futureButton;

    [Header("Panels settings")]
    public AppPanelManager panelManager;
    public MapPanel mapPanel;
    public AudioScreen audioScreen;

    [Header("Intermission settings")]
    public int TimeBetweenIntermissionAudios = 20;

    private void Start()
    {
        clueLabel = clueLabelGroup.GetComponent<TextMeshProUGUI>();
    }

    public override void Play(ActivityProgress progress, Action callback)
    {
        activityProgress = progress;
        activityProgress.activityId = activity.activityId;
        onCompletedCallback = callback;

        if (activity.futureButtons != null && activity.futureButtons.Length > 0)
        {
            futureButton.GetComponent<UITweenManager>().ResetTweens();
            futureButton.sprite = activity.futureButtons[0];
            futureButton.SetNativeSize();
        }

        LoadProgress();

        PlayNextSection();
    }

    private void LoadProgress()
    {
        switch (activityProgress.currentSection)
        {
            case 1:
                // Show main clue text
                clueLabelGroup.DOFade(0.0f, 0.2f).OnComplete(() =>
                {
                    clueLabel.text = activity.mainClue.clueText;
                    clueLabelGroup.DOFade(1.0f, 0.2f).SetDelay(0.2f);
                });
                // Set playback button for main clue
                AudioManager.Instance.SetPlayback(activity.mainClue.clueAudioName);
                // Set playback button for all intermission audios already listened
                for (int i = 0; i < activityProgress.currentIntermissionAudio; i++)
                {
                    AudioManager.Instance.SetPlayback(activity.intermissionAudios[i]);
                }
                break;
            case 2:
                AudioManager.Instance.SetPlayback(activity.onLocationArrivalAudio);
                break;
        }
    }

    private void PlayNextSection()
    {
        switch(activityProgress.currentSection)
        {
            case 0:
                StartCoroutine(PlayMainClue());
                break;
            case 1:
                StartCoroutine(WaitForLocationArrival());
                break;
            case 2:
                OnLocationArrival();
                break;
            default:

                mapPanel.StartMainActivity(activity.activityId, false);

                if (onCompletedCallback != null)
                    onCompletedCallback();

                onCompletedCallback = null;
                break;
        }
    }

    public bool CanPlayEvent(int eventId)
    {
        return activityProgress != null && activityProgress.currentSection == 2 && activityScreen.CanPlayEvent(eventId);
    }

    public bool IsCurrentlyPlayingEvent(int eventId)
    {
        return activityProgress != null && activityProgress.currentSection == 2 && activityScreen.IsCurrentlyPlaying(eventId);
    }

    public void SetEventFound(int eventId)
    {
        if (eventId == activityProgress.currentEventId)
        {
            //panelManager.DeactivateAll();

            activityScreen.UnlockEvent(eventId);
        }
    }

    private IEnumerator PlayMainClue()
    {
        AudioObject mainClueAudio = AudioManager.Instance.PlayMusicAndSetPlayback(activity.mainClue.clueAudioName);
        clueLabelGroup.DOFade(0.0f, 0.2f).OnComplete(() =>
        {
            clueLabel.text = activity.mainClue.clueText;
            clueLabelGroup.DOFade(1.0f, 0.2f).SetDelay(0.2f);
        });

        yield return new WaitForSeconds(mainClueAudio.clipLength);

        if (!mapPanel.IsPanelActive())
            mapPanel.ActivatePanel();

        mapPanel.StartMainActivity(activity.activityId, true);

        OnSectionCompleted();
    }

    private IEnumerator WaitForLocationArrival()
    {
        int time = 60 * TimeBetweenIntermissionAudios;
        while (!mapPanel.HasReachedTarget(activity.locationLatitude, activity.locationLongitude, activity.distanceToReachTarget))
        {
            if (activityProgress.currentIntermissionAudio < activity.intermissionAudios.Length && time-- <= 0)
            {
                AudioManager.Instance.PlayMusicAndSetPlayback(activity.intermissionAudios[activityProgress.currentIntermissionAudio++]);
                time = 60 * TimeBetweenIntermissionAudios;

                ProgressTracker.Instance.SaveProgress();
            }

            yield return new WaitForSeconds(1.0f);
        }

        if (Application.isMobilePlatform)
            Handheld.Vibrate();

        if (!mapPanel.IsPanelActive())
            mapPanel.ActivatePanel();

        mapPanel.StartSecondaryActivities(activity.activityId, true);

        AudioManager.Instance.ClearPlaybacks();
        AudioManager.Instance.PlayMusicAndSetPlayback(activity.onLocationArrivalAudio);

        OnSectionCompleted();
    }

    private void OnLocationArrival()
    {
        clueLabelGroup.DOFade(0.0f, 0.2f);
        activityScreen.ActivateScreen(activity, activityProgress, OnActivityScreenCompleted);
    }

    public void OpenCompletedActivityScreen(RallyActivity activity, ActivityProgress progress)
    {
        StopAllCoroutines();
        AudioController.StopMusic(0.1f);
        AudioManager.Instance.StopAllCoroutines();

        if (ProgressTracker.Instance.ProgressState.activitiesCompleted && activityScreen.GetCurrentActivityId() == activity.activityId)
        {
            DeactivateCurrentActivityScreen();
            AudioManager.Instance.ClearPlaybacks();
            rallyManager.ActivitiesCompleted();
            return;
        }

        if (ProgressTracker.Instance.ProgressState.activitiesCompleted)
        {
            audioScreen.DeactivateScreen();
        }

        clueLabelGroup.DOFade(0.0f, 0.2f);
        AudioManager.Instance.ClearPlaybacks();
        AudioManager.Instance.SetPlayback(activity.onLocationArrivalAudio);
        activityScreen.ActivateScreen(activity, progress, null);
    }

    public void OpenCurrentActivityScreen()
    {
        StopAllCoroutines();
        AudioController.StopMusic(0.1f);
        AudioManager.Instance.StopAllCoroutines();
        AudioManager.Instance.ClearPlaybacks();
        LoadProgress();
        activityScreen.ActivateScreen(activity, activityProgress, OnActivityScreenCompleted);
    }

    public void ResumeActivity()
    {
        StopAllCoroutines();
        AudioController.StopMusic(0.1f);
        AudioManager.Instance.StopAllCoroutines();

        DeactivateCurrentActivityScreen();
        AudioManager.Instance.ClearPlaybacks();
        LoadProgress();
        PlayNextSection();
    }

    public void DeactivateCurrentActivityScreen()
    {
        futureButton.GetComponent<UITweenManager>().StartTweens(false);

        activityScreen.DeactivateScreen();
    }

    private void OnActivityScreenCompleted()
    {
        activityScreen.DeactivateScreen();

        OnSectionCompleted();
    }

    private void OnSectionCompleted()
    {
        activityProgress.currentSection++;
        ProgressTracker.Instance.SaveProgress();

        PlayNextSection();
    }
}
