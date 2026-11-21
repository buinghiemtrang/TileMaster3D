using Com.TheFallenGames.OSA.Core;
using frame8.Logic.Misc.Other.Extensions;
using UnityEngine;

namespace Com.TheFallenGames.OSA.AdditionalComponents
{
	public class OSAContentDecorator : MonoBehaviour
	{
		public enum InsetEdgeEnum
		{
			START = 0,
			END = 1
		}

		public enum ControlOSAPaddingMode
		{
			DONT_CONTROL = 0,
			ONCE_AT_INIT = 1,
			ADAPTIVE = 2
		}

		[SerializeField]
		private InsetEdgeEnum _InsetEdge;

		[SerializeField]
		[Tooltip("How far from the InsetEdge should this be positioned. Can be normalized ([0, 1]), if InsetIsNormalized=true, or raw. Default is 0")]
		private float _Inset;

		[SerializeField]
		[Tooltip("If false, will interpret the Inset proeprty as the raw distance from InsetEdge, rather than the normalized inset relative to the content's full size")]
		private bool _InsetIsNormalized = true;

		[SerializeField]
		private bool _DisableWhenNotVisible = true;

		[SerializeField]
		[Tooltip("If false, won't be dragged together with the OSA's content when it's pulled when already at the scrolling limit")]
		private bool _AffectedByElasticity;

		[SerializeField]
		[Tooltip("Sets when the OSA's padding from InsetEdge will be controlled to be the same as this object's size.\nOnce at initialization, or adapting continuously, or none (i.e. you'll probably set OSA's padding manually, in case the decorator shouldn't overlap with items).\nOnly works if Inset is 0")]
		private ControlOSAPaddingMode _ControlOSAPaddingAtInsetEdge = ControlOSAPaddingMode.ONCE_AT_INIT;

		[SerializeField]
		[Tooltip("If null, will use the first an implementation of IOSA found in parents")]
		private RectTransform _OSATransform;

		private RectTransform _ParentRT;

		private RectTransform _RT;

		private IOSA _OSA;

		private bool _Initialized;

		private double _MyLastKnownSize;

		public void Init()
		{
			_RT = base.transform as RectTransform;
			_ParentRT = _RT.parent as RectTransform;
			if (_OSA == null)
			{
				_OSA = _ParentRT.GetComponentInParent(typeof(IOSA)) as IOSA;
				if (_OSA == null)
				{
					throw new OSAException("Component implementing IOSA not found in parents");
				}
			}
			else
			{
				_OSA = _OSATransform.GetComponent(typeof(IOSA)) as IOSA;
				if (_OSA == null)
				{
					throw new OSAException("Component implementing IOSA not found on the specified object '" + _OSATransform.name + "'");
				}
			}
			if (_OSA.BaseParameters.Viewport != _ParentRT)
			{
				throw new OSAException(typeof(OSAContentDecorator).Name + " can only work when attached to a direct child of OSA's Viewport.");
			}
			if (_ControlOSAPaddingAtInsetEdge != ControlOSAPaddingMode.DONT_CONTROL)
			{
				if (_OSA.IsInitialized)
				{
					Debug.Log("OSA's content padding can't be set after OSA was initialized. You're most probably calling OSA.Init manually(), in which case make sure to also manually call Init() on this decorator, before OSA.Init()");
				}
				else
				{
					if (_Inset != 0f)
					{
						Debug.Log("OSA's content padding can't be controlled if Inset (" + _Inset + ") is not 0 . ");
					}
					SetOSAPadding();
				}
			}
			_OSA.ScrollPositionChanged += OSAScrollPositionChanged;
			Vector3 localPosition = _RT.localPosition;
			_RT.anchorMax = Vector2.up;
			_RT.anchorMin = Vector2.up;
			_RT.localPosition = localPosition;
			_Initialized = true;
		}

		private void Awake()
		{
			if (!_Initialized)
			{
				Init();
			}
			base.gameObject.SetActive(value: false);
		}

		private void Update()
		{
			if (_ControlOSAPaddingAtInsetEdge == ControlOSAPaddingMode.ADAPTIVE && _OSA != null && _OSA.IsInitialized)
			{
				float num = _RT.rect.size[_OSA.GetLayoutInfoReadonly().hor0_vert1];
				if (_MyLastKnownSize != (double)num)
				{
					SetPadding(num);
					_OSA.ScheduleForceRebuildLayout();
				}
			}
		}

		private void SetOSAPadding()
		{
			Rect rect = _RT.rect;
			float num = (_OSA.IsHorizontal ? rect.width : rect.height);
			SetPadding(num);
		}

		private void SetPadding(double myNewSize)
		{
			BaseParams baseParameters = _OSA.BaseParameters;
			int num = (int)(myNewSize + 0.6000000238418579);
			if (_InsetEdge == InsetEdgeEnum.START)
			{
				if (baseParameters.Orientation == BaseParams.OrientationEnum.HORIZONTAL)
				{
					baseParameters.ContentPadding.left = num;
				}
				else
				{
					baseParameters.ContentPadding.top = num;
				}
			}
			else if (baseParameters.Orientation == BaseParams.OrientationEnum.HORIZONTAL)
			{
				baseParameters.ContentPadding.right = num;
			}
			else
			{
				baseParameters.ContentPadding.bottom = num;
			}
			_MyLastKnownSize = myNewSize;
		}

		private void OSAScrollPositionChanged(double scrollPos)
		{
			LayoutInfo layoutInfoReadonly = _OSA.GetLayoutInfoReadonly();
			double num;
			RectTransform.Edge fixedEdge;
			if (_InsetEdge == InsetEdgeEnum.START)
			{
				num = _OSA.ContentVirtualInsetFromViewportStart;
				fixedEdge = layoutInfoReadonly.startEdge;
			}
			else
			{
				num = _OSA.ContentVirtualInsetFromViewportEnd;
				fixedEdge = layoutInfoReadonly.endEdge;
			}
			float num2 = _Inset;
			float num3 = _RT.rect.size[layoutInfoReadonly.hor0_vert1];
			double num4 = num2;
			double num5 = layoutInfoReadonly.vpSize;
			if (_InsetIsNormalized)
			{
				num4 *= _OSA.GetContentSize() - (double)num3;
			}
			num += num4;
			bool flag;
			if (num >= 0.0)
			{
				if (num >= num5)
				{
					num = num5;
					flag = _DisableWhenNotVisible;
				}
				else
				{
					flag = false;
				}
			}
			else
			{
				num5 = 0f - num3;
				if (num <= num5)
				{
					num = num5;
					flag = _DisableWhenNotVisible;
				}
				else
				{
					flag = false;
				}
			}
			if (flag == base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(!flag);
			}
			if (!flag)
			{
				float newInset = ((num <= 0.0 || _AffectedByElasticity) ? ((float)num) : 0f);
				_RT.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(fixedEdge, newInset, num3);
			}
		}
	}
}
