using System;
using System.Collections.Generic;
using System.Linq;
using BestHTTP;

namespace OwnHttp
{
	public class CheckRepeatRequestsMgr : RequestsMgrBase<CheckRepeatRequestsMgr>
	{
		private readonly Dictionary<string, HttpInfo> _allRequests = new Dictionary<string, HttpInfo>();

		public OwnSingleHttp Creat(string key, HTTPMethods type, string url, Action<ResultInfo> callback)
		{
			if (!_allRequests.ContainsKey(key))
			{
				OwnSingleHttp ownSingleHttp = new OwnSingleHttp().Creat(type, url, _Result);
				ownSingleHttp.Key = key;
				HttpInfo httpInfo = new HttpInfo();
				httpInfo.Http = ownSingleHttp;
				httpInfo.ResultCallback = callback;
				_allRequests.Add(key, httpInfo);
				return ownSingleHttp;
			}
			_allRequests[key].ResultCallback = callback;
			return null;
		}

		protected override void CheckCustomTimeout()
		{
			List<HttpInfo> list = _allRequests.Values.ToList();
			for (int num = list.Count - 1; num >= 0; num--)
			{
				HttpInfo httpInfo = list[num];
				OwnSingleHttp http = httpInfo.Http;
				http.CustomTimeout -= 1f;
				if (http.CustomTimeout <= 0f)
				{
					http.ForceAbort();
					ResultInfo resultInfo = new ResultInfo();
					resultInfo.Key = http.Key;
					resultInfo.Tag = http.Tag;
					resultInfo.Response = null;
					resultInfo.State = RequestState.Fail;
					httpInfo.ResultCallback.InvokeSafe(resultInfo);
					_allRequests.Remove(http.Key);
				}
			}
		}

		public void Abort(string key)
		{
			if (_allRequests.ContainsKey(key))
			{
				_allRequests[key].Http.ForceAbort();
			}
		}

		public OwnSingleHttp GetHttp(string key)
		{
			if (_allRequests.ContainsKey(key))
			{
				return _allRequests[key].Http;
			}
			return null;
		}

		private void _Result(ResultInfo info)
		{
			if (info != null && _allRequests.ContainsKey(info.Key))
			{
				_allRequests[info.Key].ResultCallback.InvokeSafe(info);
				_allRequests.Remove(info.Key);
			}
		}
	}
}
