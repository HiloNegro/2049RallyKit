using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AppStage : MonoBehaviour
{
    protected Action onCompletedCallback = null;

    public abstract void Play(ActivityProgress progress, Action callback);
}