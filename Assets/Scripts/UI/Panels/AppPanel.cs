using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TweenAnchorPosition))]
public abstract class AppPanel : MonoBehaviour
{
    public Button PanelButton;

    protected RectTransform _transform;
    protected TweenAnchorPosition _tweenPosition;
    protected TweenAlpha _tweenAlpha;

    private AppPanelManager _panelManager;

    protected virtual void Awake()
    {
        _transform = GetComponent<RectTransform>();
        _tweenPosition = GetComponent<TweenAnchorPosition>();
        _tweenAlpha = GetComponent<TweenAlpha>();
    }

    protected virtual void Start()
    {
        TweenSetup();
    }

    protected virtual void OnEnable()
    {
        PanelButton.onClick.AddListener(ActivatePanel);
    }

    protected virtual void OnDisable()
    {
        PanelButton.onClick.RemoveListener(ActivatePanel);
    }

    public void SetPanelManager(AppPanelManager manager)
    {
        _panelManager = manager;
    }

    public virtual void ActivatePanel()
    {
        _panelManager.ActivatePanel(this);
    }

    public bool IsPanelActive()
    {
        return _tweenAlpha.IsShown();
    }

    public virtual void ShowPanel(bool show)
    {
        show = _tweenPosition.IsShown() && show ? false : show;
        _tweenPosition.StartTween(show);
        _tweenAlpha.StartTween(show);
    }

    public void ShowPanelWithouthChecks(bool show)
    {
        _tweenPosition.StartTween(show);
        _tweenAlpha.StartTween(show);
    }

    protected abstract void TweenSetup();
}