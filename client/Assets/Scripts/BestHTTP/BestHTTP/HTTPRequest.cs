using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using BestHTTP.Authentication;
using BestHTTP.Connections;
using BestHTTP.Cookies;
using BestHTTP.Forms;
using BestHTTP.Logger;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Tls;
using BestHTTP.Timings;

namespace BestHTTP
{
	public sealed class HTTPRequest : IEnumerator, IEnumerator<HTTPRequest>, IDisposable
	{
		internal struct UploadStreamInfo
		{
			public readonly Stream Stream;

			public readonly long Length;

			public UploadStreamInfo(Stream stream, long length)
			{
				Stream = null;
				Length = 0L;
			}
		}

		public static readonly byte[] EOL;

		public static readonly string[] MethodNames;

		public static int UploadChunkSize;

		public OnUploadProgressDelegate OnUploadProgress;

		public OnStreamingDataDelegate OnStreamingData;

		public Action<HTTPRequest, HTTPResponse> OnHeadersReceived;

		public OnDownloadProgressDelegate OnDownloadProgress;

		private List<Cookie> customCookies;

		private HTTPRequestStates _state;

		[CompilerGenerated]
		private Func<HTTPRequest, X509Certificate, X509Chain, bool> m_CustomCertificationValidator;

		private OnBeforeRedirectionDelegate onBeforeRedirection;

		private OnBeforeHeaderSendDelegate _onBeforeHeaderSend;

		internal OnRequestFinishedDelegate OnUpgraded;

		internal Action<HTTPRequest> OnCancellationRequested;

		private bool isKeepAlive;

		private bool disableCache;

		private bool cacheOnly;

		private int streamFragmentSize;

		private bool useStreaming;

		private HTTPFormBase FieldCollector;

		private HTTPFormBase FormImpl;

		public Uri Uri { get; set; }

		public HTTPMethods MethodType { get; set; }

		public byte[] RawData { get; set; }

		public Stream UploadStream { get; set; }

		public bool DisposeUploadStream { get; set; }

		public bool UseUploadStreamLength { get; set; }

		public bool IsKeepAlive
		{
			get
			{
				return isKeepAlive;
			}
			set
			{
				isKeepAlive = value;
			}
		}

		public bool DisableCache
		{
			get
			{
				return disableCache;
			}
			set
			{
				disableCache = value;
			}
		}

		public bool CacheOnly
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public int StreamFragmentSize
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public bool StreamChunksImmediately { get; set; }

		public int ReadBufferSizeOverride { get; set; }

		public int MaxFragmentQueueLength { get; set; }

		public OnRequestFinishedDelegate Callback { get; set; }

		public DateTime ProcessingStarted { get; private set; }

		public bool IsTimedOut => _state == HTTPRequestStates.TimedOut || _state == HTTPRequestStates.ConnectionTimedOut;

		public int Retries { get; internal set; }

		public int MaxRetries { get; set; }

		public bool IsCancellationRequested { get; private set; }

		public bool IsRedirected { get; internal set; }

		public Uri RedirectUri { get; internal set; }

		public Uri CurrentUri => Uri;

		public HTTPResponse Response { get; internal set; }

		public HTTPResponse ProxyResponse { get; internal set; }

		public Exception Exception { get; internal set; }

		public object Tag { get; set; }

		public Credentials Credentials { get; set; }

		public bool HasProxy => false;

		public Proxy Proxy { get; set; }

		public int MaxRedirects { get; set; }

		public bool UseAlternateSSL { get; set; }

		public bool IsCookiesEnabled { get; set; }

		public List<Cookie> Cookies
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public HTTPFormUsage FormUsage { get; set; }

		public HTTPRequestStates State
		{
			get
			{
				return _state;
			}
			internal set
			{
				_state = value;
			}
		}

		public int RedirectCount { get; internal set; }

		public TimeSpan ConnectTimeout { get; set; }

		public TimeSpan Timeout { get; set; }

		public bool EnableTimoutForStreaming { get; set; }

		public bool EnableSafeReadOnUnknownContentLength { get; set; }

		public ICertificateVerifyer CustomCertificateVerifyer { get; set; }

		public IClientCredentialsProvider CustomClientCredentialsProvider { get; set; }

		public List<string> CustomTLSServerNameList { get; set; }

		public LoggingContext Context { get; private set; }

		public TimingCollector Timing { get; private set; }

		internal SupportedProtocols ProtocolHandler { get; set; }

		internal bool UseStreaming => false;

		internal long UploadStreamLength => 0L;

		private Dictionary<string, List<string>> Headers { get; set; }

		public object Current => null;

		HTTPRequest IEnumerator<HTTPRequest>.Current => null;

		public event Func<HTTPRequest, X509Certificate, X509Chain, bool> CustomCertificationValidator
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event OnBeforeRedirectionDelegate OnBeforeRedirection
		{
			add
			{
			}
			remove
			{
			}
		}

		public event OnBeforeHeaderSendDelegate OnBeforeHeaderSend
		{
			add
			{
			}
			remove
			{
			}
		}

		public HTTPRequest(Uri uri)
			: this(uri, HTTPMethods.Get, true, false, null)
		{
		}

		public HTTPRequest(Uri uri, OnRequestFinishedDelegate callback)
			: this(uri, HTTPMethods.Get, true, false, callback)
		{
		}

		public HTTPRequest(Uri uri, bool isKeepAlive, OnRequestFinishedDelegate callback)
			: this(uri, HTTPMethods.Get, isKeepAlive, false, callback)
		{
		}

		public HTTPRequest(Uri uri, bool isKeepAlive, bool disableCache, OnRequestFinishedDelegate callback)
			: this(uri, HTTPMethods.Get, isKeepAlive, disableCache, callback)
		{
		}

		public HTTPRequest(Uri uri, HTTPMethods methodType)
			: this(uri, methodType, true, false, null)
		{
		}

		public HTTPRequest(Uri uri, HTTPMethods methodType, OnRequestFinishedDelegate callback)
			: this(uri, methodType, true, false, callback)
		{
		}

		public HTTPRequest(Uri uri, HTTPMethods methodType, bool isKeepAlive, OnRequestFinishedDelegate callback)
			: this(uri, methodType, isKeepAlive, false, callback)
		{
		}

		public HTTPRequest(Uri uri, HTTPMethods methodType, bool isKeepAlive, bool disableCache, OnRequestFinishedDelegate callback)
		{
			// [rebuild] BestHTTP shim: chỉ giữ phần game dùng; gửi thật qua ShimTransport (UnityWebRequest) hoặc HTTPManager.Interceptor.
			Uri = uri;
			MethodType = methodType;
			this.isKeepAlive = isKeepAlive;
			this.disableCache = disableCache;
			Callback = callback;
			Headers = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			ConnectTimeout = TimeSpan.FromSeconds(20.0);
			Timeout = TimeSpan.FromSeconds(60.0);
			MaxRedirects = 10;
			_state = HTTPRequestStates.Initial;
		}

		public void AddField(string fieldName, string value)
		{
			AddField(fieldName, value, Encoding.UTF8);
		}

		public void AddField(string fieldName, string value, Encoding e)
		{
			if (FieldCollector == null)
			{
				FieldCollector = new HTTPFormBase();
			}
			FieldCollector.AddField(fieldName, value, e);
		}

		public void AddBinaryData(string fieldName, byte[] content)
		{
			AddBinaryData(fieldName, content, null, null);
		}

		public void AddBinaryData(string fieldName, byte[] content, string fileName)
		{
			AddBinaryData(fieldName, content, fileName, null);
		}

		public void AddBinaryData(string fieldName, byte[] content, string fileName, string mimeType)
		{
			if (FieldCollector == null)
			{
				FieldCollector = new HTTPFormBase();
			}
			FieldCollector.AddBinaryData(fieldName, content, fileName, mimeType);
		}

		public void SetForm(HTTPFormBase form)
		{
			FormImpl = form;
		}

		public List<HTTPFieldData> GetFormFields()
		{
			HTTPFormBase f = FormImpl ?? FieldCollector;
			return (f == null) ? null : f.Fields;
		}

		public void ClearForm()
		{
			FormImpl = null;
			FieldCollector = null;
		}

		private HTTPFormBase SelectFormImplementation()
		{
			if (FormImpl != null)
			{
				return FormImpl;
			}
			if (FieldCollector == null)
			{
				return null;
			}
			HTTPFormBase f = FieldCollector.HasBinary ? (HTTPFormBase)new HTTPMultiPartForm() : new HTTPUrlEncodedForm();
			f.CopyFrom(FieldCollector);
			return f;
		}

		public void AddHeader(string name, string value)
		{
			List<string> list;
			if (!Headers.TryGetValue(name, out list))
			{
				list = new List<string>();
				Headers[name] = list;
			}
			list.Add(value);
		}

		public void SetHeader(string name, string value)
		{
			Headers[name] = new List<string> { value };
		}

		public bool RemoveHeader(string name)
		{
			return Headers.Remove(name);
		}

		public bool HasHeader(string name)
		{
			return Headers.ContainsKey(name);
		}

		public string GetFirstHeaderValue(string name)
		{
			List<string> list;
			return (Headers.TryGetValue(name, out list) && list.Count > 0) ? list[0] : null;
		}

		public List<string> GetHeaderValues(string name)
		{
			List<string> list;
			return Headers.TryGetValue(name, out list) ? list : null;
		}

		public void RemoveHeaders()
		{
			Headers.Clear();
		}

		public void SetRangeHeader(long firstBytePos)
		{
		}

		public void SetRangeHeader(long firstBytePos, long lastBytePos)
		{
		}

		public void EnumerateHeaders(OnHeaderEnumerationDelegate callback)
		{
			EnumerateHeaders(callback, false);
		}

		public void EnumerateHeaders(OnHeaderEnumerationDelegate callback, bool callBeforeSendCallback)
		{
			if (callback == null)
			{
				return;
			}
			foreach (KeyValuePair<string, List<string>> kv in Headers)
			{
				callback(kv.Key, kv.Value);
			}
		}

		private void SendHeaders(Stream stream)
		{
		}

		public string DumpHeaders()
		{
			StringBuilder sb = new StringBuilder();
			foreach (KeyValuePair<string, List<string>> kv in Headers)
			{
				sb.Append(kv.Key).Append(": ").Append(string.Join(", ", kv.Value)).Append("\n");
			}
			return sb.ToString();
		}

		public byte[] GetEntityBody()
		{
			if (RawData != null)
			{
				return RawData;
			}
			HTTPFormBase f = SelectFormImplementation();
			if (f == null)
			{
				return null;
			}
			f.PrepareRequest(this);
			return f.GetData();
		}

		internal UploadStreamInfo GetUpStream()
		{
			return default;
		}

		internal void SendOutTo(Stream stream)
		{
		}

		internal void UpgradeCallback()
		{
		}

		internal bool CallOnBeforeRedirection(Uri redirectUri)
		{
			return false;
		}

		internal void Prepare()
		{
		}

		internal bool CallCustomCertificationValidator(X509Certificate cert, X509Chain chain)
		{
			return false;
		}

		public HTTPRequest Send()
		{
			return HTTPManager.SendRequest(this);
		}

		public void Abort()
		{
			if (_state >= HTTPRequestStates.Finished)
			{
				return;
			}
			IsCancellationRequested = true;
			ShimTransport.Abort(this);
		}

		public void Clear()
		{
			ClearForm();
			RemoveHeaders();
		}

		private void VerboseLogging(string str)
		{
		}

		public bool MoveNext()
		{
			return _state < HTTPRequestStates.Finished;
		}

		public void Reset()
		{
		}

		public void Dispose()
		{
		}

		// [rebuild] shim: kết thúc request trên main thread và gọi Callback (giống BestHTTP: lỗi mạng → Response null, State Error/TimedOut/Aborted).
		internal void Finish(HTTPRequestStates state, HTTPResponse response, Exception exception)
		{
			if (_state >= HTTPRequestStates.Finished)
			{
				return;
			}
			_state = state;
			Response = response;
			Exception = exception;
			if (Callback != null)
			{
				try
				{
					Callback(this, response);
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogException(ex);
				}
			}
		}

		internal void MarkProcessing()
		{
			_state = HTTPRequestStates.Processing;
			ProcessingStarted = DateTime.UtcNow;
		}
	}
}
