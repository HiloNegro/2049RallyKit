using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TweenScale : UITweener<Vector3>
{
    public Transform Target;
    public Ease AnimationEase;

    protected override void DoTween()
    {
        Target.DOKill();
        Target.DOScale(_targetValue, _targetTime).SetEase(AnimationEase).SetDelay(Delay);
    }

    public override void ResetTween()
    {
        Target.DOKill();
        Target.localScale = initialValue;
        _shown = false;
    }

    protected override float ComputeCurrentDistance()
    {
        return Vector3.Distance(_targetValue, Target.localScale);
    }

    protected override float ComputeTotalDistance()
    {
        return Vector3.Distance(finalValue, initialValue);
    }
}