using System;
using System.Collections.Generic;
using UnityEngine;

namespace FQDev.Time
{
	public static class TimeManager
	{
		private class TimeBehaviour : MonoBehaviour
		{
			private Stack<LinkedListNode<ActionEvent>> _pool;

			private LinkedList<DelayCallBack> _cache;

			private DelayCallBack _delayCallBack;

			private void Awake()
			{
				_pool = new Stack<LinkedListNode<ActionEvent>>();
				_cache = new LinkedList<DelayCallBack>();
				_delayCallBack = NewDelayCallbackInternal();
			}

			private void Update()
			{
				for (LinkedListNode<DelayCallBack> linkedListNode = _cache.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
				{
					linkedListNode.Value.Update();
				}
			}

			internal DelayCallBack NewDelayCallbackInternal()
			{
				DelayCallBack delayCallBack = new DelayCallBack(_pool);
				_cache.AddLast(delayCallBack);
				return delayCallBack;
			}

			internal int SetupInvokeInternal(float time, Action callback)
			{
				return _delayCallBack.Add(time, callback);
			}

			internal void CancelInvokeInternal(int id)
			{
				_delayCallBack.Remove(id);
			}
		}

		private static readonly TimeBehaviour _timeBehaviour;

		static TimeManager()
		{
			GameObject gameObject = new GameObject("TimeManager");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			_timeBehaviour = gameObject.AddComponent<TimeBehaviour>();
		}

		public static int SetupInvoke(float time, Action callback)
		{
			return _timeBehaviour.SetupInvokeInternal(time, callback);
		}

		public static void CancelInvoke(int id)
		{
			_timeBehaviour.CancelInvokeInternal(id);
		}
	}
}
