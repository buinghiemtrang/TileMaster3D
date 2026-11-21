using System;
using System.Collections.Generic;
using UnityEngine;

public class GMUserLabelGetters : MonoBehaviour
{
	private static readonly Dictionary<string, Type> _userLabels;

	public static Dictionary<string, Type> UserLabels => null;

	public static string GetUserLabelsString()
	{
		return null;
	}

	public static bool GetGmUserLabel<T>(string key, out T value)
	{
		value = default;
		return false;
	}

	public static bool SetGmUserLabel(string key, string value)
	{
		return false;
	}

	public static void ClearGmUserLabel()
	{
	}
}
