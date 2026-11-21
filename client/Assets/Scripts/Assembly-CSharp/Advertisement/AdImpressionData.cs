namespace Advertisement
{
	public class AdImpressionData : IAdData
	{
		public int AdPlatform = 1;

		public string AdsType = "None";

		public string Mediation = "default";

		public string AdCreativeId = "Unknown";

		public string AdUnitId = "0000000";

		public float Value;

		public string Precision = "empty";

		public string Placement = "None";

		public string CurrencyCode = "USD";

		public string Country = "";

		public string HasReward = "fail";

		public string HasClick = "fail";

		public string Error = "";

		public int PlayAdType;

		public int PlayMode;

		public float AdAspectRatio;

		public float LoadTime;

		public bool IsClickAd => HasClick.Equals("succ");

		public string GetAdPlatform()
		{
			if (AdPlatform != 1)
			{
				return "Unknown";
			}
			return "MAX";
		}
	}
}
