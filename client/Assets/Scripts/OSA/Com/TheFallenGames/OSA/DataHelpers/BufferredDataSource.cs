using Com.TheFallenGames.OSA.Core;

namespace Com.TheFallenGames.OSA.DataHelpers
{
	public class BufferredDataSource<T>
	{
		public delegate void ChunkReader(T[] into, int firstItemIndex, int countToRead);

		public int BUFFER_MAX_SIZE_DEFAULT = 100;

		private LazyList<T> _LazyList;

		private ChunkReader _ChunkReader;

		private int _BufferSize;

		private readonly bool _ExpectManualBufferUpdates;

		private T[] _ReadBuffer;

		public T this[int index] => _LazyList[index];

		public int Count => _LazyList.Count;

		public int ExistingCount => _LazyList.ExistingCount;

		public int BufferSize => _BufferSize;

		public BufferredDataSource(int totalCount, ChunkReader chunkReader, int bufferMaxSize, bool expectManualBufferUpdates)
		{
			if (bufferMaxSize < 1)
			{
				bufferMaxSize = BUFFER_MAX_SIZE_DEFAULT;
			}
			else if (totalCount <= bufferMaxSize)
			{
				bufferMaxSize = totalCount;
			}
			_ChunkReader = chunkReader;
			_BufferSize = bufferMaxSize;
			_ExpectManualBufferUpdates = expectManualBufferUpdates;
			_ReadBuffer = new T[bufferMaxSize];
			_LazyList = new LazyList<T>(ValueCreator, totalCount);
		}

		public bool TryGetCachedValue(int index, out T tuple)
		{
			return _LazyList.TryGet(index, out tuple);
		}

		private T ValueCreator(int index)
		{
			T tuple;
			if (!TryGetCachedValue(index, out tuple))
			{
				int count = _LazyList.Count;
				int halfBuffer = _BufferSize / 2;
				int startOffset = index - halfBuffer;
				int firstItemIndex = (startOffset < 0) ? 0 : startOffset;
				int deficit = (startOffset < 0) ? (-startOffset) : 0;
				long endExclusiveLong = (long)halfBuffer + (long)index + (long)(_BufferSize % 2) + deficit;
				if (endExclusiveLong > int.MaxValue - 1)
				{
					endExclusiveLong = int.MaxValue;
				}
				int endExclusive = (endExclusiveLong > count) ? count : (int)endExclusiveLong;
				_LazyList.GetIndicesOfClosestExistingItems(index, out var prevIndex, out var nextIndex);
				if (firstItemIndex < prevIndex && prevIndex != -1)
				{
					firstItemIndex = prevIndex + 1;
				}
				int lastExclusive = ((endExclusive <= nextIndex || nextIndex == -1) ? endExclusive : nextIndex);
				int countToRead = lastExclusive - firstItemIndex;
				_ChunkReader(_ReadBuffer, firstItemIndex, countToRead);
				if (!_ExpectManualBufferUpdates)
				{
					_LazyList.SetOrUpdateManuallyCreatedValues(firstItemIndex, _ReadBuffer, 0, countToRead);
				}
				else
				{
					_LazyList.AllocateNullSlots(firstItemIndex, countToRead);
				}
				tuple = _LazyList.GetUnchecked(index);
			}
			return tuple;
		}

		public T GetValue(int index)
		{
			return _LazyList[index];
		}

		public T GetValueUnchecked(int index)
		{
			return _LazyList.GetUnchecked(index);
		}

		public void ManuallyUpdateCreatedValues(int firstItemIndex, T[] values, int readStartIndex, int readCount)
		{
			if (!_ExpectManualBufferUpdates)
			{
				throw new OSAException("Not supported because _ExpectManualBufferUpdates is false");
			}
			_LazyList.SetOrUpdateManuallyCreatedValues(firstItemIndex, values, readStartIndex, readCount);
		}
	}
}
