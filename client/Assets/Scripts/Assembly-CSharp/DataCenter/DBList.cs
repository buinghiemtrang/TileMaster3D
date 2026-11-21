using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	public class DBList<T>
	{
		private Func<List<T>> _funcGET;

		private Action<List<T>> _funcSet;

		private Action _funcSave;

		public T this[int index]
		{
			get
			{
				return _funcGET()[index];
			}
			set
			{
				_funcGET()[index] = value;
				_funcSave();
			}
		}

		public int Count => _funcGET().Count;

		public List<T> Get()
		{
			return _funcGET();
		}

		public void Set(List<T> value, bool save = true)
		{
			_funcSet(value);
			if (save)
			{
				_funcSave();
			}
		}

		public void Set(int index, T value, bool save = true)
		{
			_funcGET()[index] = value;
			if (save)
			{
				_funcSave();
			}
		}

		public void Init(Func<List<T>> funcGET, Action<List<T>> funcSet, Action funcSave)
		{
			_funcGET = funcGET;
			_funcSet = funcSet;
			_funcSave = funcSave;
		}

		public void Save()
		{
			_funcSave?.Invoke();
		}

		public void Add(T value, bool save = true)
		{
			_funcGET().Add(value);
			if (save)
			{
				_funcSave();
			}
		}

		public void Remove(T value, bool save = true)
		{
			_funcGET().Remove(value);
			if (save)
			{
				_funcSave();
			}
		}

		public void RemoveAt(int index, bool save = true)
		{
			_funcGET().RemoveAt(index);
			if (save)
			{
				_funcSave();
			}
		}

		public void RemoveRange(int index, int count, bool save = true)
		{
			_funcGET().RemoveRange(index, count);
			if (save)
			{
				_funcSave();
			}
		}

		public void Clear(bool save = true)
		{
			_funcGET().Clear();
			if (save)
			{
				_funcSave();
			}
		}

		public bool Contains(T value)
		{
			return _funcGET().Contains(value);
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(_funcGET());
		}

		public object Find(Func<object, bool> func)
		{
			throw new NotImplementedException(); // original game throws here (DBList<T>.Find RVA 0x178aaf4)
		}
	}
}
