using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BestHTTP;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Purchasing;
using _FrameWork.Http;

public class IAPIosOrderValidateManager : Singleton<IAPIosOrderValidateManager>
{
	public class IosOrderValidateResult
	{
		[JsonProperty("dillId")]
		public string DillId;

		[JsonProperty("status")]
		public int Status;

		[JsonProperty("isSandbox")]
		public bool IsSandbox;
	}

	public class IosOrderDeliverResult
	{
		[JsonProperty("dillId")]
		public string DillId;

		[JsonProperty("status")]
		public int Status;
	}

	private const string IosValidateUrl = "https://service.fuqiangame.com/utils/api/v1/validateIosOrderV2";

	private const string IosDeliverUrl = "https://service.fuqiangame.com/utils/api/v1/deliverIosOrder";

	private Action<bool, bool, bool, Product, string> _onOrderValidateCallBack;

	private Action<bool, string, string> _onOrderDeliverCallBack;

	public void IosOrderValidate(string receipt, Product product, Action<bool, bool, bool, Product, string> callback)
	{
		_onOrderValidateCallBack = callback;
		string iosTransactionId = GetIosTransactionId(product);
		SingleHttp singleHttp = OnlyRequestsManager.Instance.Create(iosTransactionId, HTTPMethods.Post, "https://service.fuqiangame.com/utils/api/v1/validateIosOrderV2", OnIosOrderValidateResult);
		if (singleHttp == null)
		{
			return;
		}
		singleHttp.AddCustomTimeout(30f);
		singleHttp.AddTag(product);
		var anonymous = new
		{
			dillId = iosTransactionId,
			receiptData = receipt,
			subscription = (product.definition.type == UnityEngine.Purchasing.ProductType.Subscription)
		};
		singleHttp.AddParameter(JsonConvert.SerializeObject(anonymous));
		Dictionary<string, string> headers = GetHeaders();
		Dictionary<string, string> dictionary = new Dictionary<string, string>
		{
			{ "dillId", anonymous.dillId },
			{ "receiptData", anonymous.receiptData },
			{ "subscription", anonymous.subscription.ToString().ToLower() }
		};
		Sign(headers, dictionary);
		singleHttp.AddHeaders(headers);
		singleHttp.Send();
	}

	private void OnIosOrderValidateResult(ResultInfo result)
	{
		_FrameWork.Http.ServerDataRoot<IosOrderValidateResult> serverDataRoot = ParsingServerData.ParsingData<IosOrderValidateResult>(result);
		if (serverDataRoot.code != 1 || serverDataRoot.data == null)
		{
			if (_onOrderValidateCallBack != null)
			{
				_onOrderValidateCallBack(false, false, false, (Product)serverDataRoot.tag, serverDataRoot.msg);
			}
			return;
		}
		Product product = (Product)serverDataRoot.tag;
		bool flag;
		if (serverDataRoot.data.Status == 1)
		{
			flag = (product != null && product.definition.type == UnityEngine.Purchasing.ProductType.NonConsumable);
		}
		else
		{
			flag = (serverDataRoot.data.Status == 0);
		}
		if (_onOrderValidateCallBack != null)
		{
			_onOrderValidateCallBack(flag, serverDataRoot.data.IsSandbox, serverDataRoot.data.Status == 0, product, serverDataRoot.msg);
		}
	}

	public void IosOrderDeliver(string dillId, Action<bool, string, string> callback)
	{
		_onOrderDeliverCallBack = callback;
		SingleHttp singleHttp = RepeatRequestsManager.Instance.Create(dillId, HTTPMethods.Post, "https://service.fuqiangame.com/utils/api/v1/deliverIosOrder", OnIosOrderDeliverResult, -1, true, true, 5f);
		if (singleHttp == null)
		{
			return;
		}
		singleHttp.AddCustomTimeout(30f);
		singleHttp.AddTag(dillId);
		var anonymous = new
		{
			dillId = dillId
		};
		singleHttp.AddParameter(JsonConvert.SerializeObject(anonymous));
		Dictionary<string, string> headers = GetHeaders();
		Dictionary<string, string> dictionary = new Dictionary<string, string>
		{
			{ "dillId", anonymous.dillId }
		};
		Sign(headers, dictionary);
		singleHttp.AddHeaders(headers);
		singleHttp.Send();
	}

	private void OnIosOrderDeliverResult(ResultInfo result)
	{
		_FrameWork.Http.ServerDataRoot<IosOrderDeliverResult> serverDataRoot = ParsingServerData.ParsingData<IosOrderDeliverResult>(result);
		if (serverDataRoot.code == 1 && serverDataRoot.data != null)
		{
			if (_onOrderDeliverCallBack != null)
			{
				_onOrderDeliverCallBack(true, (string)serverDataRoot.tag, serverDataRoot.msg);
			}
		}
		else
		{
			if (_onOrderDeliverCallBack != null)
			{
				_onOrderDeliverCallBack(false, (string)serverDataRoot.tag, serverDataRoot.msg);
			}
		}
	}

	public string GetIosTransactionId(Product product)
	{
		string text = product.transactionID;
		if (product.definition.type == UnityEngine.Purchasing.ProductType.NonConsumable && !string.IsNullOrEmpty(product.appleOriginalTransactionID))
		{
			text = product.appleOriginalTransactionID;
		}
		return text;
	}

	private Dictionary<string, string> GetHeaders()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("Content-Type", "application/json");
		dictionary.Add("clientId", "TileMaster3D");
		dictionary.Add("timestamp", GetLocalTimeStamp().ToString());
		dictionary.Add("requestId", Guid.NewGuid().ToString());
		dictionary.Add("solt", GetSolt());
		return dictionary;
	}

	private long GetLocalTimeStamp()
	{
		DateTime now = DateTime.Now.ToUniversalTime();
		TimeSpan timeSpan = now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
		return Convert.ToInt64(timeSpan.TotalSeconds);
	}

	private void Sign(Dictionary<string, string> headers, Dictionary<string, string> param)
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
				dictionary.Add(key2, param[key2]);
				stringBuilder.Append(key2 + "|" + param[key2] + "|");
			}
		}
		Debug.Log("IAPManager 完整签名K V 数据： " + stringBuilder.ToString());
		string solt = GetSolt();
		string dictionaryAsciiValue = GetDictionaryAsciiValue(dictionary);
		Debug.Log("IAPManager 完整签名数据 ： ae0b9ae818784f22aaf5a16f69ddd501|" + solt + "|" + dictionaryAsciiValue);
		string data = "ae0b9ae818784f22aaf5a16f69ddd501|" + solt + "|" + dictionaryAsciiValue;
		string value = SHA256EncryptString(data);
		headers.Add("sign", value);
	}

	private bool SignIgnoreHeader(string key)
	{
		return key.Equals("Content-Type") || key.Equals("solt");
	}

	private string GetSolt()
	{
		if (string.IsNullOrEmpty(DB.Account.Userid))
		{
			return "TileMaster3D";
		}
		return DB.Account.Userid;
	}

	private string SHA256EncryptString(string data)
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

	private string GetDictionaryAsciiValue(Dictionary<string, string> paramsMap)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, string> item in paramsMap.OrderBy(objDic => objDic.Key))
		{
			stringBuilder.Append(item.Value + "|");
		}
		string text = stringBuilder.ToString();
		return text.Substring(0, text.Length - 1);
	}
}
