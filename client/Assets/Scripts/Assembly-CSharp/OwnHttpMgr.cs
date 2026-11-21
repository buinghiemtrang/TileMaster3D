using System;
using System.Collections.Generic;
using System.Text;
using BestHTTP;
using BestHTTP.Forms;
using UnityEngine;

public class OwnHttpMgr
{
	private class HttpInfo
	{
		public string key;

		public Action<HTTPResponse> onCallback;
	}

	public static readonly OwnHttpMgr I = new OwnHttpMgr();

	private bool isKeepAlive;

	private bool disableCache;

	private double timeout;

	private double connectTimeout;

	private readonly Dictionary<HTTPRequest, HttpInfo> allRequests;

	public bool IsKeepAlive
	{
		set
		{
			isKeepAlive = value;
		}
	}

	public bool DisableCache
	{
		set
		{
			disableCache = value;
		}
	}

	public double Timeout
	{
		set
		{
			timeout = value;
		}
	}

	public double ConnectTimeout
	{
		set
		{
			connectTimeout = value;
		}
	}

	public OwnHttpMgr()
	{
		isKeepAlive = false;
		disableCache = true;
		connectTimeout = 20.0;
		timeout = 60.0;
		allRequests = new Dictionary<HTTPRequest, HttpInfo>();
	}

	public HTTPRequest SendGet(string url, Dictionary<string, string> headers, Action<HTTPResponse> cback, string key = "")
	{
		if (_CheckRepeatRequest(key))
		{
			return null;
		}
		HTTPRequest httpRequest = new HTTPRequest(new Uri(url), _onFinished);
		_AddRequest(key, httpRequest, cback);
		if (headers != null)
		{
			foreach (KeyValuePair<string, string> header in headers)
			{
				httpRequest.AddHeader(header.Key, header.Value);
			}
		}
		httpRequest.AddHeader("client-version", Application.version);
		_send(httpRequest);
		return httpRequest;
	}

	public void SendDelete(string url, Dictionary<string, string> headers, string body, Action<HTTPResponse> cback, string key = "")
	{
		if (!_CheckRepeatRequest(key))
		{
			HTTPRequest httpRequest = new HTTPRequest(new Uri(url), HTTPMethods.Delete, _onFinished);
			_AddRequest(key, httpRequest, cback);
			if (headers != null)
			{
				foreach (KeyValuePair<string, string> header in headers)
				{
					httpRequest.AddHeader(header.Key, header.Value);
				}
			}
			httpRequest.AddHeader("client-version", Application.version);
			if (!string.IsNullOrEmpty(body))
			{
				httpRequest.RawData = Encoding.UTF8.GetBytes(body);
			}
			_send(httpRequest);
		}
	}

	public void SendPost(string url, Dictionary<string, string> headers, Dictionary<string, string> parameters, Action<HTTPResponse> cback, string key = "")
	{
		if (!_CheckRepeatRequest(key))
		{
			HTTPRequest httpRequest = new HTTPRequest(new Uri(url), HTTPMethods.Post, _onFinished);
			_AddRequest(key, httpRequest, cback);
			if (headers != null)
			{
				foreach (KeyValuePair<string, string> header in headers)
				{
					httpRequest.AddHeader(header.Key, header.Value);
				}
			}
			httpRequest.AddHeader("client-version", Application.version);
			if (parameters != null && parameters.Count > 0)
			{
				HTTPUrlEncodedForm hTTPUrlEncodedForm = new HTTPUrlEncodedForm();
				foreach (KeyValuePair<string, string> parameter in parameters)
				{
					hTTPUrlEncodedForm.AddField(parameter.Key, parameter.Value);
				}
				httpRequest.SetForm(hTTPUrlEncodedForm);
			}
			_send(httpRequest);
		}
	}

	public void SendPost(string url, Dictionary<string, string> headers, string body, Action<HTTPResponse> cback, string key = "")
	{
		if (!_CheckRepeatRequest(key))
		{
			HTTPRequest httpRequest = new HTTPRequest(new Uri(url), HTTPMethods.Post, _onFinished);
			_AddRequest(key, httpRequest, cback);
			if (headers != null)
			{
				foreach (KeyValuePair<string, string> header in headers)
				{
					httpRequest.AddHeader(header.Key, header.Value);
				}
			}
			httpRequest.AddHeader("client-version", Application.version);
			httpRequest.RawData = Encoding.UTF8.GetBytes(body);
			_send(httpRequest);
		}
	}

	private void _onFinished(HTTPRequest req, HTTPResponse resp)
	{
		bool isTrigger = true;
		HTTPResponse resp2 = resp;
		if (req.State != HTTPRequestStates.Finished || !resp.IsSuccess)
		{
			isTrigger = req.State != HTTPRequestStates.Aborted;
			resp2 = null;
		}
		_RemoveRequest(req, resp2, isTrigger);
		switch (req.State)
		{
		case HTTPRequestStates.Finished:
			if (!resp.IsSuccess)
			{
				_debug(string.Format("Server sent Error:{0}-{1}-{2}-{3}", req.Uri, resp.StatusCode, resp.Message, resp.DataAsText));
			}
			break;
		case HTTPRequestStates.Error:
			_debug(string.Format("{0} Error: {1}", req.Uri, (req.Exception != null) ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception"));
			break;
		case HTTPRequestStates.Aborted:
			_debug(string.Format("Request Aborted! {0}", req.Uri));
			break;
		case HTTPRequestStates.ConnectionTimedOut:
			_debug(string.Format("Connection Timed Out! {0}", req.Uri));
			break;
		case HTTPRequestStates.TimedOut:
			_debug(string.Format("Processing the request Timed Out! {0}", req.Uri));
			break;
		default:
			_debug(string.Format("Unknown error! {0}", req.Uri));
			break;
		}
	}

	private void _send(HTTPRequest req)
	{
		if (req != null)
		{
			req.IsKeepAlive = isKeepAlive;
			req.DisableCache = disableCache;
			req.Timeout = TimeSpan.FromSeconds(timeout);
			req.ConnectTimeout = TimeSpan.FromSeconds(connectTimeout);
			req.Send();
		}
	}

	private bool _CheckRepeatRequest(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return false;
		}
		foreach (KeyValuePair<HTTPRequest, HttpInfo> allRequest in allRequests)
		{
			if (!string.IsNullOrEmpty(allRequest.Value.key) && allRequest.Value.key.Equals(key))
			{
				return true;
			}
		}
		return false;
	}

	private void _AddRequest(string key, HTTPRequest req, Action<HTTPResponse> cback)
	{
		if (!string.IsNullOrEmpty(key))
		{
			HttpInfo httpInfo = new HttpInfo();
			httpInfo.key = key;
			httpInfo.onCallback = cback;
			allRequests.Add(req, httpInfo);
		}
	}

	private void _RemoveRequest(HTTPRequest req, HTTPResponse resp, bool isTrigger)
	{
		if (allRequests.ContainsKey(req))
		{
			if (isTrigger && allRequests[req].onCallback != null)
			{
				allRequests[req].onCallback(resp);
			}
			allRequests.Remove(req);
		}
	}

	private void _debug(string mes)
	{
	}
}
