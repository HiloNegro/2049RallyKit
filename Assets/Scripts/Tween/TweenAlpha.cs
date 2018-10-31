using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TweenAlpha : UITweener<float>
{
    public CanvasGroup Target;

    public override void ResetTween()
    {
        Target.DOKill();
        Target.alpha = initialValue;
        _shown = Target.interactable = Target.blocksRaycasts = Mathf.Approximately(Target.alpha, 0.0f) ? false : true;
    }

    protected override void DoTween()
    {
        Target.DOKill();
        Target.DOFade(_targetValue, _targetTime).SetDelay(Delay).OnComplete(() =>
        {
            Target.interactable = Target.blocksRaycasts = Mathf.Approximately(Target.alpha, 0.0f) ? false : true;
        });
    }

    protected override float ComputeCurrentDistance()
    {
        return Mathf.Abs(_targetValue - Target.alpha);
    }

    protected override float ComputeTotalDistance()
    {
        return Mathf.Abs(finalValue - initialValue);
    }
}