using System;
using System.Collections.Generic;
using System.Text;
using BestHTTP;
using BestHTTP.Forms;
using UnityEngine;

namespace _FrameWork.Http
{
	public class SingleHttp
	{
		public string Key;

		public object Tag;

		public HTTPMethods Type;

		public float CustomTimeout;

		private bool _isKeepAlive;

		private bool _disableCache;

		private double _timeout;

		private double _connectTimeout;

		private bool _isRequesting;

		private HTTPRequest _request;

		private HTTPUrlEncodedForm _encodedForm;

		private Action<ResultInfo> _callback;

		public HTTPRequest Request => _request;

		public SingleHttp()
		{
			Key = string.Empty;
			Tag = null;
			Type = HTTPMethods.Get;
			CustomTimeout = 100f;
			_isKeepAlive = false;
			_disableCache = true;
			_connectTimeout = 20.0;
			_timeout = 60.0;
			_isRequesting = false;
		}

		public static SingleHttp Create(HTTPMethods type, string url, Action<ResultInfo> callback)
		{
			SingleHttp singleHttp = new SingleHttp();
			singleHttp.Type = type;
			singleHttp._callback = callback;
			HTTPRequest httpRequest = new HTTPRequest(new Uri(url), type, singleHttp.OnFinished);
			singleHttp._request = httpRequest;
			if (httpRequest != null && !singleHttp._isRequesting)
			{
				httpRequest.AddHeader("client-version", Application.version);
			}
			return singleHttp;
		}

		public static SingleHttp CreateGet(string url, Action<ResultInfo> callback)
		{
			return Create(HTTPMethods.Get, url, callback);
		}

		public static SingleHttp CreatePost(string url, Action<ResultInfo> callback)
		{
			return Create(HTTPMethods.Post, url, callback);
		}

		public SingleHttp AddHeaders(Dictionary<string, string> headers)
		{
			if (_request != null && !_isRequesting)
			{
				foreach (KeyValuePair<string, string> header in headers)
				{
					_request.AddHeader(header.Key, header.Value);
				}
			}
			return this;
		}

		public SingleHttp AddHeader(string key, string value)
		{
			if (_request != null && !_isRequesting)
			{
				_request.AddHeader(key, value);
			}
			return this;
		}

		public SingleHttp AddParameter(string body)
		{
			if (_request != null && !_isRequesting)
			{
				_request.RawData = Encoding.UTF8.GetBytes(body);
			}
			return this;
		}

		public SingleHttp AddParameter(string key, string value, bool isLastOne = false)
		{
			if (_request != null && !_isRequesting)
			{
				if (_encodedForm == null)
				{
					_encodedForm = new HTTPUrlEncodedForm();
				}
				_encodedForm.AddField(key, value);
				if (isLastOne)
				{
					_request.SetForm(_encodedForm);
				}
			}
			return this;
		}

		public void ForceAbort()
		{
			if (_request != null && _isRequesting)
			{
				_request.Abort();
			}
		}

		public SingleHttp AddKey(string value)
		{
			Key = value;
			return this;
		}

		public SingleHttp AddTag(object value)
		{
			Tag = value;
			return this;
		}

		public SingleHttp AddCustomTimeout(float value)
		{
			CustomTimeout = value;
			return this;
		}

		public SingleHttp Timeout(double value)
		{
			_timeout = value;
			return this;
		}

		public SingleHttp ConnectTimeout(double value)
		{
			_connectTimeout = value;
			return this;
		}

		public SingleHttp KeepAlive(bool value)
		{
			_isKeepAlive = value;
			return this;
		}

		public SingleHttp DisableCache(bool value)
		{
			_disableCache = value;
			return this;
		}

		public void Send()
		{
			if (_request == null || _isRequesting)
			{
				return;
			}
			_isRequesting = true;
			_request.IsKeepAlive = _isKeepAlive;
			_request.DisableCache = _disableCache;
			_request.Timeout = TimeSpan.FromSeconds(_timeout);
			_request.ConnectTimeout = TimeSpan.FromSeconds(_connectTimeout);
			_request.Send();
		}

		private void OnFinished(HTTPRequest req, HTTPResponse resp)
		{
			if (!_isRequesting)
			{
				return;
			}
			_isRequesting = false;
			RequestState state;
			switch (req.State)
			{
			case HTTPRequestStates.Finished:
				if (resp.IsSuccess)
				{
					state = RequestState.Success;
				}
				else
				{
					Debug(string.Concat("Server sent Error:", string.Format("{0}-", resp.StatusCode), resp.Message, "-", resp.DataAsText));
					state = RequestState.Fail;
				}
				break;
			case HTTPRequestStates.Error:
				Debug("Error:" + ((req.Exception != null) ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception"));
				state = RequestState.Fail;
				break;
			case HTTPRequestStates.Aborted:
				Debug("Request Aborted!");
				state = RequestState.Aborted;
				break;
			case HTTPRequestStates.ConnectionTimedOut:
				Debug("Connection Timed Out!");
				state = RequestState.Fail;
				break;
			case HTTPRequestStates.TimedOut:
				Debug("Processing the request Timed Out!");
				state = RequestState.Fail;
				break;
			default:
				Debug("Unknown error!");
				state = RequestState.Fail;
				break;
			}
			ResultInfo resultInfo = new ResultInfo();
			resultInfo.Key = Key;
			resultInfo.Tag = Tag;
			resultInfo.Response = resp;
			resultInfo.State = state;
			if (_callback != null)
			{
				_callback(resultInfo);
			}
		}

		private void Debug(string mes, bool isError = true)
		{
		}
	}
}
