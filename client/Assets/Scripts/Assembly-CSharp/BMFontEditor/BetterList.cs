using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace BMFontEditor
{
	public class BetterList<T>
	{
		public delegate int CompareFunc(T left, T right);

		public T[] buffer;

		public int size;

		[DebuggerHidden]
		public T this[int i]
		{
			get
			{
				return buffer[i];
			}
			set
			{
				buffer[i] = value;
			}
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		public IEnumerator GetEnumerator() // UNVERIFIED: state machine MoveNext not cross-checked in pseudo-C, written as NGUI BetterList
		{
			if (buffer != null)
			{
				for (int i = 0; i < size; i++)
				{
					yield return buffer[i];
				}
			}
		}

		private void AllocateMore()
		{
			T[] array = ((buffer != null) ? new T[System.Math.Max(buffer.Length << 1, 32)] : new T[32]);
			if (buffer != null && size > 0)
			{
				buffer.CopyTo(array, 0);
			}
			buffer = array;
		}

		private void Trim()
		{
			if (size > 0)
			{
				if (size < buffer.Length)
				{
					T[] array = new T[size];
					for (int i = 0; i < size; i++)
					{
						array[i] = buffer[i];
					}
					buffer = array;
				}
			}
			else
			{
				buffer = null;
			}
		}

		public void Clear()
		{
			size = 0;
		}

		public void Release()
		{
			size = 0;
			buffer = null;
		}

		public void Add(T item)
		{
			if (buffer == null || size == buffer.Length)
			{
				AllocateMore();
			}
			buffer[size++] = item;
		}

		public void Insert(int index, T item)
		{
			if (buffer == null || size == buffer.Length)
			{
				AllocateMore();
			}
			if (index > -1 && index < size)
			{
				for (int num = size; num > index; num--)
				{
					buffer[num] = buffer[num - 1];
				}
				buffer[index] = item;
				size++;
			}
			else
			{
				Add(item);
			}
		}

		public bool Contains(T item) // UNVERIFIED: equality via EqualityComparer<T>.Default (NGUI uses item.Equals); loop shape matches pseudo-C
		{
			if (buffer == null)
			{
				return false;
			}
			EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
			for (int i = 0; i < size; i++)
			{
				if (equalityComparer.Equals(buffer[i], item))
				{
					return true;
				}
			}
			return false;
		}

		public int IndexOf(T item) // UNVERIFIED: same as Contains
		{
			if (buffer == null)
			{
				return -1;
			}
			EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
			for (int i = 0; i < size; i++)
			{
				if (equalityComparer.Equals(buffer[i], item))
				{
					return i;
				}
			}
			return -1;
		}

		public bool Remove(T item) // UNVERIFIED: shifting loop written as NGUI BetterList.Remove
		{
			if (buffer != null)
			{
				EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
				for (int i = 0; i < size; i++)
				{
					if (equalityComparer.Equals(buffer[i], item))
					{
						size--;
						buffer[i] = default(T);
						for (int j = i; j < size; j++)
						{
							buffer[j] = buffer[j + 1];
						}
						buffer[size] = default(T);
						return true;
					}
				}
			}
			return false;
		}

		public void RemoveAt(int index)
		{
			if (buffer != null && index > -1 && index < size)
			{
				size--;
				buffer[index] = default(T);
				for (int i = index; i < size; i++)
				{
					buffer[i] = buffer[i + 1];
				}
				buffer[size] = default(T);
			}
		}

		public T Pop()
		{
			if (buffer != null && size != 0)
			{
				T result = buffer[--size];
				buffer[size] = default(T);
				return result;
			}
			return default(T);
		}

		public T[] ToArray()
		{
			Trim();
			return buffer;
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		public void Sort(CompareFunc comparer) // UNVERIFIED: bubble sort as in NGUI BetterList.Sort; pseudo-C only confirms the comparer delegate is invoked
		{
			bool flag = true;
			while (flag)
			{
				flag = false;
				for (int i = 1; i < size; i++)
				{
					if (comparer(buffer[i - 1], buffer[i]) > 0)
					{
						T val = buffer[i];
						buffer[i] = buffer[i - 1];
						buffer[i - 1] = val;
						flag = true;
					}
				}
			}
		}
	}
}
