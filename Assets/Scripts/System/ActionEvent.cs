using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

[AddComponentMenu("")]
public class ActionEvent : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    /// <summary>
    /// 
    /// </summary>
    public ActionTrigger Trigger = ActionTrigger.OnCustom;

    /// <summary>
    /// 
    /// </summary>
    public float Delay = 0.0f;

    protected bool _isHovered = false;

    /// <summary>
    /// This function is called when the trigger condition is met, before the delay.
    /// Not every child of this class should implement the functionality.
    /// </summary>
    public virtual void InstantTriggerAction() { }

    /// <summary>
    /// This function is called when the trigger condition is met, after the delary.
    /// Every child of this class should implement the functionality.
    /// </summary>
    public virtual void DelayedTriggerAction() { }

    private IEnumerator TriggerCoroutineAction()
    {
        InstantTriggerAction();

        if (Delay > 0.0f)
            yield return new WaitForSeconds(Delay);

        DelayedTriggerAction();
    }

    public virtual void TriggerAction()
    {
        StartCoroutine(TriggerCoroutineAction());
    }

    public virtual void Awake()
    {
        if (Trigger == ActionTrigger.OnAwake)
            TriggerAction();
    }

    public virtual void Start()
    {
        if (Trigger == ActionTrigger.OnStart)
            TriggerAction();
    }

    public virtual void OnEnable()
    {
        if (Trigger == ActionTrigger.OnEnable)
            TriggerAction();
    }

    public virtual void OnDisable()
    {
        if (Trigger == ActionTrigger.OnDisable)
            TriggerAction();
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (Trigger == ActionTrigger.OnClick)
            TriggerAction();
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (Trigger == ActionTrigger.OnPoiterDown)
            TriggerAction();
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (Trigger == ActionTrigger.OnPointerUp)
            TriggerAction();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Trigger == ActionTrigger.OnHover)
        {
            _isHovered = true;
            TriggerAction();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Trigger == ActionTrigger.OnHover)
        {
            _isHovered = false;
            TriggerAction();
        }
    }
}