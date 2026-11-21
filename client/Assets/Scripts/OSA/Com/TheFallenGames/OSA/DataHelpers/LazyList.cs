using System;
using System.Collections;
using System.Collections.Generic;

namespace Com.TheFallenGames.OSA.DataHelpers
{
	public class LazyList<T>
	{
		public class EnumerableLazyList : IEnumerable<T>, IEnumerable
		{
			private LazyList<T> _LazyList;

			public EnumerableLazyList(LazyList<T> lazyList)
			{
				_LazyList = lazyList;
			}

			public IEnumerator<T> GetEnumerator()
			{
				return _LazyList.GetEnumeratorForExistingItems();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		private class IniniteNullList : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
		{
			public T this[int index]
			{
				get
				{
					return default(T);
				}
				set
				{
				}
			}

			public int Count => -1;

			public bool IsReadOnly => true;

			public void Add(T item)
			{
			}

			public void Clear()
			{
			}

			public bool Contains(T item)
			{
				throw new NotImplementedException();
			}

			public void CopyTo(T[] array, int arrayIndex)
			{
			}

			public IEnumerator<T> GetEnumerator()
			{
				throw new NotImplementedException();
			}

			public int IndexOf(T item)
			{
				throw new NotImplementedException();
			}

			public void Insert(int index, T item)
			{
			}

			public bool Remove(T item)
			{
				throw new NotImplementedException();
			}

			public void RemoveAt(int index)
			{
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				throw new NotImplementedException();
			}
		}

		private Dictionary<int, T> _BackingMap;

		private List<int> _Keys = new List<int>();

		private Func<int, T> _NewValueCreator;

		private IniniteNullList _InfiniteNullList = new IniniteNullList();

		public T this[int key]
		{
			get
			{
				if (key >= Count)
				{
					throw new ArgumentOutOfRangeException("key", key, "must be less than Count=" + Count.ToString());
				}
				T val;
				if (!TryGet(key, out val))
				{
					val = _NewValueCreator(key);
					if (!_BackingMap.ContainsKey(key))
					{
						SetManuallyCreatedValueForFirstTime(key, val);
					}
				}
				return val;
			}
		}

		public int Count { get; private set; }

		public int ExistingCount => _Keys.Count;

		public EnumerableLazyList AsEnumerableForExistingItems => new EnumerableLazyList(this);

		public LazyList(Func<int, T> newValueCreator, int initialCount)
		{
			_NewValueCreator = newValueCreator;
			InitWithNewCount((initialCount < 0) ? 0 : initialCount);
		}

		public void InitWithNewCount(int newCount)
		{
			int capacity = Math.Min(newCount, 1024);
			_BackingMap = new Dictionary<int, T>(capacity);
			_Keys = new List<int>(capacity);
			Count = newCount;
		}

		public void Add(int count)
		{
			Count += count;
		}

		public void InsertAtStart(int count)
		{
			InsertWhenKeyStartIdxKnown(0, 0, count);
		}

		public void Insert(int index, int count)
		{
			int keyStartIdx = _Keys.BinarySearch(index);
			if (keyStartIdx < 0)
			{
				keyStartIdx = ~keyStartIdx;
			}
			InsertWhenKeyStartIdxKnown(keyStartIdx, index, count);
		}

		private void InsertWhenKeyStartIdxKnown(int keyStartIdx, int index, int count)
		{
			for (int i = _Keys.Count - 1; i > keyStartIdx - 1; i--)
			{
				int key = _Keys[i];
				int newKey = key + count;
				_Keys[i] = newKey;
				_BackingMap[newKey] = _BackingMap[key];
				_BackingMap.Remove(key);
			}
			Count += count;
		}

		public void Clear()
		{
			_BackingMap.Clear();
			_Keys.Clear();
			Count = 0;
		}

		[Obsolete("Use Remove(int index, int count) below")]
		public void RemoveAt(int index)
		{
			Remove(index, 1);
		}

		public int Remove(T value)
		{
			foreach (KeyValuePair<int, T> item in _BackingMap)
			{
				if (item.Value.Equals(value))
				{
					int key = item.Key;
					Remove(key, 1);
					return key;
				}
			}
			return -1;
		}

		public void Remove(int index, int count)
		{
			int i = _Keys.BinarySearch(index);
			if (i < 0)
			{
				i = ~i;
			}
			while (i < _Keys.Count)
			{
				int key = _Keys[i];
				if (key >= index + count)
				{
					break;
				}
				_Keys.RemoveAt(i);
				_BackingMap.Remove(key);
			}
			for (; i < _Keys.Count; i++)
			{
				int key2 = _Keys[i];
				int newKey = key2 - count;
				_Keys[i] = newKey;
				_BackingMap[newKey] = _BackingMap[key2];
				_BackingMap.Remove(key2);
			}
			Count -= count;
		}

		private IEnumerator<T> GetEnumeratorForExistingItems()
		{
			for (int i = 0; i < _Keys.Count; i++)
			{
				yield return _BackingMap[_Keys[i]];
			}
		}

		public bool TryGet(int index, out T val)
		{
			return _BackingMap.TryGetValue(index, out val);
		}

		public T GetUnchecked(int index)
		{
			return _BackingMap[index];
		}

		public void GetIndicesOfClosestExistingItems(int index, out int prevIndex, out int nextIndex)
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", "can't be negative");
			}
			if (index == int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("index", "can't be int.MaxValue");
			}
			int keysIdx = _Keys.BinarySearch(index);
			int nextKeyIdx;
			if (keysIdx == -1 || keysIdx == 0)
			{
				prevIndex = -1;
				nextKeyIdx = 1;
			}
			else
			{
				int prevKeyIdx;
				if (keysIdx < 0)
				{
					nextKeyIdx = ~keysIdx;
					prevKeyIdx = nextKeyIdx - 1;
				}
				else
				{
					prevKeyIdx = keysIdx - 1;
					nextKeyIdx = keysIdx + 1;
				}
				prevIndex = ((prevKeyIdx < _Keys.Count) ? _Keys[prevKeyIdx] : (-1));
			}
			nextIndex = ((nextKeyIdx >= 0 && nextKeyIdx < _Keys.Count) ? _Keys[nextKeyIdx] : (-1));
		}

		public void SetOrUpdateManuallyCreatedValue(int index, T val)
		{
			if (!_BackingMap.ContainsKey(index))
			{
				SetManuallyCreatedValueForFirstTime(index, val);
			}
			else
			{
				_BackingMap[index] = val;
			}
		}

		public void AllocateNullSlots(int startingIndex, int valuesReadCount)
		{
			SetOrUpdateManuallyCreatedValues(startingIndex, _InfiniteNullList, 0, valuesReadCount);
		}

		public void SetOrUpdateManuallyCreatedValues(int startingIndex, IList<T> values, int valsReadIndex, int valsReadCount)
		{
			int keyIdx = _Keys.BinarySearch(startingIndex);
			if (keyIdx < 0)
			{
				keyIdx = ~keyIdx;
			}
			for (int i = 0; i < valsReadCount; i++)
			{
				int key = startingIndex + i;
				_BackingMap[key] = values[valsReadIndex + i];
				int keyPos = i + keyIdx;
				if (keyPos == _Keys.Count || _Keys[keyPos] != key)
				{
					_Keys.Insert(keyPos, key);
				}
			}
		}

		private void SetManuallyCreatedValueForFirstTime(int index, T val)
		{
			_BackingMap.Add(index, val);
			_Keys.Insert(~_Keys.BinarySearch(index), index);
		}
	}
}
