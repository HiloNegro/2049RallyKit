using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CompletedScreen : MonoBehaviour
{
    public CanvasGroup screenGroup;

    private void Start()
    {
        DeactivateScreen();
    }

    private void ShowPanel(CanvasGroup group, bool show)
    {
        float target = show ? 1.0f : 0.0f;
        group.DOFade(target, 0.2f);
        group.interactable = show;
        group.blocksRaycasts = show;
    }

    public void ActivateScreen()
    {
        LoadProgress();
        ShowPanel(screenGroup, true);
    }

    public void DeactivateScreen()
    {
        ShowPanel(screenGroup, false);
    }

    private void LoadProgress()
    {
    }
}