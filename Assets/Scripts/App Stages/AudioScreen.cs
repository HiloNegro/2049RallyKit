using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AudioScreen : MonoBehaviour
{
    public CanvasGroup screenGroup;
    public Button[] playButtons;
    public Button[] recordButtons;
    public Button completeButton;
    public CanvasGroup menuPanel;
    public GameObject waitUntilReadyLabel;

    private Action onCompleteCallback;
    private ActivityProgress[] activityProgress;
    private bool[] isRecording;

    private int lastRecordedIndex = -1;
    private int lastPlayedIndex = -1;

    private bool _recording = false;

    private Coroutine recordCoroutine = null;
    private Coroutine playCoroutine = null;

    private void Start()
    {
        DeactivateScreen();
    }

    private void OnEnable()
    {
        completeButton.onClick.AddListener(OnScreenCompleted);
    }

    private void OnDisable()
    {
        completeButton.onClick.RemoveListener(OnScreenCompleted);
        DeactivateScreen();
    }

    private void ShowPanel(CanvasGroup group, bool show)
    {
        float target = show ? 1.0f : 0.0f;
        group.DOFade(target, 0.2f);
        group.interactable = show;
        group.blocksRaycasts = show;
    }

    public void ActivateScreen(ActivityProgress[] activityProgress, Action callback)
    {
        this.activityProgress = activityProgress;
        this.onCompleteCallback = callback;
        LoadProgress();
        ShowPanel(screenGroup, true);
    }

    public void DeactivateScreen()
    {
        onCompleteCallback = null;
        ShowPanel(screenGroup, false);

        for (int i = 0; i < playButtons.Length; i++)
        {
            playButtons[i].onClick.RemoveAllListeners();
        }

        for (int i = 0; i < recordButtons.Length; i++)
        {
            recordButtons[i].onClick.RemoveAllListeners();
        }
    }

    private void LoadProgress()
    {
        for (int i = 0; i < playButtons.Length; i++)
        {
            int index = i;
            playButtons[i].onClick.AddListener(() => OnPlayAudio(index));
        }

        for (int i = 0; i < recordButtons.Length; i++)
        {
            int index = i;
            recordButtons[i].onClick.AddListener(() => OnRecordAudio(index));
        }

        isRecording = new bool[activityProgress.Length];
        for (int i = 0; i < isRecording.Length; i++)
        {
            isRecording[i] = false;
        }
    }

    private void OnRecordAudio(int index)
    {
        if (isRecording[index])
        {
            if (recordCoroutine != null)
            {
                StopCoroutine(recordCoroutine);
                recordCoroutine = null;
            }

            recordButtons[index].GetComponent<Animator>().SetBool("Recording", false);
            recordButtons[index].interactable = false;
            lastRecordedIndex = index;

            if (!RARE.Instance.StopMicRecording(activityProgress[index].activityId.ToString(), OnClipRecorded))
                OnClipRecordingError();
            isRecording[index] = false;
        }
        else
        {
            //completeButton.interactable = false;
            _recording = true;

            for (int i = 0; i < playButtons.Length; i++)
            {
                playButtons[i].interactable = false;
            }

            for (int i = 0; i < recordButtons.Length; i++)
            {
                recordButtons[i].interactable = false;
            }

            recordButtons[index].interactable = true;
            recordButtons[index].GetComponent<Animator>().SetBool("Recording", true);

            isRecording[index] = true;
            RARE.Instance.StartMicRecording(65);
            recordCoroutine = StartCoroutine(StoprRecording(index));
        }
    }

    private IEnumerator StoprRecording(int index)
    {
        yield return new WaitForSeconds(60.0f);

        if (isRecording[index])
            OnRecordAudio(index);
    }

    private void OnClipRecorded(AudioClip clip, string clipName)
    {
        if (lastRecordedIndex == -1) return;

        if (clip == null && clipName == null)
        {
            OnClipRecordingError();
            return;
        }

        activityProgress[lastRecordedIndex].audioRecorded = true;
        activityProgress[lastRecordedIndex].audioName = clipName;

        for (int i = 0; i < playButtons.Length; i++)
        {
            playButtons[i].interactable = true;
        }

        for (int i = 0; i < recordButtons.Length; i++)
        {
            recordButtons[i].interactable = true;
        }

        string wavPath = Application.persistentDataPath + "/" + clipName + ".wav";
        string oggPath = Application.persistentDataPath + "/" + clipName + ".ogg";

        RARE.Instance.EncodeAndSendEmail(clipName, wavPath, oggPath, clip.frequency, clip.samples, activityProgress[lastRecordedIndex], OnActivityCompletedCallback);
    }

    private void OnClipRecordingError()
    {
        Debug.Log("Error recording from the microphone");

        lastRecordedIndex = -1;
        _recording = false;
        ProgressTracker.Instance.Logout();
    }

    private void OnPlayAudio(int index)
    {
        if (activityProgress[index].audioRecorded)
        {
            if (AudioController.IsPlaying(activityProgress[index].audioName))
            {
                AudioController.StopMusic();

                for (int i = 0; i < recordButtons.Length; i++)
                {
                    recordButtons[i].interactable = true;
                }

                for (int i = 0; i < playButtons.Length; i++)
                {
                    playButtons[i].interactable = true;
                }

                playButtons[index].GetComponent<Animator>().SetBool("Playing", false);

                if (playCoroutine != null)
                    StopCoroutine(playCoroutine);
                playCoroutine = null;
                lastPlayedIndex = -1;
                return;
            }

            for (int i = 0; i < recordButtons.Length; i++)
            {
                recordButtons[i].interactable = false;
            }

            for (int i = 0; i < playButtons.Length; i++)
            {
                playButtons[i].interactable = false;
            }
            playButtons[index].interactable = true;
            playButtons[index].GetComponent<Animator>().SetBool("Playing", true);

            lastPlayedIndex = index;
            RARE.Instance.GetAudioClipFromFile(activityProgress[index].audioName, PlayClip);
        }
    }

    private IEnumerator StopPlayback(float length, int index)
    {
        yield return new WaitForSeconds(length);

        AudioController.StopMusic();

        for (int i = 0; i < recordButtons.Length; i++)
        {
            recordButtons[i].interactable = true;
        }

        for (int i = 0; i < playButtons.Length; i++)
        {
            playButtons[i].interactable = true;
        }

        playButtons[index].GetComponent<Animator>().SetBool("Playing", false);

        playCoroutine = null;

        lastPlayedIndex = -1;
    }

    private void PlayClip(AudioClip clip, string clipName)
    {
        if (lastPlayedIndex == -1 || string.IsNullOrEmpty(clipName))
        {
            for (int i = 0; i < recordButtons.Length; i++)
            {
                recordButtons[i].interactable = true;
            }

            for (int i = 0; i < playButtons.Length; i++)
            {
                playButtons[i].interactable = true;
                playButtons[i].GetComponent<Animator>().SetBool("Playing", false);
            }

            if (playCoroutine != null)
                StopCoroutine(playCoroutine);
            playCoroutine = null;

            return;
        }

        if (clip == null || clipName == null)
        {
            Debug.Log("No audio saved locally. Retrieving audio from the server...");
            //StartCoroutine(LoadAudioFromServer());
            return;
        }

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
        playCoroutine = StartCoroutine(StopPlayback(clipObject.clipLength, lastPlayedIndex));
    }

    private IEnumerator LoadAudioFromServer()
    {
        if (lastPlayedIndex == -1) yield break;

        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(activityProgress[lastPlayedIndex].audioUrl, AudioType.WAV))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError)
            {
                Debug.Log("Could not find audio clip on the server.");
                activityProgress[lastPlayedIndex].audioRecorded = false;
                activityProgress[lastPlayedIndex].audioName = "";
                activityProgress[lastPlayedIndex].audioUrl = "";
                ProgressTracker.Instance.SaveProgress();
                lastPlayedIndex = -1;

                ProgressTracker.Instance.Logout();
            }
            else
            {
                PlayClip(UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www), activityProgress[lastRecordedIndex].audioName);
            }
        }
    }

    private void OnActivityCompletedCallback()
    {
        //completeButton.interactable = true;
        _recording = false;
        lastRecordedIndex = -1;
    }

    private void OnScreenCompleted()
    {
        completeButton.interactable = false;

        StartCoroutine(OnScreenCompletedCoroutine());
    }

    private IEnumerator OnScreenCompletedCoroutine()
    {
        menuPanel.interactable = false;

        for (int i = 0; i < recordButtons.Length; i++)
        {
            recordButtons[i].interactable = false;
        }

        for (int i = 0; i < playButtons.Length; i++)
        {
            playButtons[i].interactable = false;
        }

        if (_recording)
        {
            waitUntilReadyLabel.SetActive(true);
        }

        while (_recording)
        {
            yield return null;
        }

        waitUntilReadyLabel.SetActive(false);

        if (onCompleteCallback != null)
        {
            onCompleteCallback();
        }

        onCompleteCallback = null;
    }
}