using System;
using Com.TheFallenGames.OSA.Core;
using UnityEngine;
using frame8.Logic.Misc.Other.Extensions;

namespace Com.TheFallenGames.OSA.CustomAdapters.GridView
{
	public abstract class GridAdapter<TParams, TCellVH> : OSA<TParams, CellGroupViewsHolder<TCellVH>> where TParams : GridParams where TCellVH : CellViewsHolder, new()
	{
		private Action<int, int> _CellsRefreshed;

		protected int _CellsCount;

		private int _PrevCellsCount;

		private int _LastKnownUsedNumCellsPerGroup;

		public override bool InsertAtIndexSupported => false;

		public override bool RemoveFromIndexSupported => false;

		public int CellsCount => _CellsCount;

		public override event Action<int, int> ItemsRefreshed
		{
			add
			{
				_CellsRefreshed = (Action<int, int>)Delegate.Combine(_CellsRefreshed, value);
			}
			remove
			{
				_CellsRefreshed = (Action<int, int>)Delegate.Remove(_CellsRefreshed, value);
			}
		}

		public sealed override void InsertItems(int index, int itemsCount, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			throw new OSAException("Cannot use InsertItems() with a GridAdapter yet. Use ResetItems() instead.");
		}

		public sealed override void RemoveItems(int index, int itemsCount, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			throw new OSAException("Cannot use RemoveItems() with a GridAdapter yet. Use ResetItems() instead.");
		}

		public override void ChangeItemsCount(ItemCountChangeMode changeMode, int cellsCount, int indexIfAppendingOrRemoving = -1, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			if (changeMode != ItemCountChangeMode.RESET)
			{
				throw new OSAException("Only ItemCountChangeMode.RESET is supported with a GridAdapter for now");
			}
			_PrevCellsCount = _CellsCount;
			_CellsCount = cellsCount;
			int numberOfRequiredGroups = _Params.GetNumberOfRequiredGroups(_CellsCount);
			base.ChangeItemsCount(changeMode, numberOfRequiredGroups, indexIfAppendingOrRemoving, contentPanelEndEdgeStationary, keepVelocity);
		}

		public sealed override AbstractViewsHolder GetViewsHolderClosestToViewportLongitudinalNormalizedAbstractPoint(Canvas c, RectTransform canvasRectTransform, float viewportPoint01, float itemPoint01, out float distance)
		{
			CellGroupViewsHolder<TCellVH> cellGroupViewsHolder = base.GetViewsHolderClosestToViewportLongitudinalNormalizedAbstractPoint(c, canvasRectTransform, viewportPoint01, itemPoint01, out distance) as CellGroupViewsHolder<TCellVH>;
			if (cellGroupViewsHolder == null || cellGroupViewsHolder.NumActiveCells == 0)
			{
				return null;
			}
			return cellGroupViewsHolder.ContainingCellViewsHolders[cellGroupViewsHolder.NumActiveCells / 2];
		}

		public sealed override void ScrollTo(int cellIndex, float normalizedOffsetFromViewportStart = 0f, float normalizedPositionOfItemPivotToUse = 0f)
		{
			float num = normalizedPositionOfItemPivotToUse;
			int groupIndex = _Params.GetGroupIndex(cellIndex);
			bool flag = ScrollTo_ConvertItemPivotToUseIfPossible(groupIndex, cellIndex, ref normalizedPositionOfItemPivotToUse);
			ScrollToGroup(groupIndex, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse);
			if (flag)
			{
				if (!_InternalState.lastComputeVisibilityHadATwinPass)
				{
					return;
				}
				normalizedPositionOfItemPivotToUse = num;
			}
			flag = ScrollTo_ConvertItemPivotToUseIfPossible(groupIndex, cellIndex, ref normalizedPositionOfItemPivotToUse);
			ScrollToGroup(groupIndex, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse);
			if (flag)
			{
				if (!_InternalState.lastComputeVisibilityHadATwinPass)
				{
					return;
				}
				normalizedPositionOfItemPivotToUse = num;
			}
			ScrollTo_ConvertItemPivotToUseIfPossible(groupIndex, cellIndex, ref normalizedPositionOfItemPivotToUse);
			ScrollToGroup(groupIndex, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse);
		}

		public sealed override bool SmoothScrollTo(int cellIndex, float duration, float normalizedOffsetFromViewportStart = 0f, float normalizedPositionOfItemPivotToUse = 0f, Func<float, bool> onProgress = null, Action onDone = null, bool overrideAnyCurrentScrollingAnimation = false)
		{
			int groupIndex = _Params.GetGroupIndex(cellIndex);
			bool groupVisible = ScrollTo_ConvertItemPivotToUseIfPossible(groupIndex, cellIndex, ref normalizedPositionOfItemPivotToUse);
			Func<float, bool> newOnProgress = onProgress;
			Action onDone2 = onDone;
			if (!groupVisible)
			{
				duration *= 0.5f;
				if (onProgress != null)
				{
					newOnProgress = (float p) => onProgress == null || onProgress(p * 0.5f);
				}
				if (onDone != null)
				{
					onDone2 = delegate
					{
						if (newOnProgress != null)
						{
							newOnProgress = (float p) => onProgress == null || onProgress(p * 0.5f + 0.5f);
						}
						groupVisible = ScrollTo_ConvertItemPivotToUseIfPossible(groupIndex, cellIndex, ref normalizedPositionOfItemPivotToUse);
						SmoothScrollToGroup(groupIndex, duration, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse, newOnProgress, onDone, overrideAnyCurrentScrollingAnimation);
					};
				}
			}
			return SmoothScrollToGroup(groupIndex, duration, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse, newOnProgress, onDone2, overrideAnyCurrentScrollingAnimation);
		}

		public override bool SmoothBringToView(int cellIndex, float duration, float? spacingFromViewportEdge = null, Func<float, bool> onProgress = null, Action onDone = null, bool overrideCurrentScrollingAnimation = false)
		{
			Debug.Log("OSA: SmoothBringToView doesn't yet work with grids. Falling back to SmoothScrollTo() instead");
			return SmoothScrollTo(cellIndex, duration, 0.5f, 0.5f, onProgress, onDone, overrideCurrentScrollingAnimation);
		}

		public override void Refresh(bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			ChangeItemsCount(ItemCountChangeMode.RESET, _CellsCount, -1, contentPanelEndEdgeStationary, keepVelocity);
		}

		public sealed override int GetItemsCount()
		{
			return _CellsCount;
		}

		public virtual int GetCellGroupsCount()
		{
			return base.GetItemsCount();
		}

		public virtual int GetNumVisibleCells()
		{
			if (_VisibleItemsCount == 0)
			{
				return 0;
			}
			return (_VisibleItemsCount - 1) * _Params.CurrentUsedNumCellsPerGroup + _VisibleItems[_VisibleItemsCount - 1].NumActiveCells;
		}

		public virtual TCellVH GetCellViewsHolder(int cellViewsHolderIndex)
		{
			if (_VisibleItemsCount == 0)
			{
				return null;
			}
			if (cellViewsHolderIndex > GetNumVisibleCells() - 1)
			{
				return null;
			}
			return _VisibleItems[_Params.GetGroupIndex(cellViewsHolderIndex)].ContainingCellViewsHolders[cellViewsHolderIndex % _Params.CurrentUsedNumCellsPerGroup];
		}

		public virtual TCellVH GetCellViewsHolderIfVisible(int withCellItemIndex)
		{
			int groupIndex = _Params.GetGroupIndex(withCellItemIndex);
			CellGroupViewsHolder<TCellVH> itemViewsHolderIfVisible = GetItemViewsHolderIfVisible(groupIndex);
			if (itemViewsHolderIfVisible == null)
			{
				return null;
			}
			return GetCellViewsHolderIfVisible(itemViewsHolderIfVisible, withCellItemIndex);
		}

		public virtual TCellVH GetCellViewsHolderIfVisible(CellGroupViewsHolder<TCellVH> groupVH, int withCellItemIndex)
		{
			int num = groupVH.ItemIndex * _Params.CurrentUsedNumCellsPerGroup;
			if (withCellItemIndex < num + groupVH.NumActiveCells)
			{
				return groupVH.ContainingCellViewsHolders[withCellItemIndex - num];
			}
			return null;
		}

		public virtual void ScrollToGroup(int groupIndex, float normalizedOffsetFromViewportStart = 0f, float normalizedPositionOfItemPivotToUse = 0f)
		{
			base.ScrollTo(groupIndex, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse);
		}

		public virtual bool SmoothScrollToGroup(int groupIndex, float duration, float normalizedOffsetFromViewportStart = 0f, float normalizedPositionOfItemPivotToUse = 0f, Func<float, bool> onProgress = null, Action onDone = null, bool overrideAnyCurrentScrollingAnimation = false)
		{
			return base.SmoothScrollTo(groupIndex, duration, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse, onProgress, onDone, overrideAnyCurrentScrollingAnimation);
		}

		protected override void OnInitialized()
		{
			base.OnInitialized();
			_LastKnownUsedNumCellsPerGroup = _Params.CurrentUsedNumCellsPerGroup;
		}

		protected sealed override void OnItemsRefreshed(int prevCount, int newCount)
		{
			base.OnItemsRefreshed(prevCount, newCount);
			OnCellGroupsRefreshed(prevCount, newCount);
		}

		protected override CellGroupViewsHolder<TCellVH> CreateViewsHolder(int itemIndex)
		{
			CellGroupViewsHolder<TCellVH> newCellGroupViewsHolder = GetNewCellGroupViewsHolder();
			newCellGroupViewsHolder.Init(_Params.GetGroupPrefab(itemIndex).gameObject, _Params.Content, itemIndex, _Params.Grid.CellPrefab, _Params.CurrentUsedNumCellsPerGroup);
			for (int i = 0; i < newCellGroupViewsHolder.ContainingCellViewsHolders.Length; i++)
			{
				OnCellViewsHolderCreated(newCellGroupViewsHolder.ContainingCellViewsHolders[i], newCellGroupViewsHolder);
			}
			return newCellGroupViewsHolder;
		}

		protected override void UpdateViewsHolder(CellGroupViewsHolder<TCellVH> newOrRecycled)
		{
			int num;
			if (newOrRecycled.ItemIndex + 1 == GetCellGroupsCount())
			{
				int num2 = 0;
				if (newOrRecycled.ItemIndex > 0)
				{
					num2 = newOrRecycled.ItemIndex * _Params.CurrentUsedNumCellsPerGroup;
				}
				num = _CellsCount - num2;
			}
			else
			{
				num = _Params.CurrentUsedNumCellsPerGroup;
			}
			newOrRecycled.NumActiveCells = num;
			for (int i = 0; i < num; i++)
			{
				UpdateCellViewsHolder(newOrRecycled.ContainingCellViewsHolders[i]);
			}
		}

		protected virtual CellGroupViewsHolder<TCellVH> GetNewCellGroupViewsHolder()
		{
			return new CellGroupViewsHolder<TCellVH>();
		}

		protected virtual void OnCellViewsHolderCreated(TCellVH cellVH, CellGroupViewsHolder<TCellVH> cellGroup)
		{
		}

		protected abstract void UpdateCellViewsHolder(TCellVH viewsHolder);

		protected sealed override void OnBeforeRecycleOrDisableViewsHolder(CellGroupViewsHolder<TCellVH> inRecycleBinOrVisible, int newItemIndex)
		{
			base.OnBeforeRecycleOrDisableViewsHolder(inRecycleBinOrVisible, newItemIndex);
			if (newItemIndex == -1)
			{
				for (int i = 0; i < inRecycleBinOrVisible.NumActiveCells; i++)
				{
					OnBeforeRecycleOrDisableCellViewsHolder(inRecycleBinOrVisible.ContainingCellViewsHolders[i], -1);
				}
			}
			else
			{
				for (int j = 0; j < inRecycleBinOrVisible.NumActiveCells; j++)
				{
					OnBeforeRecycleOrDisableCellViewsHolder(inRecycleBinOrVisible.ContainingCellViewsHolders[j], newItemIndex * _Params.CurrentUsedNumCellsPerGroup + j);
				}
			}
		}

		protected override void OnItemIndexChangedDueInsertOrRemove(CellGroupViewsHolder<TCellVH> shiftedViewsHolder, int oldIndex, bool wasInsert, int removeOrInsertIndex)
		{
			base.OnItemIndexChangedDueInsertOrRemove(shiftedViewsHolder, oldIndex, wasInsert, removeOrInsertIndex);
		}

		protected override void RebuildLayoutDueToScrollViewSizeChange()
		{
			base.RebuildLayoutDueToScrollViewSizeChange();
			int lastKnownUsedNumCellsPerGroup = _LastKnownUsedNumCellsPerGroup;
			_LastKnownUsedNumCellsPerGroup = _Params.CurrentUsedNumCellsPerGroup;
			if (lastKnownUsedNumCellsPerGroup != _LastKnownUsedNumCellsPerGroup)
			{
				ClearVisibleItems();
			}
		}

		protected virtual void OnBeforeRecycleOrDisableCellViewsHolder(TCellVH viewsHolder, int newItemIndex)
		{
		}

		protected virtual void OnCellGroupsRefreshed(int prevGroupsCount, int curGroupsCount)
		{
			if (_CellsRefreshed != null)
			{
				_CellsRefreshed(_PrevCellsCount, _CellsCount);
			}
		}

		protected bool ScrollTo_ConvertItemPivotToUseIfPossible(int groupIndex, int cellIndex, ref float normalizedPositionOfItemPivotToUse)
		{
			CellGroupViewsHolder<TCellVH> itemViewsHolderIfVisible = GetItemViewsHolderIfVisible(groupIndex);
			if (itemViewsHolderIfVisible == null)
			{
				return false;
			}
			TCellVH cellViewsHolderIfVisible = GetCellViewsHolderIfVisible(itemViewsHolderIfVisible, cellIndex);
			if (cellViewsHolderIfVisible == null)
			{
				throw new OSAException("GetItemViewsHolderIfVisible " + groupIndex + " got group vh, but GetCellViewsHolderIfVisible " + cellIndex + " got null ?. Please report this bug");
			}
			float num = itemViewsHolderIfVisible.root.rect.size[_InternalState.hor0_vert1];
			float num2 = cellViewsHolderIfVisible.root.rect.size[_InternalState.hor0_vert1];
			float insetFromParentEdge = cellViewsHolderIfVisible.root.GetInsetFromParentEdge(itemViewsHolderIfVisible.root, _InternalState.startEdge);
			normalizedPositionOfItemPivotToUse = (insetFromParentEdge + normalizedPositionOfItemPivotToUse * num2) / num;
			return true;
		}
	}
}
