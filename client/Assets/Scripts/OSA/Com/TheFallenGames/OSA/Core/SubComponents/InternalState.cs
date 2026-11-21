using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using frame8.Logic.Misc.Other;
using frame8.Logic.Misc.Other.Extensions;

namespace Com.TheFallenGames.OSA.Core.SubComponents
{
	internal class InternalState<TItemViewsHolder> where TItemViewsHolder : BaseItemViewsHolder
	{
		internal LayoutInfo layoutInfo;

		internal double lastProcessedCTVirtualInsetFromVPS;

		internal double ctVirtualSize;

		internal bool computeVisibilityTwinPassScheduled;

		internal bool preferKeepingCTEndEdgeStationaryInNextComputeVisibilityTwinPass;

		internal bool lastComputeVisibilityHadATwinPass;

		private ItemsDescriptor _ItemsDesc;

		private BaseParams _SourceParams;

		internal double ProximityToLimitNeeded01ToResetPos => _SourceParams.effects.ElasticMovement ? 1.0 : 0.9999995;

		internal double vpSize => layoutInfo.vpSize;

		internal double paddingContentStart => layoutInfo.paddingContentStart;

		internal double transversalPaddingContentStart => layoutInfo.transversalPaddingContentStart;

		internal double paddingContentEnd => layoutInfo.paddingContentEnd;

		internal double paddingStartPlusEnd => layoutInfo.paddingStartPlusEnd;

		internal double spacing => layoutInfo.spacing;

		internal RectTransform.Edge startEdge => layoutInfo.startEdge;

		internal RectTransform.Edge endEdge => layoutInfo.endEdge;

		internal RectTransform.Edge transvStartEdge => layoutInfo.transvStartEdge;

		internal int hor0_vert1 => layoutInfo.hor0_vert1;

		internal int hor1_vertMinus1 => layoutInfo.hor1_vertMinus1;

		internal double ctVirtualInsetFromVPS_Cached { get; private set; }

		internal Vector2 scrollViewSize => layoutInfo.scrollViewSize;

		internal bool HasScrollViewSizeChanged
		{
			get
			{
				Rect rect = _SourceParams.ScrollViewRT.rect;
				if (_SourceParams.ItemTransversalSize == -1f && layoutInfo.transversalPaddingContentStart != -1.0)
				{
					if (layoutInfo.scrollViewSize[layoutInfo.hor0_vert1] != rect.size[layoutInfo.hor0_vert1])
					{
						return true;
					}
				}
				else if (layoutInfo.scrollViewSize != rect.size)
				{
					return true;
				}
				return _SourceParams.Viewport.rect.size[layoutInfo.hor0_vert1] != (float)layoutInfo.vpSize;
			}
		}

		internal double CTVirtualInsetFromVPE_Cached => layoutInfo.vpSize - ctVirtualSize - ctVirtualInsetFromVPS_Cached;

		internal double VirtualScrollableArea => ctVirtualSize - layoutInfo.vpSize;

		internal double AbstractPivot01 => (double)(_SourceParams.Content.pivot[layoutInfo.hor0_vert1] * (float)layoutInfo.hor1_vertMinus1 + (float)layoutInfo.hor0_vert1);

		internal static InternalState<TItemViewsHolder> CreateFromSourceParamsOrThrow(BaseParams sourceParams, ItemsDescriptor itemsDescriptor)
		{
			return new InternalState<TItemViewsHolder>(sourceParams, itemsDescriptor);
		}

		protected InternalState(BaseParams sourceParams, ItemsDescriptor itemsDescriptor)
		{
			layoutInfo = new LayoutInfo();
			_SourceParams = sourceParams;
			_ItemsDesc = itemsDescriptor;
			LayoutGroup component = sourceParams.Content.GetComponent<LayoutGroup>();
			if ((bool)component && component.enabled)
			{
				component.enabled = false;
				Debug.Log("LayoutGroup on GameObject " + component.name + " has beed disabled in order to use OSA");
			}
			ContentSizeFitter component2 = sourceParams.Content.GetComponent<ContentSizeFitter>();
			if ((bool)component2 && component2.enabled)
			{
				component2.enabled = false;
				Debug.Log("ContentSizeFitter on GameObject " + component2.name + " has beed disabled in order to use OSA");
			}
			LayoutElement component3 = sourceParams.Content.GetComponent<LayoutElement>();
			if ((bool)component3)
			{
				UnityEngine.Object.Destroy(component3);
				// theo pseudo-C (RVA 0x17d2d8c) ten trong log lay tu bien ContentSizeFitter, khong phai LayoutElement
				Debug.Log("LayoutElement on GameObject " + component2.name + " has beed DESTROYED in order to use OSA");
			}
		}

		internal void CacheScrollViewInfo()
		{
			layoutInfo.CacheScrollViewInfo(_SourceParams);
		}

		public Vector2 GetPointerPositionInCTSpace(PointerEventData currentPointerEventData)
		{
			return Singleton8<UIUtils8>.Instance.ScreenPointToLocalPointInRectangle(_SourceParams.Content, currentPointerEventData);
		}

		public double GetCTAbstractSpaceVectorLongitudinalComponentFromCTSpaceVector(Vector2 vectorCTSpace)
		{
			return (double)vectorCTSpace[layoutInfo.hor0_vert1] * (double)layoutInfo.hor1_vertMinus1;
		}

		internal double CalculateContentVirtualSize()
		{
			return _ItemsDesc.CumulatedSizeOfAllItems + layoutInfo.spacing * (double)Math.Max(0, _ItemsDesc.itemsCount - 1) + layoutInfo.paddingStartPlusEnd;
		}

		internal void CorrectPositions(List<TItemViewsHolder> vhs, bool alsoCorrectTransversalPositioning)
		{
			float insetStart = 0f;
			float size = 0f;
			int count = vhs.Count;
			double num = GetItemVirtualInsetFromParentStartUsingItemIndexInView(vhs[0].itemIndexInView);
			if (0 < count)
			{
				num += ctVirtualInsetFromVPS_Cached;
				for (int i = 0; i < count; i++)
				{
					TItemViewsHolder val = vhs[i];
					double num2 = _ItemsDesc[val.itemIndexInView];
					val.root.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(layoutInfo.startEdge, (float)num, (float)num2);
					double spacing = layoutInfo.spacing;
					if (alsoCorrectTransversalPositioning)
					{
						GetTransversalInsetStartAndSizeToUse(val, out insetStart, out size);
						val.root.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(layoutInfo.transvStartEdge, insetStart, size);
					}
					num = num + num2 + spacing;
				}
			}
		}

		internal void UpdateLastProcessedCTVirtualInsetFromVPStart()
		{
			lastProcessedCTVirtualInsetFromVPS = ctVirtualInsetFromVPS_Cached;
		}

		internal double GetVirtualAbstractNormalizedScrollPosition()
		{
			double virtualScrollableArea = VirtualScrollableArea;
			double result = 1.0;
			if (0.0 < virtualScrollableArea)
			{
				result = Math.Min(0.0, ctVirtualInsetFromVPS_Cached) / virtualScrollableArea + 1.0;
			}
			return result;
		}

		internal void UpdateCachedCTVirtInsetFromVPS(double virtualInset, bool allowOutsideBounds)
		{
			if (!allowOutsideBounds)
			{
				double virtualScrollableArea = VirtualScrollableArea;
				double num = 0.0 - virtualScrollableArea;
				double num2 = 0.0;
				if (virtualScrollableArea < 0.0)
				{
					num = AbstractPivot01 * num;
					num2 = num;
				}
				virtualInset = Math.Max(num, Math.Min(num2, virtualInset));
			}
			ctVirtualInsetFromVPS_Cached = virtualInset;
			if (_SourceParams.optimization.ForceLayoutRebuildOnDrag)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(_SourceParams.ScrollViewRT);
			}
		}

		internal double GetItemVirtualInsetFromParentStartUsingItemIndexInView(int itemIndexInView)
		{
			if (itemIndexInView < 1)
			{
				return 0.0 + layoutInfo.paddingContentStart;
			}
			return _ItemsDesc.GetItemSizeCumulative(itemIndexInView - 1) + layoutInfo.spacing * (double)itemIndexInView + layoutInfo.paddingContentStart;
		}

		internal double GetItemVirtualInsetFromParentEndUsingItemIndexInView(int itemIndexInView)
		{
			return ctVirtualSize - GetItemVirtualInsetFromParentStartUsingItemIndexInView(itemIndexInView) - _ItemsDesc[itemIndexInView];
		}

		internal double GetItemInferredRealInsetFromParentStart(int itemIndexInView)
		{
			return GetItemVirtualInsetFromParentStartUsingItemIndexInView(itemIndexInView) + ctVirtualInsetFromVPS_Cached;
		}

		internal double GetItemInferredRealInsetFromParentEnd(int itemIndexInView)
		{
			return layoutInfo.vpSize - (GetItemVirtualInsetFromParentStartUsingItemIndexInView(itemIndexInView) + ctVirtualInsetFromVPS_Cached) - _ItemsDesc[itemIndexInView];
		}

		internal double GetContentInferredRealInsetFromVPS(TItemViewsHolder firstVH)
		{
			return (double)firstVH.root.GetInsetFromParentEdge(_SourceParams.Content, layoutInfo.startEdge) - layoutInfo.paddingContentStart;
		}

		internal double GetContentInferredRealInsetFromVPE(TItemViewsHolder lastVH)
		{
			return (double)lastVH.root.GetInsetFromParentEdge(_SourceParams.Content, layoutInfo.endEdge) - layoutInfo.paddingContentEnd;
		}

		internal double ConvertItemInsetFromParentStart_FromVirtualToInferredReal(double virtualInsetFromParrentStart)
		{
			return ctVirtualInsetFromVPS_Cached + virtualInsetFromParrentStart;
		}

		internal double GetTargetCTVirtualInsetFromVPSWhenCTSmallerThanVP()
		{
			return 0.0 - (ctVirtualSize - layoutInfo.vpSize) * AbstractPivot01;
		}

		internal double GetTargetCTVirtualInsetFromVPSWhenCTSmallerThanVP(double emptyAreaWhenCTSmallerThanVP)
		{
			return AbstractPivot01 * emptyAreaWhenCTSmallerThanVP;
		}

		internal void GetTransversalInsetStartAndSizeToUse(TItemViewsHolder vh, out float insetStart, out float size)
		{
			float itemTransversalSize = _SourceParams.ItemTransversalSize;
			if (itemTransversalSize == 0f)
			{
				size = (float)layoutInfo.itemsConstantTransversalSize;
				insetStart = (float)layoutInfo.transversalPaddingContentStart;
				return;
			}
			if (itemTransversalSize == -1f)
			{
				size = vh.root.rect.size[1 - layoutInfo.hor0_vert1];
			}
			else
			{
				size = itemTransversalSize;
			}
			if (layoutInfo.transversalPaddingStartPlusEnd == -1.0)
			{
				insetStart = (float)((layoutInfo.transversalContentSize - (double)size) * 0.5);
			}
			else
			{
				insetStart = (float)layoutInfo.transversalPaddingContentStart;
			}
		}

		internal void CorrectParametersOnCTSizeChange(bool contentPanelEndEdgeStationary, out double? ctInsetFromVPSOverride, ref double additionalCTDragAbstrDelta, double newCTSize, double deltaSize)
		{
			if (0.0 <= deltaSize)
			{
				ctInsetFromVPSOverride = null;
				return;
			}
			double vpSize = layoutInfo.vpSize;
			if (newCTSize - vpSize < 0.0)
			{
				ctInsetFromVPSOverride = 0.0 - (newCTSize - vpSize) * AbstractPivot01;
				return;
			}
			if (!contentPanelEndEdgeStationary)
			{
				double cTVirtualInsetFromVPE_Cached = CTVirtualInsetFromVPE_Cached;
				if (cTVirtualInsetFromVPE_Cached <= 0.0)
				{
					if (0.0 <= cTVirtualInsetFromVPE_Cached - deltaSize)
					{
						ctInsetFromVPSOverride = vpSize - newCTSize;
						additionalCTDragAbstrDelta = cTVirtualInsetFromVPE_Cached - deltaSize;
						return;
					}
					ctInsetFromVPSOverride = null;
					return;
				}
				ctInsetFromVPSOverride = vpSize - newCTSize;
				additionalCTDragAbstrDelta = cTVirtualInsetFromVPE_Cached - deltaSize;
			}
			else
			{
				double cached = ctVirtualInsetFromVPS_Cached;
				if (cached <= 0.0)
				{
					if (cached - deltaSize < 0.0)
					{
						ctInsetFromVPSOverride = null;
						return;
					}
					ctInsetFromVPSOverride = 0.0;
					additionalCTDragAbstrDelta = 0.0 - (cached - deltaSize);
				}
				else
				{
					ctInsetFromVPSOverride = 0.0;
					additionalCTDragAbstrDelta = deltaSize - cached;
				}
			}
		}

		internal void RebuildLayoutImmediateCompat(RectTransform rectTransform)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
		}
	}
}
