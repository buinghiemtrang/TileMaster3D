using System;
using System.Runtime.CompilerServices;
using BestHTTP.SignalRCore;

namespace BestHTTP.Examples
{
	public sealed class RedirectLoggerAccessTokenAuthenticator : IAuthenticationProvider
	{
		[CompilerGenerated]
		private OnAuthenticationSuccededDelegate m_OnAuthenticationSucceded;

		[CompilerGenerated]
		private OnAuthenticationFailedDelegate m_OnAuthenticationFailed;

		private HubConnection _connection;

		public bool IsPreAuthRequired => false;

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

		public RedirectLoggerAccessTokenAuthenticator(HubConnection connection)
		{
		}

		public void StartAuthentication()
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
