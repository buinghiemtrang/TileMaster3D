// [rebuild] BestHTTP shim transport. BestHTTP gốc là asset trả phí và bản xuất chỉ có stub,
// nên phần game dùng (HTTPRequest/HTTPResponse/HTTPManager/Forms) được nối vào UnityWebRequest ở đây.
// Callback luôn chạy trên main thread, giống BestHTTP. Lỗi mạng: Response = null, State Error/TimedOut.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace BestHTTP
{
	public class ShimTransport : MonoBehaviour
	{
		private static ShimTransport _instance;

		private readonly Dictionary<HTTPRequest, UnityWebRequest> _active = new Dictionary<HTTPRequest, UnityWebRequest>();

		private static ShimTransport I
		{
			get
			{
				if (_instance == null)
				{
					GameObject go = new GameObject("BestHTTP.ShimTransport");
					UnityEngine.Object.DontDestroyOnLoad(go);
					go.hideFlags = HideFlags.HideInHierarchy;
					_instance = go.AddComponent<ShimTransport>();
				}
				return _instance;
			}
		}

		internal static void Send(HTTPRequest request)
		{
			I.StartCoroutine(I._Send(request));
		}

		// Cho fake server: chạy action sau `seconds` giây trên main thread.
		public static void RunLater(float seconds, Action action)
		{
			I.StartCoroutine(I._Later(seconds, action));
		}

		internal static void Abort(HTTPRequest request)
		{
			if (_instance != null && _instance._active.TryGetValue(request, out UnityWebRequest uwr))
			{
				_instance._active.Remove(request);
				uwr.Abort();
			}
			request.Finish(HTTPRequestStates.Aborted, null, null);
		}

		internal static void AbortAll()
		{
			if (_instance == null)
			{
				return;
			}
			foreach (HTTPRequest r in new List<HTTPRequest>(_instance._active.Keys))
			{
				Abort(r);
			}
		}

		private IEnumerator _Later(float seconds, Action action)
		{
			if (seconds > 0f)
			{
				yield return new WaitForSecondsRealtime(seconds);
			}
			else
			{
				yield return null;
			}
			action();
		}

		private IEnumerator _Send(HTTPRequest request)
		{
			string method = request.MethodType.ToString().ToUpperInvariant();
			UnityWebRequest uwr = new UnityWebRequest(request.Uri, method);
			byte[] body = request.GetEntityBody();
			if (body != null && body.Length > 0)
			{
				uwr.uploadHandler = new UploadHandlerRaw(body);
			}
			uwr.downloadHandler = new DownloadHandlerBuffer();
			request.EnumerateHeaders(delegate(string name, List<string> values)
			{
				if (!string.Equals(name, "Content-Length", StringComparison.OrdinalIgnoreCase) && !string.Equals(name, "Host", StringComparison.OrdinalIgnoreCase))
				{
					uwr.SetRequestHeader(name, string.Join(",", values));
				}
			});
			int timeout = (int)Math.Ceiling(request.Timeout.TotalSeconds);
			if (timeout > 0)
			{
				uwr.timeout = timeout;
			}
			_active[request] = uwr;
			yield return uwr.SendWebRequest();
			if (!_active.Remove(request))
			{
				uwr.Dispose();
				yield break; // đã Abort
			}
			if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.DataProcessingError)
			{
				bool timedOut = uwr.error != null && uwr.error.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0;
				Exception ex = new Exception(uwr.error);
				uwr.Dispose();
				request.Finish(timedOut ? HTTPRequestStates.TimedOut : HTTPRequestStates.Error, null, ex);
				yield break;
			}
			HTTPResponse resp = new HTTPResponse(request, (int)uwr.responseCode, uwr.error ?? "OK", uwr.downloadHandler.data);
			Dictionary<string, string> headers = uwr.GetResponseHeaders();
			if (headers != null)
			{
				foreach (KeyValuePair<string, string> kv in headers)
				{
					resp.AddHeader(kv.Key, kv.Value);
				}
			}
			uwr.Dispose();
			request.Finish(HTTPRequestStates.Finished, resp, null);
		}
	}
}
