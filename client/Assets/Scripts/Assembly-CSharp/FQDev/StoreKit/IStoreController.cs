using System.Collections.Generic;

namespace FQDev.StoreKit
{
	public interface IStoreController
	{
		void OnStoreConnectResult(bool success);

		void OnProductQueryResult(bool success, IEnumerable<Product> products);

		void OnPurchaseSuccess(Purchase result, bool isPurchase = true);

		void OnPurchaseCanceled();

		void OnPurchaseFailed(string message);
	}
}
