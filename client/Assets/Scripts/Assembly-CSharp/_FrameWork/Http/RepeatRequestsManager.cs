using System;
using System.Collections.Generic;
using System.Linq;
using BestHTTP;

namespace _FrameWork.Http
{
	public class RepeatRequestsManager : RequestBase<RepeatRequestsManager>
	{
		public SingleHttp Create(string key, HTTPMethods type, string url, Action<ResultInfo> callback, int repeatCount = -1, bool isSuccessRemoveRequest = true, bool isAbortRemoveRequest = true, float intervalTime = 1f)
		{
			if (string.IsNullOrEmpty(key))
			{
				return null;
			}
			float num = 1f;
			if (intervalTime >= 1f)
			{
				num = intervalTime;
			}
			if (!Requests.ContainsKey(key))
			{
				SingleHttp singleHttp = SingleHttp.Create(type, url, OnResult);
				singleHttp.Key = key;
				RepeatHttpInfo repeatHttpInfo = new RepeatHttpInfo();
				repeatHttpInfo.Http = singleHttp;
				repeatHttpInfo.ResultCallback = callback;
				repeatHttpInfo.RepeatCount = repeatCount;
				repeatHttpInfo.Duration = 0f;
				repeatHttpInfo.IsRequestEnd = false;
				repeatHttpInfo.RepeatRequestInterval = num;
				repeatHttpInfo.IsSuccessRemoveRequest = isSuccessRemoveRequest;
				repeatHttpInfo.IsAbortRemoveRequest = isAbortRemoveRequest;
				Requests.Add(key, repeatHttpInfo);
				return singleHttp;
			}
			RepeatHttpInfo repeatHttpInfo2 = (RepeatHttpInfo)Requests[key];
			SingleHttp http = repeatHttpInfo2.Http;
			if (http.Type == type)
			{
				repeatHttpInfo2.ResultCallback = callback;
				repeatHttpInfo2.RepeatRequestInterval = num;
				repeatHttpInfo2.RepeatCount = repeatCount;
				repeatHttpInfo2.IsAbortRemoveRequest = isAbortRemoveRequest;
				repeatHttpInfo2.IsSuccessRemoveRequest = isSuccessRemoveRequest;
				return http;
			}
			return null;
		}

		public override void Abort(string key)
		{
			if (Requests.ContainsKey(key))
			{
				RepeatHttpInfo repeatHttpInfo = (RepeatHttpInfo)Requests[key];
				if (repeatHttpInfo.IsAbortRemoveRequest)
				{
					Requests.Remove(repeatHttpInfo.Http.Key);
				}
				if (!repeatHttpInfo.IsRequestEnd)
				{
					repeatHttpInfo.Http.ForceAbort();
				}
			}
		}

		protected override void Check()
		{
			CheckCustomTimeout();
			CheckRepeatRequest();
		}

		protected override void CheckCustomTimeout()
		{
			if (Requests.Count == 0)
			{
				return;
			}
			List<HttpInfo> list = Requests.Values.ToList();
			for (int num = list.Count - 1; num >= 0; num--)
			{
				RepeatHttpInfo repeatHttpInfo = (RepeatHttpInfo)list[num];
				if (!repeatHttpInfo.IsRequestEnd)
				{
					SingleHttp http = repeatHttpInfo.Http;
					repeatHttpInfo.Duration += 1f;
					if (repeatHttpInfo.Duration >= http.CustomTimeout)
					{
						repeatHttpInfo.IsRequestEnd = true;
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

		private void CheckRepeatRequest()
		{
			if (Requests.Count == 0)
			{
				return;
			}
			List<HttpInfo> list = Requests.Values.ToList();
			for (int num = list.Count - 1; num >= 0; num--)
			{
				RepeatHttpInfo repeatHttpInfo = (RepeatHttpInfo)list[num];
				if (repeatHttpInfo.IsRequestEnd)
				{
					repeatHttpInfo.Duration += 1f;
					if (repeatHttpInfo.Duration >= repeatHttpInfo.RepeatRequestInterval)
					{
						repeatHttpInfo.Duration = 0f;
						repeatHttpInfo.IsRequestEnd = false;
						repeatHttpInfo.Http.Send();
					}
				}
			}
		}

		protected override void OnResult(ResultInfo info)
		{
			if (info != null && Requests.ContainsKey(info.Key))
			{
				RepeatHttpInfo repeatHttpInfo = (RepeatHttpInfo)Requests[info.Key];
				int repeatCount = repeatHttpInfo.RepeatCount;
				repeatHttpInfo.Duration = 0f;
				repeatHttpInfo.IsRequestEnd = true;
				if (repeatCount > 0)
				{
					repeatHttpInfo.RepeatCount = repeatCount - 1;
					repeatCount--;
				}
				if (repeatCount == 0 || (repeatHttpInfo.IsSuccessRemoveRequest && info.State == RequestState.Success))
				{
					Requests.Remove(info.Key);
				}
				if (repeatHttpInfo.ResultCallback != null)
				{
					repeatHttpInfo.ResultCallback(info);
				}
			}
		}
	}
}
