namespace Advertisement
{
	public class AdStartLoadData : IAdData
	{
		public int AdPlatform = 1;

		public string AdsType = "Invalid";

		public string Placement = "None";

		public string HasAd = "no";

		public string AdUnitId = "0000000";

		public bool IsHasAd => HasAd.Equals("has");

		public string GetAdPlatform()
		{
			if (AdPlatform != 1)
			{
				return "Unknown";
			}
			return "Max";
		}
	}
}
