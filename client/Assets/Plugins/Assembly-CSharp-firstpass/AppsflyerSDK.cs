using System;
using System.Collections.Generic;
using UnityEngine;

public class AppsflyerSDK : MonoBehaviour
{
	private static Action<string> _initFinishAction;

	private static AppsflyerConversionDelegate _conversionDelegate;

#if FAKE_SDK // [rebuild] bản FAKE_SDK không có lớp Java của SDK; khởi tạo sẽ ném lỗi trong static ctor
	private static AndroidJavaClass AppsflyerAndroid = null;
#else
	private static AndroidJavaClass AppsflyerAndroid = new AndroidJavaClass("com.kunggame.sdk.appsflyer.AppsflyerSDK");
#endif

	public static T CallAndroidFunc<T>(string method, params object[] args)
	{
#if UNITY_EDITOR || FAKE_SDK // [rebuild] Editor/FAKE_SDK không có JNI SDK; bản build giữ nguyên gốc
		return default(T);
#else
		return AppsflyerAndroid.CallStatic<AndroidJavaObject>("getInstance").Call<T>(method, args);
#endif
	}

	public static void CallAndroidFunc(string method, params object[] args)
	{
#if UNITY_EDITOR || FAKE_SDK // [rebuild] Editor/FAKE_SDK không có JNI SDK; bản build giữ nguyên gốc
		return;
#else
		AppsflyerAndroid.CallStatic<AndroidJavaObject>("getInstance").Call(method, args);
#endif
	}

	public static void Initialize(AppsflyerConversionDelegate conversionDelegate, Action<string> initFinishAction)
	{
		_initFinishAction = initFinishAction;
		AppsflyerSetting appsflyerSetting = Resources.Load<AppsflyerSetting>("AppsflyerSetting");
		_conversionDelegate = conversionDelegate;
		GameObject gameObject = new GameObject(typeof(AppsflyerSDK).ToString());
		gameObject.AddComponent<AppsflyerSDK>();
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		SetCustomerUserId(SystemInfo.deviceUniqueIdentifier);
		CallAndroidFunc("initialize", appsflyerSetting.AppKey);
	}

	public void OnConversionDataSuccess(string conversionData)
	{
		if (_conversionDelegate != null)
		{
			_conversionDelegate.OnConversionDataSuccess(conversionData, "SDK");
		}
	}

	public void OnStartWithCompletion(string uid)
	{
		if (_initFinishAction != null)
		{
			_initFinishAction(uid);
		}
	}

	public void OnConversionDataFail(string error)
	{
		if (_conversionDelegate != null)
		{
			_conversionDelegate.OnConversionDataFail(error);
		}
	}

	public void OnAppOpenAttribution(string attributionData)
	{
		if (_conversionDelegate != null)
		{
			_conversionDelegate.OnAppOpenAttribution(attributionData);
		}
	}

	public void OnAppOpenAttributionFailure(string error)
	{
		if (_conversionDelegate != null)
		{
			_conversionDelegate.OnAppOpenAttributionFailure(error);
		}
	}

	public static void SetCustomerUserId(string id)
	{
		CallAndroidFunc("setCustomerUserId", id);
	}

	public static void SendEvent(string eventName, Dictionary<string, object> eventValues)
	{
		CallAndroidFunc("sendEvent", eventName, NativeInvoke.ConvertDictionaryToJsonObject(eventValues));
	}

	public static void ValidateAndSendInAppPurchase(string productIdentifier, string price, string currency, string transactionId)
	{
	}

	public static string GetAfId()
	{
		return CallAndroidFunc<string>("getAppsflyerId");
	}
}
