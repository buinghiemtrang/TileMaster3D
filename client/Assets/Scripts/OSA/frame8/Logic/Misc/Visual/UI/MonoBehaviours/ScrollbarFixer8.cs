using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace frame8.Logic.Misc.Visual.UI.MonoBehaviours
{
	[RequireComponent(typeof(Scrollbar))]
	public class ScrollbarFixer8 : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IEndDragHandler, IDragHandler, IPointerDownHandler, IScrollRectProxy
	{
		public bool hideWhenNotNeeded = true;

		public bool autoHide = true;

		[Tooltip("A CanvasGroup will be added to the Scrollbar, if not already present, and the fade effect will be achieved by changing its alpha property")]
		public bool autoHideFadeEffect = true;

		[Tooltip("The collapsing effect will change the localScale of the Scrollbar, so the pivot's position decides in what direction it'll grow/shrink.\n Note that sometimes a really nice effect is achieved by placing the pivot slightly outside the rect (the minimized scrollbar will move outside while collapsing)")]
		public bool autoHideCollapseEffect = true;

		[Tooltip("Used if autoHide is on. Duration in seconds")]
		public float autoHideTime = 1f;

		public float autoHideFadeEffectMinAlpha = 0.8f;

		public float autoHideCollapseEffectMinScale = 0.2f;

		[Range(0.01f, 1f)]
		public float minSize = 0.1f;

		[Range(0.01f, 1f)]
		public float maxSize = 1f;

		[Range(0.015f, 2f)]
		public float sizeUpdateInterval = 0.05f;

		[Tooltip("Used to prevent updates to be processed too often, in case this is a concern")]
		public int skippedFramesBetweenPositionChanges;

		[Tooltip("If not assigned, will try to find a ScrollRect or an IScrollRectProxy in the parent")]
		public ScrollRect scrollRect;

		[Tooltip("If not assigned, will use the resolved scrollRect")]
		public RectTransform viewport;

		public UnityEvent OnScrollbarSizeChanged;

		private IScrollRectProxy _ExternalScrollRectProxy;

		private const float HIDE_EFFECT_START_DELAY_01 = 0.4f;

		private RectTransform _ScrollViewRT;

		private Scrollbar _Scrollbar;

		private CanvasGroup _CanvasGroupForFadeEffect;

		private bool _HorizontalScrollBar;

		private Vector3 _InitialScale = Vector3.one;

		private bool _Hidden;

		private bool _AutoHidden;

		private bool _HiddenNotNeeded;

		private double _LastValue;

		private float _TimeOnLastValueChange;

		private bool _Dragging;

		private IEnumerator _SlowUpdateCoroutine;

		private float _TransversalScaleOnLastDrag;

		private float _AlphaOnLastDrag;

		private bool _FullyInitialized;

		private int _FrameCountOnLastPositionUpdate;

		private bool _TriedToCallOnScrollbarSizeChangedAtLeastOnce;

		[CompilerGenerated]
		private Action<double> m_ScrollPositionChanged;

		public IScrollRectProxy externalScrollRectProxy
		{
			get
			{
				return _ExternalScrollRectProxy;
			}
			set
			{
				if (_ExternalScrollRectProxy != null)
				{
					_ExternalScrollRectProxy.ScrollPositionChanged -= ExternalScrollRectProxy_OnScrollPositionChanged;
				}
				_ExternalScrollRectProxy = value;
				if (_ExternalScrollRectProxy != null)
				{
					if ((bool)scrollRect)
					{
						scrollRect.onValueChanged.RemoveListener(ScrollRect_OnValueChangedCalled);
						scrollRect = null;
					}
					_ExternalScrollRectProxy.ScrollPositionChanged += ExternalScrollRectProxy_OnScrollPositionChanged;
					if (_FullyInitialized && _ExternalScrollRectProxy.IsInitialized)
					{
						OnScrollRectValueChanged(false);
					}
				}
			}
		}

		public bool IsInitialized => _ScrollViewRT != null;

		public Vector2 Velocity
		{
			get
			{
				throw new Exception();
			}
			set
			{
				throw new Exception();
			}
		}

		public bool IsHorizontal => scrollRect.horizontal;

		public bool IsVertical => scrollRect.vertical;

		public RectTransform Content => scrollRect.content;

		public RectTransform Viewport => scrollRect.viewport;

		public bool IsDragging => _Dragging;

		private float Time => UnityEngine.Time.unscaledTime;

		private IScrollRectProxy ScrollRectProxy
		{
			get
			{
				if (externalScrollRectProxy == null)
				{
					return this;
				}
				return externalScrollRectProxy;
			}
		}

		GameObject IScrollRectProxy.gameObject => base.gameObject;

		public event Action<double> ScrollPositionChanged
		{
			[CompilerGenerated]
			add
			{
				Action<double> action = m_ScrollPositionChanged;
				Action<double> action2;
				do
				{
					action2 = action;
					Action<double> value2 = (Action<double>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange(ref m_ScrollPositionChanged, value2, action2);
				}
				while ((object)action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<double> action = m_ScrollPositionChanged;
				Action<double> action2;
				do
				{
					action2 = action;
					Action<double> value2 = (Action<double>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange(ref m_ScrollPositionChanged, value2, action2);
				}
				while ((object)action != action2);
			}
		}

		private void Awake()
		{
			if (autoHideTime == 0f)
			{
				autoHideTime = 1f;
			}
			_Scrollbar = GetComponent<Scrollbar>();
			_InitialScale = _Scrollbar.transform.localScale;
			_LastValue = _Scrollbar.value;
			_TimeOnLastValueChange = Time;
			_HorizontalScrollBar = _Scrollbar.direction == Scrollbar.Direction.LeftToRight || _Scrollbar.direction == Scrollbar.Direction.RightToLeft;
			if (!scrollRect && externalScrollRectProxy == null)
			{
				Transform parent = base.transform;
				while ((bool)(parent = parent.parent) && !scrollRect && externalScrollRectProxy == null)
				{
					scrollRect = parent.GetComponent<ScrollRect>();
					if (!scrollRect)
					{
						externalScrollRectProxy = parent.GetComponent(typeof(IScrollRectProxy)) as IScrollRectProxy;
					}
				}
				if (!scrollRect && externalScrollRectProxy == null)
				{
					UnityEngine.Debug.LogError(GetType().Name + ": no scrollRect provided and found no " + typeof(ScrollRect).Name + " or " + typeof(IScrollRectProxy).Name + " component among ancestors. Disabling...");
					base.enabled = false;
					return;
				}
			}
			if ((bool)scrollRect)
			{
				_ScrollViewRT = scrollRect.transform as RectTransform;
				if (_HorizontalScrollBar)
				{
					if (!scrollRect.horizontal)
					{
						throw new UnityException("ScrollbarFixer8: Can't use horizontal scrollbar with non-horizontal scrollRect");
					}
					if ((bool)scrollRect.horizontalScrollbar)
					{
						UnityEngine.Debug.Log("ScrollbarFixer8: setting scrollRect.horizontalScrollbar to null (the whole point of using ScrollbarFixer8 is to NOT have any scrollbars assigned)");
						scrollRect.horizontalScrollbar = null;
					}
				}
				else
				{
					if (!scrollRect.vertical)
					{
						throw new UnityException("Can't use vertical scrollbar with non-vertical scrollRect");
					}
					if ((bool)scrollRect.verticalScrollbar)
					{
						UnityEngine.Debug.Log("ScrollbarFixer8: setting scrollRect.verticalScrollbar to null (the whole point of using ScrollbarFixer8 is to NOT have any scrollbars assigned)");
						scrollRect.verticalScrollbar = null;
					}
				}
				if (scrollRect.verticalScrollbar == _Scrollbar || scrollRect.horizontalScrollbar == _Scrollbar)
				{
					throw new UnityException("ScrollbarFixer8: Can't use the same scrollbar for both vert and hor");
				}
			}
			if ((bool)scrollRect)
			{
				scrollRect.onValueChanged.AddListener(ScrollRect_OnValueChangedCalled);
			}
		}

		private void OnEnable()
		{
			_Dragging = false;
			_SlowUpdateCoroutine = SlowUpdate();
			StartCoroutine(_SlowUpdateCoroutine);
		}

		private void Update()
		{
			if (!_FullyInitialized)
			{
				InitializeInFirstUpdate();
			}
			if ((!scrollRect && (externalScrollRectProxy == null || !externalScrollRectProxy.IsInitialized)) || (!hideWhenNotNeeded && !autoHide))
			{
				return;
			}
			if (_Dragging)
			{
				_TimeOnLastValueChange = Time;
			}
			else
			{
				if (_Hidden)
				{
					return;
				}
				if (autoHide)
				{
					float num = Time - _TimeOnLastValueChange;
					float num2 = num / autoHideTime;
					if (num2 >= 1f)
					{
						_AutoHidden = true;
						Hide();
					}
					else if (num2 >= 0.4f)
					{
						float t = (num - autoHideTime * 0.4f) / (autoHideTime * 0.6f);
						if (CheckForAudoHideFadeEffectAndInitIfNeeded())
						{
							_CanvasGroupForFadeEffect.alpha = Mathf.Lerp(_AlphaOnLastDrag, autoHideFadeEffectMinAlpha, t);
						}
						if (autoHideCollapseEffect)
						{
							Vector3 localScale = base.transform.localScale;
							localScale[(!_HorizontalScrollBar) ? 0 : 1] = Mathf.Lerp(_TransversalScaleOnLastDrag, autoHideCollapseEffectMinScale, t);
							base.transform.localScale = localScale;
						}
					}
				}
			}
		}

		private void OnDisable()
		{
			StopCoroutine(_SlowUpdateCoroutine);
		}

		private void OnDestroy()
		{
			if ((bool)scrollRect)
			{
				scrollRect.onValueChanged.RemoveListener(ScrollRect_OnValueChangedCalled);
			}
			if (externalScrollRectProxy != null)
			{
				externalScrollRectProxy.ScrollPositionChanged -= ExternalScrollRectProxy_OnScrollPositionChanged;
			}
		}

		public void SetNormalizedPosition(double normalizedPosition)
		{
			if (_HorizontalScrollBar)
			{
				scrollRect.horizontalNormalizedPosition = (float)normalizedPosition;
			}
			else
			{
				scrollRect.verticalNormalizedPosition = (float)normalizedPosition;
			}
		}

		public double GetNormalizedPosition()
		{
			if (!_HorizontalScrollBar)
			{
				return scrollRect.verticalNormalizedPosition;
			}
			return scrollRect.horizontalNormalizedPosition;
		}

		public double GetContentSize()
		{
			if (!IsHorizontal)
			{
				return Content.rect.height;
			}
			return Content.rect.width;
		}

		public double GetViewportSize()
		{
			RectTransform rectTransform = viewport;
			if (!rectTransform)
			{
				rectTransform = ((externalScrollRectProxy != null && (bool)externalScrollRectProxy.Viewport) ? externalScrollRectProxy.Viewport : _ScrollViewRT);
			}
			if (!IsHorizontal)
			{
				return rectTransform.rect.height;
			}
			return rectTransform.rect.width;
		}

		public void StopMovement()
		{
			scrollRect.StopMovement();
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			_Dragging = true;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			_Dragging = false;
		}

		public void OnDrag(PointerEventData eventData)
		{
			OnScrollRectValueChanged(false);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if ((bool)scrollRect)
			{
				scrollRect.StopMovement();
			}
			else
			{
				externalScrollRectProxy.StopMovement();
			}
		}

		private void InitializeInFirstUpdate()
		{
			if (externalScrollRectProxy != null)
			{
				if (!externalScrollRectProxy.IsInitialized)
				{
					return;
				}
				_ScrollViewRT = externalScrollRectProxy.gameObject.transform as RectTransform;
			}
			UpdateStartingValuesForAutoHideEffect();
			OnScrollRectValueChanged(true);
			_FullyInitialized = true;
		}

		private IEnumerator SlowUpdate()
		{
			WaitForSecondsRealtime waitAmount = new WaitForSecondsRealtime(sizeUpdateInterval);
			while (true)
			{
				yield return waitAmount;
				if (!_FullyInitialized)
				{
					continue;
				}
				double viewportSize = GetViewportSize();
				double contentSize = ScrollRectProxy.GetContentSize();
				double value = 1.0;
				if (contentSize > 0.0 && contentSize != double.NaN && contentSize != double.Epsilon && contentSize != double.NegativeInfinity && contentSize != double.PositiveInfinity)
				{
					value = viewportSize / contentSize;
				}
				float value2 = (float)Math.Min(1.0, Math.Max(0.0, value));
				float size = _Scrollbar.size;
				_Scrollbar.size = Mathf.Clamp(value2, minSize, maxSize);
				if (hideWhenNotNeeded)
				{
					if (value > 0.99)
					{
						if (!_Hidden)
						{
							_HiddenNotNeeded = true;
							Hide();
						}
					}
					else if (_Hidden && !_AutoHidden)
					{
						Show();
					}
				}
				if ((!_TriedToCallOnScrollbarSizeChangedAtLeastOnce || size != _Scrollbar.size) && OnScrollbarSizeChanged != null)
				{
					_TriedToCallOnScrollbarSizeChangedAtLeastOnce = true;
					OnScrollbarSizeChanged.Invoke();
				}
			}
		}

		private void Hide()
		{
			_Hidden = true;
			if (!autoHide || _HiddenNotNeeded)
			{
				base.gameObject.transform.localScale = Vector3.zero;
			}
		}

		private void Show()
		{
			base.gameObject.transform.localScale = _InitialScale;
			_HiddenNotNeeded = (_AutoHidden = (_Hidden = false));
			if (CheckForAudoHideFadeEffectAndInitIfNeeded())
			{
				_CanvasGroupForFadeEffect.alpha = 1f;
			}
			UpdateStartingValuesForAutoHideEffect();
		}

		private void UpdateStartingValuesForAutoHideEffect()
		{
			if (CheckForAudoHideFadeEffectAndInitIfNeeded())
			{
				_AlphaOnLastDrag = _CanvasGroupForFadeEffect.alpha;
			}
			if (autoHideCollapseEffect)
			{
				_TransversalScaleOnLastDrag = base.transform.localScale[(!_HorizontalScrollBar) ? 0 : 1];
			}
		}

		private bool CheckForAudoHideFadeEffectAndInitIfNeeded()
		{
			if (autoHideFadeEffect && !_CanvasGroupForFadeEffect)
			{
				_CanvasGroupForFadeEffect = GetComponent<CanvasGroup>();
				if (!_CanvasGroupForFadeEffect)
				{
					_CanvasGroupForFadeEffect = base.gameObject.AddComponent<CanvasGroup>();
				}
			}
			return autoHideFadeEffect;
		}

		private void ScrollRect_OnValueChangedCalled(Vector2 _)
		{
			OnScrollRectValueChanged(true);
		}

		private void ExternalScrollRectProxy_OnScrollPositionChanged(double _)
		{
			OnScrollRectValueChanged(true);
		}

		private void OnScrollRectValueChanged(bool fromScrollRect)
		{
			if (!fromScrollRect)
			{
				ScrollRectProxy.StopMovement();
				if (_FrameCountOnLastPositionUpdate + skippedFramesBetweenPositionChanges < UnityEngine.Time.frameCount)
				{
					ScrollRectProxy.SetNormalizedPosition(_Scrollbar.value);
					_FrameCountOnLastPositionUpdate = UnityEngine.Time.frameCount;
				}
			}
			float num = ((!fromScrollRect) ? _Scrollbar.value : ((float)ScrollRectProxy.GetNormalizedPosition()));
			_Scrollbar.SetValueWithoutNotify(num);
			if (m_ScrollPositionChanged != null)
			{
				m_ScrollPositionChanged(num);
			}
			_TimeOnLastValueChange = Time;
			if (autoHide && Math.Abs(_LastValue - (double)num) > 0.00010000000474974513)
			{
				Show();
			}
			_LastValue = num;
		}
	}
}
