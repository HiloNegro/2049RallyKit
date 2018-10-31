using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System.IO;

public class ActivityScreen : MonoBehaviour
{
    [Header("General settings")]
    public CanvasGroup screenGroup;
    public TextMeshProUGUI activityNameLabel;
    public TextMeshProUGUI activityClueLabel;
    public Image activityBackground;
    public Button nextButton;
    public Button closeButton;
    public AnimatedImage backgroundImage;
    public Image futureButton;
    public Image futureQrCode;
    public RallyManager rallyManager;
    public AppPanelManager panelManager;

    [Header("Event settings")]
    public Button[] eventStartButtons;
    public Image[] eventImages;
    public Image unlockedEventImage;

    [Header("Record audio panel")]
    public CanvasGroup recordAudioPanel;
    public Button recordAudioButton;
    public Button playAudioButton;
    public TextMeshProUGUI questionLabel;
    private bool isRecording = false;

    [Header("Animation Timelines")]
    public PlayableDirector showInformationTimeline;
    public PlayableDirector hideInformationTimeline;

    private RallyActivity activity;
    private ActivityProgress activityProgress;
    private Action onCompleteCallback;
    private bool canPlayEvent = false;
    private AudioClip currentRecording;

    private Coroutine recordCoroutine = null;
    private Coroutine playCoroutine = null;

    private bool _canClickFutureButton = false;

    private int _currentActivityId = -1;

    private void Start()
    {
        DeactivateScreen();
    }

    private void OnEnable()
    {
        for (int i = 0; i < eventStartButtons.Length; i++)
        {
            int index = i;
            eventStartButtons[i].onClick.AddListener(() => PlaySecondaryClue(index));
        }

        recordAudioButton.onClick.AddListener(OnRecordAudio);
        playAudioButton.onClick.AddListener(OnPlayAudio);

        nextButton.onClick.AddListener(OnSectionComplete);
    }

    private void OnDisable()
    {
        for (int i = 0; i < eventStartButtons.Length; i++)
        {
            int index = i;
            eventStartButtons[i].onClick.RemoveListener(() => PlaySecondaryClue(index));
        }

        recordAudioButton.onClick.RemoveListener(OnRecordAudio);
        playAudioButton.onClick.RemoveListener(OnPlayAudio);
        nextButton.onClick.RemoveListener(OnSectionComplete);
    }

    public int GetCurrentActivityId()
    {
        return _currentActivityId;
    }

    public void ActivateScreen(RallyActivity activity, ActivityProgress progress, Action callback)
    {
        this.activity = activity;
        this.activityProgress = progress;
        this.onCompleteCallback = callback;

        _currentActivityId = activity.activityId;

        backgroundImage.Stop();
        if (activity.activityGif != null && activity.activityGif.Length > 0)
        {
            backgroundImage.SpriteNames = activity.activityGif;
            backgroundImage.Frames = null;
            backgroundImage.ResetAnimation();
            backgroundImage.Play();
        }

        LoadProgress();

        ShowPanel(screenGroup, true);
    }

    public void DeactivateScreen()
    {
        activity = null;
        activityProgress = null;
        onCompleteCallback = null;

        if (onCompleteCallback == null)
        {
            futureButton.GetComponent<UITweenManager>().StartTweens(false);
            closeButton.onClick.RemoveListener(OpenAudioScreen);
        }

        _canClickFutureButton = false;
        _currentActivityId = -1;

        ShowPanel(screenGroup, false);
    }

    private void OpenAudioScreen()
    {
        closeButton.onClick.RemoveListener(OpenAudioScreen);

        activity = null;
        activityProgress = null;
        onCompleteCallback = null;

        _canClickFutureButton = false;
        _currentActivityId = -1;

        futureButton.GetComponent<UITweenManager>().StartTweens(false);

        AudioManager.Instance.ClearPlaybacks();

        ShowPanel(screenGroup, false);

        rallyManager.ActivitiesCompleted();
    }

    private void LoadProgress()
    {
        activityNameLabel.text = activity.activityName;
        //activityBackground.sprite = activity.activityBackground;

        for (int i = 0; i < eventImages.Length; i++)
        {
            bool unlocked = i < activityProgress.currentEventId;

            eventImages[i].sprite = activity.secondaryClues[i].clueSprites[0];
            //eventImages[i].raycastTarget = unlocked;
            eventImages[i].color = unlocked ? Color.white : Color.clear;

            if (unlocked)
                AudioManager.Instance.SetPlayback(activity.secondaryClues[i].infoAudioName);
        }

        if (activity.futureButtons != null && activity.futureButtons.Length > activityProgress.currentEventId)
        {
            futureButton.sprite = activity.futureButtons[activityProgress.currentEventId];
            futureButton.SetNativeSize();
            futureButton.GetComponent<UITweenManager>().StartTweens(true, 0.1f);
        }

        futureQrCode.sprite = activity.futureQrSprite;
        futureQrCode.preserveAspect = true;

        _canClickFutureButton = activity.secondaryClues.Length == activityProgress.currentEventId;

        questionLabel.text = activity.questionText;
        if (activity.secondaryClues.Length == activityProgress.currentEventId)
            AudioManager.Instance.SetPlayback(activity.onActivityCompletedAudio);
        ShowPanel(recordAudioPanel, activity.secondaryClues.Length == activityProgress.currentEventId);
        playAudioButton.gameObject.SetActive(activityProgress.audioRecorded);

        nextButton.GetComponent<UITweenManager>().StartTweens(activityProgress.audioRecorded && !activityProgress.activityCompleted);

        closeButton.GetComponent<UITweenManager>().StartTweens(ProgressTracker.Instance.ProgressState.activitiesCompleted);
        if (ProgressTracker.Instance.ProgressState.activitiesCompleted)
        {
            closeButton.onClick.AddListener(OpenAudioScreen);
        }
    }

    private void PlaySecondaryClue(int eventId)
    {
        if (eventId > activityProgress.currentEventId) return;

        if (eventId < activityProgress.currentEventId)
        {
            if (AudioController.IsPlaying(activity.secondaryClues[eventId].infoAudioName))
                AudioController.StopMusic();
            else
                AudioController.PlayMusic(activity.secondaryClues[eventId].infoAudioName);

            return;
        }

        AudioController.PlayMusic(activity.secondaryClues[activityProgress.currentEventId].clueAudioName);
        activityClueLabel.DOFade(0.0f, 0.2f).OnComplete(() =>
        {
            activityClueLabel.text = activity.secondaryClues[activityProgress.currentEventId].clueText;
            activityClueLabel.DOFade(1.0f, 0.2f);
        });

        canPlayEvent = true;
    }

    public bool CanPlayEvent(int eventId)
    {
        return eventId == activityProgress.currentEventId && canPlayEvent;
    }

    public bool IsCurrentlyPlaying(int eventId)
    {
        AnimatedImage animImage = unlockedEventImage.GetComponent<AnimatedImage>();

        return !canPlayEvent && eventId + 1 == activityProgress.currentEventId && animImage.IsPlaying();
    }

    public void UnlockEvent(int eventId)
    {
        canPlayEvent = false;
        activityProgress.currentEventId++;
        ProgressTracker.Instance.SaveProgress();

        StartCoroutine(UnlockEventCoroutine(eventId));
    }

    private IEnumerator UnlockEventCoroutine(int eventId)
    {
        eventImages[eventId].sprite = activity.secondaryClues[eventId].clueSprites[0];
        unlockedEventImage.sprite = activity.secondaryClues[eventId].clueSprites[0];
        AnimatedImage animImage = unlockedEventImage.GetComponent<AnimatedImage>();
        animImage.SpriteNames = activity.secondaryClues[eventId].clueSprites;
        animImage.Frames = null;
        animImage.ResetAnimation();
        animImage.Loop = true;
        animImage.Play();

        AudioObject eventAudio = AudioManager.Instance.PlayMusicAndSetPlayback(activity.secondaryClues[eventId].infoAudioName);
        showInformationTimeline.Stop();
        showInformationTimeline.Play();
        activityClueLabel.DOFade(0.0f, 0.2f);

        yield return new WaitForSeconds(Mathf.Max(eventAudio.clipLength, (float)showInformationTimeline.duration));

        hideInformationTimeline.Stop();
        hideInformationTimeline.Play();

        yield return new WaitForSeconds((float)hideInformationTimeline.duration);

        if (activity.futureButtons != null && activity.futureButtons.Length > eventId + 1)
        {
            futureButton.sprite = activity.futureButtons[eventId + 1];
            futureButton.SetNativeSize();
        }

        eventImages[eventId].DOColor(Color.white, 0.3f);
        //eventImages[eventId].raycastTarget = true;

        yield return new WaitForSeconds(0.5f);

        animImage.Stop();
        panelManager.DeactivateAll();

        if (activity.secondaryClues.Length == activityProgress.currentEventId)
        {
            AudioManager.Instance.PlayMusicAndSetPlayback(activity.onActivityCompletedAudio);
            _canClickFutureButton = true;
            ShowPanel(recordAudioPanel, true);
        }
    }

    public bool CanClickFutureButton()
    {
        return _canClickFutureButton;
    }

    private void ShowPanel(CanvasGroup group, bool show)
    {
        float target = show ? 1.0f : 0.0f;
        group.DOFade(target, 0.2f);
        group.interactable = show;
        group.blocksRaycasts = show;
    }

    private void OnRecordAudio()
    {
        if (isRecording)
        {
            if (recordCoroutine != null)
            {
                StopCoroutine(recordCoroutine);
                recordCoroutine = null;
            }

            recordAudioButton.GetComponent<Animator>().SetBool("Recording", false);
            recordAudioButton.interactable = false;
            if (!RARE.Instance.StopMicRecording(activity.activityId.ToString(), OnClipRecorded))
                OnClipRecordingError();
            isRecording = false;
        }
        else
        {
            playAudioButton.interactable = false;
            nextButton.interactable = false;
            recordAudioButton.GetComponent<Animator>().SetBool("Recording", true);
            isRecording = true;
            RARE.Instance.StartMicRecording(65);
            recordCoroutine = StartCoroutine(StoprRecording());
        }
    }

    private IEnumerator StoprRecording()
    {
        yield return new WaitForSeconds(60.0f);

        if (isRecording)
            OnRecordAudio();
    }

    private void OnClipRecorded(AudioClip clip, string clipName)
    {
        if (clip == null && clipName == null)
        {
            OnClipRecordingError();
            return;
        }

        playAudioButton.gameObject.SetActive(true);
        activityProgress.audioRecorded = true;
        activityProgress.audioName = clipName;
        currentRecording = clip;

        playAudioButton.interactable = true;
        nextButton.interactable = true;
        recordAudioButton.interactable = true;

        if (onCompleteCallback != null)
            nextButton.GetComponent<UITweenManager>().StartTweens(true);

        string wavPath = Application.persistentDataPath + "/" + clipName + ".wav";
        string oggPath = Application.persistentDataPath + "/" + clipName + ".ogg";

        RARE.Instance.EncodeAndSendEmail(clipName, wavPath, oggPath, clip.frequency, clip.samples, activityProgress, OnActivityCompletedCallback);
    }

    private void OnClipRecordingError()
    {
        Debug.Log("Error recording from the microphone");
        ProgressTracker.Instance.Logout();
    }

    private void OnPlayAudio()
    {
        if (activityProgress.audioRecorded)
        {
            if (AudioController.IsPlaying(activityProgress.audioName))
            {
                AudioController.StopMusic();
                recordAudioButton.interactable = true;
                playAudioButton.GetComponent<Animator>().SetBool("Playing", false);

                if (playCoroutine != null)
                    StopCoroutine(playCoroutine);
                playCoroutine = null;
                return;
            }

            if (currentRecording != null)
            {
                Debug.Log("Loading the audio recorded during this sesion");
                PlayClip(currentRecording, activityProgress.audioName);
            }
            else
            {
                Debug.Log("Trying to load the audio saved locally");
                RARE.Instance.GetAudioClipFromFile(activityProgress.audioName, PlayClip);
            }
        }
    }

    private IEnumerator StopPlayback(float length)
    {
        yield return new WaitForSeconds(length);

        AudioController.StopMusic();

        recordAudioButton.interactable = true;
        playAudioButton.GetComponent<Animator>().SetBool("Playing", false);
        playCoroutine = null;
    }

    private void PlayClip(AudioClip clip, string clipName)
    {
        if (clip == null && clipName == null)
        {
            Debug.Log("No audio saved locally. Retrieving audio from the server...");
            //StartCoroutine(LoadAudioFromServer());
            return;
        }

        if (!string.IsNullOrEmpty(clipName))
        {
            currentRecording = clip;
            if (AudioController.GetAudioItem(clipName) == null)
            {
                AudioCategory category = AudioController.GetCategory("ActivityEvents");
                AudioController.AddToCategory(category, clip, clipName);
            }
            else
            {
                AudioItem audio = AudioController.GetAudioItem(clipName);
                if (audio.subItems[0].Clip != clip)
                    audio.subItems[0].Clip = clip;
            }

            AudioObject clipObject = AudioController.PlayMusic(clipName);

            recordAudioButton.interactable = false;
            playAudioButton.GetComponent<Animator>().SetBool("Playing", true);
            playCoroutine = StartCoroutine(StopPlayback(clipObject.clipLength));
        }
    }

    private IEnumerator LoadAudioFromServer()
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(activityProgress.audioUrl, AudioType.OGGVORBIS))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("Could not find audio clip on the server.");
                activityProgress.audioRecorded = false;
                activityProgress.audioName = "";
                activityProgress.audioUrl = "";
                ProgressTracker.Instance.SaveProgress();

                ProgressTracker.Instance.Logout();
            }
            else
            {
                PlayClip(DownloadHandlerAudioClip.GetContent(www), activityProgress.audioName);
            }
        }
    }

    private void OnActivityCompletedCallback()
    {
    }

    public void OnSectionComplete()
    {
        nextButton.GetComponent<UITweenManager>().StartTweens(false);

        _canClickFutureButton = false;
        activityProgress.activityCompleted = true;
        _currentActivityId = -1;

        if (onCompleteCallback != null)
        {
            onCompleteCallback();
        }

        onCompleteCallback = null;
    }
}