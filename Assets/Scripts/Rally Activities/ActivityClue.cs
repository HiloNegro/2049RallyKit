using System;
using UnityEngine;

[Serializable]
public class ActivityClue
{
    public string clueAudioName;
    public string infoAudioName;

    [TextArea]
    public string clueText;

    public Sprite[] clueSprites;
}