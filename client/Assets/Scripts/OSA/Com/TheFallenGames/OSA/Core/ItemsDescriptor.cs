using System;
using System.Collections.Generic;

namespace Com.TheFallenGames.OSA.Core
{
	public class ItemsDescriptor
	{
		private class SizeInfo
		{
			public double size;

			public double cumulativeSize;
		}

		public int itemsCount;

		public int realIndexOfFirstItemInView;

		public int maxVisibleItemsSeenSinceLastScrollViewSizeChange;

		public int destroyedItemsSinceLastScrollViewSizeChange;

		private List<int> _Keys = new List<int>();

		private Dictionary<int, SizeInfo> _SizeInfos = new Dictionary<int, SizeInfo>();

		private double _DefaultSize;

		private bool _ChangingItemsSizesInProgress;

		private int _IndexInViewOfFirstItemThatChangesSizeDuringSizesChange;

		private int _IndexInViewOfLastItemThatChangedSizeDuringSizesChange = -1;

		private double _CumulatedSizesUntilNowDuringSizesChange;

		public double CumulatedSizeOfAllItems
		{
			get
			{
				if (itemsCount != 0)
				{
					return GetItemSizeCumulative(itemsCount - 1, false);
				}
				return 0.0;
			}
		}

		public double this[int itemIndexInView]
		{
			get
			{
				SizeInfo value;
				if (_SizeInfos.TryGetValue(itemIndexInView, out value))
				{
					return value.size;
				}
				return _DefaultSize;
			}
			set
			{
				if (!_ChangingItemsSizesInProgress)
				{
					throw new OSAException("Call BeginChangingItemsSizes() before");
				}
				if (itemIndexInView != _IndexInViewOfLastItemThatChangedSizeDuringSizesChange + 1)
				{
					throw new OSAException("Sizes can only be changed for items one by one, one after another(e.g. 3,4,5,6,7..), starting with the one passed to BeginChangingItemsSizes(int)!");
				}
				BinaryAddKeyToSortedListIfDoesntExist(itemIndexInView);
				_CumulatedSizesUntilNowDuringSizesChange += value;
				SizeInfo value2;
				if (!_SizeInfos.TryGetValue(itemIndexInView, out value2))
				{
					_SizeInfos.Add(itemIndexInView, new SizeInfo
					{
						size = value,
						cumulativeSize = _CumulatedSizesUntilNowDuringSizesChange
					});
				}
				else
				{
					value2.size = value;
					value2.cumulativeSize = _CumulatedSizesUntilNowDuringSizesChange;
				}
				_IndexInViewOfLastItemThatChangedSizeDuringSizesChange = itemIndexInView;
			}
		}

		public ItemsDescriptor(double defaultSize)
		{
			if (defaultSize <= 0.0)
			{
				throw new OSAException("ItemsDescriptor can't be initialized with a 0 or negative itemDefaultSize: " + defaultSize);
			}
			ReinitializeSizes(ItemCountChangeMode.RESET, 0, -1, defaultSize);
			ReinitializeRealIndexOfFirstItemInView(0, ItemCountChangeMode.RESET, 0, -1, -1, false);
		}

		public void ReinitializeRealIndexOfFirstItemInView(int oldCount, ItemCountChangeMode changeMode, int count, int indexIfInsertingOrRemoving, int itemIndexOfFirstVHIfInsertingOrRemoving, bool canLoop)
		{
			int num = itemsCount;
			if (num == 0)
			{
				realIndexOfFirstItemInView = -1;
				return;
			}
			if (changeMode == ItemCountChangeMode.RESET)
			{
				realIndexOfFirstItemInView = 0;
				return;
			}
			if (canLoop)
			{
				throw new OSAException("At the moment, only ItemCountChangeMode.RESET is supported when looping");
			}
			realIndexOfFirstItemInView = 0;
		}

		public void ReinitializeSizes(ItemCountChangeMode changeMode, int count, int indexIfInsertingOrRemoving, double? newDefaultSize = null)
		{
			if (newDefaultSize.HasValue)
			{
				if (newDefaultSize != _DefaultSize)
				{
					if (changeMode != ItemCountChangeMode.RESET)
					{
						throw new OSAException("Cannot preserve old sizes if the newDefaultITemSize is different!");
					}
					_DefaultSize = newDefaultSize.Value;
					_Keys.Clear();
					_SizeInfos.Clear();
				}
				AssureValidDefaultItemSizeOrThrow();
			}
			if (changeMode == ItemCountChangeMode.RESET)
			{
				if (count < 0)
				{
					throw new OSAException("OSA.ItemsDescriptor.ReinitializeSizes: Can't have negative count: " + count);
				}
				ClearAllKeysAndSizes();
				itemsCount = count;
				return;
			}
			if (indexIfInsertingOrRemoving < 0 || indexIfInsertingOrRemoving > itemsCount)
			{
				throw new ArgumentOutOfRangeException("indexIfInsertingOrRemoving", indexIfInsertingOrRemoving, "Should be positive and less than- or equal to itemsCount=" + itemsCount);
			}
			if (changeMode == ItemCountChangeMode.INSERT)
			{
				if (count < 0)
				{
					throw new ArgumentOutOfRangeException("count", count, "Cannot be negative!");
				}
				ShiftSizesKeysAfterInsertOrRemove(indexIfInsertingOrRemoving, count);
				itemsCount += count;
				return;
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", count, "Cannot be negative!");
			}
			if (indexIfInsertingOrRemoving + count > itemsCount)
			{
				throw new ArgumentOutOfRangeException("RemoveItems: index + count", indexIfInsertingOrRemoving + count, "Should be positive and less than- or or equal to itemsCount=" + itemsCount);
			}
			count = -count;
			ShiftSizesKeysAfterInsertOrRemove(indexIfInsertingOrRemoving, count);
			itemsCount += count;
		}

		private void AssureValidDefaultItemSizeOrThrow()
		{
			if (_DefaultSize <= 0.0)
			{
				throw new OSAException("ItemsDescriptor.AssureValidDefaultItemSizeOrThrow: Item prefab's size is zero or negative(" + _DefaultSize + "). This is not allowed. Is your prefab badly resized at initialization?");
			}
		}

		private void ClearAllKeysAndSizes()
		{
			_SizeInfos.Clear();
			_Keys.Clear();
		}

		private void BinaryAddKeyToSortedListIfDoesntExist(int key)
		{
			int num = _Keys.BinarySearch(key);
			if (num < 0)
			{
				_Keys.Insert(~num, key);
			}
		}

		private void BinaryRemoveKeyFromSortedList(int key)
		{
			_Keys.RemoveAt(_Keys.BinarySearch(key));
		}

		private void ShiftSizesKeysAfterInsertOrRemove(int startingKey, int amount)
		{
			if (_Keys.Count == 0)
			{
				return;
			}
			int num = _Keys.BinarySearch(startingKey);
			if (num < 0)
			{
				num = ~num;
			}
			int i = num;
			double num2 = 0.0;
			if (amount < 0)
			{
				int num3 = startingKey - amount;
				int num4 = _Keys.Count - i;
				for (; i < _Keys.Count && _Keys[i] < num3; i++)
				{
					num2 -= _SizeInfos[_Keys[i]].size - _DefaultSize;
					_SizeInfos.Remove(_Keys[i]);
					_Keys.RemoveAt(i);
					i--;
				}
				int num5 = _Keys.Count - i;
				int num6 = num4 - num5;
				int num7 = -amount - num6;
				num2 -= (double)num7 * _DefaultSize;
				for (; i < _Keys.Count; i++)
				{
					int num8 = _Keys[i];
					int num9 = num8 + amount;
					if (num9 < 0)
					{
						_SizeInfos.Remove(num8);
						_Keys.RemoveAt(i);
						i--;
						continue;
					}
					_Keys[i] = num9;
					SizeInfo sizeInfo = _SizeInfos[num8];
					sizeInfo.cumulativeSize += num2;
					_SizeInfos.Remove(num8);
					_SizeInfos[num9] = sizeInfo;
				}
			}
			else
			{
				num2 = (double)amount * _DefaultSize;
				for (int num10 = _Keys.Count - 1; num10 >= i; num10--)
				{
					int num11 = _Keys[num10];
					int num12 = num11 + amount;
					_Keys[num10] = num12;
					SizeInfo sizeInfo2 = _SizeInfos[num11];
					sizeInfo2.cumulativeSize += num2;
					_SizeInfos.Remove(num11);
					_SizeInfos[num12] = sizeInfo2;
				}
			}
		}

		public void BeginChangingItemsSizes(int indexInViewOfFirstItemThatWillChangeSize)
		{
			if (_ChangingItemsSizesInProgress)
			{
				throw new OSAException("Call EndChangingItemsSizes() when done doing it");
			}
			_ChangingItemsSizesInProgress = true;
			_IndexInViewOfFirstItemThatChangesSizeDuringSizesChange = indexInViewOfFirstItemThatWillChangeSize;
			_IndexInViewOfLastItemThatChangedSizeDuringSizesChange = _IndexInViewOfFirstItemThatChangesSizeDuringSizesChange - 1;
			_CumulatedSizesUntilNowDuringSizesChange = ((_IndexInViewOfFirstItemThatChangesSizeDuringSizesChange == 0) ? 0.0 : GetItemSizeCumulative(_IndexInViewOfFirstItemThatChangesSizeDuringSizesChange - 1));
		}

		public void EndChangingItemsSizes()
		{
			_ChangingItemsSizesInProgress = false;
			if (_IndexInViewOfLastItemThatChangedSizeDuringSizesChange == _IndexInViewOfFirstItemThatChangesSizeDuringSizesChange - 1)
			{
				return;
			}
			int num = _Keys.BinarySearch(_IndexInViewOfLastItemThatChangedSizeDuringSizesChange);
			if (num < 0)
			{
				throw new OSAException("The sizes state was corrupted");
			}
			double num2 = _CumulatedSizesUntilNowDuringSizesChange;
			int num3 = _IndexInViewOfLastItemThatChangedSizeDuringSizesChange;
			for (int i = num + 1; i < _Keys.Count; i++)
			{
				int num4 = _Keys[i];
				SizeInfo sizeInfo = _SizeInfos[num4];
				num2 += (double)(num4 - num3 - 1) * _DefaultSize + sizeInfo.size;
				sizeInfo.cumulativeSize = num2;
				num3 = num4;
			}
		}

		public int GetItemRealIndexFromViewIndex(int indexInView)
		{
			return (int)(((long)realIndexOfFirstItemInView + (long)indexInView) % itemsCount);
		}

		private int GetItemViewIndexFromRealIndexWithoutChecks(int realIndex)
		{
			return (int)(((long)itemsCount + (long)(realIndex - realIndexOfFirstItemInView)) % itemsCount);
		}

		public int GetItemViewIndexFromRealIndexChecked(int realIndex)
		{
			if (realIndex < 0)
			{
				throw new ArgumentOutOfRangeException("realIndex", realIndex, "OSA.GetItemViewIndexFromRealIndexNotAllowingTotalCount: argument should be >=0");
			}
			if (realIndex >= itemsCount)
			{
				throw new ArgumentOutOfRangeException("realIndex", realIndex, "OSA.GetItemViewIndexFromRealIndexNotAllowingTotalCount: argument should be < totalCount(=" + itemsCount + ")");
			}
			return GetItemViewIndexFromRealIndexWithoutChecks(realIndex);
		}

		public double GetItemSizeOrDefault(int itemIndexInView)
		{
			return this[itemIndexInView];
		}

		public double GetItemSizeCumulative(int itemIndexInView, bool allowInferringFromNeighborAfter = true)
		{
			if (_Keys.Count > 0)
			{
				SizeInfo value;
				if (_SizeInfos.TryGetValue(itemIndexInView, out value))
				{
					return value.cumulativeSize;
				}
				int num = _Keys.BinarySearch(itemIndexInView);
				if (num >= 0)
				{
					throw new OSAException("The sizes state was corrupted. key not in _SizesCumulative, but present in _Keys");
				}
				num = ~num;
				int num2 = num - 1;
				int num3 = int.MaxValue;
				if (num2 >= 0)
				{
					int num4 = _Keys[num2];
					num3 = itemIndexInView - num4;
					if (!allowInferringFromNeighborAfter)
					{
						return _SizeInfos[num4].cumulativeSize + (double)num3 * _DefaultSize;
					}
				}
				if (allowInferringFromNeighborAfter && num < _Keys.Count)
				{
					int num5 = _Keys[num];
					int num6 = num5 - itemIndexInView;
					if (num6 < num3)
					{
						SizeInfo sizeInfo = _SizeInfos[num5];
						return sizeInfo.cumulativeSize - sizeInfo.size - (double)(num6 - 1) * _DefaultSize;
					}
				}
				if (num2 >= 0)
				{
					int key = _Keys[num2];
					return _SizeInfos[key].cumulativeSize + (double)num3 * _DefaultSize;
				}
			}
			return (double)(itemIndexInView + 1) * _DefaultSize;
		}

		public void RotateItemsSizesOnScrollViewLooped(int newValueOf_RealIndexOfFirstItemInView)
		{
			int num = realIndexOfFirstItemInView;
			realIndexOfFirstItemInView = newValueOf_RealIndexOfFirstItemInView;
			int itemViewIndexFromRealIndexWithoutChecks = GetItemViewIndexFromRealIndexWithoutChecks(num);
			int count = _Keys.Count;
			if (count == 0 && itemViewIndexFromRealIndexWithoutChecks == 0)
			{
				return;
			}
			int[] array = _Keys.ToArray();
			double[] array2 = new double[count];
			for (int i = 0; i < count; i++)
			{
				int key = array[i];
				array2[i] = _SizeInfos[key].size;
			}
			_Keys.Clear();
			_SizeInfos.Clear();
			BeginChangingItemsSizes(0);
			int num2 = -1;
			for (int j = 0; j < count; j++)
			{
				int num3 = (array[j] + itemViewIndexFromRealIndexWithoutChecks) % itemsCount;
				if (num3 < num2)
				{
					EndChangingItemsSizes();
					BeginChangingItemsSizes(0);
					for (int k = 0; k < num3; k++)
					{
						this[k] = this[k];
					}
				}
				else
				{
					for (int l = num2 + 1; l < num3; l++)
					{
						this[l] = _DefaultSize;
					}
				}
				this[num3] = array2[j];
				num2 = num3;
			}
			EndChangingItemsSizes();
		}
	}
}
