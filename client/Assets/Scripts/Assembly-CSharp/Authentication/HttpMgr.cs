using System;
using System.Collections.Generic;
using BestHTTP;
using BestHTTP.Forms;
using Newtonsoft.Json;
using UnityEngine;

namespace Authentication
{
	public class HttpMgr : MonoBehaviour
	{
		private Action<HTTPResponse> cback;

		public void Git(string url, Action<HTTPResponse> cback)
		{
			_debug(url);
			this.cback = cback;
			HTTPRequest httpRequest = new HTTPRequest(new Uri(url), _onFinished);
			httpRequest.IsKeepAlive = false;
			httpRequest.DisableCache = true;
			httpRequest.Send();
		}

		public HTTPRequest Post(string url, Dictionary<string, string> headers, Dictionary<string, string> parameters, Action<HTTPResponse> cback)
		{
			_debug(url);
			_debug("headers:" + JsonConvert.SerializeObject(headers));
			_debug("parameters:" + JsonConvert.SerializeObject(parameters));
			this.cback = cback;
			HTTPRequest httpRequest = new HTTPRequest(new Uri(url), HTTPMethods.Post, _onFinished);
			if (headers != null)
			{
				foreach (KeyValuePair<string, string> header in headers)
				{
					httpRequest.AddHeader(header.Key, header.Value);
				}
			}
			if (parameters != null && parameters.Count > 0)
			{
				HTTPUrlEncodedForm hTTPUrlEncodedForm = new HTTPUrlEncodedForm();
				foreach (KeyValuePair<string, string> parameter in parameters)
				{
					hTTPUrlEncodedForm.AddField(parameter.Key, parameter.Value);
				}
				httpRequest.SetForm(hTTPUrlEncodedForm);
			}
			httpRequest.IsKeepAlive = false;
			httpRequest.DisableCache = true;
			httpRequest.Send();
			return httpRequest;
		}

		private void _onFinished(HTTPRequest req, HTTPResponse resp)
		{
			string mes;
			switch (req.State)
			{
			case HTTPRequestStates.Finished:
				if (resp.IsSuccess)
				{
					cback?.Invoke(resp);
					return;
				}
				mes = string.Format("Server sent Error:{0}-{1}-{2}", resp.StatusCode, resp.Message, resp.DataAsText);
				break;
			case HTTPRequestStates.Error:
			{
				string text = (req.Exception != null) ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception";
				mes = "Error:" + text;
				break;
			}
			case HTTPRequestStates.Aborted:
				mes = "Request Aborted!";
				break;
			case HTTPRequestStates.ConnectionTimedOut:
				mes = "Connection Timed Out!";
				break;
			case HTTPRequestStates.TimedOut:
				mes = "Processing the request Timed Out!";
				break;
			default:
				mes = "Unknown error!";
				break;
			}
			_debug(mes);
			cback?.Invoke(null);
		}

		private static void _debug(string mes)
		{
			Debug.Log("Authentication HTTP Request: " + mes);
		}
	}
}
