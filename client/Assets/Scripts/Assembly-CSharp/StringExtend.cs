using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class StringExtend
{
	private static readonly StringBuilder _stringBuilder = new StringBuilder();

	public static StringBuilder Append(this string self, string arg)
	{
		return _stringBuilder.Clear().Append(self).Append(arg);
	}

	public static bool isEmpty(this string it)
	{
		if (it != null)
		{
			return it.Length == 0;
		}
		return true;
	}

	public static bool isNotEmpty(this string it)
	{
		return !string.IsNullOrEmpty(it);
	}

	public static float[] ToFloat(this string da, char one)
	{
		return Array.ConvertAll(da.Split(one), float.Parse);
	}

	public static float[,] ToFloat(this string da, int col = 2, char one = ',', char two = '|')
	{
		string[] array = da.Split(one);
		float[,] array2 = new float[array.Length, col];
		for (int i = 0; i < array.Length; i++)
		{
			float[] array3 = array[i].ToFloat(two);
			for (int j = 0; j < col; j++)
			{
				array2[i, j] = array3[j];
			}
		}
		return array2;
	}

	public static int[] ToInt(this string da, char one)
	{
		return Array.ConvertAll(da.Split(one), int.Parse);
	}

	public static double[] ToDouble(this string da, char one)
	{
		return Array.ConvertAll(da.Split(one), double.Parse);
	}

	public static int[,] ToInt(this string da, int col = 2, char one = ',', char two = '|')
	{
		if (string.IsNullOrEmpty(da))
		{
			return new int[0, 0];
		}
		string[] array = da.Split(one);
		int[,] array2 = new int[array.Length, col];
		for (int i = 0; i < array.Length; i++)
		{
			int[] array3 = array[i].ToInt(two);
			for (int j = 0; j < col; j++)
			{
				array2[i, j] = array3[j];
			}
		}
		return array2;
	}

	public static double[,] ToDouble(this string da, int col = 2, char one = ',', char two = '|')
	{
		string[] array = da.Split(one);
		double[,] array2 = new double[array.Length, col];
		for (int i = 0; i < array.Length; i++)
		{
			double[] array3 = array[i].ToDouble(two);
			for (int j = 0; j < col; j++)
			{
				array2[i, j] = array3[j];
			}
		}
		return array2;
	}

	public static List<List<int>> ToIntList(this string da, char one = ',', char two = '|')
	{
		string[] array = da.Split(one);
		List<List<int>> list = new List<List<int>>();
		for (int i = 0; i < array.Length; i++)
		{
			list.Add(new List<int>(array[i].ToInt(two)));
		}
		return list;
	}

	public static string[,] ToStr(this string self, int col, char one, char two)
	{
		string[] array = self.Split(one);
		string[,] array2 = new string[array.Length, col];
		for (int i = 0; i < array.Length; i++)
		{
			string[] array3 = array[i].Split(two);
			for (int j = 0; j < col; j++)
			{
				array2[i, j] = array3[j];
			}
		}
		return array2;
	}

	public static string[] ToStr(this string self, char one)
	{
		return self.Split(one);
	}

	public static Vector2[] ToVector2(this string con, char one)
	{
		string[] array = con.Split(one);
		Vector2[] array2 = new Vector2[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = array[i].ToVector2();
		}
		return array2;
	}

	public static Vector2 ToVector2(this string con)
	{
		// UNVERIFIED: the 2 literals ("(" and ")") come from a rank-based reading of the literal names (SL_____39 / SL_____40), not from the string table directly
		string[] array = con.Replace("(", "").Replace(")", "").Split(',');
		return new Vector2(float.Parse(array[0]), float.Parse(array[1]));
	}

	public static T ToEnum<T>(this string value)
	{
		return (T)Enum.Parse(typeof(T), value);
	}

	public static Dictionary<int, int> ToDictionary(this string data, int col = 2, char one = ',', char two = '|')
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int[,] array = data.ToInt(col, one, two);
		for (int i = 0; i < array.GetLength(0); i++)
		{
			int key = array[i, 0];
			int num = array[i, 1];
			if (dictionary.ContainsKey(key))
			{
				num += dictionary[key];
			}
			dictionary[key] = num;
		}
		return dictionary;
	}
}
