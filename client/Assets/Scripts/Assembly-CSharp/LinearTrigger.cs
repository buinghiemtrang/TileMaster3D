using System;
using System.Collections.Generic;

public static class LinearTrigger
{
	private static Dictionary<string, LinearTriggerOne> _stream = new Dictionary<string, LinearTriggerOne>();

	private static int _sign = 0;

	public static LinearTriggerOne Create(string key = "")
	{
		if (string.IsNullOrEmpty(key))
		{
			_sign++;
			key = _sign.ToString();
		}
		LinearTriggerOne linearTriggerOne;
		if (!_stream.ContainsKey(key))
		{
			linearTriggerOne = new LinearTriggerOne();
			linearTriggerOne.key = key;
			_stream.Add(key, linearTriggerOne);
		}
		else
		{
			linearTriggerOne = _stream[key];
		}
		linearTriggerOne.Clear();
		return linearTriggerOne;
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

	public static void Clear(LinearTriggerOne one)
	{
		Clear(one.key);
	}

	public static void Push(Action func, string key = "")
	{
		if (string.IsNullOrEmpty(key))
		{
			key = _sign.ToString();
		}
		if (_stream.ContainsKey(key))
		{
			_stream[key].Push(func);
		}
	}

	public static void Prepend(Action func, string key = "")
	{
		if (string.IsNullOrEmpty(key))
		{
			key = _sign.ToString();
		}
		if (_stream.ContainsKey(key))
		{
			_stream[key].Prepend(func);
		}
	}

	public static void Next(string key = "")
	{
		if (string.IsNullOrEmpty(key))
		{
			key = _sign.ToString();
		}
		if (_stream.ContainsKey(key))
		{
			_stream[key].Next();
		}
	}
}
