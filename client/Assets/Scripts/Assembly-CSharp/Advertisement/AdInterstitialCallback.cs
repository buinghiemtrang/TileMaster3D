using System.Collections.Generic;

namespace Advertisement
{
	public class AdInterstitialCallback : IAdCallback
	{
		private readonly Dictionary<string, float> _interstitialLoadTime = new Dictionary<string, float>();

		public void StartLoad(string msg)
		{
			AdImpressionData adImpressionData = AdDataAnalyzer.DeserializeJson<AdImpressionData>(msg);
			_interstitialLoadTime[adImpressionData.AdUnitId] = UnityEngine.Time.realtimeSinceStartup;
			LogEvent.I.Ads_Request(adImpressionData);
		}

		public void LoadComplete(string msg)
		{
			AdImpressionData adImpressionData = AdDataAnalyzer.DeserializeJson<AdImpressionData>(msg);
			float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
			float value;
			if (!_interstitialLoadTime.TryGetValue(adImpressionData.AdUnitId, out value))
			{
				value = UnityEngine.Time.realtimeSinceStartup;
			}
			adImpressionData.LoadTime = realtimeSinceStartup - value;
			LogEvent.I.Ads_Response(adImpressionData);
		}

		public void LoadFailed(string msg)
		{
			AdImpressionData adImpressionData = AdDataAnalyzer.DeserializeJson<AdImpressionData>(msg);
			float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
			float value;
			if (!_interstitialLoadTime.TryGetValue(adImpressionData.AdUnitId, out value))
			{
				value = UnityEngine.Time.realtimeSinceStartup;
			}
			adImpressionData.LoadTime = realtimeSinceStartup - value;
			LogEvent.I.Ads_Fail(adImpressionData);
		}

		public void OnAdClick(string msg)
		{
			AdImpressionData data = AdDataAnalyzer.DeserializeJson<AdImpressionData>(msg);
			LogEvent.I.Click_ads(data);
		}

		public void ClearCache(string msg)
		{
			AdImpressionData data = AdDataAnalyzer.DeserializeJson<AdImpressionData>(msg);
			LogEvent.I.Ads_ClearCache(data);
		}

		public void OnAdShow(string msg)
		{
			AdImpressionData data = AdDataAnalyzer.DeserializeJson<AdImpressionData>(msg);
			LogEvent.I.Ads_GetPrice(data);
		}

		public void OnImpression(string msg)
		{
			AdImpressionData adImpressionData = AdDataAnalyzer.DeserializeJson<AdImpressionData>(msg);
			LogEvent.I.Ads_Interstitial(adImpressionData);
			LogEvent.I.Ads_Impression_FireBase(adImpressionData);
			LogEvent.I.Ads_Impression_ThinkingAnalytics(adImpressionData);
			LogEvent.I.FaceBook_AdValueReport(adImpressionData);
			Singleton<LogEventValueOnceTrack>.I.Trigger(ValueOnceTrackTriggerType.AdInter, adImpressionData.Value);
			Singleton<LogEventValueRepeatTrack>.I.Trigger(ValueTrackTriggerType.AdInter, adImpressionData.Value);
			if (adImpressionData.AdsType.Equals(AdTypeEnum.Native.ToString()))
			{
				LogEvent.I.Ads_Native(adImpressionData);
			}
		}
	}
}
