using System;
using System.Collections.Generic;
using System.Text;
using BestHTTP;
using BestHTTP.Forms;
using UnityEngine;

namespace OwnHttp
{
	public class OwnSingleHttp
	{
		public float CustomTimeout;

		public string Key;

		public object Tag;

		private bool _isKeepAlive;

		private bool _disableCache;

		private double _timeout;

		private double _connectTimeout;

		private HTTPRequest _request;

		private HTTPUrlEncodedForm _encodedForm;

		private Action<ResultInfo> _callback;

		public OwnSingleHttp()
		{
			CustomTimeout = 100f;
			Key = string.Empty;
			Tag = null;
			_isKeepAlive = false;
			_disableCache = true;
			_connectTimeout = 20.0;
			_timeout = 60.0;
		}

		public OwnSingleHttp Creat(HTTPMethods type, string url, Action<ResultInfo> callback)
		{
			_callback = callback;
			_request = new HTTPRequest(new Uri(url), type, _OnFinished);
			if (_request != null)
			{
				_request.AddHeader("client-version", Application.version);
			}
			return this;
		}

		public OwnSingleHttp AddHeader(string key, string value)
		{
			if (_request != null)
			{
				_request.AddHeader(key, value);
			}
			return this;
		}

		public OwnSingleHttp AddHeader(Dictionary<string, string> headers)
		{
			if (headers != null && _request != null)
			{
				foreach (KeyValuePair<string, string> header in headers)
				{
					_request.AddHeader(header.Key, header.Value);
				}
			}
			return this;
		}

		public OwnSingleHttp AddParameter(string body)
		{
			if (_request != null)
			{
				_request.RawData = Encoding.UTF8.GetBytes(body);
			}
			return this;
		}

		public OwnSingleHttp AddParameter(string key, string value, bool isLastOne = false)
		{
			if (_encodedForm == null)
			{
				_encodedForm = new HTTPUrlEncodedForm();
			}
			_encodedForm.AddField(key, value);
			if (isLastOne && _request != null)
			{
				_request.SetForm(_encodedForm);
			}
			return this;
		}

		public void ForceAbort()
		{
			if (_request != null)
			{
				_request.Abort();
			}
		}

		public OwnSingleHttp AddKey(string value)
		{
			Key = value;
			return this;
		}

		public OwnSingleHttp AddTag(object value)
		{
			Tag = value;
			return this;
		}

		public OwnSingleHttp AddCustomTimeout(float value)
		{
			CustomTimeout = value;
			return this;
		}

		public OwnSingleHttp Timeout(double value)
		{
			_timeout = value;
			return this;
		}

		public OwnSingleHttp ConnectTimeout(double value)
		{
			_connectTimeout = value;
			return this;
		}

		public OwnSingleHttp KeepAlive(bool value)
		{
			_isKeepAlive = value;
			return this;
		}

		public OwnSingleHttp DisableCache(bool value)
		{
			_disableCache = value;
			return this;
		}

		public void Send()
		{
			if (_request == null)
			{
				return;
			}
			_request.IsKeepAlive = _isKeepAlive;
			_request.DisableCache = _disableCache;
			_request.Timeout = TimeSpan.FromSeconds(_timeout);
			_request.ConnectTimeout = TimeSpan.FromSeconds(_connectTimeout);
			_request.Send();
		}

		private void _OnFinished(HTTPRequest req, HTTPResponse resp)
		{
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
					_Debug(string.Format("Server sent Error:{0}-{1}-{2}", resp.StatusCode, resp.Message, resp.DataAsText));
					state = RequestState.Fail;
				}
				break;
			case HTTPRequestStates.Error:
				_Debug("Error:" + ((req.Exception != null) ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception"));
				state = RequestState.Fail;
				break;
			case HTTPRequestStates.Aborted:
				_Debug("Request Aborted!");
				state = RequestState.Aborted;
				break;
			case HTTPRequestStates.ConnectionTimedOut:
				_Debug("Connection Timed Out!");
				state = RequestState.Fail;
				break;
			case HTTPRequestStates.TimedOut:
				_Debug("Processing the request Timed Out!");
				state = RequestState.Fail;
				break;
			default:
				_Debug("Unknown error!");
				state = RequestState.Fail;
				break;
			}
			ResultInfo resultInfo = new ResultInfo();
			resultInfo.Key = Key;
			resultInfo.Tag = Tag;
			resultInfo.State = state;
			resultInfo.Response = resp;
			_callback.InvokeSafe(resultInfo);
		}

		private void _Debug(string mes, bool isError = true)
		{
		}
	}
}
