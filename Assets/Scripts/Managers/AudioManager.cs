using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : Singleton<AudioManager>
{
    public GameObject audioPrefab;

    private List<PlayRecordedAudio> activeAudioButtons = new List<PlayRecordedAudio>();
    private List<PlayRecordedAudio> inactiveAudioButtons = new List<PlayRecordedAudio>();

    public AudioObject PlayMusicAndSetPlayback(string audioId)
    {
        AudioObject audioObject = AudioController.PlayMusic(audioId);

        StartCoroutine(SetPlaybackAfterDelay(audioId, audioObject.clipLength));

        return audioObject;
    }

    public void SetPlayback(string audioId)
    {
        StartCoroutine(SetPlaybackAfterDelay(audioId, 0.0f));
    }

    public void ClearPlaybacks()
    {
        while (activeAudioButtons.Count > 0)
        {
            PlayRecordedAudio recordedAudio = activeAudioButtons[0];
            recordedAudio.gameObject.SetActive(false);
            inactiveAudioButtons.Add(recordedAudio);
            activeAudioButtons.RemoveAt(0);
        }
    }

    private void ActivateAll(bool activate)
    {
        for (int i = 0; i < activeAudioButtons.Count; i++)
        {
            activeAudioButtons[i].interactable = activate;
        }
    }

    private IEnumerator SetPlaybackAfterDelay(string audioId, float delay)
    {
        ActivateAll(false);

        if (delay > 0.0f)
        {
            yield return new WaitForSeconds(delay);
        }

        ActivateAll(true);

        if (inactiveAudioButtons.Count == 0)
        {

            GameObject audioGO = Instantiate(audioPrefab, Vector3.zero, Quaternion.identity);
            audioGO.name = audioId;
            audioGO.transform.SetParent(transform, false);
            audioGO.transform.SetAsLastSibling();

            PlayRecordedAudio recordedAudio = audioGO.GetComponent<PlayRecordedAudio>();
            recordedAudio.recordedAudio = audioId;

            activeAudioButtons.Add(recordedAudio);
        }
        else
        {
            PlayRecordedAudio recordedAudio = inactiveAudioButtons[0];
            recordedAudio.recordedAudio = audioId;
            inactiveAudioButtons.RemoveAt(0);
            activeAudioButtons.Add(recordedAudio);
            recordedAudio.gameObject.SetActive(true);
            recordedAudio.gameObject.transform.SetAsLastSibling();
        }
    }
}