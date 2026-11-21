using System.Collections.Generic;
using UnityEngine;

public class ThinkingAnalyticsSDK
{
#if FAKE_SDK // [rebuild] bản FAKE_SDK không có lớp Java của SDK; khởi tạo sẽ ném lỗi trong static ctor
	private static AndroidJavaClass ThinkingAnalyticsAndroid = null;
#else
	private static AndroidJavaClass ThinkingAnalyticsAndroid = new AndroidJavaClass("com.kunggame.sdk.thinkingAnalytics.ThinkingAnalyticsSDK");
#endif

	public static T CallAndroidFunc<T>(string method, params object[] args)
	{
#if UNITY_EDITOR || FAKE_SDK // [rebuild] Editor/FAKE_SDK không có JNI SDK; bản build giữ nguyên gốc
		return default(T);
#else
		return ThinkingAnalyticsAndroid.CallStatic<AndroidJavaObject>("getInstance").Call<T>(method, args);
#endif
	}

	public static void CallAndroidFunc(string method, params object[] args)
	{
#if UNITY_EDITOR || FAKE_SDK // [rebuild] Editor/FAKE_SDK không có JNI SDK; bản build giữ nguyên gốc
		return;
#else
		ThinkingAnalyticsAndroid.CallStatic<AndroidJavaObject>("getInstance").Call(method, args);
#endif
	}

	public static void Initialize()
	{
		ThinkingAnalyticsSetting thinkingAnalyticsSetting = Resources.Load<ThinkingAnalyticsSetting>("ThinkingAnalyticsSetting");
		CallAndroidFunc("initialize", thinkingAnalyticsSetting.AppKey, thinkingAnalyticsSetting.ServerUrl);
		Identify(SystemInfo.deviceUniqueIdentifier);
		EnableAutoTrack();
	}

	public static void Identify(string distinctID)
	{
		CallAndroidFunc("identify", distinctID);
	}

	public static void EnableAutoTrack()
	{
		CallAndroidFunc("enableAutoTrack");
	}

	public static void SetSuperProperties(Dictionary<string, object> properties)
	{
		CallAndroidFunc("setSuperProperties", NativeInvoke.ConvertDictionaryToJsonObject(properties));
	}

	public static void TimeEvent(string eventName)
	{
		CallAndroidFunc("timeEvent", eventName);
	}

	public static void Track(string eventName)
	{
		CallAndroidFunc("track", eventName, NativeInvoke.ConvertDictionaryToJsonObject(null));
	}

	public static void Track(string eventName, Dictionary<string, object> properties)
	{
		CallAndroidFunc("track", eventName, NativeInvoke.ConvertDictionaryToJsonObject(properties));
	}

	public static void UserSet(Dictionary<string, object> properties)
	{
		CallAndroidFunc("userSet", NativeInvoke.ConvertDictionaryToJsonObject(properties));
	}

	public static void UserSetOnce(Dictionary<string, object> properties)
	{
		CallAndroidFunc("userSetOnce", NativeInvoke.ConvertDictionaryToJsonObject(properties));
	}

	public static void UserAdd(string property, object value)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add(property, value);
		UserAdd(dictionary);
	}

	public static void UserAdd(Dictionary<string, object> properties)
	{
		CallAndroidFunc("userAdd", NativeInvoke.ConvertDictionaryToJsonObject(properties));
	}

	public static void SetAccoundId(string accoundId)
	{
		CallAndroidFunc("setAccoundId", accoundId);
	}
}
