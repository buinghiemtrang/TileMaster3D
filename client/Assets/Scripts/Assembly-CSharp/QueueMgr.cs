using System.Collections;
using UnityEngine;

public class QueueMgr<T> : MonoBehaviour where T : class
{
	private Queue _items;

	protected virtual void Awake()
	{
		_items = new Queue();
	}

	protected void Add(T item)
	{
		if (item != null)
		{
			_items.Enqueue(item);
		}
	}

	protected T Execute()
	{
		if (_items.Count != 0)
		{
			return (T)_items.Dequeue();
		}
		return null;
	}

	protected void Clear()
	{
		_items.Clear();
	}

	public bool Contains(T item)
	{
		return _items.Contains(item);
	}
}
