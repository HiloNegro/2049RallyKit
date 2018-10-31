using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayRecordedAudio : Selectable, IPointerClickHandler
{
    public string recordedAudio;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable) return;

        if (AudioController.IsPlaying(recordedAudio))
            AudioController.StopMusic();
        else
            AudioController.PlayMusic(recordedAudio);
    }
}