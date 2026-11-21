using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DataCenter
{
	public static class DelayCallBack
	{
		private class TaskBehaviour : MonoBehaviour
		{
			private readonly List<UnityAction> _actions = new List<UnityAction>();

			private readonly List<float> _durations = new List<float>();

			private void OnDestroy()
			{
				lock (_durations)
				{
					_durations.Clear();
				}
				lock (_actions)
				{
					_actions.Clear();
				}
			}

			private void Update()
			{
				float deltaTime = Time.deltaTime;
				for (int num = _actions.Count - 1; num >= 0; num--)
				{
					UnityAction unityAction = _actions[num];
					if (unityAction == null || unityAction.Target == null)
					{
						_actions.RemoveAt(num);
						_durations.RemoveAt(num);
						continue;
					}
					float num2 = _durations[num] - deltaTime;
					_durations[num] = num2;
					if (num2 <= 0f)
					{
						unityAction();
						_actions.RemoveAt(num);
						_durations.RemoveAt(num);
					}
				}
			}

			public void Add(float time, UnityAction callback)
			{
				int num = _actions.IndexOf(callback);
				if (num == -1)
				{
					_actions.Add(callback);
					_durations.Add(time);
				}
				else
				{
					_durations[num] = time;
				}
			}

			public void Remove(UnityAction callback)
			{
				int num = _actions.IndexOf(callback);
				if (num != -1)
				{
					_actions[num] = null;
				}
			}
		}

		private static readonly TaskBehaviour Task;

		static DelayCallBack()
		{
			GameObject gameObject = new GameObject("DelayCallBack");
			Object.DontDestroyOnLoad(gameObject);
			Task = gameObject.AddComponent<TaskBehaviour>();
		}

		public static void Start(float time, UnityAction callback)
		{
			Task.Add(time, callback);
		}

		public static void Stop(UnityAction callback)
		{
			Task.Remove(callback);
		}
	}
}
