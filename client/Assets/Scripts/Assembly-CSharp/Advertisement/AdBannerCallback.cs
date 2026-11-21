namespace Advertisement
{
	public class AdBannerCallback : IAdCallback
	{
		public bool isBannerAdLoadSuccess;

		public void ClearCache(string msg)
		{
		}

		public void LoadComplete(string msg)
		{
			isBannerAdLoadSuccess = true;
			DispatcherMgr.Emit("BannerAdShowState", Singleton<AdManager>.I.isBannerShow && isBannerAdLoadSuccess);
		}

		public void LoadFailed(string msg)
		{
		}

		public void OnAdClick(string msg)
		{
		}

		public void OnAdShow(string msg)
		{
		}

		public void OnImpression(string msg)
		{
			AdImpressionData adImpressionData = AdDataAnalyzer.DeserializeJson<AdImpressionData>(msg);
			LogEvent.I.Ads_Impression_FireBase(adImpressionData);
			LogEvent.I.Ads_Impression_ThinkingAnalytics(adImpressionData);
			DB.Track.AdsNumBanner++;
			float value = ((adImpressionData != null) ? adImpressionData.Value : 0f);
			LogEvent.I.Track("Ads_Banner").Add("Times", DB.Track.AdsNumBanner).Add("Value", value).Add("type", adImpressionData.Placement)
				.Send();
			LogEvent.I.FaceBook_AdValueReport(adImpressionData);
			Singleton<LogEventValueOnceTrack>.I.Trigger(ValueOnceTrackTriggerType.AdBanner, value);
			Singleton<LogEventValueRepeatTrack>.I.Trigger(ValueTrackTriggerType.AdBanner, value);
		}

		public void StartLoad(string msg)
		{
		}
	}
}
