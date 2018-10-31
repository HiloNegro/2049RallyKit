using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITweenManager : MonoBehaviour
{
    private TweenAlpha[] _tweenAlpha;
    private TweenAnchorPosition[] _tweenPosition;
    private TweenScale[] _tweenScale;

    public void Awake()
    {
        _tweenAlpha = GetComponents<TweenAlpha>();
        _tweenPosition = GetComponents<TweenAnchorPosition>();
        _tweenScale = GetComponents<TweenScale>();
    }

    public void StartTweens(bool forward, float delay = 0.0f)
    {
        for (int i = 0; i < _tweenAlpha.Length; i++)
        {
            float savedDelay = _tweenAlpha[i].Delay;
            _tweenAlpha[i].Delay = delay;
            _tweenAlpha[i].StartTween(forward);
            _tweenAlpha[i].Delay = savedDelay;
        }

        for (int i = 0; i < _tweenPosition.Length; i++)
        {
            float savedDelay = _tweenPosition[i].Delay;
            _tweenPosition[i].Delay = delay;
            _tweenPosition[i].StartTween(forward);
            _tweenPosition[i].Delay = savedDelay;
        }

        for (int i = 0; i < _tweenScale.Length; i++)
        {
            float savedDelay = _tweenScale[i].Delay;
            _tweenScale[i].Delay = delay;
            _tweenScale[i].StartTween(forward);
            _tweenScale[i].Delay = savedDelay;
        }
    }

    public void ResetTweens()
    {
        for (int i = 0; i < _tweenAlpha.Length; i++)
        {
            _tweenAlpha[i].ResetTween();
        }

        for (int i = 0; i < _tweenPosition.Length; i++)
        {
            _tweenPosition[i].ResetTween();
        }

        for (int i = 0; i < _tweenScale.Length; i++)
        {
            _tweenScale[i].ResetTween();
        }
    }
}