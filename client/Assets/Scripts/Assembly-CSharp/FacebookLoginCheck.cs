using System;
using BestHTTP;
using Newtonsoft.Json;
using UnityEngine;

public class FacebookLoginCheck : Singleton<FacebookLoginCheck>
{
	private HTTPRequest request;

	private Action<bool> _checkAction;

	private const string Address = "https://service.fuqiangame.com/utils/api/v1/getFBDeletion";

	public void Check(Action<bool> callBack)
	{
		_checkAction = callBack;
		if (request != null)
		{
			request.Abort();
			request = null;
		}
		Uri uri = new Uri("https://service.fuqiangame.com/utils/api/v1/getFBDeletion");
		request = new HTTPRequest(uri, HTTPMethods.Post, delegate(HTTPRequest req, HTTPResponse resp)
		{
			if (req.State == HTTPRequestStates.Finished)
			{
				if (resp == null)
				{
					Debug.Log("Get ranking data failed:resp is null");
					OnRequestFailed(null);
				}
				else if (!resp.IsSuccess)
				{
					Debug.Log(string.Format("Get ranking data failed with server sent ERROR:{0}-{1}-{2}", resp.StatusCode, resp.Message, resp.DataAsText));
					OnRequestFailed(resp);
				}
				else
				{
					OnRequestSucceeded(resp);
				}
				request = null;
			}
			else
			{
				Debug.Log(string.Format("Get ranking data failed with request STATE: {0}", req.State));
				OnRequestFailed(resp);
				request = null;
			}
		});
		Debug.LogError("Authorization:Bearer " + DB.Account.Token);
		request.IsKeepAlive = false;
		request.DisableCache = true;
		request.ConnectTimeout = TimeSpan.FromSeconds(5.0);
		request.Timeout = TimeSpan.FromSeconds(5.0);
		request.AddHeader("client-version", Application.version);
		request.AddHeader("Content-Type", "application/json");
		request.AddHeader("Authorization", "Bearer " + DB.Account.Token);
		request.Send();
	}

	private void OnRequestSucceeded(HTTPResponse response)
	{
		FacebookDeletionRoot facebookDeletionRoot = JsonConvert.DeserializeObject<FacebookDeletionRoot>(response.DataAsText);
		bool obj;
		if (facebookDeletionRoot == null)
		{
			obj = false;
		}
		else
		{
			if (facebookDeletionRoot.code != 1)
			{
				Debug.LogError(string.Format("Request server code:{0} error:{1}", facebookDeletionRoot.code, facebookDeletionRoot.msg));
			}
			obj = facebookDeletionRoot.data.isDelete;
		}
		if (_checkAction != null)
		{
			_checkAction(obj);
		}
	}

	private void OnRequestFailed(HTTPResponse response)
	{
		if (_checkAction != null)
		{
			_checkAction(false);
		}
		if (response != null)
		{
			Debug.LogError(string.Format("FacebookLoginCheck OnRequestFailed StatusCode:{0},Message:{1}", response.StatusCode, response.Message));
		}
	}

	public void AbortCheck()
	{
		if (request != null)
		{
			request.Abort();
			request.Tag = "AbortRequest";
		}
	}
}
