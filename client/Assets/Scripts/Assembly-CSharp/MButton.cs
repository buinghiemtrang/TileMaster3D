using System;
using System.Runtime.CompilerServices;
using System.Threading;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class MButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, ICancelHandler
{
	[Serializable]
	public class VoidDelegate : UnityEvent<MButton>
	{
	}

	[CompilerGenerated]
	private Action<bool> m_eventInteractable;

	[CompilerGenerated]
	private Action<bool> m_eventInteractableState;

	[SerializeField]
	private bool m_penetrate;

	[SerializeField]
	private bool m_interactable = true;

	[SerializeField]
	private bool m_touchAni;

	[SerializeField]
	private float m_TADelay = 0.1f;

	[SerializeField]
	private float m_TAScale = 0.1f;

	public bool bPlaySound = true;

	[SerializeField]
	private string sSoundName = "button";

	public int nTag;

	public string sMsg = "";

	public object param;

	private Graphic m_graphic;

	private Collider2D m_collider2d;

	private Tween colorTw;

	private Tween scaleDownTw;

	private Tween scaleUpTw;

	private Vector3 originalScale = Vector3.one;

	public VoidDelegate onClick;

	public VoidDelegate onDown;

	public VoidDelegate onUp;

	public VoidDelegate onUnClick;

	private IPointerDownHandler anotherPointDown;

	public bool interactable
	{
		get
		{
			return m_interactable;
		}
		set
		{
			SetInteractable(value);
			m_eventInteractableState?.Invoke(value);
		}
	}

	public bool touchani => m_touchAni;

	private event Action<bool> eventInteractable
	{
		[CompilerGenerated]
		add
		{
			Action<bool> action = m_eventInteractable;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref m_eventInteractable, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<bool> action = m_eventInteractable;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref m_eventInteractable, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<bool> eventInteractableState
	{
		[CompilerGenerated]
		add
		{
			Action<bool> action = m_eventInteractableState;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref m_eventInteractableState, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<bool> action = m_eventInteractableState;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref m_eventInteractableState, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	private void Awake()
	{
		m_graphic = GetComponent<Graphic>();
		m_collider2d = GetComponent<Collider2D>();
		FindScrollRect(base.gameObject);
		InteractableSetBase[] componentsInChildren = GetComponentsInChildren<InteractableSetBase>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			eventInteractable += componentsInChildren[i].Interactable;
		}
		originalScale = base.transform.localScale;
	}

	private void Start()
	{
	}

	public void SetInteractable(bool value)
	{
		m_interactable = value;
		if (m_collider2d != null)
		{
			m_collider2d.enabled = value;
		}
		else if (m_graphic != null && onUnClick == null)
		{
			m_graphic.raycastTarget = value;
		}
		m_eventInteractable?.Invoke(value);
	}

	private void FindScrollRect(GameObject obj)
	{
		if (!(obj.transform.parent == null) && m_penetrate)
		{
			GameObject gameObject = obj.transform.parent.gameObject;
			anotherPointDown = gameObject.GetComponent<IPointerDownHandler>();
			if (anotherPointDown == null)
			{
				FindScrollRect(gameObject);
			}
		}
	}

	public void SetGraphicColor(Color color, float f_delay = 0f)
	{
		if (!(m_graphic == null))
		{
			if (f_delay != 0f)
			{
				colorTw = m_graphic.DOColor(color, f_delay).SetId("MButton_Color");
			}
			else
			{
				m_graphic.color = color;
			}
		}
	}

	public Graphic GetGraphic()
	{
		return m_graphic;
	}

	public bool ClickSelf()
	{
		if (onClick == null || !m_interactable)
		{
			return false;
		}
		DispatcherMgr.Emit("TutorialsTouchButton", this);
		if (bPlaySound)
		{
			AudioMgr.I.PlaySound(sSoundName);
		}
		onClick.Invoke(this);
		return true;
	}

	public void UnClickSelf()
	{
		if (onUnClick != null && !m_interactable)
		{
			DispatcherMgr.Emit("TutorialsTouchButton", this);
			if (bPlaySound)
			{
				AudioMgr.I.PlaySound(sSoundName);
			}
			onUnClick.Invoke(this);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!ClickSelf())
		{
			UnClickSelf();
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (m_touchAni && (m_interactable || onUnClick != null))
		{
			scaleDownTw = base.transform.DOBlendableScaleBy(Utils.Vec3(m_TAScale), m_TADelay).SetId("MButton_Down_Scale");
		}
		if (m_interactable && onDown != null)
		{
			onDown.Invoke(this);
		}
		if (anotherPointDown != null)
		{
			anotherPointDown.OnPointerDown(eventData);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (m_touchAni && (m_interactable || onUnClick != null))
		{
			scaleUpTw = base.transform.DOBlendableScaleBy(Utils.Vec3(0f - m_TAScale), m_TADelay).SetId("MButton_Up_Scale");
		}
		if (m_interactable && onUp != null)
		{
			onUp.Invoke(this);
		}
	}

	public void OnCancel(BaseEventData eventData)
	{
		OnPointerUp((PointerEventData)eventData);
	}

	private void OnDestroy()
	{
		if (colorTw != null)
		{
			colorTw.Kill();
			colorTw = null;
		}
		if (scaleDownTw != null)
		{
			scaleDownTw.Kill();
			scaleDownTw = null;
		}
		if (scaleUpTw != null)
		{
			scaleUpTw.Kill();
			scaleUpTw = null;
		}
	}
}
