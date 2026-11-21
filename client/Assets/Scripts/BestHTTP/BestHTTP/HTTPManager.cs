using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using BestHTTP.Extensions;
using BestHTTP.Logger;
using BestHTTP.PlatformSupport.FileSystem;
using BestHTTP.PlatformSupport.IL2CPP;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Tls;
using UnityEngine;

namespace BestHTTP
{
	[Il2CppEagerStaticClassConstruction]
	public static class HTTPManager
	{
		private static byte maxConnectionPerServer;

		private static HeartbeatManager heartbeats;

		private static BestHTTP.Logger.ILogger logger;

		public static TlsClientFactoryDelegate TlsClientFactory;

		public static int SendBufferSize;

		public static int ReceiveBufferSize;

		public static IIOService IOService;

		public static string UserAgent;

		private static bool IsSetupCalled;

		public static byte MaxConnectionPerServer
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public static bool KeepAliveDefaultValue { get; set; }

		public static bool IsCachingDisabled { get; set; }

		public static TimeSpan MaxConnectionIdleTime { get; set; }

		public static bool IsCookiesEnabled { get; set; }

		public static uint CookieJarSize { get; set; }

		public static bool EnablePrivateBrowsing { get; set; }

		public static TimeSpan ConnectTimeout { get; set; }

		public static TimeSpan RequestTimeout { get; set; }

		public static Func<string> RootCacheFolderProvider { get; set; }

		public static Proxy Proxy { get; set; }

		public static HeartbeatManager Heartbeats => null;

		public static BestHTTP.Logger.ILogger Logger
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public static ICertificateVerifyer DefaultCertificateVerifyer { get; set; }

		public static IClientCredentialsProvider DefaultClientCredentialsProvider { get; set; }

		public static bool UseAlternateSSLDefaultValue { get; set; }

		public static Func<HTTPRequest, X509Certificate, X509Chain, bool> DefaultCertificationValidator { get; set; }

		internal static int MaxPathLength { get; set; }

		public static bool IsQuitting { get; private set; }

		static HTTPManager()
		{
		}

		public static AbstractTlsClient DefaultTlsClientFactory(HTTPRequest request, List<string> protocols)
		{
			return null;
		}

		public static void Setup()
		{
		}

		public static HTTPRequest SendRequest(string url, OnRequestFinishedDelegate callback)
		{
			return SendRequest(new HTTPRequest(new Uri(url), HTTPMethods.Get, true, false, callback));
		}

		public static HTTPRequest SendRequest(string url, HTTPMethods methodType, OnRequestFinishedDelegate callback)
		{
			return SendRequest(new HTTPRequest(new Uri(url), methodType, true, false, callback));
		}

		public static HTTPRequest SendRequest(string url, HTTPMethods methodType, bool isKeepAlive, OnRequestFinishedDelegate callback)
		{
			return SendRequest(new HTTPRequest(new Uri(url), methodType, isKeepAlive, false, callback));
		}

		public static HTTPRequest SendRequest(string url, HTTPMethods methodType, bool isKeepAlive, bool disableCache, OnRequestFinishedDelegate callback)
		{
			return SendRequest(new HTTPRequest(new Uri(url), methodType, isKeepAlive, disableCache, callback));
		}

		// [rebuild] BestHTTP shim: Interceptor (fake server trong game) được hỏi trước; trả true = đã nhận xử lý
		// (gọi HTTPManager.Respond khi xong). Không nhận thì gửi thật bằng UnityWebRequest (ShimTransport).
		public static Func<HTTPRequest, bool> Interceptor;

		public static HTTPRequest SendRequest(HTTPRequest request)
		{
			if (request == null)
			{
				return null;
			}
			request.MarkProcessing();
			if (Interceptor != null)
			{
				bool handled = false;
				try
				{
					handled = Interceptor(request);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
				if (handled)
				{
					return request;
				}
			}
			ShimTransport.Send(request);
			return request;
		}

		// Kết thúc request với response dựng sẵn (status 0 = lỗi mạng → State Error, Response null).
		public static void Respond(HTTPRequest request, int statusCode, string body)
		{
			if (statusCode <= 0)
			{
				request.Finish(HTTPRequestStates.Error, null, new Exception("shim: network error"));
				return;
			}
			HTTPResponse resp = new HTTPResponse(request, statusCode, (statusCode == 200) ? "OK" : "Error", (body == null) ? null : System.Text.Encoding.UTF8.GetBytes(body));
			resp.AddHeader("Content-Type", "application/json; charset=utf-8");
			request.Finish(HTTPRequestStates.Finished, resp, null);
		}

		public static string GetRootCacheFolder()
		{
			return null;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		public static void ResetSetup()
		{
		}

		public static void OnUpdate()
		{
		}

		public static void OnQuit()
		{
		}

		public static void AbortAll()
		{
			ShimTransport.AbortAll();
		}
	}
}
