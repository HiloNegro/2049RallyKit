using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PanelTitleBar : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler//, IPointerEnterHandler, IPointerExitHandler
{
    public AppPanel target;
    public float distanceToFadeOut;

    private CanvasGroup _targetCanvasGroup;
    private RectTransform _targetTransform;
    private Vector2 _startPosition;

    private float _lastDragDirection;
    private float _dragAccum;
    private Vector2 _drag = Vector2.zero;

    private void Start()
    {
        _targetCanvasGroup = target.GetComponent<CanvasGroup>();
        _targetTransform = target.GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _startPosition = _targetTransform.anchoredPosition;

        _lastDragDirection = 0;
        _dragAccum = 0;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float draggingValue = eventData.delta.y;
        float draggingDirection = Mathf.Sign(draggingValue);

        if (draggingDirection != _lastDragDirection)
        {
            _lastDragDirection = draggingDirection;
            _dragAccum = 0;
        }

        _dragAccum += draggingValue;
        _drag.y = draggingValue;

        if (_targetTransform.anchoredPosition.y + _drag.y > _startPosition.y) return;

        _targetTransform.anchoredPosition += _drag;

        _targetCanvasGroup.alpha = Mathf.Lerp(1.0f, 0.0f, (_startPosition.y - _targetTransform.anchoredPosition.y) / distanceToFadeOut);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        bool show = _lastDragDirection > 0;

        if (_targetTransform.anchoredPosition.y < _startPosition.y - distanceToFadeOut)
        {
            _targetTransform.anchoredPosition = _startPosition - Vector2.up * distanceToFadeOut;
        }

        target.ShowPanelWithouthChecks(show);
    }
}