using System;
using BestHTTP.Logger;
using BestHTTP.WebSocket.Extensions;
using BestHTTP.WebSocket.Frames;

namespace BestHTTP.WebSocket
{
	public sealed class WebSocket
	{
		public OnWebSocketOpenDelegate OnOpen;

		public OnWebSocketMessageDelegate OnMessage;

		public OnWebSocketBinaryDelegate OnBinary;

		public OnWebSocketClosedDelegate OnClosed;

		public OnWebSocketErrorDelegate OnError;

		public OnWebSocketIncompleteFrameDelegate OnIncompleteFrame;

		private bool requestSent;

		private WebSocketResponse webSocket;

		public WebSocketStates State { get; private set; }

		public bool IsOpen => false;

		public int BufferedAmount => 0;

		public bool StartPingThread { get; set; }

		public int PingFrequency { get; set; }

		public TimeSpan CloseAfterNoMessage { get; set; }

		public HTTPRequest InternalRequest { get; private set; }

		public IExtension[] Extensions { get; private set; }

		public int Latency => 0;

		public DateTime LastMessageReceived => default;

		public LoggingContext Context { get; private set; }

		public WebSocket(Uri uri)
		{
		}

		public WebSocket(Uri uri, string origin, string protocol, params IExtension[] extensions)
		{
		}

		private void OnInternalRequestCallback(HTTPRequest req, HTTPResponse resp)
		{
		}

		private void OnInternalRequestUpgraded(HTTPRequest req, HTTPResponse resp)
		{
		}

		public void Open()
		{
		}

		public void Send(string message)
		{
		}

		public void Send(byte[] buffer)
		{
		}

		public void Send(byte[] buffer, ulong offset, ulong count)
		{
		}

		public void Send(WebSocketFrame frame)
		{
		}

		public void Close()
		{
		}

		public void Close(ushort code, string message)
		{
		}

		public static byte[] EncodeCloseData(ushort code, string message)
		{
			return null;
		}

		private string GetSecKey(object[] from)
		{
			return null;
		}
	}
}
