using System.Collections.Generic;
using System.Globalization;
using BestHTTP;
using FQDev.StoreKit;
using Newtonsoft.Json;
using OwnHttp;
using UnityEngine;

public class IAPAndroidOrderValidateManager : Singleton<IAPAndroidOrderValidateManager>
{
	private const string GoogleValidateUrl = "https://service.fuqiangame.com/utils/api/v1/validateGoogleOrderV2";

	private const string GoogleDeliverUrl = "https://service.fuqiangame.com/utils/api/v1/deliverIosOrder";

	private IAPManager _iapController;

	private string ValidateUrl => "https://service.fuqiangame.com/utils/api/v1/validateGoogleOrderV2";

	private string DeliverUrl => "https://service.fuqiangame.com/utils/api/v1/deliverIosOrder";

	public void OrderValidate(Product product, Purchase purchase, IAPManager iapController)
	{
		_iapController = iapController;
#if FAKE_SDK
		// [rebuild] FAKE_SDK: bỏ xác minh đơn qua server fuqiangame; coi là đơn sandbox hợp lệ, không gửi deliver.
		OrderValidateFinished(true, true, false, product, purchase, 0);
		return;
#endif
		Dictionary<string, string> commonHeaders = RequestsMgr.GetCommonHeaders();
		var anonymous = new
		{
			purchaseToken = purchase.PurchaseToken,
			subsOrProductId = purchase.ProductID,
			dillId = purchase.TransactionID,
			subscription = (product.ProductType == ProductType.Subscription)
		};
		Dictionary<string, object> dictionary = new Dictionary<string, object>
		{
			{ "purchaseToken", anonymous.purchaseToken },
			{ "subsOrProductId", anonymous.subsOrProductId },
			{ "dillId", anonymous.dillId },
			{ "subscription", anonymous.subscription.ToString().ToLower() }
		};
		RequestsMgr.Sign(ref commonHeaders, dictionary);
		OwnSingleHttp ownSingleHttp = RequestsMgr.I.Create(HTTPMethods.Post, ValidateUrl, delegate(ResultInfo info)
		{
			OnOrderValidateResponseResult(info, product, purchase);
		});
		ownSingleHttp.AddTag(product);
		ownSingleHttp.AddHeader(commonHeaders);
		ownSingleHttp.AddParameter(JsonConvert.SerializeObject(anonymous));
		ownSingleHttp.AddCustomTimeout(30f);
		ownSingleHttp.Send();
	}

	private void OnOrderValidateResponseResult(ResultInfo info, Product product, Purchase purchase)
	{
		if (info == null)
		{
			return;
		}
		if (info.State != RequestState.Success)
		{
			Debug.LogError("IAP: 订单验证失败 ：info.State != RequestState.Success," + info.Response.DataAsText);
			OrderValidateFinished(false, false, false, product, purchase, 0);
			return;
		}
		ServerDataRoot<OrderValidateInfo> serverDataRoot = JsonConvert.DeserializeObject<ServerDataRoot<OrderValidateInfo>>(info.Response.DataAsText);
		if (serverDataRoot.code != 1 || serverDataRoot.data == null)
		{
			Debug.LogError(string.Format("IAP: 订单验证失败 ：root.Code != 1 || root.data == null,Code:{0},{1}", serverDataRoot.code, info.Response.DataAsText));
			OrderValidateFinished(false, false, false, product, purchase, 0);
			return;
		}
		bool isValid;
		if (serverDataRoot.data.status == 1)
		{
			isValid = (product.ProductType == ProductType.NonConsumable);
		}
		else
		{
			isValid = (serverDataRoot.data.status == 0);
		}
		bool isSendDeliver = serverDataRoot.data.status == 0;
		bool isSandbox = serverDataRoot.data.isSandbox;
		OrderValidateFinished(isValid, isSandbox, isSendDeliver, product, purchase, serverDataRoot.data.status);
	}

	private void OrderValidateFinished(bool isValid, bool isSandBox, bool isSendDeliver, Product product, Purchase purchase, int status)
	{
		if (isValid)
		{
			if (isSandBox)
			{
				Debug.Log(string.Concat(new string[]
				{
					"IAP: CURRENCY:",
					product.PriceCurrencyCode,
					" REVENUE:",
					product.PriceAmount.ToString(CultureInfo.InvariantCulture),
					" CONTENT_TYPE:",
					purchase.TransactionID,
					" CONTENT_ID:",
					purchase.ProductID
				}));
			}
			else
			{
				_iapController.AppsFlyerLog(product.PriceCurrencyCode, product.PriceAmount.ToString(CultureInfo.InvariantCulture), purchase.TransactionID, purchase.ProductID);
			}
			_iapController.ServerOrderValidateFinished(true, purchase, product, null);
			if (isSendDeliver)
			{
				OrderDeliver(purchase.TransactionID);
			}
		}
		else
		{
			_iapController.ServerOrderValidateFinished(false, purchase, product, null);
		}
	}

	private void OrderDeliver(string dillId)
	{
		Dictionary<string, string> commonHeaders = RequestsMgr.GetCommonHeaders();
		var anonymous = new
		{
			dillId = dillId
		};
		Dictionary<string, object> dictionary = new Dictionary<string, object>
		{
			{ "dillId", anonymous.dillId }
		};
		RequestsMgr.Sign(ref commonHeaders, dictionary);
		OwnSingleHttp ownSingleHttp = RequestsMgr.I.Create(HTTPMethods.Post, DeliverUrl, delegate(ResultInfo info)
		{
		});
		ownSingleHttp.AddHeader(commonHeaders);
		ownSingleHttp.AddParameter(JsonConvert.SerializeObject(anonymous));
		ownSingleHttp.AddCustomTimeout(30f);
		ownSingleHttp.Send();
	}
}
