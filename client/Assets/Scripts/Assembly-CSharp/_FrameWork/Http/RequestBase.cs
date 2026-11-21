using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _FrameWork.Http
{
	public abstract class RequestBase<T> : MonoBehaviour where T : RequestBase<T>
	{
		protected const float CheckTimeInterval = 1f;

		private Dictionary<string, HttpInfo> _requests;

		private float _duration;

		private static T _instance;

		protected Dictionary<string, HttpInfo> Requests => _requests;

		public static T Instance
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

		protected virtual void Awake()
		{
			_duration = 0f;
			_requests = new Dictionary<string, HttpInfo>();
		}

		private void Update()
		{
			_duration += Time.deltaTime;
			if (!(_duration < 1f))
			{
				_duration = 0f;
				Check();
			}
		}

		public virtual void Abort(string key)
		{
			if (_requests.ContainsKey(key))
			{
				SingleHttp http = _requests[key].Http;
				_requests.Remove(http.Key);
				http.ForceAbort();
			}
		}

		public SingleHttp GetHttp(string key)
		{
			if (_requests.ContainsKey(key))
			{
				return _requests[key].Http;
			}
			return null;
		}

		protected virtual void OnResult(ResultInfo info)
		{
			if (_requests.ContainsKey(info.Key))
			{
				_requests[info.Key].ResultCallback(info);
				_requests.Remove(info.Key);
			}
		}

		protected virtual void Check()
		{
			CheckCustomTimeout();
		}

		protected virtual void CheckCustomTimeout()
		{
			if (_requests.Count == 0)
			{
				return;
			}
			List<HttpInfo> list = _requests.Values.ToList();
			for (int num = list.Count - 1; num >= 0; num--)
			{
				SingleHttp http = list[num].Http;
				http.CustomTimeout -= 1f;
				if (http.CustomTimeout <= 0f)
				{
					ResultInfo resultInfo = new ResultInfo();
					resultInfo.Key = http.Key;
					resultInfo.Tag = http.Tag;
					resultInfo.State = RequestState.Fail;
					resultInfo.Response = null;
					OnResult(resultInfo);
					http.ForceAbort();
				}
			}
		}
	}
}
