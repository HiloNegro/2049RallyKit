using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ARPanel : AppPanel
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        VuforiaRuntime.Instance.InitVuforia();

        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
        VuforiaARController.Instance.RegisterOnPauseCallback(OnPaused);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        VuforiaARController.Instance.UnregisterVuforiaStartedCallback(OnVuforiaStarted);
        VuforiaARController.Instance.UnregisterOnPauseCallback(OnPaused);

        if (VuforiaRuntime.Instance != null)
            VuforiaRuntime.Instance.Deinit();
    }

    private void OnVuforiaStarted()
    {
        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
    }

    private void OnPaused(bool paused)
    {
        if (!paused) // resumed
        {
            // Set again autofocus mode when app is resumed
            CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        }
    }

    protected override void TweenSetup()
    {
        float _panelHeight = _transform.sizeDelta.y;
        _tweenPosition.initialValue = new Vector2(_transform.anchoredPosition.x, -_panelHeight);
        _tweenPosition.finalValue = _transform.anchoredPosition;
        _tweenPosition.ResetTween();

        _tweenAlpha.initialValue = 0.0f;
        _tweenAlpha.finalValue = 1.0f;
        _tweenAlpha.ResetTween();
    }

    public override void ShowPanel(bool show)
    {
        show = _tweenPosition.IsShown() && show ? false : show;
        VuforiaBehaviour.Instance.enabled = show;
        VuforiaBehaviour.Instance.GetComponent<DefaultInitializationErrorHandler>().enabled = show;

        if (show)
            OnVuforiaStarted();

        base.ShowPanel(show);
    }
}