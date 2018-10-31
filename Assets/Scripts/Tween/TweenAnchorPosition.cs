using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TweenAnchorPosition : UITweener<Vector2>
{
    public RectTransform Target;

    public override void ResetTween()
    {
        Target.DOKill();
        Target.anchoredPosition = initialValue;
        _shown = false;
    }

    protected override void DoTween()
    {
        Target.DOKill();
        Target.DOAnchorPos(_targetValue, _targetTime).SetDelay(Delay);
    }

    protected override float ComputeCurrentDistance()
    {
        return Vector2.Distance(_targetValue, Target.anchoredPosition);
    }

    protected override float ComputeTotalDistance()
    {
        return Vector2.Distance(finalValue, initialValue);
    }
}