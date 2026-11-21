using System;
using BestHTTP;
using Newtonsoft.Json;
using UnityEngine;

public static class RankingUtils
{
	[Serializable]
	private class ServerInfoData
	{
		public int code;

		public string msg;

		public string data;
	}

	public static Action<HTTPRequestStates> GetMonthIssueCallback;

	private const string UrlGetMonthIssue = "https://service.fuqiangame.com/common/GetMonthIssue";

	private static HTTPRequest _requestGetMonthIssue;

	public static void ReqMonthIssue()
	{
		if (_requestGetMonthIssue != null)
		{
			_requestGetMonthIssue.Abort();
			_requestGetMonthIssue = null;
		}
		_requestGetMonthIssue = new HTTPRequest(new Uri("https://service.fuqiangame.com/common/GetMonthIssue"), delegate(HTTPRequest req, HTTPResponse resp)
		{
			if (req.State == HTTPRequestStates.Finished)
			{
				if (resp != null)
				{
					if (resp.IsSuccess)
					{
						ServerInfoData serverInfoData = JsonConvert.DeserializeObject<ServerInfoData>(resp.DataAsText);
						if (serverInfoData != null && serverInfoData.code == 1 && serverInfoData.msg.Equals(string.Empty) && serverInfoData.data != null)
						{
							DB.User.MonthIssue = serverInfoData.data;
						}
					}
					else
					{
						Debug.Log(string.Format("Get MonthIssue failed with server sent ERROR:{0}-{1}-{2}", resp.StatusCode, resp.Message, resp.DataAsText));
					}
				}
				else
				{
					Debug.Log("Get MonthIssue failed with server sent ERROR:resp is null");
				}
			}
			else
			{
				Debug.Log(string.Format("Get MonthIssue failed with request STATE: {0}", req.State));
			}
			GetMonthIssueCallback?.Invoke(req.State);
			_requestGetMonthIssue = null;
		});
		_requestGetMonthIssue.IsKeepAlive = false;
		_requestGetMonthIssue.DisableCache = true;
		_requestGetMonthIssue.ConnectTimeout = TimeSpan.FromSeconds(5.0);
		_requestGetMonthIssue.Timeout = TimeSpan.FromSeconds(5.0);
		_requestGetMonthIssue.AddHeader("Content-Type", "application/json");
		_requestGetMonthIssue.AddHeader("CommonServiceToken", "9b5ba0c0419840ccab872275f41e5cc7");
		_requestGetMonthIssue.AddHeader("client-version", Application.version);
		_requestGetMonthIssue.Send();
	}
}
