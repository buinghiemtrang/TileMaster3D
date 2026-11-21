using System.Collections.Generic;
using UnityEngine;

public class FirebaseSDK
{
#if FAKE_SDK // [rebuild] bản FAKE_SDK không có lớp Java của SDK; khởi tạo sẽ ném lỗi trong static ctor
	private static AndroidJavaClass FirebaseAndroid = null;
#else
	private static AndroidJavaClass FirebaseAndroid = new AndroidJavaClass("com.fuqian.firebase.FirebaseSDK");
#endif

	public static T CallAndroidFunc<T>(string method, params object[] args)
	{
#if UNITY_EDITOR || FAKE_SDK // [rebuild] Editor/FAKE_SDK không có JNI SDK; bản build giữ nguyên gốc
		return default(T);
#else
		return FirebaseAndroid.CallStatic<AndroidJavaObject>("getInstance").Call<T>(method, args);
#endif
	}

	public static void CallAndroidFunc(string method, params object[] args)
	{
#if UNITY_EDITOR || FAKE_SDK // [rebuild] Editor/FAKE_SDK không có JNI SDK; bản build giữ nguyên gốc
		return;
#else
		FirebaseAndroid.CallStatic<AndroidJavaObject>("getInstance").Call(method, args);
#endif
	}

	public static void Initialize()
	{
		CallAndroidFunc("initialize");
	}

	public static void SetUserProperty(string name, string property)
	{
		CallAndroidFunc("setUserProperty", name, property);
	}

	public static void LogEvent(string eventName, Dictionary<string, object> eventValues)
	{
		CallAndroidFunc("logEvent", eventName, NativeInvoke.ConvertDictionaryToJsonObject(eventValues));
	}

	public static void LogCrashCustomKey(string key, string stack)
	{
		CallAndroidFunc("logCrashCustomKey", key, stack);
	}

	public static void MonitorConsentString()
	{
		CallAndroidFunc("monitorConsentString");
	}
}
