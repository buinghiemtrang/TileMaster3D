// [rebuild] Fake server offline chạy trong game (quy tắc phạm vi: dựng server fake offline trước).
// Bật bằng scripting define FAKE_SERVER. Cắm vào BestHTTP shim qua HTTPManager.Interceptor:
// - Mọi request đều bị chặn (không ra mạng thật của hãng).
// - Endpoint có handler: trả JSON sau ~30 ms (trả tức thì dễ làm hỏng luồng chờ).
// - Endpoint chưa có handler: KHÔNG trả lời (giống lúc BestHTTP còn stub), chỉ ghi log "[FakeServer] UNHANDLED".
// Handler là hàm thuần (FakeRequest, FakeServerState) -> FakeResponse, đặt ở FakeServerHandlers.cs;
// trạng thái lưu ở FakeServerState (file JSON trong persistentDataPath). Giao thức: docs/net_protocol.md.
#if FAKE_SERVER
using System;
using System.Collections.Generic;
using System.Text;
using BestHTTP;
using UnityEngine;

public class FakeRequest
{
	public string Method;

	public Uri Uri;

	public string Path;

	public Dictionary<string, string> Query = new Dictionary<string, string>();

	public string Body;

	public HTTPRequest Raw;

	public string Header(string name)
	{
		return Raw.GetFirstHeaderValue(name);
	}
}

public class FakeResponse
{
	public int Status = 200;

	public string Body;

	public static FakeResponse Json(string body)
	{
		return new FakeResponse
		{
			Body = body
		};
	}

	public static FakeResponse Error(int status, string body = "")
	{
		return new FakeResponse
		{
			Status = status,
			Body = body
		};
	}
}

public static class FakeServer
{
	public const float LatencySeconds = 0.03f;

	private static readonly Dictionary<string, Func<FakeRequest, FakeServerState, FakeResponse>> _routes = new Dictionary<string, Func<FakeRequest, FakeServerState, FakeResponse>>(StringComparer.OrdinalIgnoreCase);

	public static readonly List<string> Unhandled = new List<string>();

	public static readonly List<string> Handled = new List<string>();

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Install()
	{
		_routes.Clear();
		Unhandled.Clear();
		Handled.Clear();
		FakeServerHandlers.Register(_routes);
		FakeServerHandlers.ForceGeneralActivity = PlayerPrefs.GetInt("FakeServer.ForceGeneralActivity", 0) == 1; // cờ test Baking Master
		HTTPManager.Interceptor = Handle;
		Debug.Log("[FakeServer] installed, routes=" + _routes.Count);
	}

	private static bool Handle(HTTPRequest raw)
	{
		FakeRequest req = new FakeRequest
		{
			Method = raw.MethodType.ToString().ToUpperInvariant(),
			Uri = raw.Uri,
			Path = raw.Uri.AbsolutePath,
			Raw = raw
		};
		_ParseQuery(raw.Uri.Query, req.Query);
		byte[] body = raw.GetEntityBody();
		req.Body = (body == null) ? string.Empty : Encoding.UTF8.GetString(body);
		string key = raw.Uri.Host + raw.Uri.AbsolutePath;
		Func<FakeRequest, FakeServerState, FakeResponse> handler;
		if (!_routes.TryGetValue(key, out handler))
		{
			_routes.TryGetValue(raw.Uri.Host + "/*", out handler);
		}
		if (handler == null)
		{
			string line = req.Method + " " + raw.Uri + " body=" + _Clip(req.Body, 300);
			Unhandled.Add(line);
			Debug.Log("[FakeServer] UNHANDLED " + line);
			return true;
		}
		FakeResponse resp;
		try
		{
			resp = handler(req, FakeServerState.I);
			FakeServerState.I.SaveIfDirty();
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			resp = FakeResponse.Error(500, "{\"code\":0,\"message\":\"fake server exception\"}");
		}
		if (resp == null)
		{
			Handled.Add(key + " -> (no reply)");
			return true;
		}
		Handled.Add(key + " -> " + resp.Status);
		Debug.Log("[FakeServer] " + req.Method + " " + key + " -> " + resp.Status + " " + _Clip(resp.Body, 200));
		ShimTransport.RunLater(LatencySeconds, delegate
		{
			HTTPManager.Respond(raw, resp.Status, resp.Body);
		});
		return true;
	}

	private static void _ParseQuery(string query, Dictionary<string, string> into)
	{
		if (string.IsNullOrEmpty(query))
		{
			return;
		}
		foreach (string part in query.TrimStart('?').Split('&'))
		{
			if (part.Length == 0)
			{
				continue;
			}
			int eq = part.IndexOf('=');
			string k = Uri.UnescapeDataString((eq < 0) ? part : part.Substring(0, eq));
			string v = (eq < 0) ? string.Empty : Uri.UnescapeDataString(part.Substring(eq + 1).Replace('+', ' '));
			into[k] = v;
		}
	}

	private static string _Clip(string s, int n)
	{
		if (s == null)
		{
			return string.Empty;
		}
		return (s.Length <= n) ? s : (s.Substring(0, n) + "…");
	}
}
#endif
