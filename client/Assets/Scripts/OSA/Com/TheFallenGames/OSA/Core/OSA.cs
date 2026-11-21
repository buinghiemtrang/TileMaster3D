using System;
using System.Collections;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core.Data.Gallery;
using Com.TheFallenGames.OSA.Core.SubComponents;
using UnityEngine;
using UnityEngine.EventSystems;
using frame8.Logic.Misc.Other;
using frame8.Logic.Misc.Other.Extensions;
using frame8.Logic.Misc.Visual.UI;

namespace Com.TheFallenGames.OSA.Core
{
	public abstract class OSA<TParams, TItemViewsHolder> : MonoBehaviour, IOSA, IScrollRectProxy, IInitializePotentialDragHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler where TParams : BaseParams where TItemViewsHolder : BaseItemViewsHolder
	{
		[SerializeField]
		protected TParams _Params;

		internal ItemsDescriptor _ItemsDesc;

		internal InternalState<TItemViewsHolder> _InternalState;

		internal List<TItemViewsHolder> _VisibleItems;

		internal double _AVGVisibleItemsCount;

		internal List<TItemViewsHolder> _RecyclableItems = new List<TItemViewsHolder>();

		internal List<TItemViewsHolder> _BufferredRecyclableItems = new List<TItemViewsHolder>();

		protected int _VisibleItemsCount;

		private Coroutine _SmoothScrollCoroutine;

		private bool _SkipComputeVisibilityInUpdateOrOnScroll;

		private float _PrevGalleryEffectAmount;

		private bool _Initialized;

		private bool _SkipInitializationChecks;

		private bool _ForceRebuildLayoutScheduled;

		private Vector2 _Velocity;

		private Vector2 _VelocityToAddOnDragEnd;

		private bool _IsDragging;

		private int _StackedDragSpeeds;

		private Vector2 _OnInitializePotentialDragPointerPosInCTSpace;

		private Vector2 _OnBeginDragPointerPosInCTSpace;

		private Vector2 _LastOnDragPointerPosInCTSpace;

		private float _LastOnDragTime;

		private float _LastOnEndDragTime;

		private ComputeVisibilityManager<TParams, TItemViewsHolder> _ComputeVisibilityManager;

		private ReleaseFromPullManager<TParams, TItemViewsHolder> _ReleaseFromPull;

		private NestingManager<TParams, TItemViewsHolder> _NestingManager;

		private ComputeVisibilityParams _ComputeVisibilityParams_Reusable_Empty = new ComputeVisibilityParams();

		private ComputeVisibilityParams _ComputeVisibilityParams_Reusable_DragUnchecked = new ComputeVisibilityParams();

		public bool IsInitialized => _Initialized;

		public Vector2 Velocity
		{
			get
			{
				return _Velocity;
			}
			set
			{
				_Velocity = value;
				int index = 1 - _InternalState.hor0_vert1;
				_Velocity[index] = 0f;
				float maxSpeed = _Params.effects.MaxSpeed;
				int hor0_vert = _InternalState.hor0_vert1;
				_Velocity[hor0_vert] = Mathf.Clamp(_Velocity[hor0_vert], 0f - maxSpeed, maxSpeed);
			}
		}

		public bool IsHorizontal => _Params.IsHorizontal;

		public bool IsVertical => !_Params.IsHorizontal;

		public RectTransform Content => _Params.Content;

		public RectTransform Viewport => _Params.Viewport;

		public BaseParams BaseParameters => Parameters;

		public MonoBehaviour AsMonoBehaviour => this;

		public double ContentVirtualInsetFromViewportStart => _InternalState.ctVirtualInsetFromVPS_Cached;

		public double ContentVirtualInsetFromViewportEnd => _InternalState.CTVirtualInsetFromVPE_Cached;

		public int VisibleItemsCount
		{
			get
			{
				return _VisibleItemsCount;
			}
			internal set
			{
				_VisibleItemsCount = value;
			}
		}

		public int RecyclableItemsCount => _RecyclableItems.Count;

		public int BufferedRecyclableItemsCount => _BufferredRecyclableItems.Count;

		public bool IsDragging => _IsDragging;

		public virtual bool InsertAtIndexSupported => true;

		public virtual bool RemoveFromIndexSupported => true;

		public float Time => _Params.UseUnscaledTime ? UnityEngine.Time.unscaledTime : UnityEngine.Time.time;

		public float DeltaTime => _Params.UseUnscaledTime ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime;

		public TParams Parameters => _Params;

		GameObject IScrollRectProxy.gameObject => base.gameObject;

		public event Action<double> ScrollPositionChanged;

		public virtual event Action Initialized;

		public virtual event Action<int, int> ItemsRefreshed;

		public virtual event Action ScrollViewSizeChanged;

		protected virtual void Awake()
		{
		}

		protected virtual void OnEnable()
		{
		}

		protected virtual void Start()
		{
			if (!IsInitialized)
			{
				Init();
			}
		}

		protected virtual void Update()
		{
			if (IsInitialized)
			{
				MyUpdate();
			}
		}

		protected virtual void LateUpdate()
		{
			if (IsInitialized)
			{
				MyLateUpdate();
			}
		}

		protected virtual void OnDisable()
		{
			if (IsInitialized)
			{
				MyOnDisable();
			}
		}

		protected virtual void OnDestroy()
		{
			Dispose();
		}

		public void SetNormalizedPosition(double normalizedPosition)
		{
			double pos = (IsHorizontal ? (1.0 - normalizedPosition) : normalizedPosition);
			SetVirtualAbstractNormalizedScrollPosition(pos, computeVisibilityNow: true, out var _);
		}

		public double GetNormalizedPosition()
		{
			double virtualAbstractNormalizedScrollPosition = GetVirtualAbstractNormalizedScrollPosition();
			if (!IsHorizontal)
			{
				return virtualAbstractNormalizedScrollPosition;
			}
			return 1.0 - virtualAbstractNormalizedScrollPosition;
		}

		public double GetContentSize()
		{
			return Math.Min(_InternalState.ctVirtualSize, 3.4028234663852886E+38);
		}

		public double GetViewportSize()
		{
			return _InternalState.vpSize;
		}

		public virtual void OnInitializePotentialDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
			{
				return;
			}
			if (_Params.ForwardDragToParents)
			{
				_NestingManager.OnInitializePotentialDrag(eventData);
			}
			if (!_Params.DragEnabled)
			{
				return;
			}
			float f = _Velocity[_InternalState.hor0_vert1];
			float f2 = _VelocityToAddOnDragEnd[_InternalState.hor0_vert1];
			bool flag = false;
			if (_Params.effects.TransientSpeedBetweenDrags)
			{
				float num = Mathf.Max(Time - _LastOnEndDragTime, 0f);
				if (num > 0f && Mathf.Abs(f) > 1f && num < 0.5f && Mathf.Sign(f) == Mathf.Sign(f2))
				{
					flag = true;
				}
				if (_Params.effects.CutMovementOnPointerDown)
				{
					if (flag)
					{
						_VelocityToAddOnDragEnd += _Velocity;
					}
					StopMovement();
					StopScrollingIfAny();
				}
			}
			else
			{
				StopMovement();
			}
			if (!flag)
			{
				_VelocityToAddOnDragEnd = Vector2.zero;
				_StackedDragSpeeds = 0;
			}
			_OnInitializePotentialDragPointerPosInCTSpace = _InternalState.GetPointerPositionInCTSpace(eventData);
			_ReleaseFromPull.inProgress = false;
		}

		public virtual void OnBeginDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
			{
				return;
			}
			if (_Params.ForwardDragToParents)
			{
				_NestingManager.OnBeginDrag(eventData);
				if (_NestingManager.CurrentDragCapturedByParent)
				{
					return;
				}
			}
			if (!base.isActiveAndEnabled || !_Params.DragEnabled)
			{
				return;
			}
			_OnBeginDragPointerPosInCTSpace = _InternalState.GetPointerPositionInCTSpace(eventData);
			_LastOnDragPointerPosInCTSpace = _OnBeginDragPointerPosInCTSpace;
			if (_Params.effects.TransientSpeedBetweenDrags)
			{
				_VelocityToAddOnDragEnd += _Velocity;
			}
			_Velocity = Vector2.zero;
			_IsDragging = true;
			_LastOnDragTime = Time;
			_ReleaseFromPull.inProgress = false;
		}

		public virtual void OnDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
			{
				return;
			}
			if (_Params.ForwardDragToParents && _NestingManager.CurrentDragCapturedByParent)
			{
				_NestingManager.OnDrag(eventData);
				return;
			}
			if (!base.isActiveAndEnabled || !_Params.DragEnabled)
			{
				return;
			}
			Vector2 pointerPositionInCTSpace = _InternalState.GetPointerPositionInCTSpace(eventData);
			Vector2 vectorCTSpace = pointerPositionInCTSpace - _LastOnDragPointerPosInCTSpace;
			double num = _InternalState.GetCTAbstractSpaceVectorLongitudinalComponentFromCTSpaceVector(vectorCTSpace);
			AllowContentOutsideBoundsMode allowOutsideBoundsMode = ((_Params.effects.ElasticMovement || (_Params.effects.LoopItems && _InternalState.VirtualScrollableArea > 0.0)) ? AllowContentOutsideBoundsMode.ALLOW : AllowContentOutsideBoundsMode.DO_NOT_ALLOW);
			Drag(num, allowOutsideBoundsMode, cancelSnappingIfAny: true);
			if (_Params.effects.Inertia)
			{
				float num2 = Time - _LastOnDragTime;
				if (num2 == 0f)
				{
					num2 = DeltaTime;
				}
				float num3 = _Velocity[_InternalState.hor0_vert1];
				double num4;
				if (_LastOnDragPointerPosInCTSpace == _OnBeginDragPointerPosInCTSpace)
				{
					Vector2 vectorCTSpace2 = pointerPositionInCTSpace - _OnInitializePotentialDragPointerPosInCTSpace;
					num4 = _InternalState.GetCTAbstractSpaceVectorLongitudinalComponentFromCTSpaceVector(vectorCTSpace2);
				}
				else
				{
					num4 = ((Math.Abs(num) < 1.0) ? 0.0 : num);
				}
				double num5 = num4 / (double)num2 * (double)_InternalState.hor1_vertMinus1;
				float value = (float)(((double)num3 + num5) / 2.0);
				float maxSpeed = _Params.effects.MaxSpeed;
				_Velocity[_InternalState.hor0_vert1] = Mathf.Clamp(value, 0f - maxSpeed, maxSpeed);
			}
			_LastOnDragTime = Time;
			_LastOnDragPointerPosInCTSpace = pointerPositionInCTSpace;
		}

		public virtual void OnEndDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
			{
				return;
			}
			if (_Params.ForwardDragToParents && _NestingManager.CurrentDragCapturedByParent)
			{
				_NestingManager.OnEndDrag(eventData);
				return;
			}
			_IsDragging = false;
			if (!base.isActiveAndEnabled || !_Params.DragEnabled)
			{
				return;
			}
			if (_Params.effects.Inertia)
			{
				bool flag = false;
				float num = Mathf.Max(Time - _LastOnDragTime, 0f);
				float num2 = Mathf.Max(DeltaTime, 0.1f);
				if (num < num2)
				{
					Vector2 pointerPositionInCTSpace = _InternalState.GetPointerPositionInCTSpace(eventData);
					Vector2 vectorCTSpace = pointerPositionInCTSpace - _OnInitializePotentialDragPointerPosInCTSpace;
					if (Math.Abs(_InternalState.GetCTAbstractSpaceVectorLongitudinalComponentFromCTSpaceVector(vectorCTSpace)) >= 3.0)
					{
						flag = true;
					}
				}
				if (flag)
				{
					float num3 = _VelocityToAddOnDragEnd[_InternalState.hor0_vert1];
					float num4 = _Velocity[_InternalState.hor0_vert1];
					if (Mathf.Sign(num3) == Mathf.Sign(num4))
					{
						float value = (num4 + num3) * (float)(1 + _StackedDragSpeeds);
						float maxSpeed = _Params.effects.MaxSpeed;
						_Velocity[_InternalState.hor0_vert1] = Mathf.Clamp(value, 0f - maxSpeed, maxSpeed);
						if (_Params.effects.TransientSpeedBetweenDrags && _StackedDragSpeeds < 10)
						{
							_StackedDragSpeeds++;
						}
					}
					else
					{
						_StackedDragSpeeds = 0;
						_VelocityToAddOnDragEnd = Vector2.zero;
					}
				}
				else
				{
					_Velocity[_InternalState.hor0_vert1] = 0f;
				}
			}
			_LastOnEndDragTime = Time;
		}

		public virtual void OnScroll(PointerEventData eventData)
		{
			if (_IsDragging)
			{
				return;
			}
			if (_Params.ForwardDragToParents && !_NestingManager.CurrentDragCapturedByParent)
			{
				_NestingManager.OnScroll(eventData);
				if (_NestingManager.CurrentScrollConsumedByParent)
				{
					return;
				}
			}
			if (!base.isActiveAndEnabled || !_Params.ScrollEnabled)
			{
				return;
			}
			Vector2 scrollDelta = eventData.scrollDelta;
			Vector2 vec = scrollDelta;
			_Params.ApplyScrollSensitivityTo(ref vec);
			float num = vec.x * (float)_InternalState.hor1_vertMinus1;
			float num2 = vec.y * (float)(-_InternalState.hor1_vertMinus1);
			float num3 = ((Mathf.Abs(scrollDelta.x) > Mathf.Abs(scrollDelta.y)) ? num : num2);
			if (num3 != 0f)
			{
				StopMovement();
				Drag(num3, AllowContentOutsideBoundsMode.DO_NOT_ALLOW, cancelSnappingIfAny: true);
			}
		}

		public void Init()
		{
			if (!base.enabled || !base.gameObject.activeInHierarchy)
			{
				throw new OSAException("Init should only be called when the GameObject is active in hierarchy and the OSA component is enabled");
			}
			_Params.PrepareForInit(firstTime: true);
			_Params.InitIfNeeded(this);
			_ReleaseFromPull = new ReleaseFromPullManager<TParams, TItemViewsHolder>(this);
			if ((bool)_Params.Snapper)
			{
				_Params.Snapper.Adapter = this;
			}
			_ItemsDesc = new ItemsDescriptor(_Params.DefaultItemSize);
			_InternalState = InternalState<TItemViewsHolder>.CreateFromSourceParamsOrThrow(_Params, _ItemsDesc);
			_Params.UpdateContentPivotFromGravityType();
			_InternalState.CacheScrollViewInfo();
			_NestingManager = new NestingManager<TParams, TItemViewsHolder>(this);
			_ComputeVisibilityManager = new ComputeVisibilityManager<TParams, TItemViewsHolder>(this);
			_VisibleItems = new List<TItemViewsHolder>();
			_AVGVisibleItemsCount = 0.0;
			_SkipInitializationChecks = true;
			Refresh();
			_InternalState.UpdateLastProcessedCTVirtualInsetFromVPStart();
			SetVirtualAbstractNormalizedScrollPosition(1.0, computeVisibilityNow: false, out var _);
			OnScrollPositionChangedInternal();
			_SkipInitializationChecks = false;
			OnInitialized();
			if (_Initialized && Initialized != null)
			{
				Initialized();
			}
		}

		public virtual void Refresh(bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			ChangeItemsCount(ItemCountChangeMode.RESET, _ItemsDesc.itemsCount, -1, contentPanelEndEdgeStationary, keepVelocity);
		}

		public virtual void ResetItems(int itemsCount, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			ChangeItemsCount(ItemCountChangeMode.RESET, itemsCount, -1, contentPanelEndEdgeStationary, keepVelocity);
		}

		public virtual void InsertItems(int index, int itemsCount, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			ChangeItemsCount(ItemCountChangeMode.INSERT, itemsCount, index, contentPanelEndEdgeStationary, keepVelocity);
		}

		public virtual void RemoveItems(int index, int itemsCount, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			ChangeItemsCount(ItemCountChangeMode.REMOVE, itemsCount, index, contentPanelEndEdgeStationary, keepVelocity);
		}

		public virtual void ChangeItemsCount(ItemCountChangeMode changeMode, int itemsCount, int indexIfInsertingOrRemoving = -1, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			ChangeItemsCountInternal(changeMode, itemsCount, indexIfInsertingOrRemoving, contentPanelEndEdgeStationary, keepVelocity, stealInsteadOfRecycle: false);
		}

		public virtual int GetItemsCount()
		{
			return _ItemsDesc.itemsCount;
		}

		public TItemViewsHolder GetItemViewsHolder(int vhIndex)
		{
			if (vhIndex >= _VisibleItemsCount)
			{
				return null;
			}
			return _VisibleItems[vhIndex];
		}

		public BaseItemViewsHolder GetBaseItemViewsHolder(int vhIndex)
		{
			return GetItemViewsHolder(vhIndex);
		}

		public Type GetViewsHolderType()
		{
			return typeof(TItemViewsHolder);
		}

		public TItemViewsHolder GetItemViewsHolderIfVisible(int withItemIndex)
		{
			for (int i = 0; i < _VisibleItemsCount; i++)
			{
				TItemViewsHolder val = _VisibleItems[i];
				if (val.ItemIndex == withItemIndex)
				{
					return val;
				}
			}
			return null;
		}

		public BaseItemViewsHolder GetBaseItemViewsHolderIfVisible(int withItemIndex)
		{
			return GetItemViewsHolderIfVisible(withItemIndex);
		}

		public TItemViewsHolder GetItemViewsHolderIfVisible(RectTransform withRoot)
		{
			for (int i = 0; i < _VisibleItemsCount; i++)
			{
				TItemViewsHolder val = _VisibleItems[i];
				if (val.root == withRoot)
				{
					return val;
				}
			}
			return null;
		}

		public float ConvertViewportLocalPointToViewportLongitudinalPointStart0End1(Vector2 localPositionInViewport)
		{
			Vector2 vector = _Params.Viewport.ConvertLocalPointToPointNormalizedBySize(localPositionInViewport);
			float num = vector[_InternalState.hor0_vert1];
			if (!_Params.IsHorizontal)
			{
				num = 1f - num;
			}
			return num;
		}

		public virtual TItemViewsHolder GetViewsHolderClosestToViewportPoint(Canvas c, RectTransform canvasRectTransform, Vector2 localPositionInViewport, float itemNormalizedAbstractPoint, out float distance)
		{
			float viewportNormalizedAbstractPoint = ConvertViewportLocalPointToViewportLongitudinalPointStart0End1(localPositionInViewport);
			return GetViewsHolderClosestToViewportLongitudinalNormalizedAbstractPoint(c, canvasRectTransform, viewportNormalizedAbstractPoint, itemNormalizedAbstractPoint, out distance) as TItemViewsHolder;
		}

		public virtual AbstractViewsHolder GetViewsHolderClosestToViewportLongitudinalNormalizedAbstractPoint(Canvas c, RectTransform canvasRectTransform, float viewportNormalizedAbstractPoint, float itemNormalizedAbstractPoint, out float distance)
		{
			int hor0_vert = _InternalState.hor0_vert1;
			int num = 1 - hor0_vert;
			Vector2 viewportPointNormalizedWithYInverted = new Vector2(viewportNormalizedAbstractPoint * (float)num + (float)hor0_vert / 2f, viewportNormalizedAbstractPoint * (float)hor0_vert + (float)num / 2f);
			Vector2 itemPointNormalizedWithYInverted = new Vector2(itemNormalizedAbstractPoint * (float)num + (float)hor0_vert / 2f, itemNormalizedAbstractPoint * (float)hor0_vert + (float)num / 2f);
			return GetViewsHolderClosestoViewportNormalizedAbastractPoint(_VisibleItems, c, canvasRectTransform, viewportPointNormalizedWithYInverted, itemPointNormalizedWithYInverted, out distance);
		}

		public LayoutInfo GetLayoutInfoReadonly()
		{
			return _InternalState.layoutInfo;
		}

		public virtual void RemoveItemWithViewsHolder(TItemViewsHolder viewsHolder, bool stealViewsHolderInsteadOfRecycle, bool contentPanelEndEdgeStationary)
		{
			ChangeItemsCountInternal(ItemCountChangeMode.REMOVE, 1, viewsHolder.ItemIndex, contentPanelEndEdgeStationary, keepVelocity: false, stealViewsHolderInsteadOfRecycle);
			if (stealViewsHolderInsteadOfRecycle && !viewsHolder.root)
			{
				throw new OSAException("RemoveItemWithViewsHolder: Internal error: vh.root destroyed. ItemIndex " + viewsHolder.ItemIndex + ", viewsHolder.root " + viewsHolder.root);
			}
		}

		public virtual void InsertItemWithViewsHolder(TItemViewsHolder viewsHolder, int atIndex, bool contentPanelEndEdgeStationary)
		{
			if (atIndex < 0 || atIndex > _ItemsDesc.itemsCount)
			{
				throw new ArgumentOutOfRangeException("atIndex", atIndex, "should be >=0 and <= itemsCount(=" + _ItemsDesc.itemsCount + ")");
			}
			if (!viewsHolder.root)
			{
				throw new ArgumentOutOfRangeException("Could not give a viewsholder that has its root destroyed/set to null");
			}
			ClearCachedRecyclableItems(includingBufferred: false);
			_RecyclableItems.Insert(0, viewsHolder);
			ChangeItemsCountInternal(ItemCountChangeMode.INSERT, 1, atIndex, contentPanelEndEdgeStationary, keepVelocity: false, stealInsteadOfRecycle: false);
		}

		public virtual bool ScrollByAbstractDelta(float abstractDeltaInCTSpace)
		{
			if (!base.isActiveAndEnabled)
			{
				return false;
			}
			if (_IsDragging)
			{
				return false;
			}
			StopMovement();
			Drag(abstractDeltaInCTSpace, AllowContentOutsideBoundsMode.DO_NOT_ALLOW, cancelSnappingIfAny: true);
			return true;
		}

		public virtual void ScrollTo(int itemIndex, float normalizedOffsetFromViewportStart = 0f, float normalizedPositionOfItemPivotToUse = 0f)
		{
			CancelAnimationsIfAny();
			double virtualScrollableArea = _InternalState.VirtualScrollableArea;
			bool flag = virtualScrollableArea > 0.0;
			double virtualInset = ScrollToHelper_GetContentStartVirtualInsetFromViewportStart_Clamped(virtualScrollableArea, itemIndex, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse);
			ContentSizeOrPositionChangeParams p = new ContentSizeOrPositionChangeParams
			{
				computeVisibilityParams = _ComputeVisibilityParams_Reusable_Empty,
				fireScrollPositionChangedEvent = true,
				allowOutsideBounds = (_Params.effects.LoopItems && flag)
			};
			SetContentVirtualInsetFromViewportStart(virtualInset, ref p, out var _);
		}

		public virtual bool SmoothScrollTo(int itemIndex, float duration, float normalizedOffsetFromViewportStart = 0f, float normalizedPositionOfItemPivotToUse = 0f, Func<float, bool> onProgress = null, Action onDone = null, bool overrideCurrentScrollingAnimation = false)
		{
			if (_SmoothScrollCoroutine != null)
			{
				if (!overrideCurrentScrollingAnimation)
				{
					return false;
				}
				CancelAnimationsIfAny();
			}
			_SmoothScrollCoroutine = StartCoroutine(SmoothScrollProgressCoroutine(itemIndex, duration, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse, onProgress, onDone));
			return true;
		}

		public virtual bool SmoothBringToView(int itemIndex, float duration, float? spacingFromViewportEdge = null, Func<float, bool> onProgress = null, Action onDone = null, bool overrideCurrentScrollingAnimation = false)
		{
			double num = spacingFromViewportEdge ?? _Params.ContentSpacing;
			float num2 = (float)(num / _InternalState.vpSize);
			TItemViewsHolder itemViewsHolderIfVisible = GetItemViewsHolderIfVisible(itemIndex);
			bool flag = itemViewsHolderIfVisible != null;
			int itemViewIndexFromRealIndexChecked = _ItemsDesc.GetItemViewIndexFromRealIndexChecked(itemIndex);
			if (flag)
			{
				if ((double)GetItemRealInsetFromParentStart(itemViewsHolderIfVisible.root) < num)
				{
					return SmoothScrollTo(itemIndex, duration, 0f + num2, 0f, onProgress, onDone, overrideCurrentScrollingAnimation);
				}
				if ((double)GetItemRealInsetFromParentEnd(itemViewsHolderIfVisible.root) < num)
				{
					return SmoothScrollTo(itemIndex, duration, 1f - num2, 1f, onProgress, onDone, overrideCurrentScrollingAnimation);
				}
			}
			else
			{
				if (_InternalState.GetItemInferredRealInsetFromParentStart(itemViewIndexFromRealIndexChecked) < num)
				{
					return SmoothScrollTo(itemIndex, duration, 0f + num2, 0f, onProgress, onDone, overrideCurrentScrollingAnimation);
				}
				if (_InternalState.GetItemInferredRealInsetFromParentEnd(itemViewIndexFromRealIndexChecked) < num)
				{
					return SmoothScrollTo(itemIndex, duration, 1f - num2, 1f, onProgress, onDone, overrideCurrentScrollingAnimation);
				}
			}
			return false;
		}

		public virtual void CancelAnimationsIfAny()
		{
			if ((bool)_Params.Snapper)
			{
				_Params.Snapper.CancelSnappingIfInProgress();
			}
			StopScrollingIfAny();
		}

		public virtual void StopScrollingIfAny()
		{
			if (_SmoothScrollCoroutine != null)
			{
				StopCoroutine(_SmoothScrollCoroutine);
				_SmoothScrollCoroutine = null;
				_SkipComputeVisibilityInUpdateOrOnScroll = false;
			}
		}

		public virtual void StopMovement()
		{
			_Velocity = Vector2.zero;
		}

		public float RequestChangeItemSizeAndUpdateLayout(TItemViewsHolder withVH, float requestedSize, bool itemEndEdgeStationary = false, bool computeVisibility = true, bool correctItemPosition = false, bool keepVelocity = false)
		{
			return RequestChangeItemSizeAndUpdateLayout(withVH.ItemIndex, requestedSize, itemEndEdgeStationary, computeVisibility, correctItemPosition, keepVelocity);
		}

		public float RequestChangeItemSizeAndUpdateLayout(int itemIndex, float requestedSize, bool itemEndEdgeStationary = false, bool computeVisibility = true, bool correctItemPosition = false, bool keepVelocity = false)
		{
			CancelAnimationsIfAny();
			bool skipComputeVisibilityInUpdateOrOnScroll = _SkipComputeVisibilityInUpdateOrOnScroll;
			_SkipComputeVisibilityInUpdateOrOnScroll = true;
			if (!keepVelocity)
			{
				StopMovement();
			}
			int itemViewIndexFromRealIndexChecked = _ItemsDesc.GetItemViewIndexFromRealIndexChecked(itemIndex);
			TItemViewsHolder itemViewsHolderIfVisible = GetItemViewsHolderIfVisible(itemIndex);
			bool flag = _InternalState.CTVirtualInsetFromVPE_Cached < 0.0;
			double curSize = _ItemsDesc[itemViewIndexFromRealIndexChecked];
			double num = ChangeItemSizeAndUpdateContentSizeAccordingly(itemViewsHolderIfVisible, itemViewIndexFromRealIndexChecked, curSize, requestedSize, itemEndEdgeStationary);
			double overrideScrollingDelta = (itemEndEdgeStationary ? 0.1 : (flag ? (-0.1) : 0.1));
			if (correctItemPosition)
			{
				CorrectPositionsOfVisibleItems(!computeVisibility, fireScrollPositionChangedEvent: false);
			}
			if (computeVisibility)
			{
				ComputeVisibilityForCurrentPositionRawParams(forceFireScrollViewPositionChangedEvent: true, potentialTwinPassCTEndStationaryPrioritizeUserPreference: false, overrideScrollingDelta);
			}
			_SkipComputeVisibilityInUpdateOrOnScroll = skipComputeVisibilityInUpdateOrOnScroll;
			return (float)num;
		}

		public double GetItemVirtualInsetFromParentStart(int itemIndex)
		{
			int itemViewIndexFromRealIndexChecked = _ItemsDesc.GetItemViewIndexFromRealIndexChecked(itemIndex);
			return _InternalState.GetItemVirtualInsetFromParentStartUsingItemIndexInView(itemViewIndexFromRealIndexChecked);
		}

		public float GetItemRealInsetFromParentStart(RectTransform withRoot)
		{
			return withRoot.GetInsetFromParentEdge(_Params.Content, _InternalState.startEdge);
		}

		public float GetItemRealInsetFromParentEnd(RectTransform withRoot)
		{
			return withRoot.GetInsetFromParentEdge(_Params.Content, _InternalState.endEdge);
		}

		public double GetVirtualAbstractNormalizedScrollPosition()
		{
			return _InternalState.GetVirtualAbstractNormalizedScrollPosition();
		}

		public double SetVirtualAbstractNormalizedScrollPosition(double pos, bool computeVisibilityNow, out bool looped, bool keepVelocity = false)
		{
			CancelAnimationsIfAny();
			bool skipComputeVisibilityInUpdateOrOnScroll = _SkipComputeVisibilityInUpdateOrOnScroll;
			_SkipComputeVisibilityInUpdateOrOnScroll = true;
			double virtualInset = (0.0 - (1.0 - pos)) * _InternalState.VirtualScrollableArea;
			ContentSizeOrPositionChangeParams p = new ContentSizeOrPositionChangeParams
			{
				computeVisibilityParams = (computeVisibilityNow ? _ComputeVisibilityParams_Reusable_Empty : null),
				fireScrollPositionChangedEvent = true,
				keepVelocity = keepVelocity
			};
			double result = SetContentVirtualInsetFromViewportStart(virtualInset, ref p, out looped);
			_SkipComputeVisibilityInUpdateOrOnScroll = skipComputeVisibilityInUpdateOrOnScroll;
			return result;
		}

		public void ScheduleForceRebuildLayout()
		{
			_ForceRebuildLayoutScheduled = true;
		}

		protected virtual void OnInitialized()
		{
			_Initialized = true;
		}

		protected virtual void OnItemsRefreshed(int prevCount, int newCount)
		{
		}

		protected virtual void CollectItemsSizes(ItemCountChangeMode changeMode, int count, int indexIfInsertingOrRemoving, ItemsDescriptor itemsDesc)
		{
			int itemsCount = itemsDesc.itemsCount;
			itemsDesc.ReinitializeSizes(changeMode, count, indexIfInsertingOrRemoving, _Params.DefaultItemSize);
			int itemIndexOfFirstVHIfInsertingOrRemoving = ((_VisibleItemsCount > 0) ? _VisibleItems[0].ItemIndex : (-1));
			itemsDesc.ReinitializeRealIndexOfFirstItemInView(itemsCount, changeMode, count, indexIfInsertingOrRemoving, itemIndexOfFirstVHIfInsertingOrRemoving, _Params.effects.LoopItems);
		}

		protected abstract TItemViewsHolder CreateViewsHolder(int itemIndex);

		protected abstract void UpdateViewsHolder(TItemViewsHolder newOrRecycled);

		protected virtual void OnItemIndexChangedDueInsertOrRemove(TItemViewsHolder shiftedViewsHolder, int oldIndex, bool wasInsert, int removeOrInsertIndex)
		{
		}

		protected virtual bool IsRecyclable(TItemViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
		{
			return true;
		}

		internal bool InternalShouldDestroyRecyclableItem(TItemViewsHolder inRecycleBin, bool isInExcess)
		{
			return ShouldDestroyRecyclableItem(inRecycleBin, isInExcess);
		}

		internal void InternalDestroyRecyclableItem(TItemViewsHolder vh, bool isVisible)
		{
			OnBeforeDestroyViewsHolder(vh, isVisible);
			try
			{
				UnityEngine.Object.Destroy(vh.root.gameObject);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		protected virtual bool ShouldDestroyRecyclableItem(TItemViewsHolder inRecycleBin, bool isInExcess)
		{
			return isInExcess;
		}

		internal void InternalOnBeforeRecycleOrDisableViewsHolder(TItemViewsHolder inRecycleBinOrVisible, int newItemIndex)
		{
			OnBeforeRecycleOrDisableViewsHolder(inRecycleBinOrVisible, newItemIndex);
		}

		protected virtual void OnBeforeRecycleOrDisableViewsHolder(TItemViewsHolder inRecycleBinOrVisible, int newItemIndex)
		{
			if (newItemIndex == -1)
			{
				inRecycleBinOrVisible.UnmarkForRebuild();
			}
		}

		protected virtual void OnBeforeDestroyViewsHolder(TItemViewsHolder vh, bool isActive)
		{
		}

		protected virtual void ClearCachedRecyclableItems(bool includingBufferred = true)
		{
			if (_RecyclableItems != null)
			{
				DestroyAndClearItemsInList(_RecyclableItems);
			}
			if (includingBufferred && _BufferredRecyclableItems != null)
			{
				DestroyAndClearItemsInList(_BufferredRecyclableItems);
			}
		}

		private void DestroyAndClearItemsInList(List<TItemViewsHolder> vhs)
		{
			foreach (TItemViewsHolder vh in vhs)
			{
				if (vh != null && vh.root != null)
				{
					InternalDestroyRecyclableItem(vh, isVisible: false);
				}
			}
			vhs.Clear();
		}

		protected virtual void ClearVisibleItems()
		{
			if (_VisibleItems != null)
			{
				foreach (TItemViewsHolder visibleItem in _VisibleItems)
				{
					if (visibleItem != null)
					{
						OnBeforeRecycleOrDisableViewsHolder(visibleItem, -1);
						if (visibleItem.root != null)
						{
							InternalDestroyRecyclableItem(visibleItem, isVisible: true);
						}
					}
				}
				_VisibleItems.Clear();
				_VisibleItemsCount = 0;
			}
			if (_ReleaseFromPull != null)
			{
				_ReleaseFromPull.inProgress = false;
			}
		}

		protected virtual void OnScrollViewSizeChanged()
		{
		}

		protected virtual void RebuildLayoutDueToScrollViewSizeChange()
		{
			ClearCachedRecyclableItems();
			_Params.PrepareForInit(firstTime: false);
			_Params.InitIfNeeded(this);
			if ((bool)_Params.Snapper)
			{
				_Params.Snapper.Adapter = this;
			}
			_InternalState.CacheScrollViewInfo();
			_ItemsDesc.maxVisibleItemsSeenSinceLastScrollViewSizeChange = 0;
			_ItemsDesc.destroyedItemsSinceLastScrollViewSizeChange = 0;
		}

		protected virtual void PostRebuildLayoutDueToScrollViewSizeChange()
		{
		}

		protected virtual float UpdateItemSizeOnTwinPass(TItemViewsHolder viewsHolder)
		{
			return ForceRebuildViewsHolder(viewsHolder);
		}

		protected virtual void OnItemHeightChangedPreTwinPass(TItemViewsHolder viewsHolder)
		{
		}

		protected virtual void OnItemWidthChangedPreTwinPass(TItemViewsHolder viewsHolder)
		{
		}

		protected void ScheduleComputeVisibilityTwinPass(bool preferContentEndEdgeStationaryIfSizeChanges = false)
		{
			_InternalState.computeVisibilityTwinPassScheduled = true;
			_InternalState.preferKeepingCTEndEdgeStationaryInNextComputeVisibilityTwinPass = preferContentEndEdgeStationaryIfSizeChanges;
		}

		protected virtual float ForceRebuildViewsHolder(TItemViewsHolder viewsHolder)
		{
			viewsHolder.MarkForRebuild();
			_InternalState.RebuildLayoutImmediateCompat(viewsHolder.root);
			float result = viewsHolder.root.rect.size[_InternalState.hor0_vert1];
			viewsHolder.UnmarkForRebuild();
			return result;
		}

		protected void ForceRebuildViewsHolderAndUpdateSize(TItemViewsHolder viewsHolder, bool itemEndEdgeStationary = false, bool correctPosition = true)
		{
			float requestedSize = ForceRebuildViewsHolder(viewsHolder);
			RequestChangeItemSizeAndUpdateLayout(viewsHolder, requestedSize, itemEndEdgeStationary, computeVisibility: true, correctPosition);
		}

		protected virtual AbstractViewsHolder GetViewsHolderClosestToViewportNormalizedPoint(ICollection<TItemViewsHolder> viewsHolders, Canvas c, RectTransform canvasRectTransform, Vector2 viewportPointNormalized, Vector2 itemPointNormalized, out float distance)
		{
			TItemViewsHolder result = null;
			float num = float.MaxValue;
			foreach (TItemViewsHolder viewsHolder in viewsHolders)
			{
				float magnitude = Singleton8<UIUtils8>.Instance.GetWorldVectorBetweenCustomLocalPivots(_Params.Viewport, viewportPointNormalized, viewsHolder.root, itemPointNormalized, c, canvasRectTransform).magnitude;
				if (magnitude < num)
				{
					result = viewsHolder;
					num = magnitude;
				}
			}
			distance = num;
			return result;
		}

		protected virtual AbstractViewsHolder GetViewsHolderClosestoViewportNormalizedAbastractPoint(ICollection<TItemViewsHolder> viewsHolders, Canvas c, RectTransform canvasRectTransform, Vector2 viewportPointNormalizedWithYInverted, Vector2 itemPointNormalizedWithYInverted, out float distance)
		{
			viewportPointNormalizedWithYInverted.y = 1f - viewportPointNormalizedWithYInverted.y;
			itemPointNormalizedWithYInverted.y = 1f - itemPointNormalizedWithYInverted.y;
			return GetViewsHolderClosestToViewportNormalizedPoint(viewsHolders, c, canvasRectTransform, viewportPointNormalizedWithYInverted, itemPointNormalizedWithYInverted, out distance);
		}

		protected virtual void OnScrollPositionChanged(double normPos)
		{
		}

		public void SetViewsHolderEnabled(TItemViewsHolder vh, bool vhEnabled)
		{
			if (_Params.optimization.ScaleToZeroInsteadOfDisable)
			{
				Vector3 localScale;
				if (vhEnabled)
				{
					localScale = Vector3.one;
					if (!vh.root.gameObject.activeSelf)
					{
						vh.root.gameObject.SetActive(value: true);
					}
				}
				else
				{
					localScale = Vector3.zero;
				}
				vh.root.localScale = localScale;
			}
			else if (vh.root.gameObject.activeSelf != vhEnabled)
			{
				vh.root.gameObject.SetActive(vhEnabled);
			}
		}

		public void SetViewsHolderEnabled(TItemViewsHolder vh)
		{
			if (_Params.optimization.ScaleToZeroInsteadOfDisable)
			{
				if (!vh.root.gameObject.activeSelf)
				{
					vh.root.gameObject.SetActive(value: true);
				}
				vh.root.localScale = Vector3.one;
			}
			else if (!vh.root.gameObject.activeSelf)
			{
				vh.root.gameObject.SetActive(value: true);
			}
		}

		public bool IsViewsHolderEnabled(TItemViewsHolder vh)
		{
			if (!vh.root.gameObject.activeSelf)
			{
				return false;
			}
			if (_Params.optimization.ScaleToZeroInsteadOfDisable)
			{
				return vh.root.localScale != Vector3.zero;
			}
			return true;
		}

		public void SetViewsHolderDisabled(TItemViewsHolder vh)
		{
			if (_Params.optimization.ScaleToZeroInsteadOfDisable)
			{
				vh.root.localScale = Vector3.zero;
			}
			else if (vh.root.gameObject.activeSelf)
			{
				vh.root.gameObject.SetActive(value: false);
			}
		}

		protected virtual void Dispose()
		{
			_Initialized = false;
			_ForceRebuildLayoutScheduled = false;
			if (_SmoothScrollCoroutine != null)
			{
				try
				{
					StopCoroutine(_SmoothScrollCoroutine);
				}
				catch
				{
				}
				_SmoothScrollCoroutine = null;
			}
			ClearCachedRecyclableItems();
			_RecyclableItems = null;
			_BufferredRecyclableItems = null;
			ClearVisibleItems();
			_VisibleItems = null;
			_Params = null;
			_InternalState = null;
			if (ItemsRefreshed != null)
			{
				ItemsRefreshed = null;
			}
		}

		private void Drag(double abstrDeltaInCTSpace, AllowContentOutsideBoundsMode allowOutsideBoundsMode, bool cancelSnappingIfAny)
		{
			Drag(abstrDeltaInCTSpace, allowOutsideBoundsMode, cancelSnappingIfAny, out var _, out var _);
		}

		private void Drag(double abstrDeltaInCTSpace, AllowContentOutsideBoundsMode allowOutsideBoundsMode, bool cancelSnappingIfAny, out bool done, out bool looped)
		{
			done = false;
			looped = false;
			double num;
			double num2;
			if (abstrDeltaInCTSpace > 0.0)
			{
				num = _InternalState.ctVirtualInsetFromVPS_Cached;
				num2 = abstrDeltaInCTSpace;
			}
			else
			{
				num = _InternalState.CTVirtualInsetFromVPE_Cached;
				num2 = 0.0 - abstrDeltaInCTSpace;
			}
			bool updateCachedCTVirtualInset_AllowOutsideBounds;
			double num3;
			if (num + num2 < 0.0)
			{
				updateCachedCTVirtualInset_AllowOutsideBounds = allowOutsideBoundsMode != AllowContentOutsideBoundsMode.DO_NOT_ALLOW;
				num3 = 1.0;
			}
			else
			{
				bool flag;
				if (allowOutsideBoundsMode == AllowContentOutsideBoundsMode.DO_NOT_ALLOW || (allowOutsideBoundsMode == AllowContentOutsideBoundsMode.ALLOW_IF_OUTSIDE_AMOUNT_SHRINKS && num + num2 >= num))
				{
					if (num >= 0.0)
					{
						return;
					}
					flag = false;
					updateCachedCTVirtualInset_AllowOutsideBounds = false;
					abstrDeltaInCTSpace = Math.Min(num2, 0.0 - num) * (double)Math.Sign(abstrDeltaInCTSpace);
				}
				else
				{
					flag = num >= 0.0;
					updateCachedCTVirtualInset_AllowOutsideBounds = true;
				}
				num3 = 1.0;
				if (flag && _Params.effects.ElasticMovement)
				{
					double num4 = Math.Min(1.0, num / _InternalState.vpSize);
					num3 = (1.0 - num4) * (double)_Params.effects.PullElasticity;
				}
			}
			looped = DragVisibleItemsRangeUnchecked(0, _VisibleItemsCount, abstrDeltaInCTSpace * num3, updateCachedCTVirtualInset: true, updateCachedCTVirtualInset_ComputeVisibility: true, updateCachedCTVirtualInset_AllowOutsideBounds, cancelSnappingIfAny);
			done = true;
		}

		internal bool DragVisibleItemsRangeUnchecked(int vhStartIndex, int vhEndIndexExcl, double abstractDelta, bool updateCachedCTVirtualInset, bool updateCachedCTVirtualInset_ComputeVisibility, bool updateCachedCTVirtualInset_AllowOutsideBounds = true, bool updateCachedCTVirtualInset_CancelSnappingIfAny = true)
		{
			int hor1_vertMinus = _InternalState.hor1_vertMinus1;
			for (int i = vhStartIndex; i < vhEndIndexExcl; i++)
			{
				TItemViewsHolder val = _VisibleItems[i];
				Vector3 localPosition = val.root.localPosition;
				localPosition[_InternalState.hor0_vert1] += (float)(abstractDelta * (double)hor1_vertMinus);
				val.root.localPosition = localPosition;
			}
			if (updateCachedCTVirtualInset)
			{
				_ComputeVisibilityParams_Reusable_DragUnchecked.overrideDelta = abstractDelta;
				double? contentInsetOverride = null;
				if (_VisibleItemsCount == 0)
				{
					contentInsetOverride = _InternalState.ctVirtualInsetFromVPS_Cached + abstractDelta;
				}
				ContentSizeOrPositionChangeParams p = new ContentSizeOrPositionChangeParams
				{
					cancelSnappingIfAny = updateCachedCTVirtualInset_CancelSnappingIfAny,
					allowOutsideBounds = updateCachedCTVirtualInset_AllowOutsideBounds,
					computeVisibilityParams = (updateCachedCTVirtualInset_ComputeVisibility ? _ComputeVisibilityParams_Reusable_DragUnchecked : null),
					fireScrollPositionChangedEvent = true,
					keepVelocity = true,
					contentInsetOverride = contentInsetOverride
				};
				return UpdateCTVrtInsetFromVPS(ref p);
			}
			return false;
		}

		private IEnumerator SmoothScrollProgressCoroutine(int itemIndex, double duration, double normalizedOffsetFromViewportStart = 0.0, double normalizedPositionOfItemPivotToUse = 0.0, Func<float, bool> onProgress = null, Action onDone = null)
		{
			double vsa = _InternalState.VirtualScrollableArea;
			double num = -vsa;
			double num2 = 0.0;
			if (vsa <= 0.0)
			{
				_SmoothScrollCoroutine = null;
				if (onProgress == null || onProgress(1f))
				{
					onDone?.Invoke();
				}
				yield break;
			}
			bool ignorOnScroll_lastValue = _SkipComputeVisibilityInUpdateOrOnScroll;
			_SkipComputeVisibilityInUpdateOrOnScroll = true;
			StopMovement();
			if (_Params.optimization.ForceLayoutRebuildOnBeginSmoothScroll)
			{
				Canvas.ForceUpdateCanvases();
			}
			Func<double> getTargetVrtInset = () =>
			{
				vsa = _InternalState.VirtualScrollableArea;
				return ScrollToHelper_GetContentStartVirtualInsetFromViewportStart_Clamped(vsa, itemIndex, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse);
			};
			double initialVrtInsetFromParent = -1.0;
			double targetVrtInsetFromParent = -1.0;
			bool needToCalculateInitialInset = true;
			bool needToCalculateTargetInset = true;
			bool notCanceledByCaller = true;
			double startTime = Time;
			double elapsedTime = 0.0;
			double progress = 0.0;
			WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
			ContentSizeOrPositionChangeParams contentPosChangeParams = new ContentSizeOrPositionChangeParams
			{
				computeVisibilityParams = _ComputeVisibilityParams_Reusable_Empty,
				fireScrollPositionChangedEvent = true,
				allowOutsideBounds = true
			};
			bool looped = false;
			Action<double> setInsetAndUpdateLocalsFn = delegate(double inset)
			{
				SetContentVirtualInsetFromViewportStart(inset, ref contentPosChangeParams, out looped);
			};
			double originalStartTime = startTime;
			double originalDuration = duration;
			bool atLeastOneTwinPassDetected = false;
			Action updateTwinPassLocalVariable = delegate
			{
				atLeastOneTwinPassDetected = atLeastOneTwinPassDetected || _InternalState.lastComputeVisibilityHadATwinPass;
			};
			bool flag;
			do
			{
				yield return null;
				updateTwinPassLocalVariable();
				yield return endOfFrame;
				updateTwinPassLocalVariable();
				if (_SmoothScrollCoroutine == null)
				{
					yield break;
				}
				if (needToCalculateInitialInset)
				{
					initialVrtInsetFromParent = _InternalState.ctVirtualInsetFromVPS_Cached;
					if (!_Params.effects.LoopItems)
					{
						initialVrtInsetFromParent = Math.Max(num, Math.Min(num2, initialVrtInsetFromParent));
					}
					needToCalculateInitialInset = _Params.UseUnscaledTime;
				}
				if (needToCalculateTargetInset || atLeastOneTwinPassDetected)
				{
					targetVrtInsetFromParent = getTargetVrtInset();
					needToCalculateTargetInset = _Params.UseUnscaledTime;
					if (atLeastOneTwinPassDetected && targetVrtInsetFromParent != initialVrtInsetFromParent)
					{
						double time = Time;
						if (time - startTime > duration)
						{
							double num3 = time - originalStartTime;
							duration = ((originalDuration <= num3) ? (num3 - originalDuration) : originalDuration);
							startTime = time;
							initialVrtInsetFromParent = _InternalState.ctVirtualInsetFromVPS_Cached;
						}
					}
				}
				elapsedTime = (double)Time - startTime;
				progress = ((elapsedTime < duration) ? Math.Sin(elapsedTime / duration * (Math.PI / 2.0)) : 1.0);
				double obj = initialVrtInsetFromParent + (targetVrtInsetFromParent - initialVrtInsetFromParent) * progress;
				setInsetAndUpdateLocalsFn(obj);
				updateTwinPassLocalVariable();
				flag = true;
				if (!_Params.effects.LoopItems || !looped)
				{
					flag = duration > elapsedTime;
					if (flag)
					{
						flag = onProgress == null || onProgress((float)progress);
						notCanceledByCaller = flag;
					}
				}
			}
			while (flag);
			if (notCanceledByCaller)
			{
				setInsetAndUpdateLocalsFn(getTargetVrtInset());
				updateTwinPassLocalVariable();
				if (atLeastOneTwinPassDetected)
				{
					setInsetAndUpdateLocalsFn(getTargetVrtInset());
				}
				_SkipComputeVisibilityInUpdateOrOnScroll = ignorOnScroll_lastValue;
				_SmoothScrollCoroutine = null;
				if (onProgress == null || onProgress(1f))
				{
					onDone?.Invoke();
				}
			}
			else
			{
				_SkipComputeVisibilityInUpdateOrOnScroll = ignorOnScroll_lastValue;
				_SmoothScrollCoroutine = null;
			}
		}

		private double ScrollToHelper_GetContentStartVirtualInsetFromViewportStart_Clamped(double vsa, int itemIndex, double normalizedItemOffsetFromStart, double normalizedPositionOfItemPivotToUse)
		{
			double num = ((vsa > 0.0 && _Params.effects.LoopItems) ? (_InternalState.vpSize / 2.0) : 0.0);
			double val = 0.0 - vsa - num;
			int itemViewIndexFromRealIndexChecked = _ItemsDesc.GetItemViewIndexFromRealIndexChecked(itemIndex);
			double num2 = _ItemsDesc[itemViewIndexFromRealIndexChecked];
			double val2 = _InternalState.vpSize * normalizedItemOffsetFromStart - num2 * normalizedPositionOfItemPivotToUse - _InternalState.GetItemVirtualInsetFromParentStartUsingItemIndexInView(itemViewIndexFromRealIndexChecked);
			val2 = Math.Min(num, val2);
			return Math.Max(val, val2);
		}

		internal double SetContentVirtualInsetFromViewportStart(double virtualInset, ref ContentSizeOrPositionChangeParams p, out bool looped)
		{
			looped = false;
			_ReleaseFromPull.inProgress = false;
			double num = virtualInset - _InternalState.ctVirtualInsetFromVPS_Cached;
			if (!p.keepVelocity)
			{
				StopMovement();
			}
			AllowContentOutsideBoundsMode allowOutsideBoundsMode = ((_Params.effects.LoopItems && _InternalState.VirtualScrollableArea > 0.0) ? AllowContentOutsideBoundsMode.ALLOW : AllowContentOutsideBoundsMode.ALLOW_IF_OUTSIDE_AMOUNT_SHRINKS);
			Drag(num, allowOutsideBoundsMode, p.cancelSnappingIfAny, out var _, out looped);
			CorrectPositionsOfVisibleItems(alsoCorrectTransversalPositioning: false, p.fireScrollPositionChangedEvent);
			return num;
		}

		private void RecycleAllVisibleViewsHolders()
		{
			while (_VisibleItemsCount > 0)
			{
				RecycleOrStealViewsHolder(0, steal: false);
			}
		}

		private void RecycleOrStealViewsHolder(int vhIndex, bool steal)
		{
			TItemViewsHolder val = _VisibleItems[vhIndex];
			if (!steal)
			{
				OnBeforeRecycleOrDisableViewsHolder(val, -1);
				SetViewsHolderDisabled(val);
			}
			_VisibleItems.RemoveAt(vhIndex);
			_VisibleItemsCount--;
			if (!steal)
			{
				_RecyclableItems.Add(val);
			}
		}

		private bool UpdateCTVrtInsetFromVPS(ref ContentSizeOrPositionChangeParams p)
		{
			double ctVirtualInsetFromVPS_Cached = _InternalState.ctVirtualInsetFromVPS_Cached;
			double num = _InternalState.paddingContentStart;
			double inset;
			if (!p.contentInsetOverride.HasValue)
			{
				if (_VisibleItemsCount > 0)
				{
					TItemViewsHolder val = _VisibleItems[0];
					int itemIndexInView = val.itemIndexInView;
					if (itemIndexInView > 0)
					{
						num += _ItemsDesc.GetItemSizeCumulative(itemIndexInView - 1) + (double)itemIndexInView * _InternalState.spacing;
					}
					double num2 = val.root.GetInsetFromParentEdge(_Params.Content, _InternalState.startEdge);
					inset = num2 - num;
				}
				else
				{
					inset = 0.0;
				}
			}
			else
			{
				inset = p.contentInsetOverride.Value;
			}
			bool skipComputeVisibilityInUpdateOrOnScroll = _SkipComputeVisibilityInUpdateOrOnScroll;
			_SkipComputeVisibilityInUpdateOrOnScroll = true;
			if (p.cancelSnappingIfAny && (bool)_Params.Snapper)
			{
				_Params.Snapper.CancelSnappingIfInProgress();
			}
			if (!p.keepVelocity)
			{
				StopMovement();
			}
			_InternalState.UpdateCachedCTVirtInsetFromVPS(inset, p.allowOutsideBounds);
			bool result = p.computeVisibilityParams != null && ComputeVisibilityForCurrentPosition(p.computeVisibilityParams);
			if (p.fireScrollPositionChangedEvent)
			{
				OnScrollPositionChangedInternal();
			}
			_SkipComputeVisibilityInUpdateOrOnScroll = skipComputeVisibilityInUpdateOrOnScroll;
			if (_Params.effects.HasContentVisual)
			{
				Rect uvRect = _Params.effects.ContentVisual.uvRect;
				Vector2 position = uvRect.position;
				float num3 = position[_InternalState.hor0_vert1];
				double vpSize = _InternalState.vpSize;
				double num4 = _InternalState.ctVirtualInsetFromVPS_Cached - ctVirtualInsetFromVPS_Cached;
				double d = (num4 + num4 * (double)_Params.effects.ContentVisualParallaxEffect) / vpSize;
				double num5 = (d - Math.Truncate(d)) * (double)(-_InternalState.hor1_vertMinus1);
				if (num5 < 0.0)
				{
					num5 += 1.0;
				}
				double d2 = num5 + (double)num3;
				double num6 = d2 - Math.Truncate(d2);
				if (num6 < 0.0)
				{
					num6 += 1.0;
				}
				position[_InternalState.hor0_vert1] = (float)num6;
				uvRect.position = position;
				_Params.effects.ContentVisual.uvRect = uvRect;
			}
			return result;
		}

		private void ShiftViewsHolderItemIndexAndFireEvent(TItemViewsHolder vh, int shift, bool wasInsert, int insertOrRemoveIndex)
		{
			int itemIndex = vh.ItemIndex;
			vh.ShiftIndex(shift, _ItemsDesc.itemsCount);
			OnItemIndexChangedDueInsertOrRemove(vh, itemIndex, wasInsert, insertOrRemoveIndex);
		}

		private void ShiftViewsHolderItemIndexInView(TItemViewsHolder vh, int shift)
		{
			vh.ShiftIndexInView(shift, _ItemsDesc.itemsCount);
		}

		private void ChangeItemsCountInternal(ItemCountChangeMode changeMode, int count, int indexIfInsertingOrRemoving, bool contentPanelEndEdgeStationary, bool keepVelocity, bool stealInsteadOfRecycle)
		{
			if (!_Initialized && !_SkipInitializationChecks)
			{
				throw new OSAException("ChangeItemsCountInternal: OSA not initialized. Before using it, make sure the GameObject is active in hierarchy, the OSA component is enabled, and Start has been called. If you overrode OnInitialized, please call base.OnInitialized() on the first line of your function");
			}
			int itemsCount = _ItemsDesc.itemsCount;
			switch (changeMode)
			{
			case ItemCountChangeMode.INSERT:
				if (indexIfInsertingOrRemoving < 0 || indexIfInsertingOrRemoving > itemsCount)
				{
					throw new ArgumentOutOfRangeException("indexIfInsertingOrRemoving", indexIfInsertingOrRemoving, "should be >=0 and <= than itemsCount(=" + itemsCount + ")");
				}
				if ((long)itemsCount + (long)count > 2147483646)
				{
					throw new ArgumentOutOfRangeException("newCount", (long)itemsCount + (long)count, "should be <= MAX_COUNT(=" + 2147483646 + ")");
				}
				break;
			case ItemCountChangeMode.REMOVE:
				if (indexIfInsertingOrRemoving < 0 || indexIfInsertingOrRemoving >= itemsCount)
				{
					throw new ArgumentOutOfRangeException("indexIfInsertingOrRemoving", indexIfInsertingOrRemoving, "should be >=0 and < than itemsCount(=" + itemsCount + ")");
				}
				if (count < 1)
				{
					throw new ArgumentOutOfRangeException("count", count, "should be > 0");
				}
				if (indexIfInsertingOrRemoving + count > itemsCount)
				{
					throw new ArgumentOutOfRangeException("indexIfInsertingOrRemoving + count", count, "indexIfInsertingOrRemoving+count = " + (indexIfInsertingOrRemoving + count) + " should be <= itemsCount(=" + _ItemsDesc.itemsCount + ")");
				}
				break;
			}
			bool loopItems = _Params.effects.LoopItems;
			if (loopItems && changeMode != ItemCountChangeMode.RESET)
			{
				throw new OSAException("ChangeItemsCountInternal: At the moment, only ItemCountChangeMode.RESET is supported when looping. Use ResetItems()");
			}
			CancelAnimationsIfAny();
			if (_ReleaseFromPull.inProgress)
			{
				if (_VisibleItemsCount > 0)
				{
					_ReleaseFromPull.FinishNowByDraggingItems(computeVisibility: false);
				}
				else
				{
					_ReleaseFromPull.FinishNowBySettingContentInset(computeVisibility: false);
				}
			}
			bool skipComputeVisibilityInUpdateOrOnScroll = _SkipComputeVisibilityInUpdateOrOnScroll;
			_SkipComputeVisibilityInUpdateOrOnScroll = true;
			int num = -1;
			if (itemsCount > 0 && changeMode != ItemCountChangeMode.RESET)
			{
				num = ((indexIfInsertingOrRemoving != itemsCount) ? _ItemsDesc.GetItemViewIndexFromRealIndexChecked(indexIfInsertingOrRemoving) : (_ItemsDesc.GetItemViewIndexFromRealIndexChecked(indexIfInsertingOrRemoving - 1) + 1));
			}
			Vector2 velocity = _Velocity;
			if (!keepVelocity)
			{
				StopMovement();
			}
			double num2 = _InternalState.CalculateContentVirtualSize();
			CollectItemsSizes(changeMode, count, indexIfInsertingOrRemoving, _ItemsDesc);
			int itemsCount2 = _ItemsDesc.itemsCount;
			double num3 = _InternalState.CalculateContentVirtualSize();
			double num4 = num3 - num2;
			double additionalCTDragAbstrDelta = 0.0;
			_InternalState.CorrectParametersOnCTSizeChange(contentPanelEndEdgeStationary, out var _, ref additionalCTDragAbstrDelta, num3, num4);
			double virtualScrollableArea = _InternalState.VirtualScrollableArea;
			double cTVirtualInsetFromVPE_Cached = _InternalState.CTVirtualInsetFromVPE_Cached;
			if (GetNumExcessRecycleableItems() > 0)
			{
				throw new OSAException("ChangeItemsCountInternal: GetNumExcessObjects() > 0 when calling ChangeItemsCountInternal(); this may be due ComputeVisibility not being finished executing yet");
			}
			int num5 = -1;
			double? num6 = null;
			double? contentInsetOverride = null;
			if (_VisibleItemsCount > 0 && changeMode != ItemCountChangeMode.RESET)
			{
				TItemViewsHolder val = _VisibleItems[0];
				int itemIndexInView = val.itemIndexInView;
				num5 = num - itemIndexInView;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			switch (changeMode)
			{
			case ItemCountChangeMode.INSERT:
			{
				for (int i = 0; i < _VisibleItemsCount; i++)
				{
					TItemViewsHolder val2 = _VisibleItems[i];
					if (val2.itemIndexInView >= num)
					{
						ShiftViewsHolderItemIndexInView(val2, count);
					}
					if (val2.ItemIndex >= indexIfInsertingOrRemoving)
					{
						ShiftViewsHolderItemIndexAndFireEvent(val2, count, wasInsert: true, indexIfInsertingOrRemoving);
					}
				}
				flag = virtualScrollableArea < 0.0;
				if (contentPanelEndEdgeStationary)
				{
					if (flag)
					{
						contentInsetOverride = _InternalState.ctVirtualInsetFromVPS_Cached - num4;
					}
					if (_VisibleItemsCount > 0)
					{
						int num7 = num5 - 1;
						num6 = -0.1;
						if (num7 < 0)
						{
							num6 = 0.1;
						}
						else if (num5 >= _VisibleItemsCount)
						{
							DragVisibleItemsRangeUnchecked(0, _VisibleItemsCount, 0.0 - num4, updateCachedCTVirtualInset: false, updateCachedCTVirtualInset_ComputeVisibility: false);
						}
						else
						{
							double firstVHInsetFromEdge = _VisibleItems[num7].root.GetInsetFromParentEdge(_Params.Content, _InternalState.endEdge);
							DragVisibleItemsRangeUnchecked(0, num5, 0.0 - num4, updateCachedCTVirtualInset: false, updateCachedCTVirtualInset_ComputeVisibility: false);
							int num8;
							for (num8 = 0; num7 + num8 >= 0; num8--)
							{
								TItemViewsHolder val3 = _VisibleItems[num7 + num8];
								if ((double)val3.root.GetInsetFromParentEdge(_Params.Content, _InternalState.endEdge) > _InternalState.vpSize)
								{
									break;
								}
							}
							for (int num9 = num7 + num8; num9 >= 0; num9--)
							{
								RecycleOrStealViewsHolder(num9, stealInsteadOfRecycle);
							}
							int vhStartIndex = num5 - (num7 + num8 + 1);
							double num10 = AddViewsHoldersAndMakeVisible(firstVHInsetFromEdge, _InternalState.endEdge, vhStartIndex, num + count - 1, count, 0, -1);
							if (contentInsetOverride.HasValue)
							{
								contentInsetOverride -= num10;
							}
							else
							{
								contentInsetOverride = _InternalState.ctVirtualInsetFromVPS_Cached - num10;
							}
							flag = true;
						}
					}
					break;
				}
				if (flag)
				{
					contentInsetOverride = _InternalState.ctVirtualInsetFromVPS_Cached;
				}
				if (_VisibleItemsCount <= 0)
				{
					break;
				}
				if (num5 < _VisibleItemsCount)
				{
					if (num5 < 0)
					{
						num6 = 0.1;
						DragVisibleItemsRangeUnchecked(0, _VisibleItemsCount, num4, updateCachedCTVirtualInset: false, updateCachedCTVirtualInset_ComputeVisibility: false);
						break;
					}
					double firstVHInsetFromEdge2 = _VisibleItems[num5].root.GetInsetFromParentEdge(_Params.Content, _InternalState.startEdge);
					DragVisibleItemsRangeUnchecked(num5, _VisibleItemsCount, num4, updateCachedCTVirtualInset: false, updateCachedCTVirtualInset_ComputeVisibility: false);
					int j;
					for (j = num5; j < _VisibleItemsCount; j++)
					{
						TItemViewsHolder val4 = _VisibleItems[j];
						if ((double)val4.root.GetInsetFromParentEdge(_Params.Content, _InternalState.startEdge) > _InternalState.vpSize)
						{
							break;
						}
					}
					while (j < _VisibleItemsCount)
					{
						RecycleOrStealViewsHolder(j, stealInsteadOfRecycle);
					}
					AddViewsHoldersAndMakeVisible(firstVHInsetFromEdge2, _InternalState.startEdge, num5, num, count, 1, 1);
				}
				else
				{
					num6 = -0.1;
				}
				break;
			}
			case ItemCountChangeMode.REMOVE:
			{
				if (_VisibleItemsCount <= 0)
				{
					break;
				}
				flag = virtualScrollableArea < 0.0;
				int num11 = num5 + count;
				int num12 = num11 - 1;
				if (contentPanelEndEdgeStationary)
				{
					if (flag)
					{
						contentInsetOverride = _InternalState.ctVirtualInsetFromVPS_Cached - num4 + additionalCTDragAbstrDelta;
					}
					int num13;
					if (num11 > _VisibleItemsCount)
					{
						if (num5 < 0)
						{
							flag2 = true;
							break;
						}
						num6 = 0.1;
						num13 = _VisibleItemsCount - num5;
						if (num13 <= 0)
						{
							num5 = _VisibleItemsCount;
							flag3 = false;
						}
						else
						{
							for (int k = 0; k < num13; k++)
							{
								RecycleOrStealViewsHolder(num5, stealInsteadOfRecycle);
							}
							flag3 = true;
						}
					}
					else if (num5 < 0)
					{
						if (num12 < 0)
						{
							num5 = 0;
							flag3 = false;
						}
						else
						{
							num6 = 0.1;
							num5 = 0;
							num13 = num11;
							for (int l = 0; l < num13; l++)
							{
								RecycleOrStealViewsHolder(num5, stealInsteadOfRecycle);
							}
							flag3 = true;
						}
					}
					else
					{
						num6 = 0.1;
						num13 = count;
						for (int m = 0; m < num13; m++)
						{
							RecycleOrStealViewsHolder(num5, stealInsteadOfRecycle);
						}
						flag3 = true;
					}
					DragVisibleItemsRangeUnchecked(0, num5, additionalCTDragAbstrDelta - num4, updateCachedCTVirtualInset: false, updateCachedCTVirtualInset_ComputeVisibility: false);
					if (num5 < _VisibleItemsCount)
					{
						if (additionalCTDragAbstrDelta != 0.0)
						{
							DragVisibleItemsRangeUnchecked(num5, _VisibleItemsCount, additionalCTDragAbstrDelta, updateCachedCTVirtualInset: false, updateCachedCTVirtualInset_ComputeVisibility: false);
						}
						for (int n = num5; n < _VisibleItemsCount; n++)
						{
							TItemViewsHolder vh = _VisibleItems[n];
							ShiftViewsHolderItemIndexInView(vh, -count);
							ShiftViewsHolderItemIndexAndFireEvent(vh, -count, wasInsert: false, indexIfInsertingOrRemoving);
						}
					}
					break;
				}
				if (flag)
				{
					contentInsetOverride = _InternalState.ctVirtualInsetFromVPS_Cached + additionalCTDragAbstrDelta;
				}
				int num14 = count;
				int num15;
				if (num5 < 0)
				{
					if (num12 >= 0)
					{
						if (num12 >= _VisibleItemsCount)
						{
							flag2 = true;
							break;
						}
						num14 = -num5;
					}
					flag3 = num12 >= 0;
					num6 = -0.1;
					num15 = 0;
				}
				else if (num5 < _VisibleItemsCount)
				{
					num14 = Math.Max(0, num11 - _VisibleItemsCount);
					num6 = -0.1;
					flag3 = true;
					num15 = num5;
				}
				else
				{
					flag3 = false;
					num15 = num5;
				}
				int num16 = count - num14;
				for (int num17 = 0; num17 < num16; num17++)
				{
					RecycleOrStealViewsHolder(num15, stealInsteadOfRecycle);
				}
				if (additionalCTDragAbstrDelta != 0.0)
				{
					DragVisibleItemsRangeUnchecked(0, num15, additionalCTDragAbstrDelta, updateCachedCTVirtualInset: false, updateCachedCTVirtualInset_ComputeVisibility: false);
				}
				if (num15 < _VisibleItemsCount)
				{
					DragVisibleItemsRangeUnchecked(num15, _VisibleItemsCount, num4 + additionalCTDragAbstrDelta, updateCachedCTVirtualInset: false, updateCachedCTVirtualInset_ComputeVisibility: false);
					for (int num18 = num15; num18 < _VisibleItemsCount; num18++)
					{
						TItemViewsHolder vh2 = _VisibleItems[num18];
						ShiftViewsHolderItemIndexInView(vh2, -count);
						ShiftViewsHolderItemIndexAndFireEvent(vh2, -count, wasInsert: false, indexIfInsertingOrRemoving);
					}
				}
				break;
			}
			case ItemCountChangeMode.RESET:
				flag2 = true;
				break;
			}
			if (flag2)
			{
				double num19 = (contentPanelEndEdgeStationary ? (_InternalState.ctVirtualInsetFromVPS_Cached - num4) : _InternalState.ctVirtualInsetFromVPS_Cached);
				contentInsetOverride = num19 + additionalCTDragAbstrDelta;
				flag3 = false;
			}
			ContentSizeOrPositionChangeParams p = new ContentSizeOrPositionChangeParams
			{
				keepVelocity = keepVelocity,
				contentEndEdgeStationary = contentPanelEndEdgeStationary,
				allowOutsideBounds = flag,
				contentInsetOverride = contentInsetOverride
			};
			OnCumulatedSizesOfAllItemsChanged(ref p);
			if (itemsCount2 == 0)
			{
				ClearVisibleItems();
				ClearCachedRecyclableItems(includingBufferred: true);
			}
			else if (flag2)
			{
				RecycleAllVisibleViewsHolders();
			}
			double overrideScrollingDelta = (num6.HasValue ? num6.Value : ((itemsCount == 0) ? 0.0 : (contentPanelEndEdgeStationary ? 0.1 : ((cTVirtualInsetFromVPE_Cached < 0.0) ? (-0.1) : 0.1))));
			bool flag4 = !flag3 || (additionalCTDragAbstrDelta == 0.0 && _InternalState.VirtualScrollableArea > 0.0);
			if (_InternalState.computeVisibilityTwinPassScheduled)
			{
				_InternalState.computeVisibilityTwinPassScheduled = false;
				ComputeVisibilityTwinPass(_InternalState.preferKeepingCTEndEdgeStationaryInNextComputeVisibilityTwinPass);
			}
			else
			{
				_InternalState.lastComputeVisibilityHadATwinPass = false;
			}
			if (flag3)
			{
				CorrectPositionsOfVisibleItems(alsoCorrectTransversalPositioning: true, fireScrollPositionChangedEvent: false);
			}
			ComputeVisibilityForCurrentPositionRawParams(forceFireScrollViewPositionChangedEvent: false, potentialTwinPassCTEndStationaryPrioritizeUserPreference: true, overrideScrollingDelta);
			if (!flag4)
			{
				ComputeVisibilityForCurrentPositionRawParams(forceFireScrollViewPositionChangedEvent: false, potentialTwinPassCTEndStationaryPrioritizeUserPreference: true, 0.0 - overrideScrollingDelta);
			}
			CorrectPositionsOfVisibleItems(alsoCorrectTransversalPositioning: true, fireScrollPositionChangedEvent: true);
			if (keepVelocity)
			{
				_Velocity = velocity;
			}
			OnItemsRefreshed(itemsCount, itemsCount2);
			if (ItemsRefreshed != null)
			{
				ItemsRefreshed(itemsCount, itemsCount2);
			}
			_SkipComputeVisibilityInUpdateOrOnScroll = skipComputeVisibilityInUpdateOrOnScroll;
		}

		private void MyUpdate()
		{
			if (_InternalState.computeVisibilityTwinPassScheduled)
			{
				throw new OSAException("ScheduleComputeVisibilityTwinPass() can only be called during UpdateViewsHolder() or OnItemIndexChangedDueInsertOrRemove() !!!");
			}
			if (_ForceRebuildLayoutScheduled || _InternalState.HasScrollViewSizeChanged)
			{
				_ForceRebuildLayoutScheduled = false;
				OnScrollViewSizeChangedBase();
				RebuildLayoutDueToScrollViewSizeChange();
				Refresh(contentPanelEndEdgeStationary: false, keepVelocity: true);
				PostRebuildLayoutDueToScrollViewSizeChange();
			}
		}

		private void MyLateUpdate()
		{
			bool inProgress = _ReleaseFromPull.inProgress;
			double virtualScrollableArea = _InternalState.VirtualScrollableArea;
			double num = 0.0 - virtualScrollableArea;
			double emptyAreaWhenCTSmallerThanVP = Math.Max(0.0, num);
			bool flag = num <= 0.0;
			bool flag2 = false;
			if (!inProgress && flag && !_IsDragging && !_SkipComputeVisibilityInUpdateOrOnScroll && (bool)_Params.Snapper)
			{
				_Params.Snapper.StartSnappingIfNeeded();
				flag2 = _Params.Snapper.SnappingInProgress;
			}
			UpdateGalleryEffectIfNeeded(onlyIfEffectAmountChanged: true);
			float deltaTime = DeltaTime;
			bool flag3 = true;
			AllowContentOutsideBoundsMode allowOutsideBoundsMode = AllowContentOutsideBoundsMode.DO_NOT_ALLOW;
			bool flag4 = false;
			float currentVelocity = _Velocity[_InternalState.hor0_vert1];
			if (!flag)
			{
				_ReleaseFromPull.targetCTInsetFromVPS = _InternalState.GetTargetCTVirtualInsetFromVPSWhenCTSmallerThanVP(emptyAreaWhenCTSmallerThanVP);
				if (_IsDragging)
				{
					_ReleaseFromPull.inProgress = false;
					_Velocity[_InternalState.hor0_vert1] = 0f;
				}
				else
				{
					flag4 = _VisibleItemsCount > 0;
					if (flag4)
					{
						double num2 = _ReleaseFromPull.CalculateFirstItemInsetFromVPS();
						double num3 = _ReleaseFromPull.CalculateFirstItemTargetInsetFromVPS();
						_ReleaseFromPull.inProgress = Math.Abs(num2 - num3) >= 1.0;
						if (_Params.effects.ElasticMovement)
						{
							float currentVelocity2 = currentVelocity * (float)_InternalState.hor1_vertMinus1;
							Mathf.SmoothDamp((float)num2, (float)num3, ref currentVelocity2, _Params.effects.ReleaseTime, float.PositiveInfinity, deltaTime);
							if (_ReleaseFromPull.inProgress)
							{
								currentVelocity = currentVelocity2 * (float)_InternalState.hor1_vertMinus1;
							}
							else
							{
								currentVelocity = 0f;
							}
						}
						else
						{
							if (_ReleaseFromPull.inProgress)
							{
								_ReleaseFromPull.FinishNowByDraggingItems(computeVisibility: true);
							}
							currentVelocity = 0f;
						}
					}
					else
					{
						_ReleaseFromPull.inProgress = false;
					}
					_Velocity[_InternalState.hor0_vert1] = currentVelocity;
				}
			}
			else if (_IsDragging || flag2)
			{
				_ReleaseFromPull.inProgress = false;
			}
			else
			{
				double ctVirtualInsetFromVPS_Cached = _InternalState.ctVirtualInsetFromVPS_Cached;
				bool flag5 = ctVirtualInsetFromVPS_Cached > 0.0;
				double num4;
				if (flag5)
				{
					_ReleaseFromPull.targetCTInsetFromVPS = 0.0;
					num4 = ctVirtualInsetFromVPS_Cached;
				}
				else
				{
					double cTVirtualInsetFromVPE_Cached = _InternalState.CTVirtualInsetFromVPE_Cached;
					if (cTVirtualInsetFromVPE_Cached > 0.0)
					{
						_ReleaseFromPull.targetCTInsetFromVPS = num;
						num4 = cTVirtualInsetFromVPE_Cached;
					}
					else
					{
						num4 = 0.0;
					}
				}
				bool flag6 = num4 > 0.0;
				_ReleaseFromPull.inProgress = flag6;
				if (_Params.effects.ElasticMovement)
				{
					if (flag6)
					{
						allowOutsideBoundsMode = AllowContentOutsideBoundsMode.ALLOW_IF_OUTSIDE_AMOUNT_SHRINKS;
						flag3 = false;
						double num5 = Math.Min(num4, 3.4028234663852886E+38) * (double)(flag5 ? _InternalState.hor1_vertMinus1 : (-_InternalState.hor1_vertMinus1));
						float f = Mathf.SmoothDamp((float)num5, 0f, ref currentVelocity, _Params.effects.ReleaseTime, float.PositiveInfinity, deltaTime);
						_ReleaseFromPull.inProgress = Mathf.Abs(f) >= 1f;
						if (_ReleaseFromPull.inProgress)
						{
							_Velocity[_InternalState.hor0_vert1] = currentVelocity;
						}
						else
						{
							_Velocity[_InternalState.hor0_vert1] = 0f;
							currentVelocity = 0f;
							_ReleaseFromPull.FinishNowBySettingContentInset(computeVisibility: true);
						}
					}
					else
					{
						bool flag7 = inProgress && num4 < 0.1;
						flag3 = !flag7;
						allowOutsideBoundsMode = ((!flag7) ? AllowContentOutsideBoundsMode.ALLOW : AllowContentOutsideBoundsMode.DO_NOT_ALLOW);
						if (inProgress)
						{
							_Velocity[_InternalState.hor0_vert1] = 0f;
							currentVelocity = 0f;
							if (flag7)
							{
								_ReleaseFromPull.FinishNowBySettingContentInset(computeVisibility: true);
							}
						}
					}
				}
				else if (_Params.effects.LoopItems)
				{
					allowOutsideBoundsMode = AllowContentOutsideBoundsMode.ALLOW;
					if (flag6 && (virtualScrollableArea <= 0.0 || _VisibleItemsCount != _ItemsDesc.itemsCount))
					{
						_ReleaseFromPull.FinishNowBySettingContentInset(computeVisibility: true);
					}
				}
				else if (flag6)
				{
					_ReleaseFromPull.FinishNowBySettingContentInset(computeVisibility: true);
				}
			}
			if (_Params.effects.Inertia && !flag2)
			{
				float num6 = Mathf.Pow(1f - _Params.effects.InertiaDecelerationRate, deltaTime);
				if (_IsDragging)
				{
					_VelocityToAddOnDragEnd *= num6;
					float magnitude = _VelocityToAddOnDragEnd.magnitude;
					if (magnitude < 1f)
					{
						_VelocityToAddOnDragEnd = Vector2.zero;
					}
					else
					{
						float num7 = magnitude / _Params.effects.MaxSpeed;
						if (num7 > 1f)
						{
							_VelocityToAddOnDragEnd /= num7;
						}
					}
				}
				else if (flag3)
				{
					if (Mathf.Abs(currentVelocity) < 2f)
					{
						_Velocity[_InternalState.hor0_vert1] = 0f;
						int index = 1 - _InternalState.hor0_vert1;
						_Velocity[index] *= num6;
					}
					else
					{
						_Velocity *= num6;
					}
				}
			}
			Velocity = _Velocity;
			currentVelocity = _Velocity[_InternalState.hor0_vert1];
			if (!_IsDragging && !flag2 && currentVelocity != 0f)
			{
				float num8 = currentVelocity * (float)_InternalState.hor1_vertMinus1;
				if ((double)Math.Abs(num8) > 0.001)
				{
					double num9 = (double)num8 * (double)deltaTime;
					if (flag4)
					{
						DragVisibleItemsRangeUnchecked(0, _VisibleItemsCount, num9, updateCachedCTVirtualInset: true, updateCachedCTVirtualInset_ComputeVisibility: true);
					}
					else
					{
						Drag(num9, allowOutsideBoundsMode, cancelSnappingIfAny: false, out var _, out var _);
					}
				}
			}
		}

		private void MyOnDisable()
		{
			CancelAnimationsIfAny();
			_SkipComputeVisibilityInUpdateOrOnScroll = false;
		}

		private void OnScrollPositionChangedInternal()
		{
			UpdateGalleryEffectIfNeeded(onlyIfEffectAmountChanged: false);
			double normalizedPosition = GetNormalizedPosition();
			OnScrollPositionChanged(normalizedPosition);
			if (ScrollPositionChanged != null)
			{
				ScrollPositionChanged(normalizedPosition);
			}
		}

		private double GetDeltaForComputeVisibility()
		{
			return _InternalState.ctVirtualInsetFromVPS_Cached - _InternalState.lastProcessedCTVirtualInsetFromVPS;
		}

		private bool ComputeVisibilityForCurrentPosition(ComputeVisibilityParams p)
		{
			if (p.overrideDelta.HasValue)
			{
				return ComputeVisibilityForCurrentPositionRawParams(p.forceFireScrollPositionChangedEvent, p.potentialTwinPassCTEndStationaryPrioritizeUserPreference, p.overrideDelta.Value);
			}
			return ComputeVisibilityForCurrentPosition(p.forceFireScrollPositionChangedEvent, p.potentialTwinPassCTEndStationaryPrioritizeUserPreference);
		}

		private bool ComputeVisibilityForCurrentPositionRawParams(bool forceFireScrollViewPositionChangedEvent, bool potentialTwinPassCTEndStationaryPrioritizeUserPreference, double overrideScrollingDelta)
		{
			_InternalState.lastProcessedCTVirtualInsetFromVPS = _InternalState.ctVirtualInsetFromVPS_Cached - overrideScrollingDelta;
			return ComputeVisibilityForCurrentPosition(forceFireScrollViewPositionChangedEvent, potentialTwinPassCTEndStationaryPrioritizeUserPreference);
		}

		private bool ComputeVisibilityForCurrentPosition(bool forceFireScrollViewPositionChangedEvent, bool potentialTwinPassCTEndStationaryPrioritizeUserPreference)
		{
			if (_InternalState.computeVisibilityTwinPassScheduled)
			{
				throw new OSAException("ScheduleComputeVisibilityTwinPass() can only be called during UpdateViewsHolder() or OnItemIndexChangedDueInsertOrRemove() !!!");
			}
			Vector2 velocity = _Velocity;
			double deltaForComputeVisibility = GetDeltaForComputeVisibility();
			bool result = false;
			if (_Params.effects.LoopItems)
			{
				result = LoopIfNeeded(deltaForComputeVisibility);
			}
			_ComputeVisibilityManager.ComputeVisibility(deltaForComputeVisibility);
			bool flag;
			if (!_InternalState.computeVisibilityTwinPassScheduled)
			{
				flag = deltaForComputeVisibility != 0.0;
				_InternalState.lastComputeVisibilityHadATwinPass = false;
			}
			else
			{
				_InternalState.computeVisibilityTwinPassScheduled = false;
				int num = 20;
				int num2 = 20;
				double num3 = ((deltaForComputeVisibility == 0.0) ? (-0.1) : deltaForComputeVisibility);
				bool contentEndEdgeStationary = ((potentialTwinPassCTEndStationaryPrioritizeUserPreference || deltaForComputeVisibility == 0.0) ? _InternalState.preferKeepingCTEndEdgeStationaryInNextComputeVisibilityTwinPass : (deltaForComputeVisibility > 0.0));
				double ctVirtualSize;
				do
				{
					ComputeVisibilityTwinPass(contentEndEdgeStationary);
					ctVirtualSize = _InternalState.ctVirtualSize;
					_ComputeVisibilityManager.ComputeVisibility(num3);
					_ComputeVisibilityManager.ComputeVisibility(0.0 - num3);
					if (_InternalState.computeVisibilityTwinPassScheduled)
					{
						ComputeVisibilityTwinPass(contentEndEdgeStationary);
						_InternalState.computeVisibilityTwinPassScheduled = false;
					}
					num--;
					if (num == 0)
					{
						throw new OSAException("Max iterations (" + num2 + ") reached for TwinPass. \nIf you're using ContentSizeFitter, make sure the DefaultItemSize is smaller than the size of any generated item.\nIf you're also using BaseParamsWithPrefab for the params, DefaultItemSize will be automatically set to the prefab's size, so in this case make the prefab as small as possible instead.");
					}
				}
				while (ctVirtualSize != _InternalState.ctVirtualSize);
				flag = true;
			}
			_InternalState.UpdateLastProcessedCTVirtualInsetFromVPStart();
			if (!_IsDragging)
			{
				_Velocity = velocity;
			}
			if (forceFireScrollViewPositionChangedEvent || flag)
			{
				OnScrollPositionChangedInternal();
			}
			return result;
		}

		private void ComputeVisibilityTwinPass(bool contentEndEdgeStationary)
		{
			if (_VisibleItemsCount == 0)
			{
				throw new OSAException("computeVisibilityTwinPassScheduled, but there are no visible items.ScheduleComputeVisibilityTwinPass() can only be called during UpdateViewsHolder() or OnItemIndexChangedDueInsertOrRemove() !!!");
			}
			int itemsCount = GetItemsCount();
			if (_Params.effects.LoopItems && itemsCount > 800000000)
			{
				throw new OSAException("If looping is enabled, ComputeVisibilityTwinPass can only be used if item count is less than " + 800000000 + " (currently having " + itemsCount + "). This prevents UI overlaps due to rounding errors");
			}
			bool skipComputeVisibilityInUpdateOrOnScroll = _SkipComputeVisibilityInUpdateOrOnScroll;
			_SkipComputeVisibilityInUpdateOrOnScroll = true;
			double[] array = new double[_VisibleItemsCount];
			Action<TItemViewsHolder> action = ((!_Params.IsHorizontal) ? new Action<TItemViewsHolder>(OnItemHeightChangedPreTwinPass) : new Action<TItemViewsHolder>(OnItemWidthChangedPreTwinPass));
			for (int i = 0; i < _VisibleItemsCount; i++)
			{
				TItemViewsHolder val = _VisibleItems[i];
				array[i] = UpdateItemSizeOnTwinPass(val);
				action(val);
			}
			OnItemsSizesChangedExternally(_VisibleItems, array, contentEndEdgeStationary);
			_SkipComputeVisibilityInUpdateOrOnScroll = skipComputeVisibilityInUpdateOrOnScroll;
			_InternalState.lastComputeVisibilityHadATwinPass = true;
		}

		private bool LoopIfNeeded(double delta)
		{
			if (delta == 0.0)
			{
				return false;
			}
			double virtualScrollableArea = _InternalState.VirtualScrollableArea;
			if (virtualScrollableArea <= 0.0)
			{
				return false;
			}
			if (_VisibleItemsCount == 0)
			{
				_ = _ItemsDesc.itemsCount;
				return false;
			}
			ContentSizeOrPositionChangeParams p = new ContentSizeOrPositionChangeParams
			{
				keepVelocity = true,
				allowOutsideBounds = true
			};
			TItemViewsHolder val = _VisibleItems[0];
			TItemViewsHolder val2 = _VisibleItems[_VisibleItemsCount - 1];
			int itemIndexInView = val2.itemIndexInView;
			int num = _ItemsDesc.itemsCount - 1;
			int firstItemIndexInViewIfLooped;
			if (delta > 0.0)
			{
				if (val.itemIndexInView != 0)
				{
					return false;
				}
				if (itemIndexInView == num && val2.root.GetInsetFromParentEdge(_Params.Content, _InternalState.endEdge) >= 0f)
				{
					return false;
				}
				if (val.root.GetInsetFromParentEdge(_Params.Content, _InternalState.startEdge) < 0f)
				{
					return false;
				}
				firstItemIndexInViewIfLooped = _ItemsDesc.GetItemRealIndexFromViewIndex(itemIndexInView + 1);
				for (int i = 0; i < _VisibleItemsCount; i++)
				{
					_VisibleItems[i].itemIndexInView = _ItemsDesc.itemsCount - _VisibleItemsCount + i;
				}
			}
			else
			{
				if (itemIndexInView != num)
				{
					return false;
				}
				if (val.itemIndexInView == 0 && val.root.GetInsetFromParentEdge(_Params.Content, _InternalState.startEdge) >= 0f)
				{
					return false;
				}
				if (val2.root.GetInsetFromParentEdge(_Params.Content, _InternalState.endEdge) < 0f)
				{
					return false;
				}
				firstItemIndexInViewIfLooped = val.ItemIndex;
				for (int j = 0; j < _VisibleItemsCount; j++)
				{
					_VisibleItems[j].itemIndexInView = j;
				}
			}
			_ItemsDesc.RotateItemsSizesOnScrollViewLooped(firstItemIndexInViewIfLooped);
			UpdateCTVrtInsetFromVPS(ref p);
			CorrectPositionsOfVisibleItems(alsoCorrectTransversalPositioning: true, fireScrollPositionChangedEvent: true);
			_InternalState.UpdateLastProcessedCTVirtualInsetFromVPStart();
			return true;
		}

		private void CorrectPositionsOfVisibleItems(bool alsoCorrectTransversalPositioning, bool fireScrollPositionChangedEvent)
		{
			if (_VisibleItemsCount > 0)
			{
				_InternalState.CorrectPositions(_VisibleItems, alsoCorrectTransversalPositioning);
			}
			if (fireScrollPositionChangedEvent)
			{
				OnScrollPositionChangedInternal();
			}
		}

		internal TItemViewsHolder ExtractRecyclableViewsHolderOrCreateNew(int indexOfItemThatWillBecomeVisible, double sizeOfItem)
		{
			TItemViewsHolder val = TryExtractRecyclableViewsHolderFrom(_RecyclableItems, indexOfItemThatWillBecomeVisible, sizeOfItem);
			if (val == null)
			{
				val = TryExtractRecyclableViewsHolderFrom(_BufferredRecyclableItems, indexOfItemThatWillBecomeVisible, sizeOfItem);
				if (val == null)
				{
					val = CreateViewsHolder(indexOfItemThatWillBecomeVisible);
				}
			}
			return val;
		}

		private TItemViewsHolder TryExtractRecyclableViewsHolderFrom(IList<TItemViewsHolder> vhsToChooseFrom, int indexOfItemThatWillBecomeVisible, double sizeOfItem)
		{
			int num = 0;
			while (num < vhsToChooseFrom.Count)
			{
				TItemViewsHolder val = vhsToChooseFrom[num];
				if (IsRecyclable(val, indexOfItemThatWillBecomeVisible, sizeOfItem))
				{
					OnBeforeRecycleOrDisableViewsHolder(val, indexOfItemThatWillBecomeVisible);
					vhsToChooseFrom.RemoveAt(num);
					return val;
				}
				num++;
			}
			return null;
		}

		internal void AddViewsHolderAndMakeVisible(TItemViewsHolder vh, int vhIndex, int itemIndex, int itemIndexInView, double realInsetFromEdge, RectTransform.Edge insetEdge, double size)
		{
			_VisibleItems.Insert(vhIndex, vh);
			_VisibleItemsCount++;
			if (itemIndexInView < 0 || itemIndexInView >= _ItemsDesc.itemsCount)
			{
				throw new OSAException("OSA internal error: itemIndexInView " + itemIndexInView + ", while itemsCount is " + _ItemsDesc.itemsCount);
			}
			vh.ItemIndex = itemIndex;
			vh.itemIndexInView = itemIndexInView;
			RectTransform root = vh.root;
			root.SetParent(_Params.Content, worldPositionStays: false);
			SetViewsHolderEnabled(vh);
			UpdateViewsHolder(vh);
			if (!IsViewsHolderEnabled(vh))
			{
				string text = (_Params.optimization.ScaleToZeroInsteadOfDisable ? "have a zero scale" : "be disabled");
				throw new OSAException("AddViewsHolderAndMakeVisible: VH detected to " + text + " after UpdateViewsHolder() was called on it. This is not allowed. " + vh.root);
			}
			Vector2 constantAnchorPosForAllItems = _InternalState.layoutInfo.constantAnchorPosForAllItems;
			root.anchorMax = constantAnchorPosForAllItems;
			root.anchorMin = constantAnchorPosForAllItems;
			if (_Params.optimization.KeepItemsSortedInHierarchy && vhIndex < _Params.Content.childCount)
			{
				root.SetSiblingIndex(vhIndex);
			}
			root.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(insetEdge, (float)realInsetFromEdge, (float)size);
			_InternalState.GetTransversalInsetStartAndSizeToUse(vh, out var insetStart, out var size2);
			root.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(_InternalState.transvStartEdge, insetStart, size2);
		}

		private double AddViewsHoldersAndMakeVisible(double firstVHInsetFromEdge, RectTransform.Edge insetEdge, int vhStartIndex, int startIndexInView, int maxCount, int vhIndexIncrement, int itemIndexInViewIncrementSign)
		{
			int num = 0;
			int num2 = startIndexInView;
			int num3 = vhStartIndex;
			double num4 = firstVHInsetFromEdge;
			while (num < maxCount && num4 <= _InternalState.vpSize)
			{
				double num5 = _ItemsDesc[num2];
				int itemRealIndexFromViewIndex = _ItemsDesc.GetItemRealIndexFromViewIndex(num2);
				TItemViewsHolder vh = ExtractRecyclableViewsHolderOrCreateNew(itemRealIndexFromViewIndex, num5);
				AddViewsHolderAndMakeVisible(vh, num3, itemRealIndexFromViewIndex, num2, num4, insetEdge, num5);
				num4 += num5 + _InternalState.spacing;
				num++;
				num3 += vhIndexIncrement;
				num2 += itemIndexInViewIncrementSign;
			}
			double num6 = ((startIndexInView > 0) ? _ItemsDesc.GetItemSizeCumulative(startIndexInView - 1) : 0.0);
			return _ItemsDesc.GetItemSizeCumulative(startIndexInView + maxCount - 1) - num6 + (double)maxCount * _InternalState.spacing;
		}

		internal int GetNumExcessRecycleableItems()
		{
			if (_RecyclableItems.Count > 1)
			{
				int num = _RecyclableItems.Count + _VisibleItemsCount - GetMaxNumObjectsToKeepInMemory();
				if (num > 0)
				{
					return num;
				}
			}
			return 0;
		}

		private int GetMaxNumObjectsToKeepInMemory()
		{
			if (_Params.optimization.RecycleBinCapacity <= 0)
			{
				return _ItemsDesc.maxVisibleItemsSeenSinceLastScrollViewSizeChange + _ItemsDesc.destroyedItemsSinceLastScrollViewSizeChange + 1;
			}
			return _Params.optimization.RecycleBinCapacity + _VisibleItemsCount;
		}

		internal IList<TItemViewsHolder> CreateBufferredRecycleableItems(int count, int indexToPass = -1)
		{
			List<TItemViewsHolder> list = new List<TItemViewsHolder>(count);
			for (int i = 0; i < count; i++)
			{
				TItemViewsHolder val = CreateViewsHolder(indexToPass);
				val.root.SetParent(_Params.Content, worldPositionStays: false);
				SetViewsHolderDisabled(val);
				list.Add(val);
			}
			return list;
		}

		internal void AddBufferredRecycleableItems(IList<TItemViewsHolder> vhs)
		{
			foreach (TItemViewsHolder vh in vhs)
			{
				AddBufferredRecycleableItem(vh);
			}
		}

		internal void AddBufferredRecycleableItem(TItemViewsHolder vh)
		{
			_BufferredRecyclableItems.Add(vh);
		}

		private void UpdateGalleryEffectIfNeeded(bool onlyIfEffectAmountChanged)
		{
			float overallAmount = _Params.effects.Gallery.OverallAmount;
			if (_PrevGalleryEffectAmount == overallAmount && onlyIfEffectAmountChanged)
			{
				return;
			}
			if (overallAmount == 0f)
			{
				if (_PrevGalleryEffectAmount == overallAmount)
				{
					return;
				}
				RemoveGalleryEffectFromItems(_RecyclableItems);
				RemoveGalleryEffectFromItems(_BufferredRecyclableItems);
				RemoveGalleryEffectFromItems(_VisibleItems);
			}
			else
			{
				if (_VisibleItemsCount == 0 || _ItemsDesc.itemsCount == 0)
				{
					return;
				}
				for (int i = 0; i < _VisibleItemsCount; i++)
				{
					TItemViewsHolder val = _VisibleItems[i];
					double vhCenterRealInsetFromStart = (double)val.root.GetInsetFromParentEdge(_Params.Content, _InternalState.startEdge) + _ItemsDesc[val.itemIndexInView] / 2.0;
					val.root.localScale = ComposeGalleryEffectFinalAmount(_Params.effects.Gallery.Scale, vhCenterRealInsetFromStart);
					val.root.localEulerAngles = ComposeGalleryEffectFinalAmount(_Params.effects.Gallery.Rotation, vhCenterRealInsetFromStart);
				}
			}
			_PrevGalleryEffectAmount = _Params.effects.Gallery.OverallAmount;
		}

		private Vector3 ComposeGalleryEffectFinalAmount(GalleryAnimation effectParams, double vhCenterRealInsetFromStart)
		{
			double vpSize = _InternalState.vpSize;
			double val = Math.Max(0.0, vhCenterRealInsetFromStart / vpSize);
			val = Math.Min(1.0, val);
			float viewportPivot = effectParams.ViewportPivot;
			double num = Math.Min(2.0, Math.Abs(val - (double)viewportPivot));
			double num2 = Math.Abs((double)viewportPivot - 0.5) + 0.5;
			float amount = Mathf.Clamp01(Mathf.Pow(1f - (float)(num / num2), Mathf.Clamp(effectParams.Exponent, 1f, 50f)));
			float t = effectParams.Amount * _Params.effects.Gallery.OverallAmount;
			if (effectParams == _Params.effects.Gallery.Scale)
			{
				Vector3 one = Vector3.one;
				Vector3 b = effectParams.TransformSpace.Transform(amount);
				float minValue = _Params.effects.Gallery.Scale.MinValue;
				b.x = Mathf.Max(minValue, b.x);
				b.y = Mathf.Max(minValue, b.y);
				b.z = Mathf.Max(minValue, b.z);
				return Vector3.Lerp(one, b, t);
			}
			Vector3 zero = Vector3.zero;
			Quaternion b2 = Quaternion.Lerp(Quaternion.Euler(effectParams.TransformSpace.From), Quaternion.Euler(effectParams.TransformSpace.To), amount);
			Vector3 _ = b2.eulerAngles;
			return Quaternion.Lerp(Quaternion.Euler(zero), b2, t).eulerAngles;
		}

		private void RemoveGalleryEffectFromItems(IList<TItemViewsHolder> vhs)
		{
			if (vhs == null)
			{
				return;
			}
			foreach (TItemViewsHolder vh in vhs)
			{
				if (vh == null || !vh.root)
				{
					continue;
				}
				if (_Params.optimization.ScaleToZeroInsteadOfDisable)
				{
					if (vh.root.localScale != Vector3.zero)
					{
						vh.root.localScale = Vector3.zero;
					}
				}
				else
				{
					vh.root.localScale = Vector3.one;
				}
				vh.root.localEulerAngles = Vector3.zero;
			}
		}

		private void OnCumulatedSizesOfAllItemsChanged(ref ContentSizeOrPositionChangeParams p)
		{
			_InternalState.ctVirtualSize = _InternalState.CalculateContentVirtualSize();
			_ReleaseFromPull.inProgress = false;
			UpdateCTVrtInsetFromVPS(ref p);
		}

		private double ChangeItemSizeAndUpdateContentSizeAccordingly(TItemViewsHolder viewsHolder, int itemIndexInView, double curSize, double requestedSize, bool itemEndEdgeStationary)
		{
			if (_ItemsDesc.itemsCount == 0)
			{
				throw new OSAException("Cannot change item size if the items count is 0!");
			}
			if (_Params.effects.LoopItems && itemEndEdgeStationary)
			{
				throw new OSAException("If looping is enabled, changing an item's size with contentPanelEndEdgeStationary=true is not supported yet");
			}
			double num = _InternalState.CalculateContentVirtualSize();
			_ItemsDesc.BeginChangingItemsSizes(itemIndexInView);
			double num2 = (_ItemsDesc[itemIndexInView] = requestedSize);
			_ItemsDesc.EndChangingItemsSizes();
			double num3 = num2 - curSize;
			if (viewsHolder != null)
			{
				RectTransform.Edge edge;
				float inset;
				if (itemEndEdgeStationary)
				{
					DragVisibleItemsRangeUnchecked(0, _VisibleItems.IndexOf(viewsHolder), 0.0 - num3, updateCachedCTVirtualInset: false, updateCachedCTVirtualInset_ComputeVisibility: false);
					edge = _InternalState.endEdge;
					inset = viewsHolder.root.GetInsetFromParentEdge(_Params.Content, edge);
				}
				else
				{
					DragVisibleItemsRangeUnchecked(_VisibleItems.IndexOf(viewsHolder) + 1, _VisibleItemsCount, num3, updateCachedCTVirtualInset: false, updateCachedCTVirtualInset_ComputeVisibility: false);
					edge = _InternalState.startEdge;
					inset = viewsHolder.root.GetInsetFromParentEdge(_Params.Content, edge);
				}
				viewsHolder.root.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(edge, inset, (float)num2);
			}
			double num4 = _InternalState.CalculateContentVirtualSize();
			double deltaSize = num4 - num;
			double additionalCTDragAbstrDelta = 0.0;
			_InternalState.CorrectParametersOnCTSizeChange(itemEndEdgeStationary, out var ctInsetFromVPSOverride, ref additionalCTDragAbstrDelta, num4, deltaSize);
			double? contentInsetOverride = null;
			if (ctInsetFromVPSOverride.HasValue)
			{
				contentInsetOverride = ctInsetFromVPSOverride.Value;
			}
			else if (viewsHolder == null)
			{
				int itemIndexInView2 = _VisibleItems[0].itemIndexInView;
				contentInsetOverride = ((!itemEndEdgeStationary || itemIndexInView >= itemIndexInView2) ? ((!itemEndEdgeStationary && itemIndexInView < itemIndexInView2) ? new double?(_InternalState.ctVirtualInsetFromVPS_Cached - num3) : new double?(_InternalState.ctVirtualInsetFromVPS_Cached)) : new double?(_InternalState.ctVirtualInsetFromVPS_Cached));
			}
			if (additionalCTDragAbstrDelta != 0.0)
			{
				DragVisibleItemsRangeUnchecked(0, _VisibleItemsCount, additionalCTDragAbstrDelta, updateCachedCTVirtualInset: false, updateCachedCTVirtualInset_ComputeVisibility: false);
			}
			bool flag = _InternalState.VirtualScrollableArea <= 0.0;
			ContentSizeOrPositionChangeParams p = new ContentSizeOrPositionChangeParams
			{
				keepVelocity = true,
				contentEndEdgeStationary = itemEndEdgeStationary,
				allowOutsideBounds = (_Params.effects.LoopItems || flag),
				contentInsetOverride = contentInsetOverride,
				fireScrollPositionChangedEvent = true
			};
			OnCumulatedSizesOfAllItemsChanged(ref p);
			return num2;
		}

		private void OnItemsSizesChangedExternally(List<TItemViewsHolder> vhs, double[] sizes, bool itemEndEdgeStationary)
		{
			if (_ItemsDesc.itemsCount == 0)
			{
				throw new OSAException("Cannot change item sizes externally if the items count is 0!");
			}
			int count = vhs.Count;
			double num = _InternalState.CalculateContentVirtualSize();
			_ItemsDesc.BeginChangingItemsSizes(vhs[0].itemIndexInView);
			for (int i = 0; i < count; i++)
			{
				TItemViewsHolder val = vhs[i];
				_ItemsDesc[val.itemIndexInView] = sizes[i];
			}
			_ItemsDesc.EndChangingItemsSizes();
			double num2 = _InternalState.CalculateContentVirtualSize();
			double num3 = num2 - num;
			double additionalCTDragAbstrDelta = 0.0;
			_InternalState.CorrectParametersOnCTSizeChange(itemEndEdgeStationary, out var _, ref additionalCTDragAbstrDelta, num2, num3);
			double num4 = (itemEndEdgeStationary ? num3 : 0.0);
			ContentSizeOrPositionChangeParams p = new ContentSizeOrPositionChangeParams
			{
				cancelSnappingIfAny = true,
				contentEndEdgeStationary = itemEndEdgeStationary,
				keepVelocity = true,
				allowOutsideBounds = true,
				contentInsetOverride = _InternalState.ctVirtualInsetFromVPS_Cached - num4 + additionalCTDragAbstrDelta
			};
			OnCumulatedSizesOfAllItemsChanged(ref p);
			CorrectPositionsOfVisibleItems(alsoCorrectTransversalPositioning: true, fireScrollPositionChangedEvent: true);
		}

		private void OnScrollViewSizeChangedBase()
		{
			if (ScrollViewSizeChanged != null)
			{
				ScrollViewSizeChanged();
			}
			OnScrollViewSizeChanged();
		}
	}
}
