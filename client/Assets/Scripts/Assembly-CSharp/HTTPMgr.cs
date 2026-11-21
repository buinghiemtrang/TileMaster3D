using System;
using BestHTTP;

public static class HTTPMgr
{
	public static HTTPRequest Request(string sUrl, Action<HTTPRequest, HTTPResponse> callback)
	{
		HTTPRequest request = new HTTPRequest(new Uri(sUrl), (req, resp) =>
		{
			if (req.State == HTTPRequestStates.Finished && resp.IsSuccess)
			{
				if (callback != null)
				{
					callback(req, resp);
				}
				return;
			}
			if (callback != null)
			{
				callback(req, null);
			}
		});
		request.ConnectTimeout = TimeSpan.FromSeconds(5.0);
		request.Timeout = TimeSpan.FromSeconds(5.0);
		request.IsKeepAlive = false;
		return request;
	}
}
