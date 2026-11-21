using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class MouseTouchListening : MonoBehaviour
{
	[Serializable]
	public class FunTouch : UnityEvent<Vector3>
	{
	}

	[Serializable]
	public class FunMove : UnityEvent<Vector3, Vector3>
	{
	}

	[Serializable]
	public class FunScale : UnityEvent<Vector3, float>
	{
	}

	private readonly Dictionary<int, Vector2> _dictTouch = new Dictionary<int, Vector2>();

	private bool _bMove;

	public FunTouch FunTouchOptDown;

	public FunTouch FunTouchOptUp;

	public FunTouch FunTouchOptClick;

	public FunMove FunTouchOptMove;

	public FunScale FunTouchOptScale;

	private void Awake()
	{
		EventTrigger eventTrigger = base.gameObject.GetComponent<EventTrigger>();
		if (eventTrigger == null)
		{
			eventTrigger = base.gameObject.AddComponent<EventTrigger>();
		}
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerDown;
		entry.callback.AddListener(OnPointerDown);
		EventTrigger.Entry entry2 = new EventTrigger.Entry();
		entry2.eventID = EventTriggerType.PointerUp;
		entry2.callback.AddListener(OnPointerUp);
		EventTrigger.Entry entry3 = new EventTrigger.Entry();
		entry3.eventID = EventTriggerType.Drag;
		entry3.callback.AddListener(OnDrag);
		EventTrigger.Entry entry4 = new EventTrigger.Entry();
		entry4.eventID = EventTriggerType.Cancel;
		entry4.callback.AddListener(OnCancel);
		eventTrigger.triggers.Add(entry);
		eventTrigger.triggers.Add(entry2);
		eventTrigger.triggers.Add(entry3);
		eventTrigger.triggers.Add(entry4);
	}

	private void Update()
	{
		float num = Input.GetAxis("Mouse ScrollWheel");
		if (num != 0f && FunTouchOptScale != null)
		{
			FunTouchOptScale.Invoke(Vector3.zero, num * -500f);
		}
	}

	public void OnPointerDown(BaseEventData baseData)
	{
		PointerEventData pointerEventData = baseData as PointerEventData;
		if (pointerEventData != null)
		{
			_dictTouch[pointerEventData.pointerId] = pointerEventData.position;
			if (_dictTouch.Count == 1)
			{
				_bMove = false;
				if (FunTouchOptDown != null)
				{
					FunTouchOptDown.Invoke(pointerEventData.position);
				}
			}
		}
	}

	public void OnPointerUp(BaseEventData baseData)
	{
		PointerEventData pointerEventData = baseData as PointerEventData;
		if (pointerEventData == null)
		{
			return;
		}
		_dictTouch.Remove(pointerEventData.pointerId);
		if (_dictTouch.Count == 0)
		{
			if (!_bMove && FunTouchOptClick != null)
			{
				FunTouchOptClick.Invoke(pointerEventData.position);
			}
			if (FunTouchOptUp != null)
			{
				FunTouchOptUp.Invoke(pointerEventData.position);
			}
		}
	}

	public void OnDrag(BaseEventData baseData)
	{
		PointerEventData pointerEventData = baseData as PointerEventData;
		if (pointerEventData == null)
		{
			return;
		}
		Vector2 position = pointerEventData.position;
		_bMove = true;
		if (_dictTouch.Count == 1)
		{
			if (FunTouchOptMove != null)
			{
				FunTouchOptMove.Invoke(position, position - _dictTouch[pointerEventData.pointerId]);
			}
			_dictTouch[pointerEventData.pointerId] = position;
		}
		else
		{
			if (_dictTouch.Count != 2)
			{
				return;
			}
			Vector2[] array = _dictTouch.Values.ToArray();
			Vector2 vector = array[0];
			Vector2 vector2 = array[1];
			_dictTouch[pointerEventData.pointerId] = pointerEventData.position;
			Vector2[] array2 = _dictTouch.Values.ToArray();
			float num = Vector2.Distance(array2[0], array2[1]) - Vector2.Distance(vector, vector2);
			if (num != 0f && FunTouchOptScale != null)
			{
				FunTouchOptScale.Invoke(vector + (vector2 - vector) * 0.5f, 0f - num);
			}
		}
	}

	public void OnCancel(BaseEventData baseData)
	{
		OnPointerUp(baseData);
	}
}
