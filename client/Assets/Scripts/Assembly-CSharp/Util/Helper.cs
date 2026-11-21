using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleJSON;
using UnityEngine;

namespace Util
{
	public static class Helper
	{
		public static int[,] MergerReward(int[,] one, int[,] two)
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			for (int i = 0; i < one.GetLength(0); i++)
			{
				dictionary.Add(one[i, 0], one[i, 1]);
			}
			for (int j = 0; j < two.GetLength(0); j++)
			{
				int key = two[j, 0];
				if (!dictionary.ContainsKey(key))
				{
					dictionary.Add(key, two[j, 1]);
				}
				else
				{
					dictionary[key] += two[j, 1];
				}
			}
			int[,] array = new int[dictionary.Count, 2];
			int num = 0;
			foreach (KeyValuePair<int, int> item in dictionary)
			{
				array[num, 0] = item.Key;
				array[num, 1] = item.Value;
				num++;
			}
			return array;
		}

		public static Color32 ColorFromHex(string hexString)
		{
			Color32 result = new Color(0f, 0f, 0f, 1f);
			if (!hexString.Equals(""))
			{
				if (hexString.IndexOf('#') == 0)
				{
					hexString = hexString.Substring(1);
				}
				result.r = Convert.ToByte(hexString.Substring(0, 2), 16);
				result.g = Convert.ToByte(hexString.Substring(2, 2), 16);
				result.b = Convert.ToByte(hexString.Substring(4, 2), 16);
				if (hexString.Length == 8)
				{
					result.a = Convert.ToByte(hexString.Substring(6, 2), 16);
				}
			}
			return result;
		}

		public static Texture2D LoadImage(string sFile)
		{
			using FileStream fileStream = new FileStream(sFile, FileMode.Open, FileAccess.Read);
			fileStream.Seek(0L, SeekOrigin.Begin);
			byte[] array = new byte[fileStream.Length];
			fileStream.Read(array, 0, (int)fileStream.Length);
			fileStream.Close();
			fileStream.Dispose();
			Texture2D texture2D = new Texture2D(1, 1);
			texture2D.LoadImage(array);
			texture2D.name = sFile;
			texture2D.Apply();
			return texture2D;
		}

		public static long DateTimeToTimeStamp(DateTime time)
		{
			DateTime dateTime = TimeZoneInfo.ConvertTimeToUtc(new DateTime(1970, 1, 1, 0, 0, 0, 0));
			return (time.Ticks - dateTime.Ticks) / 10000;
		}

		public static DateTime TimeStampToDateTime(long date)
		{
			DateTime dateTime = TimeZoneInfo.ConvertTimeToUtc(new DateTime(1970, 1, 1));
			long ticks = long.Parse(date + "0000");
			TimeSpan value = new TimeSpan(ticks);
			return dateTime.Add(value);
		}

		public static string FormatTimer(int n_time, string param, bool b_shifty = false)
		{
			int num = n_time / 3600;
			string newValue;
			string oldValue;
			if (num <= 0 && b_shifty)
			{
				newValue = "";
				oldValue = "hh:";
			}
			else
			{
				newValue = num.ToString("00");
				oldValue = "hh";
			}
			string text = param.Replace(oldValue, newValue);
			text = text.Replace("mm", (n_time % 3600 / 60).ToString("00"));
			return text.Replace("ss", (n_time % 60).ToString("00"));
		}

		public static string FormatTimer(int time)
		{
			return $"{time / 3600:00}:{time % 3600 / 60:00}:{time % 60:00}";
		}

		public static string FormatTimerD(int n_time)
		{
			int num = n_time / 86400;
			int num2 = n_time / 3600 % 24;
			if (num <= 0)
			{
				int num3 = n_time % 3600 / 60;
				int num4 = n_time % 60;
				return $"{num2:00}:{num3:00}:{num4:00}";
			}
			return $"{num}d {num2:00}h";
		}

		public static JSONNode LoadJson(string sFile)
		{
			if (System.IO.File.Exists(sFile))
			{
				using FileStream fileStream = new FileStream(sFile, FileMode.Open, FileAccess.Read);
				fileStream.Seek(0L, SeekOrigin.Begin);
				byte[] array = new byte[fileStream.Length];
				fileStream.Read(array, 0, (int)fileStream.Length);
				fileStream.Close();
				fileStream.Dispose();
				return JSON.Parse(Encoding.UTF8.GetString(array));
			}
			return null;
		}

		public static void WriteJson(string sFile, JSONNode json)
		{
			System.IO.File.WriteAllText(sFile, json.ToString());
		}

		public static long BitConvert64(string identifier)
		{
			return BitConverter.ToInt64(Encoding.Default.GetBytes(identifier), 0);
		}

		public static int BitConvert32(string identifier)
		{
			return BitConverter.ToInt32(Encoding.Default.GetBytes(identifier), 0);
		}

		public static bool IsNetworkReachability()
		{
			return Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork || Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
		}
	}
}
