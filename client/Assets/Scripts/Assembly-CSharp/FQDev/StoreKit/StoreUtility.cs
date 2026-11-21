using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Purchasing;

namespace FQDev.StoreKit
{
	public class StoreUtility : MonoBehaviour
	{
		private static StoreUtility _instance;

		private IStoreController _controller;

		private readonly Dictionary<string, Product> _products = new Dictionary<string, Product>();

#if !FAKE_SDK
		private static readonly AndroidJavaClass _androidJavaClass = new AndroidJavaClass("com.fuqian.StoreKit.GoogleBilling");
#endif

		// [rebuild] FAKE_SDK: chuyển lệnh Java GoogleBilling sang FakeStore (Assets/Scripts/Assembly-CSharp/FakeSdk/FakeBilling.cs).
		// Không có FAKE_SDK: giữ nguyên gốc (CallStatic thẳng vào Java).
		private static void _Call(string method, params object[] args)
		{
#if FAKE_SDK
			FakeBilling.Call(method, args);
#else
			_androidJavaClass.CallStatic(method, args);
#endif
		}

		public static StoreUtility Instance => _instance;

		public bool StoreConnected { get; private set; }

		public static void Init(IStoreController controller)
		{
			if (_instance == null)
			{
				GameObject gameObject = new GameObject("StoreUtility");
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				_instance = gameObject.AddComponent<StoreUtility>();
			}
			_instance._controller = controller;
			_instance.Init();
		}

		private void Init()
		{
			_Call("setPublicKey", "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAiYylOUtxxiusw8adH5UXqSdTANx38qzti//CS+1ON7vT9z2ScMkzLRnV6aXt1wDLc9zH56KeRJDaax89krQ3QoVD0jsP/2M6CQWMgIzS7Xhy4tnU/ayzW5oVPYIgCKIbsbEY+ANumJ1mTOqYbOLbQ1flf5TGtj/oZHxAzY46H3kxPgmwYD1B652UGpVVqY73wu+QnkGHLkqtT09dU7wI2LEIlZecxh0ZL4bB79MUIVAWVW9drVOT/PdAkmqdg598pwJsxiu9PrkeA2yjwWBWzZeamHMzMSEOieyAI7zjE1GbrnqoShryFyUQNZJx27xQAgmdzfV0l7sE3DEr6tiNyQIDAQAB");
			_Call("init");
		}

		public void QueryProducts(IEnumerable<string> productIDs)
		{
			if (productIDs != null)
			{
				string[] array = productIDs.Where((string id) => !string.IsNullOrEmpty(id)).Distinct().ToArray();
				_Call("queryInAppProducts", new object[1] { array });
			}
		}

		public Product GetProduct(string id)
		{
			Product value = null;
			_products.TryGetValue(id, out value);
			return value;
		}

		public void Purchase(string productId, UnityEngine.Purchasing.ProductType extraInfo)
		{
			GetProduct(productId);
			_Call("purchase", productId, extraInfo.ToString());
		}

		public void QueryIncompletePurchase()
		{
			_Call("queryIncompleteOrder");
		}

		public void ConsumeInappPurchase(string purchaseToken)
		{
			_Call("consumePurchaseAsync", purchaseToken);
		}

		public void OnGooglePlayConnectSuccess(string message)
		{
			StoreConnected = true;
			_controller.OnStoreConnectResult(success: true);
		}

		public void OnGooglePlayConnectFailed(string message)
		{
			StoreConnected = false;
			_controller.OnStoreConnectResult(success: false);
		}

		public void OnQueryProductSuccess(string jsonData)
		{
			List<Product> list = JsonConvert.DeserializeObject<List<Product>>(jsonData);
			if (list != null && list.Count > 0)
			{
				_products.Clear();
				foreach (Product item in list)
				{
					_products.Add(item.ID, item);
				}
			}
			_controller.OnProductQueryResult(success: true, list);
		}

		public void AcknowledgePurchaseAsync(string purchaseToken)
		{
			_Call("acknowledgePurchaseAsync", purchaseToken);
		}

		public void OnQueryProductFailed(string message)
		{
			_controller.OnProductQueryResult(success: false, null);
		}

		public void OnPurchaseFailed(string message)
		{
			_controller.OnPurchaseFailed(message);
		}

		public void OnPurchaseCanceled(string message)
		{
			_controller.OnPurchaseCanceled();
		}

		public void OnPurchaseSuccess(string jsonData)
		{
			Purchase result = JsonConvert.DeserializeObject<Purchase>(jsonData);
			_controller.OnPurchaseSuccess(result);
		}

		public void OnConsumeSuccess(string message)
		{
		}

		public void OnConsumeFailed(string message)
		{
		}

		public void OnQueryIncompleteOrderFinish(string jsonData)
		{
			if (string.IsNullOrEmpty(jsonData))
			{
				return;
			}
			List<Purchase> list = JsonConvert.DeserializeObject<List<Purchase>>(jsonData);
			if (list == null || list.Count <= 0)
			{
				return;
			}
			foreach (Purchase item in list)
			{
				Debug.Log(string.Format("StoreKit - purchase restore {0} - {1} — {2}", item.ProductID, item.PurchaseState, item.IsAcknowledged));
				if (string.Equals(item.IsAcknowledged, "true"))
				{
					cfbt_Stores storeInfoByProductID = Global.pStores.GetStoreInfoByProductID(item.ProductID);
					Debug.Log(string.Format("StoreKit - ad gType {0} - {1} — {2}", storeInfoByProductID.gType, storeInfoByProductID.sType, item.IsAcknowledged));
					if (storeInfoByProductID.productType == 1 && storeInfoByProductID.gType == 2 && !Global.pStores.AdFree)
					{
						Debug.Log("StoreKit - ad _controller.OnPurchaseSuccess(purchase,false)");
						_controller.OnPurchaseSuccess(item, isPurchase: false);
					}
				}
				else
				{
					_controller.OnPurchaseSuccess(item);
				}
			}
		}

		public void OnQuerySubscribeFinish()
		{
		}
	}
}
