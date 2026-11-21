using System;
using System.Collections.Generic;
using System.Linq;
using FQDev.StoreKit;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour, IStoreListener, FQDev.StoreKit.IStoreController
{
	public Action<bool, List<string>, string> initCallback;

	public Action initSuccEndCall;

	public Action<bool, string, string, string> buyCall;

	public static IAPManager instance;

	public bool isInitialized;

	private UnityEngine.Purchasing.IStoreController _storeController;

	private IExtensionProvider _storeExtension;

	private List<ProductInfo> infos;

	private List<string> _productIDs;

	private string _purchasingProductId;

	public UnityEngine.Purchasing.IStoreController StoreController => _storeController;

	private void Awake()
	{
		instance = this;
	}

	public void Init(List<ProductInfo> infos)
	{
		if (isInitialized)
		{
			return;
		}
		this.infos = infos;
		StoreUtility.Init(this);
	}

	public void OnInitialized(UnityEngine.Purchasing.IStoreController controller, IExtensionProvider extensions)
	{
		_storeController = controller;
		_storeExtension = extensions;
		isInitialized = true;
		Debug("init success");
		if (_productIDs == null)
		{
			_productIDs = new List<string>();
		}
		_productIDs.Clear();
		foreach (UnityEngine.Purchasing.Product item in _storeController.products.all)
		{
			_productIDs.Add(item.definition.id);
		}
		initCallback?.Invoke(true, _productIDs, string.Empty);
		initSuccEndCall?.Invoke();
	}

	public void OnInitializeFailed(InitializationFailureReason error)
	{
		string text = error.ToString("G");
		Debug("init fail: " + text);
		initCallback?.Invoke(false, null, text);
	}

	public void OnInitializeFailed(InitializationFailureReason error, string message)
	{
		string text = error.ToString("G");
		Debug("init fail: " + text + " message:" + message);
		initCallback?.Invoke(false, null, message);
	}

	public bool Buy(cfbt_Stores cfg)
	{
		if (!isInitialized)
		{
			Debug("ID:" + cfg.id + ". Not init.");
			return false;
		}
		string text = _productId(cfg.id);
		if (StoreUtility.Instance.GetProduct(text) == null)
		{
			Debug("ID:" + text + ".Not found or is not available for purchase");
			return false;
		}
		Global.IsShowAdOrPurchase = true;
		AdvertisingMgr.I.EnableWaiting();
		DB.GiftStalls.AddRecordStoreIndex(cfg.index);
		_purchasingProductId = text;
		StoreUtility.Instance.Purchase(text, (UnityEngine.Purchasing.ProductType)cfg.productType);
		return true;
	}

	public void OnPurchaseFailed(UnityEngine.Purchasing.Product pro, PurchaseFailureReason p)
	{
		LogEvent.I.Track("IAP_Buy_Callback")
			.Add("Dill_ID", pro.transactionID)
			.Add("Product_Id", pro.definition.id)
			.Add("State", "false")
			.CommonAttr(true)
			.Send(false);
		string text = p.ToString("G");
		Debug("ID:" + pro.definition.id + ". purchase fail: " + text);
		buyCall?.Invoke(false, pro.definition.id, pro.transactionID, text);
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
	{
		LogEvent.I.Track("IAP_Buy_Callback")
			.Add("Dill_ID", e.purchasedProduct.transactionID)
			.Add("Product_Id", e.purchasedProduct.definition.id)
			.Add("State", "true")
			.CommonAttr(true)
			.Send(false);
		string receipt = FQDev.StoreKit.Purchase.GetReceipt(e);
		IAPIosOrderValidateManager.I.IosOrderValidate(receipt, e.purchasedProduct, ServerIosOrderValidateFinished);
		return PurchaseProcessingResult.Pending;
	}

	private void ServerIosOrderValidateFinished(bool isValid, bool isSandBox, bool isSendDeliver, UnityEngine.Purchasing.Product product, string error)
	{
		if (isValid)
		{
			if (isSandBox)
			{
				Debug(string.Concat(new string[]
				{
					"CURRENCY:",
					product.metadata.isoCurrencyCode,
					" REVENUE:",
					product.metadata.localizedPrice.ToString(),
					" CONTENT_TYPE:",
					product.transactionID,
					" CONTENT_ID:",
					product.definition.id
				}));
			}
			else
			{
				AppsFlyerLog(product.metadata.isoCurrencyCode, product.metadata.localizedPrice.ToString(), product.transactionID, product.definition.id);
			}
			buyCall?.Invoke(true, product.definition.id, product.transactionID, string.Empty);
			string iosTransactionId = IAPIosOrderValidateManager.I.GetIosTransactionId(product);
			_storeController.ConfirmPendingPurchase(product);
			if (isSendDeliver)
			{
				IAPIosOrderValidateManager.I.IosOrderDeliver(iosTransactionId, null);
			}
		}
		else
		{
			buyCall?.Invoke(false, product.definition.id, product.transactionID, string.Empty);
			LogEvent.I.Track("Server_Ios_Order_Validate_Fail")
				.Add("Dill_ID", product.transactionID)
				.Add("Product_Id", product.definition.id)
				.Add("Error", error)
				.CommonAttr(true)
				.Send(false);
		}
	}

	public void RestorePurchases(Action cBack)
	{
		if (!isInitialized)
		{
			Debug("RestorePurchases FAIL. Not init.");
			cBack?.Invoke();
			return;
		}
		LogEvent.I.Track("Restore_Purchases_Start")
			.Add("Platform", Application.platform.ToString())
			.CommonAttr(true)
			.Send(false);
		if (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.OSXPlayer)
		{
			Debug(string.Format("RestorePurchases FAIL. Not supported on this platform. Current = {0}", Application.platform));
			cBack?.Invoke();
			return;
		}
		Debug("RestorePurchases started ...");
		_storeExtension.GetExtension<IAppleExtensions>().RestoreTransactions(delegate(bool result, string content)
		{
			Debug(string.Format("RestorePurchases result: {0}", result));
			if (!result)
			{
				cBack?.Invoke();
			}
			LogEvent.I.Track("Restore_Purchases_Result")
				.Add("Result", result)
				.CommonAttr(true)
				.Send(false);
		});
	}

	public void AppsFlyerLog(string isoCurrencyCode, string localizedPrice, string transactionID, string productID)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("af_currency", isoCurrencyCode);
		dictionary.Add("af_revenue", localizedPrice);
		dictionary.Add("af_quantity", "1");
		dictionary.Add("af_content_type", transactionID);
		dictionary.Add("af_content_id", productID);
		AppsflyerSDK.SendEvent("af_purchase", dictionary);
	}

	public string MoneyStrSale(string str, float sale)
	{
		string[] array = MoneySplit(str);
		Debug("MoneyStrSale symbol:" + array[0] + ",value:" + array[1] + ",sale:" + sale.ToString());
		float num = float.Parse(array[1]) * sale;
		string text = array[0] + string.Format("{0:F2}", num);
		Debug("MoneyStrSale rs:" + text);
		return text;
	}

	public string[] MoneySplit(string str)
	{
		Debug("MoneyStrSale str.Length:" + str.Length);
		int num = 0;
		for (int i = 0; i < str.Length; i++)
		{
			int num2 = Convert.ToInt32(str[i]);
			if (num2 >= 48 && num2 <= 57)
			{
				num = i;
				break;
			}
		}
		string text = str.Substring(0, num);
		string text2 = str.Substring(num, str.Length - num);
		return new string[2] { text, text2 };
	}

	public bool AnalysisSubscriptionProduct(UnityEngine.Purchasing.Product[] products)
	{
		IAppleExtensions extension = _storeExtension.GetExtension<IAppleExtensions>();
		Dictionary<string, string> dictionary = extension.GetIntroductoryPriceDictionary();
		bool result = false;
		if (products != null)
		{
			for (int i = 0; i < products.Length; i++)
			{
				UnityEngine.Purchasing.Product product = products[i];
				if (product.availableToPurchase)
				{
					if (!product.hasReceipt)
					{
						Debug("ID:" + product.definition.id + ". no receipt");
					}
					else if (product.definition.type != UnityEngine.Purchasing.ProductType.Subscription)
					{
						Debug("ID:" + product.definition.id + ". no Subscription Product");
					}
					else if (_checkIfProductIsAvailableForSubscriptionManager(product.receipt))
					{
						string intro_json = null;
						if (dictionary != null && dictionary.ContainsKey(product.definition.storeSpecificId))
						{
							intro_json = dictionary[product.definition.storeSpecificId];
						}
						SubscriptionInfo subscriptionInfo = new SubscriptionManager(product, intro_json).getSubscriptionInfo();
						if (!result)
						{
							result = subscriptionInfo.isSubscribed() == Result.True;
						}
						Debug("product id is: " + subscriptionInfo.getProductId());
						Debug("purchase date is: " + subscriptionInfo.getPurchaseDate().ToString());
						Debug("is subscribed? " + subscriptionInfo.isSubscribed().ToString());
						Debug("is expired? " + subscriptionInfo.isExpired().ToString());
						Debug("is cancelled? " + subscriptionInfo.isCancelled().ToString());
						Debug("product is in free trial peroid? " + subscriptionInfo.isFreeTrial().ToString());
						Debug("product is auto renewing? " + subscriptionInfo.isAutoRenewing().ToString());
						Debug("subscription remaining valid time until next billing date is: " + subscriptionInfo.getRemainingTime().ToString());
						Debug("is this product in introductory price period? " + subscriptionInfo.isIntroductoryPricePeriod().ToString());
						Debug("the product introductory price period is: " + subscriptionInfo.getIntroductoryPricePeriod().ToString());
						Debug("the number of product introductory price period cycles is: " + subscriptionInfo.getIntroductoryPricePeriodCycles().ToString());
						Debug("the product introductory localized price is: " + subscriptionInfo.getIntroductoryPrice());
						Debug("subscription next billing date is: " + subscriptionInfo.getExpireDate().ToString());
					}
				}
				else
				{
					Debug("ID:" + product.definition.id + ". not available for purchase");
				}
			}
		}
		return result;
	}

	private bool _checkIfProductIsAvailableForSubscriptionManager(string receipt)
	{
		Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
		if (!dictionary.ContainsKey("Store") || !dictionary.ContainsKey("Payload"))
		{
			Debug("The product receipt does not contain enough information");
			return false;
		}
		string a = (string)dictionary["Store"];
		string text = (string)dictionary["Payload"];
		if (text == null)
		{
			return false;
		}
		if (string.Equals(a, "GooglePlay"))
		{
			Dictionary<string, object> dictionary2 = (Dictionary<string, object>)MiniJson.JsonDecode(text);
			if (!dictionary2.ContainsKey("json"))
			{
				Debug("The product receipt does not contain enough information, the \"json\" field is missing");
				return false;
			}
			Dictionary<string, object> dictionary3 = (Dictionary<string, object>)MiniJson.JsonDecode((string)dictionary2["json"]);
			if (dictionary3 == null || !dictionary3.ContainsKey("developerPayload"))
			{
				Debug("The product receipt does not contain enough information, the \"developerPayload\" field is missing");
				return false;
			}
			Dictionary<string, object> dictionary4 = MiniJson.JsonDecode((string)dictionary3["developerPayload"]) as Dictionary<string, object>;
			if (dictionary4 != null && dictionary4.ContainsKey("is_free_trial") && dictionary4.ContainsKey("has_introductory_price_trial"))
			{
				return true;
			}
			Debug("The product receipt does not contain enough information, the product is not purchased using 1.19 or later");
			return false;
		}
		if (!string.Equals(a, "AppleAppStore"))
		{
			return string.Equals(a, "MacAppStore");
		}
		return true;
	}

	public bool IsInitialized()
	{
		return isInitialized;
	}

	private string _productId(string id)
	{
		return id.Split('|')[0];
	}

	private void Debug(string mes)
	{
		UnityEngine.Debug.Log("IAPManager " + mes);
	}

	public string GetProductIdById(ProductInfo config)
	{
		return _productId(config.productId);
	}

	public void OnStoreConnectResult(bool success)
	{
		if (success)
		{
			StoreUtility.Instance.QueryProducts(infos.Select(GetProductIdById));
		}
		else
		{
			Debug("OnStoreConnectResult false");
		}
	}

	public void OnProductQueryResult(bool success, IEnumerable<FQDev.StoreKit.Product> products)
	{
		if (!success)
		{
			initCallback?.Invoke(false, null, string.Empty);
			Debug("OnProductQueryResult false");
			return;
		}
		isInitialized = true;
		if (_productIDs == null)
		{
			_productIDs = new List<string>();
		}
		_productIDs.Clear();
		if (products != null)
		{
			foreach (FQDev.StoreKit.Product product in products)
			{
				_productIDs.Add(product.ID);
			}
		}
		initCallback?.Invoke(true, _productIDs, string.Empty);
		initSuccEndCall?.Invoke();
	}

	public void OnPurchaseSuccess(Purchase result, bool isPurchase = true)
	{
		if (result == null || result.SignatureFlag != "true" || result.PurchaseState != 1)
		{
			buyCall?.Invoke(false, result?.ProductID, result?.TransactionID, string.Empty);
			return;
		}
		FQDev.StoreKit.Product product = StoreUtility.Instance.GetProduct(result.ProductID);
		if (!isPurchase)
		{
			buyCall?.Invoke(true, result.ProductID, null, string.Empty);
			return;
		}
		Debug(GetType().Name + " Google v2 order validate: ");
		IAPAndroidOrderValidateManager.I.OrderValidate(product, result, this);
	}

	public void ServerOrderValidateFinished(bool isSuccess, Purchase purchase, FQDev.StoreKit.Product product, string errorInfo)
	{
		if (isSuccess)
		{
			buyCall?.Invoke(true, purchase.ProductID, purchase.TransactionID, string.Empty);
			cfbt_Stores storeInfoByProductID = Global.pStores.GetStoreInfoByProductID(purchase.ProductID);
			if (storeInfoByProductID == null || storeInfoByProductID.productType != 1)
			{
				StoreUtility.Instance.ConsumeInappPurchase(purchase.PurchaseToken);
			}
			else
			{
				StoreUtility.Instance.AcknowledgePurchaseAsync(purchase.PurchaseToken);
			}
		}
		else
		{
			buyCall?.Invoke(false, product.ID, null, string.Empty);
		}
	}

	public void OnPurchaseCanceled()
	{
		buyCall?.Invoke(false, null, null, string.Empty);
	}

	public void OnPurchaseFailed(string message)
	{
		buyCall?.Invoke(false, null, null, string.Empty);
	}
}
