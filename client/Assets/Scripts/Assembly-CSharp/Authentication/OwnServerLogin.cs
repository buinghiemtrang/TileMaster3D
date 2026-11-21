using System;
using System.Collections.Generic;
using BestHTTP;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SignInWithApple;

namespace Authentication
{
	public class OwnServerLogin : MonoBehaviour
	{
		private static OwnServerLogin _instance;

		private HttpMgr _httpMgr;

		private SdkLogin _sdkLogin;

		private SdkLoginListener _sdkLoginListener;

		private Action<PlatformType, int, ServerInfo> _onLoginCallback;

		private PlatformType _type;

		private bool _login;

		private HTTPRequest _signReqGuest;

		public static OwnServerLogin Instance
		{
			get
			{
				if (!_instance)
				{
					GameObject gameObject = new GameObject("LoginManager");
					_instance = gameObject.AddComponent<OwnServerLogin>();
				}
				return _instance;
			}
		}

		private void Awake()
		{
			_signReqGuest = null;
			if (!_httpMgr)
			{
				_httpMgr = base.gameObject.AddComponent<HttpMgr>();
			}
			if (!_sdkLogin)
			{
				_sdkLogin = base.gameObject.AddComponent<SdkLogin>();
			}
			if (!_sdkLoginListener)
			{
				_sdkLoginListener = base.gameObject.AddComponent<SdkLoginListener>();
			}
			_type = PlatformType.None;
		}

		public void Logout(int platformID)
		{
			if (platformID == 1)
			{
				_sdkLogin.GoogleLogout();
			}
			else if (platformID == 0)
			{
				_sdkLogin.FacebookLogout();
			}
			_login = false;
		}

		public void Login(PlatformType type, Action<PlatformType, int, ServerInfo> loginCallback, SignInWithApple signInWithApple = null)
		{
			if (_type != PlatformType.None)
			{
				return;
			}
			_onLoginCallback = loginCallback;
			if (type == PlatformType.Guest)
			{
				_type = PlatformType.Guest;
				LoginGuest();
				return;
			}
			if (!_login)
			{
				_login = true;
				AbortGuestSignIn();
				SceneMgr.OpenWaiting(45f, 1f, delegate
				{
					_sdkLoginListener.onFinished = null;
				});
				_type = type;
				_sdkLoginListener.onFinished = delegate(bool isSuccess, PlatformType platform, string token)
				{
					if (_type == platform)
					{
						if (isSuccess)
						{
							SceneMgr.OpenWaiting(-1f, 0f);
							Dictionary<string, string> dictionary = new Dictionary<string, string>();
							dictionary.Add("Content-Type", "application/x-www-form-urlencoded");
							dictionary.Add("client-version", Application.version);
							Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
							dictionary2.Add("grant_type", "external");
							dictionary2.Add("scope", "all.full_access");
							dictionary2.Add("client_id", "TileMaster3D");
							dictionary2.Add("client_secret", "ae0b9ae818784f22aaf5a16f69ddd501");
							dictionary2.Add("external_token", token);
							dictionary2.Add("provider", platform.ToString().ToLower());
							_httpMgr.Post("https://service.fuqiangame.com/connect/token", dictionary, dictionary2, OnLoginRequestFinished);
						}
						else
						{
							_login = false;
							SceneMgr.CloseWaiting();
							if (_onLoginCallback != null)
							{
								_onLoginCallback(_type, 0, null);
							}
							_type = PlatformType.None;
						}
					}
				};
				if (type == PlatformType.Apple)
				{
					if ((bool)signInWithApple)
					{
						_sdkLogin.AppleLogin(signInWithApple, _sdkLoginListener);
					}
				}
				else if (type == PlatformType.Google)
				{
					_sdkLogin.GoogleLogin();
				}
				else if (type == PlatformType.Facebook)
				{
					_sdkLogin.FacebookLogin();
				}
			}
		}

		private void LoginGuest()
		{
			DispatcherMgr.Emit("Guest_Login", GuestLoginState.Start);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Content-Type", "application/x-www-form-urlencoded");
			dictionary.Add("client-version", Application.version);
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			dictionary2.Add("grant_type", "external");
			dictionary2.Add("scope", "all.full_access");
			dictionary2.Add("client_id", "TileMaster3D");
			dictionary2.Add("client_secret", "ae0b9ae818784f22aaf5a16f69ddd501");
			dictionary2.Add("external_token", SystemInfo.deviceUniqueIdentifier);
			dictionary2.Add("provider", "guest");
			_signReqGuest = _httpMgr.Post("https://service.fuqiangame.com/connect/token", dictionary, dictionary2, OnLoginRequestFinished);
		}

		public void AbortGuestSignIn()
		{
			if (DataCenterLoginMgr.I != null)
			{
				DataCenterLoginMgr.I.AbortSignIn();
			}
			if (_signReqGuest != null)
			{
				_signReqGuest.Abort();
				_signReqGuest.Tag = "AbortRequest";
			}
		}

		private void OnLoginRequestFinished(HTTPResponse resp)
		{
			bool isAborted = _signReqGuest != null && ((string)_signReqGuest.Tag == "AbortRequest");
			_signReqGuest = null;
			if (resp == null)
			{
				SceneMgr.CloseWaiting();
				if (_onLoginCallback != null)
				{
					_onLoginCallback((!isAborted) ? _type : PlatformType.Abort, 0, null);
				}
			}
			else
			{
				ServerInfo serverInfo = JsonConvert.DeserializeObject<ServerInfo>(resp.DataAsText);
				if (serverInfo == null)
				{
					if (_onLoginCallback != null)
					{
						_onLoginCallback(_type, 0, null);
					}
					return;
				}
				if (serverInfo.data != null && serverInfo.data.user != null && _type == PlatformType.Apple)
				{
					serverInfo.data.user.DisplayName = PlayerPrefs.GetString("Apple_user_displayName", string.Empty);
				}
				if (_onLoginCallback != null)
				{
					_onLoginCallback(_type, 1, serverInfo);
				}
			}
			_type = PlatformType.None;
		}
	}
}
