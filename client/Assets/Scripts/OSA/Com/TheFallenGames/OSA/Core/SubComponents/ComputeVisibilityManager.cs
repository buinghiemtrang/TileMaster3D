using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.TheFallenGames.OSA.Core.SubComponents
{
	internal class ComputeVisibilityManager<TParams, TItemViewsHolder> where TParams : BaseParams where TItemViewsHolder : BaseItemViewsHolder
	{
		private OSA<TParams, TItemViewsHolder> _Adapter;

		private TParams _Params;

		private InternalState<TItemViewsHolder> _InternalState;

		private ItemsDescriptor _ItemsDesc;

		private string debugString;

		private bool negativeScroll;

		private double vpSize;

		private RectTransform.Edge negStartEdge_posEndEdge;

		private TItemViewsHolder nlvHolder;

		private int endItemIndexInView;

		private int neg1_posMinus1;

		private int neg1_pos0;

		private int neg0_pos1;

		private double ctVrtInsetFromVPS;

		private int itemsCount;

		private bool thereWereVisibletems;

		private double currentItemRealInset_negStart_posEnd;

		private bool negNLVCandidateIsBeforeVP_posNLVCandidateIsAfterVP;

		private TItemViewsHolder startingLVHolder;

		private double negCTVrtInsetFromVPS_posCTVrtInsetFromVPE;

		private int nlvIndexInView;

		private double nlvSize;

		private double nlvSizePlusSpacing;

		private double currentRealInsetToUseForNLV_negFromCTS_posFromCTE;

		public ComputeVisibilityManager(OSA<TParams, TItemViewsHolder> adapter)
		{
			_Adapter = adapter;
			_Params = _Adapter.Parameters;
			_InternalState = _Adapter._InternalState;
			_ItemsDesc = _Adapter._ItemsDesc;
		}

		public void ComputeVisibility(double abstractDelta)
		{
			int startingVHIndexInView = 0;
			debugString = null;
			negativeScroll = abstractDelta <= 0.0;
			vpSize = _InternalState.vpSize;
			if (!negativeScroll)
			{
				neg1_posMinus1 = -1;
				negStartEdge_posEndEdge = _InternalState.endEdge;
			}
			else
			{
				neg1_posMinus1 = 1;
				negStartEdge_posEndEdge = _InternalState.startEdge;
			}
			neg1_pos0 = (neg1_posMinus1 + 1) / 2;
			neg0_pos1 = 1 - neg1_pos0;
			thereWereVisibletems = _Adapter.VisibleItemsCount > 0;
			itemsCount = _ItemsDesc.itemsCount;
			endItemIndexInView = (itemsCount - 1) * neg1_pos0;
			ctVrtInsetFromVPS = _InternalState.ctVirtualInsetFromVPS_Cached;
			negCTVrtInsetFromVPS_posCTVrtInsetFromVPE = (negativeScroll ? ctVrtInsetFromVPS : _InternalState.CTVirtualInsetFromVPE_Cached);
			if (!thereWereVisibletems)
			{
				startingVHIndexInView = (itemsCount - 1) * neg0_pos1;
			}
			else
			{
				PrepareOutsideItemsForPotentialRecycleAndGetNextFirstVisibleIndexInView(out startingVHIndexInView);
			}
			if (itemsCount > 0)
			{
				bool inferred = false;
				if (!thereWereVisibletems || _InternalState.vpSize * 2.0 < Math.Abs(abstractDelta))
				{
					double estimatedAVGVisibleItems = Math.Min(_InternalState.vpSize / (_InternalState.spacing + (double)_Params.DefaultItemSize), _Adapter._AVGVisibleItemsCount);
					if (!((double)itemsCount <= estimatedAVGVisibleItems))
					{
						inferred = InferFirstVisibleVHIndexInViewAndInset(ref startingVHIndexInView, estimatedAVGVisibleItems);
					}
				}
				bool reachable = (!negativeScroll) ? (endItemIndexInView <= startingVHIndexInView) : (startingVHIndexInView <= endItemIndexInView);
				if (reachable)
				{
					if (!inferred)
					{
						currentItemRealInset_negStart_posEnd = ((!negativeScroll) ? _InternalState.GetItemInferredRealInsetFromParentEnd(startingVHIndexInView) : _InternalState.GetItemInferredRealInsetFromParentStart(startingVHIndexInView));
					}
					while (FindCorrectNLVFromCurrent(startingVHIndexInView))
					{
						int itemIndex = _ItemsDesc.GetItemRealIndexFromViewIndex(nlvIndexInView);
						nlvHolder = _Adapter.ExtractRecyclableViewsHolderOrCreateNew(itemIndex, nlvSize);
						_Adapter.AddViewsHolderAndMakeVisible(nlvHolder, _Adapter.VisibleItemsCount * neg1_pos0, itemIndex, nlvIndexInView, currentItemRealInset_negStart_posEnd, negStartEdge_posEndEdge, nlvSize);
						startingVHIndexInView = neg1_posMinus1 + nlvIndexInView;
						currentItemRealInset_negStart_posEnd += nlvSizePlusSpacing;
						if (!negativeScroll)
						{
							if (startingVHIndexInView < endItemIndexInView)
							{
								break;
							}
						}
						else if (endItemIndexInView < startingVHIndexInView)
						{
							break;
						}
					}
				}
				// UNVERIFIED: pseudo-C con co khoi Debug.Log("OSA.ComputeVisibility(" + abstractDelta + <chuoi 3 ky tu, nhan SL_______26 co 42 gia tri> + debugString) khi debugString != null; debugString luon null trong ham nay nen khoi nay khong bao gio chay, bo qua.
			}
			if (_ItemsDesc.maxVisibleItemsSeenSinceLastScrollViewSizeChange < _Adapter.VisibleItemsCount)
			{
				_ItemsDesc.maxVisibleItemsSeenSinceLastScrollViewSizeChange = _Adapter.VisibleItemsCount;
			}
			PostComputeVisibilityCleanRecyclableItems();
			_Adapter._AVGVisibleItemsCount = _Adapter._AVGVisibleItemsCount * 0.9 + (double)_Adapter.VisibleItemsCount * 0.1;
		}

		private bool FindCorrectNLVFromCurrent(int currentLVItemIndexInView)
		{
			nlvIndexInView = currentLVItemIndexInView;
			while (true)
			{
				nlvSize = _ItemsDesc[nlvIndexInView];
				double spacing = _InternalState.spacing;
				double num = -nlvSize;
				double num2 = currentItemRealInset_negStart_posEnd;
				nlvSizePlusSpacing = nlvSize + spacing;
				currentRealInsetToUseForNLV_negFromCTS_posFromCTE = num2;
				negNLVCandidateIsBeforeVP_posNLVCandidateIsAfterVP = num >= num2;
				if (num < num2)
				{
					if (vpSize < num2)
					{
						return false;
					}
					return true;
				}
				if (nlvIndexInView == endItemIndexInView)
				{
					return false;
				}
				nlvIndexInView = neg1_posMinus1 + nlvIndexInView;
				currentItemRealInset_negStart_posEnd = nlvSize + spacing + num2;
			}
		}

		private void PrepareOutsideItemsForPotentialRecycleAndGetNextFirstVisibleIndexInView(out int startingVHIndexInView)
		{
			startingLVHolder = _Adapter._VisibleItems[(_Adapter.VisibleItemsCount - 1) * neg1_pos0];
			startingVHIndexInView = neg1_posMinus1 + startingLVHolder.itemIndexInView;
			int num = (_Adapter.VisibleItemsCount - 1) * neg0_pos1;
			TItemViewsHolder val = _Adapter._VisibleItems[num];
			double num2 = ((!negativeScroll) ? _InternalState.GetItemVirtualInsetFromParentEndUsingItemIndexInView(val.itemIndexInView) : _InternalState.GetItemVirtualInsetFromParentStartUsingItemIndexInView(val.itemIndexInView));
			while (true)
			{
				num2 = num2 + _ItemsDesc[val.itemIndexInView] + _InternalState.spacing;
				if (0.0 - num2 < negCTVrtInsetFromVPS_posCTVrtInsetFromVPE)
				{
					break;
				}
				_Adapter._RecyclableItems.Add(val);
				_Adapter._VisibleItems.RemoveAt(num);
				_Adapter.VisibleItemsCount--;
				if (_Adapter.VisibleItemsCount == 0)
				{
					break;
				}
				num -= neg0_pos1;
				val = _Adapter._VisibleItems[num];
			}
		}

		private bool InferFirstVisibleVHIndexInViewAndInset(ref int currentInferredFirstVisibleVHIndexInView, double estimatedAVGVisibleItems)
		{
			double virtualAbstractNormalizedScrollPosition = _InternalState.GetVirtualAbstractNormalizedScrollPosition();
			int num = (int)Math.Round((1.0 - virtualAbstractNormalizedScrollPosition) * ((double)(itemsCount - 1) - (double)neg1_pos0 * estimatedAVGVisibleItems));
			num = Math.Max(0, Math.Min(itemsCount - 1, num));
			int num2 = num;
			double itemSizeOrDefault = _ItemsDesc.GetItemSizeOrDefault(num);
			double num3 = ((!negativeScroll) ? _InternalState.GetItemInferredRealInsetFromParentEnd(num) : _InternalState.GetItemInferredRealInsetFromParentStart(num));
			int num4 = neg0_pos1 * itemsCount - neg1_pos0;
			bool searchBackward = true;
			if (num3 <= 0.0 - itemSizeOrDefault)
			{
				do
				{
					int num5 = neg1_posMinus1 + num2;
					if (num5 == num4)
					{
						break;
					}
					itemSizeOrDefault = _ItemsDesc.GetItemSizeOrDefault(num5);
					num3 = num3 + itemSizeOrDefault + _InternalState.spacing;
					num2 = num5;
				}
				while (num3 <= 0.0 - itemSizeOrDefault);
				searchBackward = num2 == num;
			}
			if (searchBackward)
			{
				int num6 = num - neg1_posMinus1;
				num2 = num;
				while (num6 != num4)
				{
					double itemSizeOrDefault2 = _ItemsDesc.GetItemSizeOrDefault(num6);
					double num7 = num3 - (itemSizeOrDefault2 + _InternalState.spacing);
					if (num7 <= 0.0 - itemSizeOrDefault2)
					{
						break;
					}
					num2 = num6;
					num3 = num7;
					num6 = num6 - neg1_posMinus1;
				}
				if (num2 < 0)
				{
					throw new OSAException(string.Concat("index ", num2.ToString(), ", currentInferredFirstVisibleVHIndexInView ", currentInferredFirstVisibleVHIndexInView.ToString()));
				}
				if (itemsCount <= num2)
				{
					throw new OSAException(string.Concat("index ", num2.ToString(), " >= itemsCount ", itemsCount.ToString(), ", currentInferredFirstVisibleVHIndexInView ", currentInferredFirstVisibleVHIndexInView.ToString()));
				}
			}
			if (!thereWereVisibletems || (!negativeScroll ? (num2 <= currentInferredFirstVisibleVHIndexInView) : (currentInferredFirstVisibleVHIndexInView <= num2)))
			{
				currentInferredFirstVisibleVHIndexInView = num2;
				currentItemRealInset_negStart_posEnd = num3;
				return true;
			}
			return false;
		}

		private void PostComputeVisibilityCleanRecyclableItems()
		{
			List<TItemViewsHolder> recyclableItems = _Adapter._RecyclableItems;
			for (int num = recyclableItems.Count - 1; num >= 0; num--)
			{
				TItemViewsHolder val = recyclableItems[num];
				if (_Adapter.IsViewsHolderEnabled(val))
				{
					_Adapter.InternalOnBeforeRecycleOrDisableViewsHolder(val, -1);
					_Adapter.SetViewsHolderDisabled(val);
				}
				recyclableItems.RemoveAt(num);
				if (_Adapter.InternalShouldDestroyRecyclableItem(val, _Adapter.GetNumExcessRecycleableItems() > 0))
				{
					_Adapter.InternalDestroyRecyclableItem(val, false);
					_ItemsDesc.destroyedItemsSinceLastScrollViewSizeChange++;
				}
				else
				{
					_Adapter.AddBufferredRecycleableItem(val);
				}
			}
		}
	}
}
