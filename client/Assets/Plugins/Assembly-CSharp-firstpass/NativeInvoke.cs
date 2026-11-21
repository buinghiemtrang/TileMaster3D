using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class NativeInvoke : MonoBehaviour
{
	public static void Initialize()
	{
		UnityEngine.Object.DontDestroyOnLoad(new GameObject(typeof(NativeInvoke).ToString()));
	}

	public static AndroidJavaObject ConvertDictionaryToJsonObject(Dictionary<string, object> dictionary)
	{
		if (dictionary == null)
		{
			return null;
		}
		try
		{
			string text = JsonConvert.SerializeObject(dictionary);
			return new AndroidJavaObject("org.json.JSONObject", text);
		}
		catch (Exception message)
		{
			Debug.LogError("ThinkingAnalytics: unexpected exception: " + message);
			return null;
		}
	}
}
