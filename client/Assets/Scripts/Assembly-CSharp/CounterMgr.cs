using System;
using System.Collections.Generic;

public static class CounterMgr
{
	private static Dictionary<string, StructCounterOne> _stream = new Dictionary<string, StructCounterOne>();

	private static int _sign = 0;

	public static string Create(Action func, bool destroy = true)
	{
		_sign++;
		string text = _sign.ToString();
		Create(func, text, destroy);
		return text;
	}

	public static StructCounterOne Create(Action func, string key, bool destroy = true)
	{
		StructCounterOne structCounterOne;
		if (!_stream.ContainsKey(key))
		{
			structCounterOne = new StructCounterOne();
			_stream.Add(key, structCounterOne);
		}
		else
		{
			structCounterOne = _stream[key];
		}
		structCounterOne.count = 0;
		structCounterOne.func = func;
		structCounterOne.destroy = destroy;
		return structCounterOne;
	}

	public static void Clear(string key = "")
	{
		if (string.IsNullOrEmpty(key))
		{
			key = _sign.ToString();
		}

		if (_stream.ContainsKey(key))
		{
			_stream.Remove(key);
		}
	}

	public static void Init(string key)
	{
		if (_stream.ContainsKey(key))
		{
			_stream[key].count = 0;
		}
	}

	public static void Add(string key = "")
	{
		if (string.IsNullOrEmpty(key))
		{
			key = _sign.ToString();
		}

		if (_stream.ContainsKey(key))
		{
			_stream[key].count++;
		}
	}

	public static void Del(string key = "")
	{
		if (string.IsNullOrEmpty(key))
		{
			key = _sign.ToString();
		}

		if (!_stream.ContainsKey(key))
		{
			return;
		}
		StructCounterOne structCounterOne = _stream[key];
		structCounterOne.count--;
		if (structCounterOne.count == 0)
		{
			if (structCounterOne.destroy)
			{
				_stream.Remove(key);
			}
			if (structCounterOne.func != null)
			{
				structCounterOne.func();
			}
		}
	}

	public static bool Finish(string key = "")
	{
		if (string.IsNullOrEmpty(key))
		{
			key = _sign.ToString();
		}

		if (_stream.ContainsKey(key))
		{
			StructCounterOne structCounterOne = _stream[key];
			if (structCounterOne.count == 0)
			{
				if (structCounterOne.func != null)
				{
					structCounterOne.func();
				}
				if (structCounterOne.destroy)
				{
					_stream.Remove(key);
				}
				return true;
			}
		}
		return false;
	}

	public static int Count(string key = "")
	{
		if (string.IsNullOrEmpty(key))
		{
			key = _sign.ToString();
		}

		if (!_stream.ContainsKey(key))
		{
			return 0;
		}
		return _stream[key].count;
	}
}
