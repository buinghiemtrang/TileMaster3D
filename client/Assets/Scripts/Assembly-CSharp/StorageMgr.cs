using Newtonsoft.Json;
using System;
using UnityEngine;

public static class StorageMgr
{
	public static int GetInt(string key, int value)
	{
		return PlayerPrefs.GetInt(key, value);
	}

	public static float GetFloat(string key, float value)
	{
		return PlayerPrefs.GetFloat(key, value);
	}

	public static bool GetBool(string key, bool b)
	{
		return PlayerPrefs.GetInt(key, b ? 1 : 0) == 1;
	}

	public static string GetString(string key, string value)
	{
		return PlayerPrefs.GetString(key, value);
	}

	public static DateTime GetDateTime(string key, DateTime value)
	{
		string @string = PlayerPrefs.GetString(key, string.Empty);
		if (!string.IsNullOrEmpty(@string) && DateTime.TryParse(@string, out var result))
		{
			value = result;
		}
		return value;
	}

	public static T GetObject<T>(string key, string json)
	{
		return JsonConvert.DeserializeObject<T>(PlayerPrefs.GetString(key, json));
	}

	public static void DelKey(string key)
	{
		PlayerPrefs.DeleteKey(key);
	}

	public static void SetInt(string key, int value)
	{
		PlayerPrefs.SetInt(key, value);
	}

	public static void SetFloat(string key, float value)
	{
		PlayerPrefs.SetFloat(key, value);
	}

	public static void SetBool(string key, bool b)
	{
		PlayerPrefs.SetInt(key, b ? 1 : 0);
	}

	public static void SetString(string key, string value)
	{
		PlayerPrefs.SetString(key, value);
	}

	public static void SetObject<T>(string key, T obj)
	{
		PlayerPrefs.SetString(key, JsonConvert.SerializeObject(obj));
	}

	public static bool IsHave(string key)
	{
		return PlayerPrefs.HasKey(key);
	}
}
