using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LoadingCircle : MonoBehaviour
{
    public RectTransform ProgressImageTransform;
    public float RotateSpeed = 200.0f;
    public float FadeTime = 1.0f;
    public CanvasGroup LoadingCanvasGroup;

    private void FixedUpdate()
    {
        ProgressImageTransform.Rotate(0.0f, 0.0f, RotateSpeed * Time.fixedDeltaTime);
    }

    public void Show(bool show)
    {
        LoadingCanvasGroup.DOKill();
        float target = show ? 1.0f : 0.0f;
        LoadingCanvasGroup.DOFade(target, FadeTime);
    }
}