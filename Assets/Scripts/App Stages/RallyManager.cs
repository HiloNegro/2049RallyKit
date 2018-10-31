using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RallyManager : MonoBehaviour
{
    public AppStage introductionStage;
    public AppStage[] rallyActivities;

    public AudioScreen audioScreen;
    public CompletedScreen waitScreen;
    public CompletedScreen completedScreen;
    public AnimatedImage completedAnimation;

    public string audioCongratulations;
    public string audioRecordAudios;
    public string audioWriteStory;

    private static DateTime unlockDate = new DateTime(2018, 5, 25, 16, 0, 0);
    //private static DateTime unlockDate = new DateTime(2018, 5, 15, 22, 08, 0);
    private static DateTime debugDate = new DateTime(2000, 1, 1, 12, 0, 0);

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.3f);

        Debug.Log(unlockDate);

        LoadNextSection();
    }

    public void OnTutorialCompleted()
    {
        ProgressTracker.Instance.ProgressState.tutorialPlayed = true;
        OnSectionCompleted();
    }

    public void OnSectionCompleted()
    {
        Debug.Log("Section completed");
        ProgressTracker.Instance.ProgressState.OnActivityCompleted();
        ProgressTracker.Instance.SaveProgress();

        AudioManager.Instance.ClearPlaybacks();

        LoadNextSection();
    }

    private IEnumerator WaitForUnlockDate()
    {
        waitScreen.ActivateScreen();

        float touchTime = 0.0f;
        int touchCount = 0;
        while (DateTime.Now < unlockDate)
        {
            while (Input.GetKey(KeyCode.Space) || Input.touchCount == 3)
            {
                touchTime += Time.deltaTime;

                if (touchTime >= 5.0f)
                {
                    touchCount++;
                    touchTime = 0.0f;
                    Handheld.Vibrate();

                    if (touchCount >= 2)
                    {
                        break;
                    }
                }

                yield return null;
            }

            if (touchCount >= 2)
            {
                unlockDate = debugDate;
                break;
            }

            touchCount = 0;
            touchTime = 0.0f;

            yield return new WaitForSeconds(3.0f);
        }

        waitScreen.DeactivateScreen();

        LoadNextSection();
    }

    public void OnActivitiesCompleted()
    {
        ProgressTracker.Instance.ProgressState.activitiesCompleted = true;
        ProgressTracker.Instance.ProgressState.OnActivityCompleted();
        ProgressTracker.Instance.SaveProgress();

        AudioManager.Instance.ClearPlaybacks();

        AudioController.PlayMusic(audioRecordAudios);

        ActivitiesCompleted();
    }

    public void OnRallyCompleted()
    {
        ProgressTracker.Instance.ProgressState.rallyCompleted = true;
        ProgressTracker.Instance.SaveProgress();

        AudioManager.Instance.ClearPlaybacks();

        StartCoroutine(OnRallyCompletedCoroutine());
    }

    private IEnumerator OnRallyCompletedCoroutine()
    {
        audioScreen.DeactivateScreen();

        completedScreen.ActivateScreen();
        completedAnimation.Play();

        AudioObject audioObject = AudioManager.Instance.PlayMusicAndSetPlayback(audioCongratulations);

        yield return new WaitForSeconds(audioObject.clipLength + 1.0f);

        AudioManager.Instance.PlayMusicAndSetPlayback(audioWriteStory);

        yield return new WaitForSeconds(audioObject.clipLength + 1.0f);
    }

    public bool CanDetectMarkers()
    {
        // TODO: Re-think this method.
        return true;
    }

    public bool CanPlayEventForActivity(ActivityStage activity)
    {
        int currentActivity = ProgressTracker.Instance.ProgressState.GetCurrentActivityIndex();
        return currentActivity >= 0 && currentActivity < rallyActivities.Length && rallyActivities[currentActivity] == activity;
    }

    private void LoadNextSection()
    {
        if (!ProgressTracker.Instance.ProgressState.tutorialPlayed)
        {
            introductionStage.Play(null, OnTutorialCompleted);
            return;
        }

        if (DateTime.Now < unlockDate)
        {
            StartCoroutine(WaitForUnlockDate());
            return;
        }

        if (ProgressTracker.Instance.ProgressState.rallyCompleted)
        {
            RallyCompleted();
        }
        else if (ProgressTracker.Instance.ProgressState.activitiesCompleted)
        {
            ActivitiesCompleted();
        }
        else
        {
            int index = ProgressTracker.Instance.ProgressState.GetCurrentActivityIndex();
            if (index == -1)
            {
                OnActivitiesCompleted();
            }
            else if (index >= 0 && index < rallyActivities.Length)
            {
                ActivityProgress progress = ProgressTracker.Instance.ProgressState.GetCurrentActivityProgress();
                rallyActivities[index].Play(progress, OnSectionCompleted);
            }
        }
    }

    public void ActivitiesCompleted()
    {
        AudioManager.Instance.SetPlayback(audioRecordAudios);

        audioScreen.ActivateScreen(ProgressTracker.Instance.ProgressState.GetAllActivityProgress(), OnRallyCompleted);
    }

    private void RallyCompleted()
    {
        AudioManager.Instance.SetPlayback(audioCongratulations);
        AudioManager.Instance.SetPlayback(audioWriteStory);

        completedScreen.ActivateScreen();
        completedAnimation.Play();
    }
}