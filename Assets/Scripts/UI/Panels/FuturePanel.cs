
public class FuturePanel : AppPanel
{
    public ActivityScreen activityScreen;

    protected override void TweenSetup()
    {
        _tweenPosition.initialValue = _tweenPosition.finalValue = _transform.anchoredPosition;
        _tweenPosition.ResetTween();

        _tweenAlpha.initialValue = 0.0f;
        _tweenAlpha.finalValue = 1.0f;
        _tweenAlpha.ResetTween();
    }

    public override void ActivatePanel()
    {
        if (activityScreen.CanClickFutureButton())
            base.ActivatePanel();
    }
}