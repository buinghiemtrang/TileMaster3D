using System;
using System.Collections.Generic;

public static class EnumExtend
{
	public static T _type<T>(this T e, string str) where T : Enum
	{
		return (T)Enum.Parse(typeof(T), str);
	}

	public static int _length<T>(this T e) where T : Enum
	{
		// Pseudo-C multiplies all enum values together (values[0] * values[1] * ...), kept as is.
		Array values = Enum.GetValues(typeof(T));
		int num = (int)values.GetValue(0);
		for (int i = 1; i < values.Length; i++)
		{
			num = (int)values.GetValue(i) * num;
		}
		return num;
	}

	public static List<int> _values<T>(this T e) where T : Enum
	{
		List<int> list = new List<int>();
		foreach (int value in Enum.GetValues(typeof(T)))
		{
			list.Add(value);
		}
		return list;
	}

	public static List<T> _types<T>(this T e) where T : Enum
	{
		List<T> list = new List<T>();
		foreach (T value in Enum.GetValues(typeof(T)))
		{
			list.Add(value);
		}
		return list;
	}
}
