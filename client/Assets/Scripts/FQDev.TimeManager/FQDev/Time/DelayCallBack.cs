using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FQDev.Time
{
	public class DelayCallBack
	{
		private readonly Stack<LinkedListNode<ActionEvent>> _pool;

		private readonly LinkedList<ActionEvent> _actions;

		private int _counter;

		public DelayCallBack(Stack<LinkedListNode<ActionEvent>> pool = null)
		{
			if (pool == null)
			{
				pool = new Stack<LinkedListNode<ActionEvent>>();
			}
			_pool = pool;
			_actions = new LinkedList<ActionEvent>();
		}

		public void Update()
		{
			if (_actions == null || _actions.Count == 0)
			{
				return;
			}
			float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
			while (_actions.Count >= 1)
			{
				LinkedListNode<ActionEvent> first = _actions.First;
				if (realtimeSinceStartup <= first.Value.TimeStamp)
				{
					break;
				}
				_actions.Remove(first);
				first.Value.Invoke();
			}
		}

		public int Add(float time, Action callback)
		{
			LinkedListNode<ActionEvent> linkedListNode = ((_pool.Count == 0) ? new LinkedListNode<ActionEvent>(new ActionEvent()) : _pool.Pop());
			linkedListNode.Value.ID = ++_counter;
			linkedListNode.Value.Handler = callback;
			linkedListNode.Value.TimeStamp = UnityEngine.Time.realtimeSinceStartup + time;
			AddNode(linkedListNode);
			return linkedListNode.Value.ID;
		}

		private void AddNode(LinkedListNode<ActionEvent> node)
		{
			LinkedListNode<ActionEvent> linkedListNode = _actions.First;
			if (linkedListNode == null)
			{
				_actions.AddFirst(node);
				return;
			}
			float timeStamp = node.Value.TimeStamp;
			do
			{
				if (timeStamp <= linkedListNode.Value.TimeStamp)
				{
					_actions.AddBefore(linkedListNode, node);
					return;
				}
				linkedListNode = linkedListNode.Next;
			}
			while (linkedListNode != null);
			_actions.AddLast(node);
		}

		public void Remove(int id)
		{
			if (_actions.Count != 0)
			{
				ActionEvent value = _actions.FirstOrDefault((ActionEvent n) => n.ID == id);
				RemoveNode(_actions.Find(value));
			}
		}

		private void RemoveNode(LinkedListNode<ActionEvent> node)
		{
			if (node != null)
			{
				_actions.Remove(node);
				_pool.Push(node);
			}
		}
	}
}
