using System;
using System.Collections.Generic;
using Advertisement;
using Newtonsoft.Json;

public class AdManager : Singleton<AdManager>
{
	private string TAG = "AdManager:";

	private AdInterstitialCallback interstitialCallbackHandle;

	private AdRewardCallback rewardCallbackHandle;

	private AdBannerCallback bannerCallback;

	public const string BannerAdShowStateKey = "BannerAdShowState";

	public bool isBannerShow;

	public bool isTopBannerShow;

	private bool isBannerAdLoadSuccess;

	private Action<string> OnInterstitialAdClose;

	private Action<string> OnRewardAdClose;

	public void Initialize()
	{
		AdvertisingProxy.Instance.Initialize("AppLovinMax", Global.GetChannelName());
		bannerCallback = new AdBannerCallback();
		AdvertisingProxy.Instance.RegisterBannerListener(bannerCallback.LoadComplete, bannerCallback.OnImpression);
		interstitialCallbackHandle = new AdInterstitialCallback();
		AdvertisingProxy.Instance.RegisterInterstitialListener(interstitialCallbackHandle.StartLoad, interstitialCallbackHandle.LoadComplete, interstitialCallbackHandle.LoadFailed, interstitialCallbackHandle.OnAdShow, interstitialCallbackHandle.OnAdClick, interstitialCallbackHandle.ClearCache, interstitialCallbackHandle.OnImpression);
		rewardCallbackHandle = new AdRewardCallback();
		AdvertisingProxy.Instance.RegisterRewardListener(rewardCallbackHandle.StartLoad, rewardCallbackHandle.LoadComplete, rewardCallbackHandle.LoadFailed, rewardCallbackHandle.OnAdShow, rewardCallbackHandle.OnAdClick, rewardCallbackHandle.ClearCache, rewardCallbackHandle.OnImpression);
		AdvertisingMgr.I.UpdateAdvertisingCd();
	}

	public static string SendCommonData()
	{
		return JsonConvert.SerializeObject(new Dictionary<string, string>
		{
			{
				"OpenUi",
				SceneMgr.sCurSceneName
			},
			{
				"InterstitialAdCount",
				DB.Other.InterstitialSucNum.ToString()
			},
			{
				"BannerAdCount",
				DB.Track.AdsNumBanner.ToString()
			},
			{
				"IsNew",
				(DB.Track.PlayDays == 1) ? "true" : "false"
			},
			{
				"Level_ID",
				(DB.User.Maxlv + 1).ToString()
			}
		});
	}

	public void ShowBanner()
	{
		int curlv = DB.User.Curlv;
		if (!DB.User.AdFree && curlv + 1 >= cf_banner_const.UnlockBannerLv)
		{
			isBannerShow = true;
			AdvertisingProxy.ShowBanner();
		}
		DispatcherMgr.Emit("BannerAdShowState", isBannerShow && isBannerAdLoadSuccess);
	}

	public void HideBanner()
	{
		isBannerShow = false;
		AdvertisingProxy.HideBanner();
		DispatcherMgr.Emit("BannerAdShowState", isBannerShow && isBannerAdLoadSuccess);
	}

	public void ShowBannerTop()
	{
		if (!DB.User.AdFree)
		{
			isTopBannerShow = true;
			AdvertisingProxy.ShowBannerTop();
		}
	}

	public void HideBannerTop()
	{
		isTopBannerShow = false;
		AdvertisingProxy.HideBannerTop();
	}

	private void OnBannerImpression(AdImpressionData data)
	{
	}

	public bool IsInterstitialAdOk(bool isCheckLoadState)
	{
		return AdvertisingProxy.IsReadyInterstitial();
	}

	public void ShowInterstitial(Action<string> callback, INTER_PLACE placement)
	{
		AdvertisingMgr.I.EnableWaiting();
		OnInterstitialAdClose = callback;
		AdvertisingProxy.ShowInterstitialAds(OnInterstitialClose, placement.ToString(), SendCommonData());
	}

	private void OnInterstitialClose(string msg)
	{
		AdImpressionData adImpressionData = JsonConvert.DeserializeObject<AdImpressionData>(msg);
		AdvertisingMgr.I.DisableWaiting();
		if (OnInterstitialAdClose != null)
		{
			OnInterstitialAdClose.InvokeSafe(adImpressionData.HasReward);
		}
	}

	public bool IsRewardVideoOk(bool isCheckLoadState)
	{
		return AdvertisingProxy.IsReadyRewardAd();
	}

	public void ShowRewardedVideo(Action<string> endCall, REWARD_PLACE placement)
	{
		AdvertisingMgr.I.EnableWaiting();
		OnRewardAdClose = endCall;
		AdvertisingProxy.ShowRewardAds(OnRewardClose, placement.ToString());
	}

	private void OnRewardClose(string msg)
	{
		AdImpressionData adImpressionData = JsonConvert.DeserializeObject<AdImpressionData>(msg);
		AdvertisingMgr.I.DisableWaiting();
		if (OnRewardAdClose != null)
		{
			OnRewardAdClose.InvokeSafe(adImpressionData.HasReward);
		}
	}
}
