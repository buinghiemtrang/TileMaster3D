using System;
using System.Collections.Generic;

public class LinearTriggerOne
{
	public string key = "";

	private int _index;

	public readonly List<Action> _methods = new List<Action>();

	public bool IsComplete => _methods.Count == _index;

	public void Clear()
	{
		_methods.Clear();
	}

	public void Init()
	{
		_index = 0;
	}

	public LinearTriggerOne Push(Action func)
	{
		_methods.Add(func);
		return this;
	}

	public LinearTriggerOne Prepend(Action func)
	{
		_methods.Insert(0, func);
		return this;
	}

	public void Next()
	{
		if (!IsComplete)
		{
			_methods[_index++].InvokeSafe();
		}
	}
}
