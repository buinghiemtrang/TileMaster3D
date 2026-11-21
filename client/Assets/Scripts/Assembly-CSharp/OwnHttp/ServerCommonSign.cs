using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OwnHttp
{
	public class ServerCommonSign : RequestsMgrBase<RequestsMgr>
	{
		public static Dictionary<string, string> GetCommonHeaders(bool isEnterToken = false)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Content-Type", "application/json");
			dictionary.Add("clientId", "TileMaster3D");
			dictionary.Add("timestamp", GetLocalTimeStamp().ToString());
			dictionary.Add("requestId", Guid.NewGuid().ToString());
			dictionary.Add("solt", GetSolt());
			if (isEnterToken)
			{
				dictionary.Add("Authorization", DB.Account.Token);
			}
			return dictionary;
		}

		public static string Sign(ref Dictionary<string, string> headers, Dictionary<string, object> param)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			StringBuilder stringBuilder = new StringBuilder();
			if (headers != null)
			{
				foreach (string key in headers.Keys)
				{
					if (!SignIgnoreHeader(key))
					{
						dictionary.Add(key, headers[key]);
						stringBuilder.Append(key + "|" + headers[key] + "|");
					}
				}
			}
			if (param != null)
			{
				foreach (string key2 in param.Keys)
				{
					string text = param[key2].ToString();
					if (param[key2] is bool)
					{
						text = text.ToLower();
					}
					dictionary.Add(key2, text);
					stringBuilder.Append(string.Format("{0}|{1}|", key2, param[key2]));
				}
			}
			string solt = GetSolt();
			string dictionaryAsciiValue = GetDictionaryAsciiValue(dictionary);
			string data = "ae0b9ae818784f22aaf5a16f69ddd501|" + solt + "|" + dictionaryAsciiValue;
			string text2 = SHA256EncryptString(data);
			headers.Add("sign", text2);
			return text2;
		}

		private static long GetLocalTimeStamp()
		{
			return Convert.ToInt64((ServerDateTimeMgr.I.NetTimeUtc - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
		}

		private static bool SignIgnoreHeader(string key)
		{
			return key.Equals("Content-Type") || key.Equals("solt") || key.Equals("client_secret") || key.Equals("Authorization");
		}

		private static string GetSolt()
		{
			string userid = DB.Account.Userid;
			if (string.IsNullOrEmpty(userid))
			{
				return "TileMaster3D";
			}
			return DB.Account.Userid;
		}

		private static string SHA256EncryptString(string data)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(data);
			byte[] array = SHA256.Create().ComputeHash(bytes);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("x2"));
			}
			return stringBuilder.ToString();
		}

		private static string GetDictionaryAsciiValue(Dictionary<string, string> paramsMap)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> item in paramsMap.OrderBy(kvp => kvp.Key))
			{
				stringBuilder.Append(item.Value + "|");
			}
			return stringBuilder.ToString().Substring(0, stringBuilder.ToString().Length - 1);
		}
	}
}
