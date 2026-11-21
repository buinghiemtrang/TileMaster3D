using System.Runtime.CompilerServices;

namespace BestHTTP.SignalR.Authentication
{
	internal class HeaderAuthenticator : IAuthenticationProvider
	{
		[CompilerGenerated]
		private OnAuthenticationSuccededDelegate m_OnAuthenticationSucceded;

		[CompilerGenerated]
		private OnAuthenticationFailedDelegate m_OnAuthenticationFailed;

		public string User { get; private set; }

		public string Roles { get; private set; }

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

		public HeaderAuthenticator(string user, string roles)
		{
		}

		public void StartAuthentication()
		{
		}

		public void PrepareRequest(HTTPRequest request, RequestTypes type)
		{
		}
	}
}
