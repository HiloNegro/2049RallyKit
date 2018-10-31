using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using TMPro;

public class InstructionsPanel : AppPanel
{
    public TextMeshProUGUI instructionsText;

    protected override void TweenSetup()
    {
        _tweenPosition.initialValue = new Vector2(_transform.anchoredPosition.x, _transform.anchoredPosition.y - 50.0f);
        _tweenPosition.finalValue = _transform.anchoredPosition;
        _tweenPosition.ResetTween();

        _tweenAlpha.initialValue = 0.0f;
        _tweenAlpha.finalValue = 1.0f;
        _tweenAlpha.ResetTween();
    }

    public void SetInstructions(string text)
    {
        instructionsText.DOFade(0.0f, 0.2f).OnComplete(() =>
        {
            instructionsText.text = text;
            instructionsText.DOFade(1.0f, 0.2f);
        });
    }
}