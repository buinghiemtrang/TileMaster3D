using System;
using BestHTTP;

namespace _FrameWork.Http
{
	public class OnlyRequestsManager : RequestBase<OnlyRequestsManager>
	{
		public SingleHttp Create(string key, HTTPMethods type, string url, Action<ResultInfo> callback)
		{
			if (string.IsNullOrEmpty(key))
			{
				return null;
			}
			if (!Requests.ContainsKey(key))
			{
				SingleHttp singleHttp = SingleHttp.Create(type, url, OnResult);
				singleHttp.Key = key;
				HttpInfo httpInfo = new HttpInfo();
				httpInfo.Http = singleHttp;
				httpInfo.ResultCallback = callback;
				Requests.Add(key, httpInfo);
				return singleHttp;
			}
			HttpInfo httpInfo2 = Requests[key];
			SingleHttp http = httpInfo2.Http;
			if (http.Type == type)
			{
				httpInfo2.ResultCallback = callback;
				return http;
			}
			return null;
		}
	}
}
