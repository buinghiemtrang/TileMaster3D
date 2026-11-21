using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BestHTTP;
using FQDev.StoreKit;
using Newtonsoft.Json;
using OwnHttp;

public class IAPOrderValidateManager : Singleton<IAPOrderValidateManager>
{
	private const string IOSValidateUrl = "https://service.fuqiangame.com/utils/api/v1/validateIosOrder";

	private const string GoogleValidateUrl = "https://service.fuqiangame.com/utils/api/v1/validateGoogleOrder";

	private Action<bool, bool, Purchase, Product, string> _onOrderValidateCallBack;

	public void OrderValidate(string receipt, Product product, Purchase purchase, Action<bool, bool, Purchase, Product, string> callback)
	{
		Dictionary<string, string> headers = GetHeaders();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		cfbt_Stores storeInfoByProductID = Global.pStores.GetStoreInfoByProductID(purchase.ProductID);
		var anonymous = new
		{
			purchaseToken = receipt,
			subsOrProductId = product.ID,
			subscription = (storeInfoByProductID.productType == 2)
		};
		dictionary.Add("purchaseToken", anonymous.purchaseToken);
		dictionary.Add("subsOrProductId", anonymous.subsOrProductId);
		dictionary.Add("subscription", anonymous.subscription.ToString().ToLower());
		Sign(headers, dictionary);
		_onOrderValidateCallBack = callback;
		OwnSingleHttp ownSingleHttp = RequestsMgr.I.Create(HTTPMethods.Post, "https://service.fuqiangame.com/utils/api/v1/validateGoogleOrder", delegate(ResultInfo info)
		{
			OnGoogleOrderValidateResult(info, purchase);
		});
		ownSingleHttp.AddHeader(headers);
		ownSingleHttp.AddParameter(JsonConvert.SerializeObject(anonymous));
		ownSingleHttp.AddTag(product);
		ownSingleHttp.AddCustomTimeout(50f);
		ownSingleHttp.Send();
	}

	private void OnGoogleOrderValidateResult(ResultInfo info, Purchase purchase)
	{
		if (info == null)
		{
			return;
		}
		if (info.State != RequestState.Success)
		{
			if (_onOrderValidateCallBack != null)
			{
				_onOrderValidateCallBack(false, false, null, null, info.Response.DataAsText);
			}
			return;
		}
		GoogleOrderValidateRoot googleOrderValidateRoot = JsonConvert.DeserializeObject<GoogleOrderValidateRoot>(info.Response.DataAsText);
		bool flag = false;
		bool flag2 = false;
		if (googleOrderValidateRoot != null && googleOrderValidateRoot.code == 1)
		{
			if (googleOrderValidateRoot.data != null && googleOrderValidateRoot.data.purchaseType != null)
			{
				flag = googleOrderValidateRoot.data.purchaseType.ToString().Equals("0");
			}
			if (googleOrderValidateRoot.data != null)
			{
				flag2 = (googleOrderValidateRoot.data.purchaseState == 0);
			}
		}
		if (_onOrderValidateCallBack != null)
		{
			_onOrderValidateCallBack(flag2, flag, purchase, (Product)info.Tag, info.Response.DataAsText);
		}
	}

	private void OnIosOrderValidateResult(ResultInfo info, Purchase purchase)
	{
		if (info == null)
		{
			return;
		}
		if (info.State != RequestState.Success)
		{
			if (_onOrderValidateCallBack != null)
			{
				_onOrderValidateCallBack(false, false, null, null, info.Response.DataAsText);
			}
			return;
		}
		iOSOrderValidateRoot iOSOrderValidateRoot = JsonConvert.DeserializeObject<iOSOrderValidateRoot>(info.Response.DataAsText);
		bool flag = false;
		bool flag2 = false;
		if (iOSOrderValidateRoot != null && iOSOrderValidateRoot.code == 1)
		{
			if (iOSOrderValidateRoot.data != null)
			{
				flag = iOSOrderValidateRoot.data.environment.Equals("Sandbox");
				flag2 = (iOSOrderValidateRoot.data.status == 0);
			}
		}
		if (_onOrderValidateCallBack != null)
		{
			_onOrderValidateCallBack(flag2, flag, purchase, (Product)info.Tag, info.Response.DataAsText);
		}
	}

	private Dictionary<string, string> GetHeaders()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("Content-Type", "application/json");
		dictionary.Add("clientId", "TileMaster3D");
		dictionary.Add("timestamp", DateTimeExtend.GetLocalTimeStamp().ToString());
		dictionary.Add("requestId", Guid.NewGuid().ToString());
		dictionary.Add("solt", GetSolt());
		return dictionary;
	}

	private string Sign(Dictionary<string, string> headers, Dictionary<string, string> param)
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
		string solt = GetSolt();
		string dictionaryAsciiValue = GetDictionaryAsciiValue(dictionary);
		string data = "ae0b9ae818784f22aaf5a16f69ddd501|" + solt + "|" + dictionaryAsciiValue;
		string text = SHA256EncryptString(data);
		headers.Add("sign", text);
		return text;
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
