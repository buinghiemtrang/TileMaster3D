using UnityEngine;
using UnityEngine.EventSystems;

public class EventTriggerListener : EventTrigger
{
	public delegate void VoidDelegate(GameObject go);

	public delegate void VoidDelegatee(PointerEventData eventData);

	public VoidDelegate onClick;

	public VoidDelegate onDown;

	public VoidDelegate onEnter;

	public VoidDelegate onExit;

	public VoidDelegate onUp;

	public VoidDelegate onSelect;

	public VoidDelegate onUpdateSelect;

	public VoidDelegate onBeginDrag;

	public VoidDelegate onEndDrag;

	public VoidDelegatee onDrag;

	public static EventTriggerListener Get(GameObject go)
	{
		EventTriggerListener eventTriggerListener = go.GetComponent<EventTriggerListener>();
		if (eventTriggerListener == null)
		{
			eventTriggerListener = go.AddComponent<EventTriggerListener>();
		}
		return eventTriggerListener;
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (onClick != null)
		{
			AudioMgr.I.PlaySound("button");
			onClick(base.gameObject);
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (onDown != null)
		{
			onDown(base.gameObject);
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (onEnter != null)
		{
			onEnter(base.gameObject);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (onExit != null)
		{
			onExit(base.gameObject);
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		if (onUp != null)
		{
			onUp(base.gameObject);
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		if (onSelect != null)
		{
			onSelect(base.gameObject);
		}
	}

	public override void OnUpdateSelected(BaseEventData eventData)
	{
		if (onUpdateSelect != null)
		{
			onUpdateSelect(base.gameObject);
		}
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		if (onBeginDrag != null)
		{
			onBeginDrag(base.gameObject);
		}
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		if (onEndDrag != null)
		{
			onEndDrag(base.gameObject);
		}
	}

	public override void OnDrag(PointerEventData eventData)
	{
		if (onDrag != null)
		{
			onDrag(eventData);
		}
	}
}
