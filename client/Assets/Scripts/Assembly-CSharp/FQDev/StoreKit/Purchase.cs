using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Purchasing;

namespace FQDev.StoreKit
{
	[JsonObject]
	public class Purchase
	{
		public class IOSReceipt
		{
			public string Payload;

			public string Store;

			public string TransactionID;
		}

		public string ProductID { get; set; }

		public string TransactionID { get; set; }

		public string PurchaseToken { get; set; }

		public int PurchaseState { get; set; }

		public string ObfuscatedAccountId { get; set; }

		public string SignatureFlag { get; set; }

		public string IsAcknowledged { get; set; }

		public static string GetReceipt(PurchaseEventArgs e)
		{
			IOSReceipt iOSReceipt = JsonConvert.DeserializeObject<IOSReceipt>(e.purchasedProduct.receipt);
			if (iOSReceipt != null)
			{
				return iOSReceipt.Payload;
			}
			Debug.LogError("解析Receipt 失败 ： " + e.purchasedProduct.receipt);
			return e.purchasedProduct.receipt;
		}
	}
}
