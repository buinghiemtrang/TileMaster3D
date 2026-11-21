using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using frame8.Logic.Misc.Other;
using frame8.Logic.Misc.Other.Extensions;
using frame8.Logic.Misc.Visual.UI.MonoBehaviours;

namespace Com.TheFallenGames.OSA.Core
{
	public class Snapper8 : MonoBehaviour
	{
		public float snapWhenSpeedFallsBelow = 50f;

		public float viewportSnapPivot01 = 0.5f;

		public float itemSnapPivot01 = 0.5f;

		public float snapDuration = 0.3f;

		public float snapAllowedError = 1f;

		public Scrollbar scrollbar;

		[Tooltip("If the current drag distance is not enough to change the currently centered item, snapping to the next item will still occur if the current speed is bigger than this. Set to a negative value to disable (default). This was initially useful for things like page views")]
		public float minSpeedToAllowSnapToNext = -1f;

		public bool skipIfReachedExtremity = true;

		[CompilerGenerated]
		private Action m_SnappingStarted;

		[CompilerGenerated]
		private Action m_SnappingEndedOrCancelled;

		private IOSA _Adapter;

		private ScrollbarFixer8 _ScrollbarFixer;

		private bool _SnappingDoneAndEndSnappingEventPending;

		private bool _SnapNeeded;

		private bool _SnappingCancelled;

		private Func<float> _GetSignedAbstractSpeed;

		private int _LastSnappedItemIndex = -1;

		private bool _SnapToNextOnlyEnabled;

		private bool _StartCalled;

		private Canvas _Canvas;

		private RectTransform _CanvasRT;

		private bool _TriedToGetCanvasAtLeastOnce;

		private int _LastItemIndexUnFinishedSnap = -1;

		public IOSA Adapter
		{
			set
			{
				if (_Adapter == value)
				{
					return;
				}
				if (_Adapter != null)
				{
					_Adapter.ScrollPositionChanged -= OnScrolled;
					_Adapter.ItemsRefreshed -= OnItemsRefreshed;
				}
				_Adapter = value;
				if (_Adapter != null)
				{
					_Adapter.ScrollPositionChanged += OnScrolled;
					_Adapter.ItemsRefreshed += OnItemsRefreshed;
					if (_Adapter.BaseParameters.IsHorizontal)
					{
						_GetSignedAbstractSpeed = () => _Adapter.Velocity[0];
					}
					else
					{
						_GetSignedAbstractSpeed = () => 0f - _Adapter.Velocity[1];
					}
				}
			}
		}

		public bool SnappingInProgress { get; private set; }

		private bool IsPointerDraggingOnScrollRect
		{
			get
			{
				if (_Adapter != null)
				{
					return _Adapter.IsDragging;
				}
				return false;
			}
		}

		private bool IsPointerDraggingOnScrollbar
		{
			get
			{
				if (_ScrollbarFixer != null)
				{
					return _ScrollbarFixer.IsDragging;
				}
				return false;
			}
		}

		public event Action SnappingStarted
		{
			[CompilerGenerated]
			add
			{
				Action action = m_SnappingStarted;
				Action action2;
				do
				{
					action2 = action;
					Action value2 = (Action)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange(ref m_SnappingStarted, value2, action2);
				}
				while ((object)action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action action = m_SnappingStarted;
				Action action2;
				do
				{
					action2 = action;
					Action value2 = (Action)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange(ref m_SnappingStarted, value2, action2);
				}
				while ((object)action != action2);
			}
		}

		public event Action SnappingEndedOrCancelled
		{
			[CompilerGenerated]
			add
			{
				Action action = m_SnappingEndedOrCancelled;
				Action action2;
				do
				{
					action2 = action;
					Action value2 = (Action)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange(ref m_SnappingEndedOrCancelled, value2, action2);
				}
				while ((object)action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action action = m_SnappingEndedOrCancelled;
				Action action2;
				do
				{
					action2 = action;
					Action value2 = (Action)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange(ref m_SnappingEndedOrCancelled, value2, action2);
				}
				while ((object)action != action2);
			}
		}

		private void Start()
		{
			if (minSpeedToAllowSnapToNext < 0f)
			{
				minSpeedToAllowSnapToNext = float.MaxValue;
			}
			_SnapToNextOnlyEnabled = minSpeedToAllowSnapToNext != float.MaxValue;
			if ((bool)scrollbar)
			{
				_ScrollbarFixer = scrollbar.GetComponent<ScrollbarFixer8>();
				if (!_ScrollbarFixer)
				{
					throw new OSAException("ScrollbarFixer8 should be attached to Scrollbar");
				}
			}
			if ((bool)scrollbar)
			{
				scrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);
			}
			_StartCalled = true;
		}

		private void OnDisable()
		{
			CancelSnappingIfInProgress();
		}

		private void OnDestroy()
		{
			if ((bool)scrollbar)
			{
				scrollbar.onValueChanged.RemoveListener(OnScrollbarValueChanged);
			}
			Adapter = null;
			m_SnappingStarted = null;
			m_SnappingEndedOrCancelled = null;
		}

		internal void CancelSnappingIfInProgress()
		{
			_SnappingDoneAndEndSnappingEventPending = false;
			_SnapNeeded = false;
			if (SnappingInProgress)
			{
				_SnappingCancelled = true;
				SnappingInProgress = false;
			}
		}

		internal void StartSnappingIfNeeded()
		{
			if (!_StartCalled)
			{
				return;
			}
			if (_SnappingDoneAndEndSnappingEventPending)
			{
				OnSnappingEndedOrCancelled();
				return;
			}
			if (_Adapter == null || !_Adapter.IsInitialized)
			{
				return;
			}
			if (_Adapter.VisibleItemsCount > 20)
			{
				MonoBehaviour.print("OSA: Slowing down automatically because the current visible items count(" + _Adapter.VisibleItemsCount + ") is way too big for a Snapper8 component! Check your item sizes or viewport size, or disable Snapper8 if not needed!");
			}
			float num = _GetSignedAbstractSpeed();
			float num2 = Mathf.Abs(num);
			if (SnappingInProgress || !_SnapNeeded || !(num2 < snapWhenSpeedFallsBelow) || IsPointerDraggingOnScrollRect || IsPointerDraggingOnScrollbar)
			{
				return;
			}
			double normalizedPosition = _Adapter.GetNormalizedPosition();
			if (skipIfReachedExtremity && (normalizedPosition == 0.0 || normalizedPosition == 1.0))
			{
				return;
			}
			float distanceToTarget;
			AbstractViewsHolder middleVH = GetMiddleVH(out distanceToTarget);
			if (middleVH == null)
			{
				return;
			}
			_SnapNeeded = false;
			if (distanceToTarget <= snapAllowedError)
			{
				return;
			}
			int itemIndex = middleVH.ItemIndex;
			int num3 = ((num2 >= minSpeedToAllowSnapToNext && (itemIndex == _LastSnappedItemIndex || itemIndex == _LastItemIndexUnFinishedSnap)) ? ((!(num < 0f)) ? Mathf.Max(itemIndex - 1, 0) : Mathf.Min(itemIndex + 1, _Adapter.GetItemsCount() - 1)) : itemIndex);
			SnappingInProgress = true;
			_SnappingCancelled = false;
			bool continuteAnimation = true;
			bool cancelledOrEnded = false;
			_LastItemIndexUnFinishedSnap = num3;
			int indexToSnapTo = num3;
			_Adapter.SmoothScrollTo(indexToSnapTo, snapDuration, viewportSnapPivot01, itemSnapPivot01, delegate(float progress)
			{
				continuteAnimation = true;
				if (progress == 1f || _SnappingCancelled || IsPointerDraggingOnScrollRect || IsPointerDraggingOnScrollbar)
				{
					cancelledOrEnded = true;
					_LastItemIndexUnFinishedSnap = -1;
					if (_SnappingCancelled || IsPointerDraggingOnScrollRect || IsPointerDraggingOnScrollbar)
					{
						continuteAnimation = false;
					}
					else
					{
						_LastSnappedItemIndex = indexToSnapTo;
					}
					_SnappingDoneAndEndSnappingEventPending = true;
				}
				return continuteAnimation;
			}, null, true);
			if (cancelledOrEnded)
			{
				if (continuteAnimation)
				{
					OnSnappingStarted();
				}
				return;
			}
			if (SnappingInProgress)
			{
				OnSnappingStarted();
			}
		}

		private Canvas FindOrGetCanvas()
		{
			if (_TriedToGetCanvasAtLeastOnce)
			{
				return _Canvas;
			}
			_TriedToGetCanvasAtLeastOnce = true;
			return _Canvas = GetComponentInParent<Canvas>();
		}

		private RectTransform FindOrGetCanvasRT()
		{
			if (_TriedToGetCanvasAtLeastOnce)
			{
				return _CanvasRT;
			}
			_TriedToGetCanvasAtLeastOnce = true;
			Canvas canvas = FindOrGetCanvas();
			return _CanvasRT = (canvas ? (canvas.transform as RectTransform) : null);
		}

		private AbstractViewsHolder GetMiddleVH(out float distanceToTarget)
		{
			Vector2 customPivotOnFirstRect = (_Adapter.BaseParameters.IsHorizontal ? new Vector2(viewportSnapPivot01, 0f) : new Vector2(0f, 1f - viewportSnapPivot01));
			Vector2 customPivotOnSecondRect = (_Adapter.BaseParameters.IsHorizontal ? new Vector2(itemSnapPivot01, 0f) : new Vector2(0f, 1f - itemSnapPivot01));
			return _Adapter.GetViewsHolderClosestToViewportLongitudinalNormalizedAbstractPoint(FindOrGetCanvas(), FindOrGetCanvasRT(), customPivotOnFirstRect.x, customPivotOnSecondRect.x, out distanceToTarget);
		}

		private void OnScrolled(double _)
		{
			if (!SnappingInProgress)
			{
				_SnapNeeded = true;
				if (_SnapToNextOnlyEnabled && !IsPointerDraggingOnScrollbar && !IsPointerDraggingOnScrollRect)
				{
					UpdateLastSnappedIndexFromMiddleVH();
				}
			}
		}

		private void OnScrollbarValueChanged(float _)
		{
			if (IsPointerDraggingOnScrollbar)
			{
				CancelSnappingIfInProgress();
			}
		}

		private void OnItemsRefreshed(int newCount, int prevCount)
		{
			if (newCount == prevCount)
			{
				if (_SnapToNextOnlyEnabled)
				{
					UpdateLastSnappedIndexFromMiddleVH();
				}
			}
			else
			{
				_LastSnappedItemIndex = -1;
			}
		}

		private void UpdateLastSnappedIndexFromMiddleVH()
		{
			float distanceToTarget;
			AbstractViewsHolder middleVH = GetMiddleVH(out distanceToTarget);
			_LastSnappedItemIndex = ((middleVH == null) ? (-1) : middleVH.ItemIndex);
		}

		private void OnSnappingStarted()
		{
			if (m_SnappingStarted != null)
			{
				m_SnappingStarted();
			}
		}

		private void OnSnappingEndedOrCancelled()
		{
			_SnappingDoneAndEndSnappingEventPending = false;
			SnappingInProgress = false;
			if (m_SnappingEndedOrCancelled != null)
			{
				m_SnappingEndedOrCancelled();
			}
		}
	}
}
