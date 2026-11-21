using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class KGUtils : MonoBehaviour
{
	private static KGUtils _instance;

	private static AndroidJavaClass _javaClass;

	private Action<string> _onAttResult;

	private Action<bool> _onUmpResult;

	public static KGUtils Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = CreateInstance();
			}
			return _instance;
		}
	}

	protected static AndroidJavaObject JavaClass
	{
		get
		{
			if (_javaClass == null)
			{
				_javaClass = new AndroidJavaClass("com.fuqian.ump.GoogleMobileAdsConsentWrapper");
			}
			return _javaClass;
		}
	}

	private static KGUtils CreateInstance()
	{
		GameObject gameObject = new GameObject("KGUtils");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		return gameObject.AddComponent<KGUtils>();
	}

	private static void CallAndroidFunc(string method, params object[] args)
	{
#if !UNITY_EDITOR && !FAKE_SDK // [rebuild] Editor/FAKE_SDK không có JNI SDK
		JavaClass.CallStatic(method, args);
#endif
	}

	public void CheckAtt(Action<string> onResult)
	{
		_onAttResult = onResult;
		if (onResult != null)
		{
			onResult("true");
		}
	}

	public void OnAttResult(string result)
	{
		if (_onAttResult != null)
		{
			_onAttResult(result);
		}
	}

	public void CheckUmp(Action<bool> onResult)
	{
		_onUmpResult = onResult;
		CallAndroidFunc("gatherConsent", "KGUtils", "OnUmpResult");
	}

	public void ResetUmp()
	{
		CallAndroidFunc("reset");
	}

	public void OnUmpResult(string result)
	{
		if (_onUmpResult != null)
		{
			_onUmpResult(result == "yes");
		}
	}

	public void NativeLog(string result)
	{
		Debug.Log("Native Log: " + result);
	}

	public static AndroidJavaObject ConvertDictionaryToJsonObject(Dictionary<string, object> dictionary)
	{
		if (dictionary == null)
		{
			return null;
		}
		string text = JsonConvert.SerializeObject(dictionary);
		return new AndroidJavaObject("org.json.JSONObject", text);
	}
}
