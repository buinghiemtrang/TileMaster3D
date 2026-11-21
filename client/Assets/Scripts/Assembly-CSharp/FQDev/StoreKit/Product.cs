using Newtonsoft.Json;
using UnityEngine.Purchasing;

namespace FQDev.StoreKit
{
	[JsonObject]
	public class Product
	{
		public UnityEngine.Purchasing.Product OriginalProduct;

		public string ID { get; set; }

		public string Type { get; set; }

		public string Description { get; set; }

		public string Title { get; set; }

		public string Price { get; set; }

		public string PriceCurrencyCode { get; set; }

		public decimal PriceAmount { get; set; }

		public ProductType ProductType { get; set; }

		public static Product Create(UnityEngine.Purchasing.Product product)
		{
			Product product2 = new Product();
			product2.ID = product.definition.id;
			product2.PriceCurrencyCode = product.metadata.isoCurrencyCode;
			product2.PriceAmount = product.metadata.localizedPrice;
			product2.OriginalProduct = product;
			product2.ProductType = (ProductType)product.definition.type;
			return product2;
		}
	}
}
