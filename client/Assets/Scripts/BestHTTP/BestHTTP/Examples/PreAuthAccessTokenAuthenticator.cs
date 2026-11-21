using System;
using System.Runtime.CompilerServices;
using BestHTTP.SignalRCore;

namespace BestHTTP.Examples
{
	public sealed class PreAuthAccessTokenAuthenticator : IAuthenticationProvider
	{
		[CompilerGenerated]
		private OnAuthenticationSuccededDelegate m_OnAuthenticationSucceded;

		[CompilerGenerated]
		private OnAuthenticationFailedDelegate m_OnAuthenticationFailed;

		private Uri authenticationUri;

		private HTTPRequest authenticationRequest;

		private bool isCancellationRequested;

		public bool IsPreAuthRequired => false;

		public string Token { get; private set; }

		public event OnAuthenticationSuccededDelegate OnAuthenticationSucceded
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

		public event OnAuthenticationFailedDelegate OnAuthenticationFailed
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

		public PreAuthAccessTokenAuthenticator(Uri authUri)
		{
		}

		public void StartAuthentication()
		{
		}

		private void OnAuthenticationRequestFinished(HTTPRequest req, HTTPResponse resp)
		{
		}

		private void AuthenticationFailed(string reason)
		{
		}

		public void PrepareRequest(HTTPRequest request)
		{
		}

		public Uri PrepareUri(Uri uri)
		{
			return null;
		}

		public void Cancel()
		{
		}
	}
}
