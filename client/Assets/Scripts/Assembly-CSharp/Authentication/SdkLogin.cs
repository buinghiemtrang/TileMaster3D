using System;
using UnityEngine;
using UnityEngine.SignInWithApple;

namespace Authentication
{
	public class SdkLogin : MonoBehaviour
	{
		private AndroidJavaObject _jo;

		private SdkLoginListener sdkLoginListener;

		private void Awake()
		{
			_jo = _getActivityJavaObject();
		}

		public void FacebookLogin()
		{
			_jo.Call("FacebookLogin");
		}

		public void FacebookLogout()
		{
			_jo.Call("FacebookLogout");
		}

		public void GoogleLogin()
		{
			_jo.Call("GoogleLogin");
		}

		public void GoogleLogout()
		{
			_jo.Call("GoogleLogout");
		}

		public void AppleLogin(SignInWithApple siwa, SdkLoginListener listener)
		{
		}

		private void _appleLoginCallback(SignInWithApple.CallbackArgs args)
		{
			if (string.IsNullOrEmpty(args.error))
			{
				_debug("apple args.credentialState:" + args.credentialState);
				_debug("apple args.userInfo.userId:" + args.userInfo.userId);
				_debug("apple args.userInfo.idToken:" + args.userInfo.idToken);
				_debug("apple args.userInfo.error:" + args.userInfo.error);
				_debug("apple args.userInfo.displayName:" + args.userInfo.displayName);
				_debug("apple args.userInfo.email:" + args.userInfo.email);
				_debug("apple args.userInfo.userDetectionStatus:" + args.userInfo.userDetectionStatus);
				if (args.userInfo.userId == "")
				{
					_debug("apple login fail");
					sdkLoginListener.LoginCall("login|fail|apple");
				}
				else
				{
					_debug("apple login succ");
					sdkLoginListener.LoginCall("login|succ|apple|" + args.userInfo.idToken + "|" + args.userInfo.displayName);
				}
			}
			else
			{
				_debug("apple args.error: " + args.error);
				sdkLoginListener.LoginCall("login|fail|apple");
			}
		}

		private AndroidJavaObject _getActivityJavaObject()
		{
#if UNITY_EDITOR || FAKE_SDK // [rebuild] Editor/FAKE_SDK không có JNI SDK
			return null;
#endif
			if (_jo == null)
			{
				_jo = new AndroidJavaClass("com.higgs.tilemaster3d.mUnityPlayerActivity").GetStatic<AndroidJavaObject>("instance");
			}
			return _jo;
		}

		public static float Ios_Version()
		{
			return 0f;
		}

		private static void _debug(string mes)
		{
			Debug.Log("Authentication SDK Login: " + mes);
		}
	}
}
