using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BestHTTP;

namespace OwnHttp
{
	public class RequestsMgr : RequestsMgrBase<RequestsMgr>
	{
		private readonly List<HttpInfo> _https;

		private readonly Dictionary<string, HttpInfo> _https2;

		private static int _counterId;

		public RequestsMgr()
		{
			_https = new List<HttpInfo>();
			_https2 = new Dictionary<string, HttpInfo>();
			_counterId = 0;
		}

		public OwnSingleHttp Create(HTTPMethods type, string url, Action<ResultInfo> callback)
		{
			OwnSingleHttp ownSingleHttp = new OwnSingleHttp();
			ownSingleHttp.Creat(type, url, _Result);
			_counterId++;
			string text = _counterId.ToString();
			ownSingleHttp.AddKey(text);
			HttpInfo httpInfo = new HttpInfo();
			httpInfo.Http = ownSingleHttp;
			httpInfo.ResultCallback = callback;
			_https2.Add(text, httpInfo);
			return ownSingleHttp;
		}

		protected override void CheckCustomTimeout()
		{
			for (int num = _https.Count - 1; num >= 0; num--)
			{
				HttpInfo httpInfo = _https[num];
				OwnSingleHttp http = httpInfo.Http;
				http.CustomTimeout -= 1f;
				if (http.CustomTimeout <= 0f)
				{
					http.ForceAbort();
					ResultInfo resultInfo = new ResultInfo();
					resultInfo.Key = http.Key;
					resultInfo.Tag = http.Tag;
					resultInfo.Response = null;
					resultInfo.State = RequestState.Fail;
					if (httpInfo.ResultCallback != null)
					{
						httpInfo.ResultCallback(resultInfo);
					}
					_https.Remove(httpInfo);
				}
			}
		}

		private void _Result(ResultInfo info)
		{
			if (info != null && _https2.ContainsKey(info.Key))
			{
				_https2[info.Key].ResultCallback.InvokeSafe(info);
				_https2.Remove(info.Key);
			}
		}

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
