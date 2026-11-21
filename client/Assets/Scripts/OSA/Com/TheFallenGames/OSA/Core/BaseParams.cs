using System;
using Com.TheFallenGames.OSA.Core.Data.Gallery;
using frame8.Logic.Misc.Other.Extensions;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Com.TheFallenGames.OSA.Core
{
	[Serializable]
	public class BaseParams
	{
		public enum OrientationEnum
		{
			VERTICAL = 0,
			HORIZONTAL = 1
		}

		public enum ContentGravity
		{
			FROM_PIVOT = 0,
			START = 1,
			CENTER = 2,
			END = 3
		}

		[Serializable]
		public class Effects
		{
			public const float DEFAULT_MAX_SPEED = 10000f;

			public const float MAX_SPEED = 1000000f;

			public const float MAX_SPEED_IF_LOOPING = 10000f;

			[Tooltip("This RawImage will be scrolled together with the content. \nThe content is always stationary (this is the way the recycling process works), so the scrolling effect is faked by scrolling the texture's x/y.\nTip: use a seamless/looping background texture for best visual results")]
			[FormerlySerializedAs("contentVisual")]
			[SerializeField]
			private RawImage _ContentVisual;

			[FormerlySerializedAs("elasticMovement")]
			[SerializeField]
			private bool _ElasticMovement = true;

			[FormerlySerializedAs("pullElasticity")]
			[SerializeField]
			private float _PullElasticity = 0.3f;

			[FormerlySerializedAs("releaseTime")]
			[SerializeField]
			private float _ReleaseTime = 0.1f;

			[FormerlySerializedAs("inertia")]
			[SerializeField]
			private bool _Inertia = true;

			[Tooltip("What percent (0=0%, 1=100%) of the velociy will be lost per second after the drag ended. 1=all(immediate stop), 0=none(maitain constant scrolling speed indefinitely)")]
			[Range(0f, 1f)]
			[FormerlySerializedAs("inertiaDecelerationRate")]
			[SerializeField]
			private float _InertiaDecelerationRate = 0.865f;

			[SerializeField]
			[Tooltip("Stop any movement from inertia or scrolling animations when a mouse click/touch begins")]
			private bool _CutMovementOnPointerDown = true;

			[FormerlySerializedAs("maxSpeed")]
			[SerializeField]
			private float _MaxSpeed = 10000f;

			[Tooltip("If enabled, multiple drags in the same direction will lead to greater speeds")]
			[SerializeField]
			private bool _TransientSpeedBetweenDrags = true;

			[Tooltip("If true: When the last item is reached, the first one appears after it, basically allowing you to scroll infinitely.\n Initially intended for things like spinners, but it can be used for anything alike.\n It may interfere with other functionalities in some very obscure/complex contexts/setups, so be sure to test the hell out of it.\n Also please note that sometimes during dragging the content, the actual looping changes the Unity's internal PointerEventData for the current click/touch pointer id, so if you're also externally tracking the current click/touch, in this case only 'PointerEventData.pointerCurrentRaycast' and 'PointerEventData.position'(current position) are preserved, the other ones are reset to defaults to assure a smooth loop transition. Sorry for the long decription. Here's an ASCII potato: @")]
			[FormerlySerializedAs("loopItems")]
			[SerializeField]
			private bool _LoopItems;

			[Tooltip("The contentVisual's additional drag factor. Examples:\n-2: the contentVisual will move exactly by the same amount as the items, but in the opposite direction\n-1: no movement\n 0: same speed (together with the items)\n 1: 2x faster in the same direction\n 2: 3x faster etc.")]
			[Range(-5f, 5f)]
			[SerializeField]
			private float _ContentVisualParallaxEffect = -0.85f;

			[SerializeField]
			private GalleryEffectParams _Gallery = new GalleryEffectParams();

			[Range(0f, 1f)]
			[FormerlySerializedAs("galleryEffectAmount")]
			[SerializeField]
			[HideInInspector]
			private float _GalleryEffectAmount;

			[Range(0f, 1f)]
			[FormerlySerializedAs("galleryEffectViewportPivot")]
			[SerializeField]
			[HideInInspector]
			private float _GalleryEffectViewportPivot = 0.5f;

			private bool _HasContentVisual;

			[Obsolete("Use ContentVisual instead", true)]
			public RawImage contentVisual
			{
				get
				{
					return _ContentVisual;
				}
				set
				{
					_ContentVisual = value;
				}
			}

			public RawImage ContentVisual
			{
				get
				{
					return _ContentVisual;
				}
				set
				{
					_ContentVisual = value;
				}
			}

			[Obsolete("Use ElasticMovement instead", true)]
			public bool elasticMovement
			{
				get
				{
					return _ElasticMovement;
				}
				set
				{
					_ElasticMovement = value;
				}
			}

			public bool ElasticMovement
			{
				get
				{
					return _ElasticMovement;
				}
				set
				{
					_ElasticMovement = value;
				}
			}

			[Obsolete("Use PullElasticity instead", true)]
			public float pullElasticity
			{
				get
				{
					return _PullElasticity;
				}
				set
				{
					_PullElasticity = value;
				}
			}

			public float PullElasticity
			{
				get
				{
					return _PullElasticity;
				}
				set
				{
					_PullElasticity = value;
				}
			}

			[Obsolete("Use ReleaseTime instead", true)]
			public float releaseTime
			{
				get
				{
					return _ReleaseTime;
				}
				set
				{
					_ReleaseTime = value;
				}
			}

			public float ReleaseTime
			{
				get
				{
					return _ReleaseTime;
				}
				set
				{
					_ReleaseTime = value;
				}
			}

			[Obsolete("Use Inertia instead", true)]
			public bool inertia
			{
				get
				{
					return _Inertia;
				}
				set
				{
					_Inertia = value;
				}
			}

			public bool Inertia
			{
				get
				{
					return _Inertia;
				}
				set
				{
					_Inertia = value;
				}
			}

			[Obsolete("Use InertiaDecelerationRate instead", true)]
			public float inertiaDecelerationRate
			{
				get
				{
					return _InertiaDecelerationRate;
				}
				set
				{
					_InertiaDecelerationRate = value;
				}
			}

			public float InertiaDecelerationRate
			{
				get
				{
					return _InertiaDecelerationRate;
				}
				set
				{
					_InertiaDecelerationRate = value;
				}
			}

			public bool CutMovementOnPointerDown
			{
				get
				{
					return _CutMovementOnPointerDown;
				}
				set
				{
					_CutMovementOnPointerDown = value;
				}
			}

			[Obsolete("Use MaxSpeed instead", true)]
			public float maxSpeed
			{
				get
				{
					return _MaxSpeed;
				}
				set
				{
					_MaxSpeed = value;
				}
			}

			public float MaxSpeed
			{
				get
				{
					return _MaxSpeed;
				}
				set
				{
					_MaxSpeed = value;
				}
			}

			public bool TransientSpeedBetweenDrags
			{
				get
				{
					return _TransientSpeedBetweenDrags;
				}
				protected set
				{
					_TransientSpeedBetweenDrags = value;
				}
			}

			[Obsolete("Use LoopItems instead", true)]
			public bool loopItems
			{
				get
				{
					return _LoopItems;
				}
				set
				{
					_LoopItems = value;
				}
			}

			public bool LoopItems
			{
				get
				{
					return _LoopItems;
				}
				set
				{
					_LoopItems = value;
				}
			}

			public float ContentVisualParallaxEffect
			{
				get
				{
					return _ContentVisualParallaxEffect;
				}
				set
				{
					_ContentVisualParallaxEffect = value;
				}
			}

			public GalleryEffectParams Gallery
			{
				get
				{
					return _Gallery;
				}
				set
				{
					_Gallery = value;
				}
			}

			[Obsolete("Use GalleryEffectAmount instead", true)]
			public float galleryEffectAmount
			{
				get
				{
					return _Gallery.OverallAmount;
				}
				set
				{
				_Gallery.OverallAmount = value;
				}
			}

			[Obsolete("Use Gallery.OverallAmount instead")]
			public float GalleryEffectAmount
			{
				get
				{
					return _Gallery.OverallAmount;
				}
				set
				{
				_Gallery.OverallAmount = value;
				}
			}

			[Obsolete("Use GalleryEffectViewportPivot instead", true)]
			public float galleryEffectViewportPivot
			{
				get
				{
					return _Gallery.Scale.ViewportPivot;
				}
				set
				{
				_Gallery.Scale.ViewportPivot = value;
				}
			}

			[Obsolete("Use Gallery.Scale.ViewportPivot instead")]
			public float GalleryEffectViewportPivot
			{
				get
				{
					return _Gallery.Scale.ViewportPivot;
				}
				set
				{
				_Gallery.Scale.ViewportPivot = value;
				}
			}

			public bool HasContentVisual => _HasContentVisual;

			public void InitIfNeeded()
			{
				_HasContentVisual = _ContentVisual != null;
				float num = (_LoopItems ? 10000f : 1000000f);
				string text = (_LoopItems ? "MAX_SPEED_IF_LOOPING" : "MAX_SPEED");
				float num2 = Mathf.Clamp(_MaxSpeed, 0f, num);
				if (Mathf.Abs(num2 - _MaxSpeed) > 1f)
				{
					Debug.Log("OSA: maxSpeed(" + _MaxSpeed.ToString("#########.00") + ") value is negative or exceeded " + text + "(" + num.ToString("#########.00") + "). Clamped it to " + num2.ToString("#########.00"));
					_MaxSpeed = num2;
				}
				if (_ElasticMovement && _LoopItems)
				{
					_ElasticMovement = false;
					Debug.Log("OSA: 'elasticMovement' was set to false, because 'loopItems' is true. Elasticity only makes sense when there is an end");
				}
				if (_HasContentVisual)
				{
					_ContentVisual.rectTransform.MatchParentSize(preservePivot: true);
				}
				InitGalleryEffectMigrations();
			}

			private void InitGalleryEffectMigrations()
			{
				float num = 0f;
				if (_GalleryEffectAmount != num)
				{
					string text = "OSA: Please go to BaseParams.cs, comment the [HideInInspector] attribute on _GalleryEffectAmount, then set this property to " + num + " (the default) in inspector, as this property will be removed in next versions. Use Gallery.OverallAmount instead to set the gallery effect amount. It's available through inspector. Will use _GalleryEffectAmount as the active one.";
					if (_Gallery.OverallAmount != num)
					{
						// UNVERIFIED: the closing literal after the number (")") could not be resolved uniquely from the string table
						text = text + ". Additional migration warning: Both _GalleryEffectAmount and Gallery.OverallAmount are non-default (non " + num + ")";
					}
					_Gallery.OverallAmount = _GalleryEffectAmount;
					Debug.Log(text);
				}
				num = 0.5f;
				if (_GalleryEffectViewportPivot != num)
				{
					string text2 = "OSA: Please go to BaseParams.cs, comment the [HideInInspector] attribute on _GalleryEffectViewportPivot, then set this property to " + num + " (the default) in inspector, as this property will be removed in next versions. Use Gallery.Scale.ViewportPivot instead to set the gallery effect pivot. It's available through inspector. Preserving the value of _GalleryEffectViewportPivot as the active one.";
					if (_Gallery.Scale.ViewportPivot != num)
					{
						// UNVERIFIED: the closing literal after the number (")") could not be resolved uniquely from the string table
						text2 = text2 + ". Additional migration warning: Both _GalleryEffectViewportPivot and Gallery.Scale.ViewportPivot are non-default (non " + num + ")";
					}
					_Gallery.Scale.ViewportPivot = _GalleryEffectViewportPivot;
					Debug.Log(text2);
				}
			}
		}

		[Serializable]
		public class Optimization
		{
			[Tooltip("How much objects besides the visible ones to keep in memory at max. \nBy default, no more than the heuristically found \"ideal\" number of items will be held in memory.\nSet to a positive integer to limit it. In this case, you'll either use more RAM than needed or more CPU than needed. One advantage is that you can get a predictable usage of the resources (for example, by specifying a constant bin size, no item will be destroyed if the actual visible items count won't exceed that).\nNote that this doesn't include the 'buffered' items, which by design can't be directly destroyed")]
			[FormerlySerializedAs("recycleBinCapacity")]
			[SerializeField]
			private int _RecycleBinCapacity = -1;

			[FormerlySerializedAs("scaleToZeroInsteadOfDisable")]
			[SerializeField]
			private bool _ScaleToZeroInsteadOfDisable;

			[FormerlySerializedAs("forceLayoutRebuildOnBeginSmoothScroll")]
			[SerializeField]
			private bool _ForceLayoutRebuildOnBeginSmoothScroll = true;

			[FormerlySerializedAs("forceLayoutRebuildOnDrag")]
			[SerializeField]
			private bool _ForceLayoutRebuildOnDrag;

			[SerializeField]
			[Tooltip("Whether to sort the actual GameObjects representing the items under the Content. Only use if needed, as it slightly affects performance. Off by default")]
			private bool _KeepItemsSortedInHierarchy;

			[Obsolete("Use RecycleBinCapacity instead", true)]
			public int recycleBinCapacity
			{
				get
				{
					return _RecycleBinCapacity;
				}
				set
				{
					_RecycleBinCapacity = value;
				}
			}

			public int RecycleBinCapacity
			{
				get
				{
					return _RecycleBinCapacity;
				}
				set
				{
					_RecycleBinCapacity = value;
				}
			}

			[Obsolete("Use ScaleToZeroInsteadOfDisable instead", true)]
			public bool scaleToZeroInsteadOfDisable
			{
				get
				{
					return _ScaleToZeroInsteadOfDisable;
				}
				set
				{
					_ScaleToZeroInsteadOfDisable = value;
				}
			}

			public bool ScaleToZeroInsteadOfDisable
			{
				get
				{
					return _ScaleToZeroInsteadOfDisable;
				}
				set
				{
					_ScaleToZeroInsteadOfDisable = value;
				}
			}

			[Obsolete("Use ForceLayoutRebuildOnBeginSmoothScroll instead", true)]
			public bool forceLayoutRebuildOnBeginSmoothScroll
			{
				get
				{
					return _ForceLayoutRebuildOnBeginSmoothScroll;
				}
				set
				{
					_ForceLayoutRebuildOnBeginSmoothScroll = value;
				}
			}

			public bool ForceLayoutRebuildOnBeginSmoothScroll
			{
				get
				{
					return _ForceLayoutRebuildOnBeginSmoothScroll;
				}
				set
				{
					_ForceLayoutRebuildOnBeginSmoothScroll = value;
				}
			}

			[Obsolete("Use ForceLayoutRebuildOnDrag instead", true)]
			public bool forceLayoutRebuildOnDrag
			{
				get
				{
					return _ForceLayoutRebuildOnDrag;
				}
				set
				{
					_ForceLayoutRebuildOnDrag = value;
				}
			}

			public bool ForceLayoutRebuildOnDrag
			{
				get
				{
					return _ForceLayoutRebuildOnDrag;
				}
				set
				{
					_ForceLayoutRebuildOnDrag = value;
				}
			}

			public bool KeepItemsSortedInHierarchy
			{
				get
				{
					return _KeepItemsSortedInHierarchy;
				}
				set
				{
					_KeepItemsSortedInHierarchy = value;
				}
			}
		}

		[SerializeField]
		private RectTransform _Content;

		[Tooltip("If null, the scrollRect is considered to be the viewport")]
		[SerializeField]
		[FormerlySerializedAs("viewport")]
		private RectTransform _Viewport;

		[SerializeField]
		private OrientationEnum _Orientation;

		[SerializeField]
		private Scrollbar _Scrollbar;

		[Tooltip("The sensivity to the Mouse's scrolling wheel or similar input methods. Not related to dragging or scrolling via scrollbar")]
		[SerializeField]
		private float _ScrollSensivity = 100f;

		[Tooltip("The sensivity to the Mouse's horizontal scrolling wheel (where supported) or similar input methods that send scroll signals on the horizontal axis. Not related to dragging or scrolling via scrollbar. \nIt's set to a positive value by default to comply with Unity's ScrollRect (which for some reason inverts the left and right directions). \nIf you'll nest OSA in a regular horizontal ScrollRect, set the ScrollRect's sensivity to a positive value and OSA's ScrollSensivityOnXAxis to a negative value to get an intuitive scroll")]
		[SerializeField]
		private float _ScrollSensivityOnXAxis = 100f;

		[SerializeField]
		[Tooltip("Padding for the 4 edges of the content panel.\nTip: if using a fixed, constant ItemTransversalSize, also set the paddings in the transversal direction to -1 (left/right for vertical ScrollView and vice-versa). This aligns the item in the center, transversally")]
		[FormerlySerializedAs("contentPadding")]
		private RectOffset _ContentPadding = new RectOffset();

		[SerializeField]
		[FormerlySerializedAs("contentGravity")]
		private ContentGravity _Gravity = ContentGravity.START;

		[Tooltip("The space between items")]
		[SerializeField]
		[FormerlySerializedAs("contentSpacing")]
		private float _ContentSpacing;

		[Tooltip("The size of all items for which the size is not specified in CollectItemSizes()")]
		[SerializeField]
		private float _DefaultItemSize = 60f;

		[Tooltip("You'll probably need this if the scroll view is a child of another scroll view. If enabled, the first parent that implements all of IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler and IEndDragHandler, as well as the first IScrollHandler found in parents (but it should be on the same game object as other handlers, IF they're found),  will receive these events when they occur on this scroll view. This works both with Unity's ScrollRect and OSA")]
		[SerializeField]
		private bool _ForwardDragToParents;

		[Tooltip("It forwards the drag/scroll in the same direction to the parent when the current scrolled position is at boundary, thus allowing scrolling through nested Scroll Views that have the same scroll direction. \nForwardDragToParents also needs to be enabled for this to work.")]
		[SerializeField]
		private bool _ForwardDragSameDirectionAtBoundary;

		[Tooltip("Allows you to click and drag the content directly (enabled by default). The property ForwardDragToParents is not affected by this")]
		[SerializeField]
		private bool _DragEnabled = true;

		[Tooltip("Allows you to scroll by mouse wheel or other similar input devices (enabled by default). The property ForwardDragToParents is not affected by this")]
		[SerializeField]
		private bool _ScrollEnabled = true;

		[Tooltip("If enabled, will use Time.unscaledTime instead of Time.time, which means the animations, inertia etc. won't be affected by the Time.timeScale")]
		[SerializeField]
		private bool _UseUnscaledTime = true;

		[Tooltip("The item size in the direction perpendicular to scrolling direction. Width for vertical ScrollViews, and vice-versa.\n-1 = items won't have their widths (heights for horizontal ScrollViews) changed at all - you'll be responsible for them\n0 = fill to available space, taking ContentPadding into account\n>0 = fixed size; in this case, it's better to also set transversal padding to -1 so the item will be centered\n")]
		[SerializeField]
		private float _ItemTransversalSize;

		public Effects effects = new Effects();

		public Optimization optimization = new Optimization();

		private RectTransform _ScrollViewRT;

		private Snapper8 _Snapper;

		public RectTransform Content
		{
			get
			{
				return _Content;
			}
			set
			{
				_Content = value;
			}
		}

		public RectTransform Viewport
		{
			get
			{
				return _Viewport;
			}
			set
			{
				_Viewport = value;
			}
		}

		public OrientationEnum Orientation
		{
			get
			{
				return _Orientation;
			}
			set
			{
				_Orientation = value;
			}
		}

		public Scrollbar Scrollbar
		{
			get
			{
				return _Scrollbar;
			}
			set
			{
				_Scrollbar = value;
			}
		}

		public float ScrollSensivity
		{
			get
			{
				return _ScrollSensivity;
			}
			set
			{
				_ScrollSensivity = value;
			}
		}

		public float ScrollSensivityOnXAxis
		{
			get
			{
				return _ScrollSensivityOnXAxis;
			}
			set
			{
				_ScrollSensivityOnXAxis = value;
			}
		}

		public RectOffset ContentPadding
		{
			get
			{
				return _ContentPadding;
			}
			set
			{
				_ContentPadding = value;
			}
		}

		public ContentGravity Gravity
		{
			get
			{
				return _Gravity;
			}
			set
			{
				_Gravity = value;
			}
		}

		public float ContentSpacing
		{
			get
			{
				return _ContentSpacing;
			}
			set
			{
				_ContentSpacing = value;
			}
		}

		public float DefaultItemSize
		{
			get
			{
				return _DefaultItemSize;
			}
			protected set
			{
				_DefaultItemSize = value;
			}
		}

		public bool ForwardDragToParents
		{
			get
			{
				return _ForwardDragToParents;
			}
			protected set
			{
				_ForwardDragToParents = value;
			}
		}

		public bool ForwardDragSameDirectionAtBoundary
		{
			get
			{
				return _ForwardDragSameDirectionAtBoundary;
			}
			protected set
			{
				_ForwardDragSameDirectionAtBoundary = value;
			}
		}

		public bool DragEnabled
		{
			get
			{
				return _DragEnabled;
			}
			protected set
			{
				_DragEnabled = value;
			}
		}

		public bool ScrollEnabled
		{
			get
			{
				return _ScrollEnabled;
			}
			protected set
			{
				_ScrollEnabled = value;
			}
		}

		public bool UseUnscaledTime
		{
			get
			{
				return _UseUnscaledTime;
			}
			set
			{
				_UseUnscaledTime = value;
			}
		}

		public float ItemTransversalSize
		{
			get
			{
				return _ItemTransversalSize;
			}
			set
			{
				_ItemTransversalSize = value;
			}
		}

		public bool IsHorizontal => _Orientation == OrientationEnum.HORIZONTAL;

		public RectTransform ScrollViewRT => _ScrollViewRT;

		public Snapper8 Snapper => _Snapper;

		public void PrepareForInit(bool firstTime)
		{
			if (!_Content)
			{
				throw new OSAException("Content cannot be null");
			}
			_Content.MatchParentSize(preservePivot: true);
			if (!firstTime)
			{
				Canvas.ForceUpdateCanvases();
			}
		}

		public virtual void InitIfNeeded(IOSA iAdapter)
		{
			_ScrollViewRT = iAdapter.AsMonoBehaviour.transform as RectTransform;
			LayoutRebuilder.ForceRebuildLayoutImmediate(_ScrollViewRT);
			AssertValidWidthHeight(_ScrollViewRT);
			ScrollRect component = _ScrollViewRT.GetComponent<ScrollRect>();
			if ((bool)component && component.enabled)
			{
				throw new OSAException("The ScrollRect is not needed anymore starting with v4.0. Remove or disable it!");
			}
			if (!_Content)
			{
				throw new OSAException("Content not set!");
			}
			if (!_Viewport)
			{
				_Viewport = _ScrollViewRT;
				if (_Content.parent != _ScrollViewRT)
				{
					throw new OSAException("Content's parent should be the ScrollView itself if there's no viewport specified!");
				}
			}
			if (!_Snapper)
			{
				_Snapper = _ScrollViewRT.GetComponent<Snapper8>();
			}
			if (_ForwardDragSameDirectionAtBoundary && !_ForwardDragToParents)
			{
				Debug.Log("OSA: ForwardDragSameDirectionAtBoundary is true, but ForwardDragToParents is false. This will have no effect");
			}
			if ((_ContentPadding.left == -1) == (_ContentPadding.right != -1))
			{
				Debug.Log("OSA: ContentPadding.right and .left should either be both zero or positive or both -1. Setting both to 0...");
				_ContentPadding.right = 0;
				_ContentPadding.left = 0;
			}
			if ((_ContentPadding.top == -1) == (_ContentPadding.bottom != -1))
			{
				Debug.Log("OSA: ContentPadding.top and .bottom should either be both zero or positive or both -1. Setting both to 0...");
				_ContentPadding.bottom = 0;
				_ContentPadding.top = 0;
			}
			effects.InitIfNeeded();
			if (!effects.LoopItems)
			{
				return;
			}
			int num = (int)_ContentSpacing;
			bool flag = false;
			if (_Orientation == OrientationEnum.HORIZONTAL)
			{
				if (_ContentPadding.left != num)
				{
					_ContentPadding.left = num;
					flag = true;
				}
				if (_ContentPadding.right != num)
				{
					_ContentPadding.right = num;
					flag = true;
				}
			}
			else
			{
				if (_ContentPadding.top != num)
				{
					_ContentPadding.top = num;
					flag = true;
				}
				if (_ContentPadding.bottom != num)
				{
					_ContentPadding.bottom = num;
					flag = true;
				}
			}
			if (flag)
			{
				Debug.Log("OSA: setting conteng padding to be the same as content spacing (" + _ContentSpacing.ToString("#############.##") + "), because looping is enabled");
			}
		}

		public void UpdateContentPivotFromGravityType()
		{
			if (_Gravity != ContentGravity.FROM_PIVOT)
			{
				bool isHorizontal = _Orientation == OrientationEnum.HORIZONTAL;
				float num = ((_Gravity < ContentGravity.END) ? (1f / (float)_Gravity) : 0f);
				Vector2 pivot = ((!isHorizontal) ? new Vector2(0.5f, num) : new Vector2(1f - num, 0.5f));
				_Content.pivot = pivot;
			}
		}

		public void ApplyScrollSensitivityTo(ref Vector2 vec)
		{
			vec.x *= _ScrollSensivityOnXAxis;
			vec.y *= _ScrollSensivity;
		}

		internal void AssertValidWidthHeight(RectTransform rt)
		{
			string text = null;
			float num = 0f;
			Rect rect = rt.rect;
			if (rect.width < 1f)
			{
				text = "width";
				num = rect.width;
			}
			else if (rect.height < 1f)
			{
				text = "height";
				num = rect.height;
			}
			if (text != null)
			{
				throw new OSAException("OSA: '" + rt.name + "' reports a zero or negative " + text + "(" + num + "). \nThis can happen if you don't have a Canvas component in the OSA's parents or if you accidentally set an invalid size in editor. \nIf you instantiate the ScrollView at runtime, make sure you use the version of Object.Instantiate(..) that also takes the parent so it can be directly instantiated in it. The parent should be a Canvas or a descendant of a Canvas");
			}
		}
	}
}
