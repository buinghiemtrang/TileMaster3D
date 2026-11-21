using System;
using UnityEngine;

namespace OwnHttp
{
	public class RequestsMgrBase<T> : MonoBehaviour where T : Component
	{
		protected class HttpInfo
		{
			public OwnSingleHttp Http;

			public Action<ResultInfo> ResultCallback;
		}

		private const float CheckTimeInterval = 1f;

		private float _duration;

		private static T _instance;

		public static T I
		{
			get
			{
				if (!(_instance != null))
				{
					GameObject gameObject = new GameObject(typeof(T).Name);
					_instance = gameObject.AddComponent<T>();
				}
				return _instance;
			}
		}

		private void Update()
		{
			_duration += Time.deltaTime;
			if (!(_duration < 1f))
			{
				_duration = 0f;
				CheckCustomTimeout();
			}
		}

		protected virtual void CheckCustomTimeout()
		{
		}
	}
}
