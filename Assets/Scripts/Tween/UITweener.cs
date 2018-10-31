public abstract class UITweener<E> : ActionEvent
{
    public E initialValue;
    public E finalValue;
    public float animationTime;

    protected E _targetValue;
    protected E _targetDistance;
    protected float _targetTime;
    protected bool _shown = false;

    public virtual void StartTween(bool forward)
    {
        _shown = !forward;

        UpdateTargetValue();
        DoTween();

        _shown = forward;
    }

    public override void InstantTriggerAction()
    {
        base.InstantTriggerAction();

        StartTween(!_shown);
    }

    public bool IsShown()
    {
        return _shown;
    }

    protected virtual void UpdateTargetValue()
    {
        _targetValue = _shown ? initialValue : finalValue;

        float distance = ComputeCurrentDistance();
        float totalDistance = ComputeTotalDistance();

        _targetTime = totalDistance != 0.0f ? distance * animationTime / ComputeTotalDistance() : 0.0f;
    }

    public abstract void ResetTween();
    protected abstract void DoTween();
    protected abstract float ComputeCurrentDistance();
    protected abstract float ComputeTotalDistance();
}