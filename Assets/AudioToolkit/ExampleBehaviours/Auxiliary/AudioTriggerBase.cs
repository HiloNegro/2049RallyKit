using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public abstract class AudioTriggerBase : MonoBehaviour, IPointerClickHandler
{
    public enum EventType
    {
        Start,
        Awake,
        OnDestroy,
        OnCollisionEnter,
        OnCollisionExit,
        OnEnable,
        OnDisable,
        OnClick
    }

    public EventType triggerEvent = EventType.Start;

    protected virtual void Awake()
    {
        _CheckEvent( EventType.Awake );
    }

    protected virtual void Start()
    {
        _CheckEvent( EventType.Start );
    }

    protected virtual void OnDestroy()
    {
        if ( triggerEvent == EventType.OnDestroy && AudioController.DoesInstanceExist() ) // otherwise errors can occur if the entire scene gets destroyed
        {
            _CheckEvent( EventType.OnDestroy );
        }
    }

    protected virtual void OnCollisionEnter()
    {
        _CheckEvent( EventType.OnCollisionEnter );
    }

    protected virtual void OnCollisionExit()
    {
        _CheckEvent( EventType.OnCollisionExit );
    }

    protected virtual void OnEnable()
    {
        _CheckEvent( EventType.OnEnable );
    }

    protected virtual void OnDisable()
    {
        _CheckEvent( EventType.OnDisable );
    }

    abstract protected void _OnEventTriggered();

    protected virtual void _CheckEvent( EventType eventType )
    {
        if ( triggerEvent == eventType )
        {
            _OnEventTriggered();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _CheckEvent(EventType.OnClick);
    }
}
