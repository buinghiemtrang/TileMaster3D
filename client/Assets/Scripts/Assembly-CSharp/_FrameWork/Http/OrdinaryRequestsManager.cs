using System;
using BestHTTP;

namespace _FrameWork.Http
{
	public class OrdinaryRequestsManager : RequestBase<OrdinaryRequestsManager>
	{
		private static int _counter;

		public SingleHttp Create(HTTPMethods type, string url, Action<ResultInfo> callback)
		{
			SingleHttp singleHttp = SingleHttp.Create(type, url, OnResult);
			_counter++;
			string text = _counter.ToString();
			singleHttp.Key = text;
			HttpInfo httpInfo = new HttpInfo();
			httpInfo.Http = singleHttp;
			httpInfo.ResultCallback = callback;
			Requests.Add(text, httpInfo);
			return singleHttp;
		}
	}
}
