using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class AdvertisingProxy : MonoBehaviour
{
	private static AdvertisingProxy _instance;

	private static Action _onInitFinish;

	private static Action<string> _onRewardAdStartLoadDelegate;

	private static Action<string> _onRewardAdLoadedDelegate;

	private static Action<string> _onRewardAdLoadFailedDelegate;

	private static Action<string> _onRewardAdShowDelegate;

	private static Action<string> _onRewardAdCloseDelegate;

	private static Action<string> _onRewardAdClickedDelegate;

	private static Action<string> _onRewardAdClearCacheDelegate;

	private static Action<string> _onRewardAdImpressionDelegate;

	private static Action<string> _onInterstitialAdStartLoadDelegate;

	private static Action<string> _onInterstitialAdLoadedDelegate;

	private static Action<string> _onInterstitialAdLoadFailedDelegate;

	private static Action<string> _onInterstitialAdShowDelegate;

	private static Action<string> _onInterstitialAdCloseDelegate;

	private static Action<string> _onInterstitialAdClickedDelegate;

	private static Action<string> _onInterstitialAdClearCacheDelegate;

	private static Action<string> _onInterstitialAdImpressionDelegate;

	private static Action<string> _onBannerLoadedDelegate;

	private static Action<string> _onBannerImpressionDelegate;

#if FAKE_SDK // [rebuild] bản FAKE_SDK không có lớp Java của SDK; khởi tạo sẽ ném lỗi trong static ctor
	private static AndroidJavaClass AdvertisingProxyAndroid = null;
#else
	private static AndroidJavaClass AdvertisingProxyAndroid = new AndroidJavaClass("com.fuqian.ads.common.AdvertisingProxy");
#endif

	public static AdvertisingProxy Instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject gameObject = new GameObject(typeof(AdvertisingProxy).ToString());
				_instance = gameObject.AddComponent<AdvertisingProxy>();
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
			}
			return _instance;
		}
	}

	public static T CallAndroidFunc<T>(string method, params object[] args)
	{
#if FAKE_SDK // [rebuild] fake SDK quảng cáo (FakeSdk/FakeAds.cs)
		object r = FakeAds.Call(method, args);
		return (r is T) ? (T)r : default(T);
#elif UNITY_EDITOR // [rebuild] Editor không có JNI; bản build giữ nguyên gốc
		return default(T);
#else
		return AdvertisingProxyAndroid.CallStatic<AndroidJavaObject>("getInstance").Call<T>(method, args);
#endif
	}

	public static void CallAndroidFunc(string method, params object[] args)
	{
#if FAKE_SDK // [rebuild] fake SDK quảng cáo (FakeSdk/FakeAds.cs)
		FakeAds.Call(method, args);
#elif UNITY_EDITOR // [rebuild] Editor không có JNI; bản build giữ nguyên gốc
		return;
#else
		AdvertisingProxyAndroid.CallStatic<AndroidJavaObject>("getInstance").Call(method, args);
#endif
	}

	public void Initialize(string adsNative, string channel)
	{
		CallAndroidFunc("initialize", GetInitParams(adsNative, channel), adsNative);
	}

	public static void ShowRewardAds(Action<string> callfunc, string placement)
	{
		_onRewardAdCloseDelegate = callfunc;
		CallAndroidFunc("showRewardAds", placement);
	}

	public static bool IsReadyRewardAd()
	{
		return CallAndroidFunc<bool>("isReadyRewardAd");
	}

	public static void ShowInterstitialAds(Action<string> callfunc, string placement, string commonData)
	{
		_onInterstitialAdCloseDelegate = callfunc;
		CallAndroidFunc("sendCommonData", commonData);
		CallAndroidFunc("showInterstitialAds", placement);
	}

	public static bool IsReadyInterstitial()
	{
		return CallAndroidFunc<bool>("isReadyInterstitial");
	}

	public static void ShowBanner()
	{
		CallAndroidFunc("showBanner");
	}

	public static void HideBanner()
	{
		CallAndroidFunc("hideBanner");
	}

	public static void ShowBannerTop()
	{
		CallAndroidFunc("showBannerTop");
	}

	public static void HideBannerTop()
	{
		CallAndroidFunc("hideBannerTop");
	}

	public void JumpToSetting()
	{
		CallAndroidFunc("jumpToSetting");
	}

	public void SetBannerParam(bool isShowCollapsibleBanner, bool isUseTestBannerId)
	{
		CallAndroidFunc("setBannerParam", isShowCollapsibleBanner, isUseTestBannerId);
	}

	public static void SetHasUserConsent(bool isHas)
	{
		CallAndroidFunc("setHasUserConsent", isHas);
	}

	public void RegisterOnInitFinish(Action action)
	{
		if (action != null)
		{
			_onInitFinish = (Action)Delegate.Combine(_onInitFinish, action);
		}
	}

	public void RegisterRewardListener(Action<string> onStartLoadCallFunc, Action<string> onLoadedCallFunc, Action<string> onLoadFailedCallFunc, Action<string> onShowCallFunc, Action<string> onClickCallFunc, Action<string> onClearCacheCallFunc, Action<string> onImpressionCallFunc)
	{
		if (onStartLoadCallFunc != null)
		{
			_onRewardAdStartLoadDelegate = (Action<string>)Delegate.Combine(_onRewardAdStartLoadDelegate, onStartLoadCallFunc);
		}
		if (onLoadedCallFunc != null)
		{
			_onRewardAdLoadedDelegate = (Action<string>)Delegate.Combine(_onRewardAdLoadedDelegate, onLoadedCallFunc);
		}
		if (onLoadFailedCallFunc != null)
		{
			_onRewardAdLoadFailedDelegate = (Action<string>)Delegate.Combine(_onRewardAdLoadFailedDelegate, onLoadFailedCallFunc);
		}
		if (onShowCallFunc != null)
		{
			_onRewardAdShowDelegate = (Action<string>)Delegate.Combine(_onRewardAdShowDelegate, onShowCallFunc);
		}
		if (onClickCallFunc != null)
		{
			_onRewardAdClickedDelegate = (Action<string>)Delegate.Combine(_onRewardAdClickedDelegate, onClickCallFunc);
		}
		if (onClearCacheCallFunc != null)
		{
			_onRewardAdClearCacheDelegate = (Action<string>)Delegate.Combine(_onRewardAdClearCacheDelegate, onClearCacheCallFunc);
		}
		if (onImpressionCallFunc != null)
		{
			_onRewardAdImpressionDelegate = (Action<string>)Delegate.Combine(_onRewardAdImpressionDelegate, onImpressionCallFunc);
		}
	}

	public void RegisterInterstitialListener(Action<string> onStartLoadCallFunc, Action<string> onLoadedCallFunc, Action<string> onLoadFailedCallFunc, Action<string> onShowCallFunc, Action<string> onClickCallFunc, Action<string> onClearCacheCallFunc, Action<string> onImpressionCallFunc)
	{
		if (onStartLoadCallFunc != null)
		{
			_onInterstitialAdStartLoadDelegate = (Action<string>)Delegate.Combine(_onInterstitialAdStartLoadDelegate, onStartLoadCallFunc);
		}
		if (onLoadedCallFunc != null)
		{
			_onInterstitialAdLoadedDelegate = (Action<string>)Delegate.Combine(_onInterstitialAdLoadedDelegate, onLoadedCallFunc);
		}
		if (onLoadFailedCallFunc != null)
		{
			_onInterstitialAdLoadFailedDelegate = (Action<string>)Delegate.Combine(_onInterstitialAdLoadFailedDelegate, onLoadFailedCallFunc);
		}
		if (onShowCallFunc != null)
		{
			_onInterstitialAdShowDelegate = (Action<string>)Delegate.Combine(_onInterstitialAdShowDelegate, onShowCallFunc);
		}
		if (onClickCallFunc != null)
		{
			_onInterstitialAdClickedDelegate = (Action<string>)Delegate.Combine(_onInterstitialAdClickedDelegate, onClickCallFunc);
		}
		if (onClearCacheCallFunc != null)
		{
			_onInterstitialAdClearCacheDelegate = (Action<string>)Delegate.Combine(_onInterstitialAdClearCacheDelegate, onClearCacheCallFunc);
		}
		if (onImpressionCallFunc != null)
		{
			_onInterstitialAdImpressionDelegate = (Action<string>)Delegate.Combine(_onInterstitialAdImpressionDelegate, onImpressionCallFunc);
		}
	}

	public void RegisterBannerListener(Action<string> onLoadedCallFunc, Action<string> onImpressionCallFunc)
	{
		if (onLoadedCallFunc != null)
		{
			_onBannerLoadedDelegate = (Action<string>)Delegate.Combine(_onBannerLoadedDelegate, onLoadedCallFunc);
		}
		if (onImpressionCallFunc != null)
		{
			_onBannerImpressionDelegate = (Action<string>)Delegate.Combine(_onBannerImpressionDelegate, onImpressionCallFunc);
		}
	}

	public void OnInitFinish(string msg)
	{
		Debug.LogError("AdvertisingProxy.OnInitFinish");
		if (_onInitFinish != null)
		{
			_onInitFinish();
		}
	}

	public void OnRewardAdStartLoad(string msg)
	{
		if (_onRewardAdStartLoadDelegate != null)
		{
			_onRewardAdStartLoadDelegate(msg);
		}
	}

	public void OnRewardAdLoaded(string msg)
	{
		if (_onRewardAdLoadedDelegate != null)
		{
			_onRewardAdLoadedDelegate(msg);
		}
	}

	public void OnRewardAdLoadFailed(string msg)
	{
		if (_onRewardAdLoadFailedDelegate != null)
		{
			_onRewardAdLoadFailedDelegate(msg);
		}
	}

	public void OnRewardAdShow(string msg)
	{
		if (_onRewardAdShowDelegate != null)
		{
			_onRewardAdShowDelegate(msg);
		}
	}

	public void OnRewardAdClose(string msg)
	{
		if (_onRewardAdCloseDelegate != null)
		{
			_onRewardAdCloseDelegate(msg);
		}
		_onRewardAdCloseDelegate = null;
	}

	public void OnRewardAdClick(string msg)
	{
		if (_onRewardAdClickedDelegate != null)
		{
			_onRewardAdClickedDelegate(msg);
		}
	}

	public void OnRewardAdClearCache(string msg)
	{
		if (_onRewardAdClearCacheDelegate != null)
		{
			_onRewardAdClearCacheDelegate(msg);
		}
	}

	public void OnRewardAdImpression(string msg)
	{
		if (_onRewardAdImpressionDelegate != null)
		{
			_onRewardAdImpressionDelegate(msg);
		}
	}

	public void OnInterstitialAdStartLoad(string msg)
	{
		if (_onInterstitialAdStartLoadDelegate != null)
		{
			_onInterstitialAdStartLoadDelegate(msg);
		}
	}

	public void OnInterstitialAdLoaded(string msg)
	{
		if (_onInterstitialAdLoadedDelegate != null)
		{
			_onInterstitialAdLoadedDelegate(msg);
		}
	}

	public void OnInterstitialAdLoadFailed(string msg)
	{
		if (_onInterstitialAdLoadFailedDelegate != null)
		{
			_onInterstitialAdLoadFailedDelegate(msg);
		}
	}

	public void OnInterstitialAdShow(string msg)
	{
		if (_onInterstitialAdShowDelegate != null)
		{
			_onInterstitialAdShowDelegate(msg);
		}
	}

	public void OnInterstitialAdClose(string msg)
	{
		if (_onInterstitialAdCloseDelegate != null)
		{
			_onInterstitialAdCloseDelegate(msg);
		}
		_onInterstitialAdCloseDelegate = null;
	}

	public void OnInterstitialAdClick(string msg)
	{
		if (_onInterstitialAdClickedDelegate != null)
		{
			_onInterstitialAdClickedDelegate(msg);
		}
	}

	public void OnInterstitialAdClearCache(string msg)
	{
		if (_onInterstitialAdClearCacheDelegate != null)
		{
			_onInterstitialAdClearCacheDelegate(msg);
		}
	}

	public void OnInterstitialAdImpression(string msg)
	{
		if (_onInterstitialAdImpressionDelegate != null)
		{
			_onInterstitialAdImpressionDelegate(msg);
		}
	}

	public void OnBannerLoaded(string msg)
	{
		if (_onBannerLoadedDelegate != null)
		{
			_onBannerLoadedDelegate(msg);
		}
	}

	public void OnBannerImpression(string msg)
	{
		if (_onBannerImpressionDelegate != null)
		{
			_onBannerImpressionDelegate(msg);
		}
	}

	private static string GetInitParams(string adsNative, string channel)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (adsNative == "Admob")
		{
			AdmobAdSetting admobAdSetting = Resources.Load<AdmobAdSetting>("AdmobAdSetting");
			dictionary.Add("AppID", admobAdSetting.android.AppID);
			dictionary.Add("RewardUnitID", admobAdSetting.android.RewardUnitID);
			dictionary.Add("InterstitialID", admobAdSetting.android.InterstitialID);
			dictionary.Add("BannerID", admobAdSetting.android.BannerID);
		}
		else if (adsNative == "Ironsource")
		{
			IronsourceSetting ironsourceSetting = Resources.Load<IronsourceSetting>("IronsourceSetting");
			dictionary.Add("AppID", ironsourceSetting.android.AppID);
		}
		else if (adsNative == "AppLovinMax")
		{
			AppLovinMaxSetting appLovinMaxSetting = Resources.Load<AppLovinMaxSetting>("AppLovinMaxSetting");
			dictionary.Add("SDK_ID", appLovinMaxSetting.android.SDKID);
			dictionary.Add("RewardUnitID", appLovinMaxSetting.android.RewardUnitID);
			dictionary.Add("InterstitialID", appLovinMaxSetting.android.InterstitialID);
			dictionary.Add("BannerID", appLovinMaxSetting.android.BannerID);
			dictionary.Add("TopBannerID", appLovinMaxSetting.android.TopBannerID);
			dictionary.Add("Amazon_AppID", appLovinMaxSetting.android.ApsAppID);
			dictionary.Add("Amazon_RewardUnitID", appLovinMaxSetting.android.ApsRewardUUID);
			dictionary.Add("Amazon_InterstitialID", appLovinMaxSetting.android.ApsInterstitialUUID);
			dictionary.Add("Amazon_BannerID", appLovinMaxSetting.android.ApsBannerUUID);
			dictionary.Add("Amazon_TopBannerID", appLovinMaxSetting.android.TopApsBannerUUID);
		}
		else if (adsNative == "Hyperbid")
		{
			HyperbidSetting hyperbidSetting = Resources.Load<HyperbidSetting>("HyperbidSetting");
			dictionary.Add("AppID", hyperbidSetting.android.AppID);
			dictionary.Add("AppKey", hyperbidSetting.android.APPKey);
			dictionary.Add("RewardUnitID", hyperbidSetting.android.RewardUnitID);
			dictionary.Add("InterstitialID", hyperbidSetting.android.InterstitialID);
			dictionary.Add("BannerID", hyperbidSetting.android.BannerID);
			dictionary.Add("TopBannerID", hyperbidSetting.android.TopBannerID);
			dictionary.Add("Channel", channel);
			dictionary.Add("Amazon_AppID", hyperbidSetting.android.ApsAppID);
			dictionary.Add("Amazon_RewardUnitID", hyperbidSetting.android.ApsRewardUUID);
			dictionary.Add("Amazon_InterstitialID", hyperbidSetting.android.ApsInterstitialUUID);
			dictionary.Add("Amazon_BannerID", hyperbidSetting.android.ApsBannerUUID);
			dictionary.Add("Top_Amazon_BannerID", hyperbidSetting.android.TopApsBannerUUID);
		}
		dictionary.Add("USER_ID", SystemInfo.deviceUniqueIdentifier);
		dictionary["gm"] = "no";
		return JsonConvert.SerializeObject(dictionary);
	}

	public static void ShowMediaTestSuite(string mediaName)
	{
		CallAndroidFunc("showMediaTestSuite", mediaName);
	}
}
