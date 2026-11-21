using System;
using System.Collections.Generic;
using AOT;

namespace UnityEngine.SignInWithApple
{
	public class SignInWithApple : MonoBehaviour
	{
		public struct CallbackArgs
		{
			public UserCredentialState credentialState;

			public UserInfo userInfo;

			public string error;
		}

		public delegate void Callback(CallbackArgs args);

		private delegate void LoginCompleted(int result, UserInfo info);

		private delegate void GetCredentialStateCompleted(UserCredentialState state);

		private static Callback s_LoginCompletedCallback;

		private static Callback s_CredentialStateCallback;

		private static readonly Queue<Action> s_EventQueue;

		[Header("Event fired when login is complete.")]
		public SignInWithAppleEvent onLogin;

		[Header("Event fired when the users credential state has been retrieved.")]
		public SignInWithAppleEvent onCredentialState;

		[Header("Event fired when there is an error.")]
		public SignInWithAppleEvent onError;

		static SignInWithApple()
		{
			s_EventQueue = new Queue<Action>();
		}

		[MonoPInvokeCallback(typeof(LoginCompleted))]
		private static void LoginCompletedCallback(int result, UserInfo info)
		{
			CallbackArgs args = default(CallbackArgs);
			if (result == 0)
			{
				args.error = info.error;
			}
			else
			{
				args.userInfo = new UserInfo
				{
					userId = info.userId,
					email = info.email,
					displayName = info.displayName,
					idToken = info.idToken,
					userDetectionStatus = info.userDetectionStatus
				};
			}
			s_LoginCompletedCallback(args);
			s_LoginCompletedCallback = null;
		}

		[MonoPInvokeCallback(typeof(GetCredentialStateCompleted))]
		private static void GetCredentialStateCallback(UserCredentialState state)
		{
			CallbackArgs args = default(CallbackArgs);
			args.credentialState = state;
			s_CredentialStateCallback(args);
			s_CredentialStateCallback = null;
		}

		public void GetCredentialState(string userID)
		{
			GetCredentialState(userID, TriggerCredentialStateEvent);
		}

		public void GetCredentialState(string userID, Callback callback)
		{
			if (userID == null)
			{
				throw new InvalidOperationException("Credential state fetch called without a user id.");
			}
			if (s_CredentialStateCallback != null)
			{
				throw new InvalidOperationException("Credential state fetch called while another request is in progress.");
			}
			s_CredentialStateCallback = callback;
		}

		private void GetCredentialStateInternal(string userID)
		{
		}

		public void Login()
		{
			Login(TriggerOnLoginEvent);
		}

		public void Login(Callback callback)
		{
			if (s_LoginCompletedCallback != null)
			{
				throw new InvalidOperationException("Login called while another login is in progress");
			}
			s_LoginCompletedCallback = callback;
		}

		private void LoginInternal()
		{
		}

		private void TriggerOnLoginEvent(CallbackArgs args)
		{
			if (args.error != null)
			{
				TriggerOnErrorEvent(args);
				return;
			}
			s_EventQueue.Enqueue(delegate
			{
				if (onLogin != null)
				{
					onLogin.Invoke(args);
				}
			});
		}

		private void TriggerCredentialStateEvent(CallbackArgs args)
		{
			if (args.error != null)
			{
				TriggerOnErrorEvent(args);
				return;
			}
			s_EventQueue.Enqueue(delegate
			{
				if (onCredentialState != null)
				{
					onCredentialState.Invoke(args);
				}
			});
		}

		private void TriggerOnErrorEvent(CallbackArgs args)
		{
			s_EventQueue.Enqueue(delegate
			{
				if (onError != null)
				{
					onError.Invoke(args);
				}
			});
		}

		public void Update()
		{
			while (s_EventQueue.Count > 0)
			{
				Action action = s_EventQueue.Dequeue();
				action();
			}
		}
	}
}
