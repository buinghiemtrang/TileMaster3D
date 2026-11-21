using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class FacebookSDK : MonoBehaviour
{
	private static Action<FBUserInfo> OnComplateUserInfoDelegate;

	private static Action<string> OnCompleteLoginDelegate;

#if FAKE_SDK // [rebuild] bản FAKE_SDK không có lớp Java của SDK; khởi tạo sẽ ném lỗi trong static ctor
	private static AndroidJavaClass FacebookAndroid = null;
#else
	private static AndroidJavaClass FacebookAndroid = new AndroidJavaClass("com.kunggame.sdk.facebook.FacebookSDK");
#endif

	public static T CallAndroidFunc<T>(string method, params object[] args)
	{
#if UNITY_EDITOR || FAKE_SDK // [rebuild] Editor/FAKE_SDK không có JNI SDK; bản build giữ nguyên gốc
		return default(T);
#else
		return FacebookAndroid.CallStatic<AndroidJavaObject>("getInstance").Call<T>(method, args);
#endif
	}

	public static void CallAndroidFunc(string method, params object[] args)
	{
#if UNITY_EDITOR || FAKE_SDK // [rebuild] Editor/FAKE_SDK không có JNI SDK; bản build giữ nguyên gốc
		return;
#else
		FacebookAndroid.CallStatic<AndroidJavaObject>("getInstance").Call(method, args);
#endif
	}

	public static void Initialize()
	{
		GameObject gameObject = new GameObject(typeof(FacebookSDK).ToString());
		gameObject.AddComponent<FacebookSDK>();
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		CallAndroidFunc("initialize");
	}

	public static void LogAppEvent(string eventName)
	{
		CallAndroidFunc("logAppEvent", eventName);
	}

	public static void LogAppEvent(string eventName, Dictionary<string, object> properties)
	{
		CallAndroidFunc("logAppEvent", eventName, NativeInvoke.ConvertDictionaryToJsonObject(properties));
	}

	public static void LogAppEvent(string eventName, double valueToSum, Dictionary<string, object> properties)
	{
		CallAndroidFunc("logAppEvent", eventName, valueToSum, NativeInvoke.ConvertDictionaryToJsonObject(properties));
	}

	public static void LogAdValue2Purchase(float pointValue, float valueReal)
	{
		CallAndroidFunc("logAdValue2Purchase", pointValue, valueReal);
	}

	public static void LogAdValueImpression(float valueReal)
	{
		CallAndroidFunc("logAdValueImpression", valueReal);
	}

	public static void Login(Action<string> callfunc)
	{
		OnCompleteLoginDelegate = callfunc;
		CallAndroidFunc("login");
	}

	public void OnCompleteLogin(string channel)
	{
		if (OnCompleteLoginDelegate != null)
		{
			OnCompleteLoginDelegate(channel);
		}
		OnCompleteLoginDelegate = null;
	}

	public static void getUserInfo(Action<FBUserInfo> callfunc)
	{
		OnComplateUserInfoDelegate = callfunc;
		CallAndroidFunc("getUserInfo");
	}

	public static string GetUserID()
	{
		return CallAndroidFunc<string>("getUserID");
	}

	public void OnGetUserInfo(string msg)
	{
		FBUserInfo obj = JsonConvert.DeserializeObject<FBUserInfo>(msg);
		if (OnComplateUserInfoDelegate != null)
		{
			OnComplateUserInfoDelegate(obj);
		}
		OnComplateUserInfoDelegate = null;
	}

	public static void AutoLogin()
	{
		CallAndroidFunc("autoLogin");
	}

	public static void Logout()
	{
		CallAndroidFunc("logout");
	}
}
