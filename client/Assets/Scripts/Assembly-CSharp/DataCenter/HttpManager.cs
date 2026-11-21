using System;
using BestHTTP;
using UnityEngine;

namespace DataCenter
{
	public static class HttpManager
	{
		private static HTTPRequest Request(Uri url, HTTPMethods methodType, bool isKeepAlive, bool disableCache, Action<HTTPRequest, HTTPResponse> callback)
		{
			return new HTTPRequest(url, methodType, isKeepAlive, disableCache, delegate(HTTPRequest req, HTTPResponse resp)
			{
				switch (req.State)
				{
				case HTTPRequestStates.Finished:
					if (!resp.IsSuccess)
					{
						Debug.LogWarning(string.Format("Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}", resp.StatusCode, resp.Message, resp.DataAsText));
						if (callback != null)
						{
							callback(req, null);
						}
						return;
					}
					Debug.Log("====HTTPMgr.Request finished Successfully");
					if (callback != null)
					{
						callback(req, resp);
					}
					return;
				case HTTPRequestStates.Error:
					Debug.LogError("Request Finished with Error! " + ((req.Exception != null) ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception"));
					break;
				case HTTPRequestStates.Aborted:
					Debug.LogWarning("Request Aborted!");
					break;
				case HTTPRequestStates.ConnectionTimedOut:
					Debug.LogError("Connection Timed Out!");
					break;
				case HTTPRequestStates.TimedOut:
					Debug.LogError("Processing the request Timed Out!");
					break;
				}
				if (callback != null)
				{
					callback(req, null);
				}
			});
		}

		public static HTTPRequest Post(Uri url, Action<HTTPRequest, HTTPResponse> callback)
		{
			return Request(url, HTTPMethods.Post, false, true, callback);
		}

		public static HTTPRequest Get(Uri url, Action<HTTPRequest, HTTPResponse> callback)
		{
			return Request(url, HTTPMethods.Get, false, true, callback);
		}
	}
}
