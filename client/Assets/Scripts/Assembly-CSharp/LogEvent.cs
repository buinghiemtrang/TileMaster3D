using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Advertisement;
using GameMail;
using GamePark.BrushstrokeEliminate.BaseData;
using GameScene_Level;
using UnityEngine;

public class LogEvent
{
	public class PointKey
	{
		public float key;

		public float value;
	}

	public enum TeamPosition
	{
		None = 0,
		Leader = 1,
		Member = 2,
		Player = 3
	}

	private static readonly string LoginType;

	private static LogEvent _instance;

	private static Dictionary<string, object> _keyValuePairs;

	private static string _keySend;

	private HashSet<string> sFirebaseAnalyticsExclusions;

	private const string Tag = "DataPoint  :  ";

	private string[] _countryInfo;

	private int[] _AF_AdRewardAD;

	private int[] _us_rewardAd_count;

	private float[] _us_rewardAd_ECPM_Value;

	private int[] _mx_rewardAd_count;

	private float[] _mx_rewardAd_ECPM_Value;

	private int[] _br_rewardAd_count;

	private float[] _br_rewardAd_ECPM_Value;

	private int[] _AF_InsertionADs;

	private int[] _us_insertionAd_count;

	private float[] _us_insertionAd_ECPM_Value;

	private int[] _mx_insertionAd_count;

	private float[] _mx_insertionAd_ECPM_Value;

	private int[] _br_insertionAd_count;

	private float[] _br_insertionAd_ECPM_Value;

	private int[] _AF_Level;

	private int[] _AF_LevelWin;

	private int[] _AF_PropHintUse;

	private int[] _AF_Revival;

	private string[] _AF_Purchase_Product;

	private int[] _mx_time;

	private int[] _us_time;

	private int[] _br_time;

	private DateTime _joinTime;

	private REWARD_PLACE _reward_place;

	private INTER_PLACE _inter_place;

	private float[] fUnitRevenueValue;

	private string[] sUnitRevenueValue;

	private const string AdPlatformName = "AppLovinMax";

	private const string CurrencyName = "USD";

	private Dictionary<string, object> _firebaseImpressionParam;

	private DateTime _logChannelStartTime;

	private Dictionary<string, object> _keyNullValuePairs;

	private readonly float[] _firebasePurchase;

	private readonly string _firebaseRevenueTidu001Key;

	private readonly string _firebaseRevenueTidu03Key;

	public static LogEvent I => _instance;

	static LogEvent()
	{
		LoginType = "app";
		_instance = new LogEvent();
		_keyValuePairs = new Dictionary<string, object>();
		_keySend = "";
	}

	public LogEvent()
	{
		sFirebaseAnalyticsExclusions = new HashSet<string>
		{
			"Level_Start", "af_AdRewardAD_1", "af_AdRewardAD_11", "af_AdRewardAD_12", "af_AdRewardAD_13", "af_AdRewardAD_14", "af_AdRewardAD_16", "af_AdRewardAD_17", "af_AdRewardAD_18", "af_AdRewardAD_19",
			"af_AdRewardAD_80", "af_InsertionADs_1", "af_InsertionADs_11", "af_InsertionADs_12", "af_InsertionADs_13", "af_InsertionADs_14", "af_InsertionADs_150", "af_InsertionADs_16", "af_InsertionADs_17", "af_InsertionADs_18",
			"af_InsertionADs_19", "af_Level_1", "af_Level_1000", "af_Level_11", "af_Level_12", "af_Level_1200", "af_Level_13", "af_Level_130", "af_Level_14", "af_Level_15",
			"af_Level_1500", "af_Level_16", "af_Level_17", "af_Level_18", "af_Level_180", "af_Level_1800", "af_Level_19", "af_Level_200", "af_Level_2100", "af_Level_250",
			"af_Level_300", "af_Level_400", "af_Level_500", "af_Level_800", "af_LevelWin_1", "af_LevelWin_1000", "af_LevelWin_11", "af_LevelWin_12", "af_LevelWin_1200", "af_LevelWin_13",
			"af_LevelWin_14", "af_LevelWin_15", "af_LevelWin_1500", "af_LevelWin_16", "af_LevelWin_17", "af_LevelWin_18", "af_LevelWin_180", "af_LevelWin_1800", "af_LevelWin_19", "af_LevelWin_200",
			"af_LevelWin_2100", "af_LevelWin_250", "af_LevelWin_300", "af_LevelWin_400", "af_LevelWin_500", "af_LevelWin_800", "app_clear_data", "app_cold_start", "app_exception", "app_remove",
			"app_store_refund", "app_update", "Coins_Get", "Deduct_Energy", "Game_Pause", "Gift_Show", "LoginDaily_Firstday", "Master_Pass_Open", "os_update", "Shoping_Commodity_Reward"
		};
		_countryInfo = new string[3] { "US", "MX", "BR" };
		_AF_AdRewardAD = new int[23]
		{
			1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
			11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
			25, 40, 80
		};
		_us_rewardAd_count = new int[3] { 2, 2, 2 };
		_us_rewardAd_ECPM_Value = new float[3] { 7f, 10f, 15f };
		_mx_rewardAd_count = new int[2] { 2, 2 };
		_mx_rewardAd_ECPM_Value = new float[2] { 2f, 3f };
		_br_rewardAd_count = new int[2] { 3, 3 };
		_br_rewardAd_ECPM_Value = new float[2] { 1.5f, 3f };
		_AF_InsertionADs = new int[28]
		{
			1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
			11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
			25, 30, 35, 40, 50, 80, 100, 150
		};
		_us_insertionAd_count = new int[3] { 12, 12, 12 };
		_us_insertionAd_ECPM_Value = new float[3] { 4f, 6f, 9f };
		_mx_insertionAd_count = new int[2] { 11, 11 };
		_mx_insertionAd_ECPM_Value = new float[2] { 1.5f, 2f };
		_br_insertionAd_count = new int[2] { 11, 11 };
		_br_insertionAd_ECPM_Value = new float[2] { 1f, 2f };
		_AF_Level = new int[45]
		{
			1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
			11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
			25, 30, 35, 40, 45, 50, 60, 70, 80, 90,
			100, 130, 150, 180, 200, 250, 300, 400, 500, 800,
			1000, 1200, 1500, 1800, 2100
		};
		_AF_LevelWin = new int[45]
		{
			1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
			11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
			25, 30, 35, 40, 45, 50, 60, 70, 80, 90,
			100, 130, 150, 180, 200, 250, 300, 400, 500, 800,
			1000, 1200, 1500, 1800, 2100
		};
		_AF_PropHintUse = new int[4] { 10, 15, 20, 25 };
		_AF_Revival = new int[4] { 10, 15, 20, 25 };
		_AF_Purchase_Product = new string[3]
		{
			"com.tilemaster3d.cn.google.big_bundle,com.tilemaster3d.cn.apple.big_bundle",
			"com.tilemaster3d.cn.google.noads,com.tilemaster3d.cn.apple.noads",
			"com.tilemaster3d.cn.google.gold_pig,com.tilemaster3d.cn.apple.gold_pig"
		};
		_mx_time = new int[3] { 10, 20, 40 };
		_us_time = new int[3] { 10, 20, 40 };
		_br_time = new int[3] { 10, 20, 40 };
		fUnitRevenueValue = new float[6] { 0.01f, 0.05f, 0.1f, 0.2f, 0.3f, 0.4f };
		sUnitRevenueValue = new string[6] { "001", "005", "01", "02", "03", "04" };
		_firebaseImpressionParam = new Dictionary<string, object>();
		_keyNullValuePairs = new Dictionary<string, object>();
		_firebasePurchase = new float[4] { 0.99f, 1f, 1.99f, 9.99f };
		_firebaseRevenueTidu001Key = "revenue_001_tidu_001";
		_firebaseRevenueTidu03Key = "revenue_01_tidu_03";
	}

	public void Mail_Click(string mailId)
	{
		Track("Mail_Click")
			.Add("Mail_ID", mailId)
			.Add("MailType", GameMail.MailType.ServerReward.ToString())
			.CommonAttr(false)
			.Send(false);
	}

	public void Mail_Receive(string mailId, string rewardContent)
	{
		Track("Mail_Receive")
			.Add("Mail_ID", mailId)
			.Add("MailType", GameMail.MailType.ServerReward.ToString())
			.Add("EmailContent", rewardContent)
			.CommonAttr(false)
			.Send(false);
	}

	public void Init()
	{
		ThinkingAnalyticsSDK.Initialize();
		ThinkingAnalyticsSDK.EnableAutoTrack();
	}

	public void SetSuperProperties(Dictionary<string, object> superProperties)
	{
		ThinkingAnalyticsSDK.SetSuperProperties(superProperties);
	}

	public void SetUserProperty(string name, string property)
	{
		FirebaseSDK.SetUserProperty(name, property);
	}

	public void TimeEvent(string key)
	{
		ThinkingAnalyticsSDK.TimeEvent(key);
	}

	public LogEvent Track(string key = "")
	{
		_keyValuePairs.Clear();
		_keySend = key;
		return _instance;
	}

	public LogEvent Add(string key, string value)
	{
		_keyValuePairs[key] = value;
		return _instance;
	}

	public LogEvent Add(string key, double? value)
	{
		_keyValuePairs[key] = value;
		return _instance;
	}

	public LogEvent Add(string key, long value)
	{
		_keyValuePairs[key] = value;
		return _instance;
	}

	public LogEvent Add(string key, int value)
	{
		_keyValuePairs[key] = value;
		return _instance;
	}

	public LogEvent Add(string key, float value)
	{
		_keyValuePairs[key] = value;
		return _instance;
	}

	public LogEvent Add(string key, bool value)
	{
		_keyValuePairs[key] = value;
		return _instance;
	}

	public LogEvent Add(string key, object value)
	{
		_keyValuePairs[key] = value;
		return _instance;
	}

	public void Send(bool b_apps_flyer_analytics = false)
	{
		ThinkingAnalyticsSDK.Track(_keySend, _keyValuePairs);
		if (b_apps_flyer_analytics)
		{
			AppsflyerSDK.SendEvent(_keySend, _keyValuePairs);
			if (!sFirebaseAnalyticsExclusions.Contains(_keySend))
			{
				FirebaseSDK.LogEvent(_keySend, _keyValuePairs);
			}
		}
	}

	public void SendFireBase()
	{
		FirebaseSDK.LogEvent(_keySend, _keyValuePairs);
	}

	public static void SendFireBase(string key, Dictionary<string, object> param)
	{
		FirebaseSDK.LogEvent(key, param);
	}

	public void SendAF()
	{
		AppsflyerSDK.SendEvent(_keySend, _keyValuePairs);
		if (!sFirebaseAnalyticsExclusions.Contains(_keySend))
		{
			FirebaseSDK.LogEvent(_keySend, _keyValuePairs);
		}
	}

	public void OnlySendAf()
	{
		AppsflyerSDK.SendEvent(_keySend, _keyValuePairs);
	}

	public void UserSet()
	{
		ThinkingAnalyticsSDK.UserSet(_keyValuePairs);
	}

	public void UserSetOnce()
	{
		ThinkingAnalyticsSDK.UserSetOnce(_keyValuePairs);
	}

	public void UserAdd()
	{
		ThinkingAnalyticsSDK.UserAdd(_keyValuePairs);
	}

	[Conditional("OPEN_TEST_BUTTON")]
	public static void LogTrack(string title, Dictionary<string, object> keyValuePairs)
	{
		string text = string.Concat("DataPoint  :  <color=red>", title, "</color>");
		if (keyValuePairs.Count > 0)
		{
			text += "  -->  ";
			foreach (KeyValuePair<string, object> keyValuePair in keyValuePairs)
			{
				if (keyValuePair.Value != null)
				{
					text = string.Concat(text, "[<b><color=yellow>", keyValuePair.Key, "</color></b> = <b><color=#00FFFF>", keyValuePair.Value.ToString(), "</color> </b>]");
				}
			}
		}
		UnityEngine.Debug.Log(text);
	}

	public static bool IsNewUserByVersion(string startVersion)
	{
		Version v1;
		Version v2;
		if (Version.TryParse(startVersion, out v1))
		{
			string input = DB.Account.NewUserVersion;
			if (Version.TryParse(input, out v2))
			{
				return v2 >= v1;
			}
		}
		return false;
	}

	private void TrackArrayAF(string key, int value, int[] array, Func<string, bool> callback = null)
	{
		Track("af_" + key).Add(key, value).SendAF();
		FacebookSDK.LogAppEvent("fb_" + key);
		if (Array.IndexOf(array, value) != -1)
		{
			Track(string.Format("af_{0}_{1}", key, value)).Add(key, value).SendAF();
			string text = string.Format("fb_{0}_{1}", key, value);
			if (callback == null || !callback(text))
			{
				FacebookSDK.LogAppEvent(text);
			}
		}
	}

	public void AF_AdRewardAD()
	{
		DB.Track.AfAdRewardAD++;
		TrackArrayAF("AdRewardAD", DB.Track.AfAdRewardAD, _AF_AdRewardAD);
	}

	public void AB_AdRewardAd(float value)
	{
		DB.Track.AfRewardEcpmALL += value * 1000f;
		float targetValue = DB.Track.AfRewardEcpmALL / (float)DB.Track.AfAdRewardAD;
		string region = GetRegion();
		UnityEngine.Debug.Log(string.Format("FacebookSDK AB_AdRewardAd country : {0} value: {1} avg:{2} AfAdRewardAD: {3}", region, value * 1000f, targetValue, DB.Track.AfAdRewardAD));
		if (_countryInfo.Contains(region))
		{
			string countryInfo = ((region == "US") ? "US" : ((region == "MX") ? "MX" : "BR"));
			int[] count = ((region == "US") ? _us_rewardAd_count : ((region == "MX") ? _mx_rewardAd_count : _br_rewardAd_count));
			float[] val = ((region == "US") ? _us_rewardAd_ECPM_Value : ((region == "MX") ? _mx_rewardAd_ECPM_Value : _br_rewardAd_ECPM_Value));
			SendAB_RewardAd(countryInfo, count, val, DB.Track.AfAdRewardAD, targetValue);
		}
	}

	private void SendAB_RewardAd(string countryInfo, int[] count, float[] value, int targetCount, float targetValue)
	{
		UnityEngine.Debug.Log(string.Format("FacebookSDK: SendAB_RewardAd : {0}", targetCount));
		for (int i = 0; i < count.Length; i++)
		{
			if (count[i] <= targetCount && value[i] <= targetValue)
			{
				FB_TryLog(string.Format("{0}_AD{1}_{2}", countryInfo, count[i], (int)value[i]));
			}
		}
	}

	public void AF_InsertionADs()
	{
		DB.Track.AfInsertionADs++;
		TrackArrayAF("InsertionADs", DB.Track.AfInsertionADs, _AF_InsertionADs);
		Track("").Add("AdsInterstitialNum", DB.Track.AfInsertionADs);
		UserSet();
	}

	public void AB_InsertionAd(float value)
	{
		DB.Track.AfInterEcpmALL += value * 1000f;
		float targetValue = DB.Track.AfInterEcpmALL / (float)DB.Track.AfInsertionADs;
		string region = GetRegion();
		UnityEngine.Debug.Log(string.Format("FacebookSDK AB_InsertionAd country : {0} value: {1} avg:{2} AfInsertionADs:{3}", region, value * 1000f, targetValue, DB.Track.AfInsertionADs));
		if (_countryInfo.Contains(region))
		{
			string countryInfo = ((region == "US") ? "US" : ((region == "MX") ? "MX" : "BR"));
			int[] count = ((region == "US") ? _us_insertionAd_count : ((region == "MX") ? _mx_insertionAd_count : _br_insertionAd_count));
			float[] val = ((region == "US") ? _us_insertionAd_ECPM_Value : ((region == "MX") ? _mx_insertionAd_ECPM_Value : _br_insertionAd_ECPM_Value));
			SendAB_InsertionAd(countryInfo, count, val, DB.Track.AfInsertionADs, targetValue);
		}
	}

	private void SendAB_InsertionAd(string countryInfo, int[] count, float[] value, int targetCount, float targetValue)
	{
		UnityEngine.Debug.Log(string.Format("FacebookSDK: SendAB_InsertionAd : {0}", targetCount));
		for (int i = 0; i < count.Length; i++)
		{
			if (count[i] <= targetCount && value[i] <= targetValue)
			{
				FB_TryLog(string.Format("{0}_INS{1}_{2}", countryInfo, count[i], (int)value[i]));
			}
		}
	}

	public void AF_Level(int value)
	{
		TrackArrayAF("Level", value, _AF_Level);
	}

	public void AF_LevelWin(int value)
	{
		TrackArrayAF("LevelWin", value, _AF_LevelWin);
	}

	public void AF_PropHintUse()
	{
		DB.Track.AfPropHintUse++;
		TrackArrayAF("hint", DB.Track.AfPropHintUse, _AF_PropHintUse);
	}

	public void AF_Revival()
	{
		DB.Track.AfRevival++;
		TrackArrayAF("revival", DB.Track.AfRevival, _AF_Revival);
	}

	public void AF_FirstLogin()
	{
		Track("af_First_Login").Add("First_Login", "").SendAF();
		FacebookSDK.LogAppEvent("fb_First_Login");
	}

	public void AF_Purchase(cfbt_Stores cfg)
	{
		if (Array.IndexOf(_AF_Purchase_Product, cfg.id) != -1)
		{
			string pid = Global.pStores.ProductId(cfg.id);
			int val = (int)(cfg.usd * 100f);
			Track(string.Format("af_{0}_{1}", pid, val)).Add(pid, val).SendAF();
			FacebookSDK.LogAppEvent(string.Format("fb_{0}_{1}", pid, val));
		}
	}

	public void UpdateJoinTime()
	{
		_joinTime = DateTime.Now;
	}

	public void TimeLogEvent()
	{
		DateTime now = DateTime.Now;
		DB.Track.GameTimes += (int)(now - _joinTime).TotalSeconds;
		_joinTime = now;
		string region = GetRegion();
		UnityEngine.Debug.Log("FacebookSDK country : " + region);
		if (_countryInfo.Contains(region))
		{
			SendTimeEvent(region, (region == "US") ? _us_time : ((region == "MX") ? _mx_time : _br_time));
		}
	}

	private void SendTimeEvent(string countryInfo, int[] times)
	{
		int mins = DB.Track.GameTimes / 60;
		UnityEngine.Debug.Log(string.Format("FacebookSDK SendTimeEvent : {0}", (float)mins));
		for (int i = 0; i < times.Length; i++)
		{
			if (times[i] <= mins)
			{
				FB_TryLog(string.Format("{0}_time_{1}", countryInfo, times[i]));
			}
		}
	}

	private void FB_TryLog(string key)
	{
		UnityEngine.Debug.Log("FacebookSDK FB_TryLog: " + key);
		if (!DB.Track.LogKeys.ContainsKey(key))
		{
			UnityEngine.Debug.Log(string.Concat("FacebookSDK FB_TryLog: ", key, " succ"));
			DB.Track.SetLogKeys(key, 1);
			FacebookSDK.LogAppEvent(key);
		}
	}

	private static string GetRegion()
	{
		string c = NativePlatform.Helper.GetCountryCode();
		if (string.IsNullOrEmpty(c))
		{
			c = RegionInfo.CurrentRegion.TwoLetterISORegionName;
		}
		return c.ToUpper();
	}

	public void AF_Media(string mediaSource)
	{
		Track("af_media").Add("media_source", mediaSource);
		OnlySendAf();
	}

	private string _Get_RewardedVideo_Place(string value)
	{
		if (!Enum.IsDefined(typeof(REWARD_PLACE), value) || value.Equals(((REWARD_PLACE)0).ToString()))
		{
			value = _reward_place.ToString();
		}
		return value;
	}

	public void Ads_Click(REWARD_PLACE placement)
	{
		_reward_place = placement;
		Track("Ads_Click").Ads_CommonAttr(placement).Send(false);
	}

	public void Ads_Return(REWARD_PLACE placement, string s_adv_name)
	{
		Track("Ads_Return").Add("AdsContent", s_adv_name).Ads_CommonAttr(placement).Send(false);
		Track("").Add("AdsNum", 1);
		UserAdd();
	}

	public void Ads_Suc(REWARD_PLACE placement)
	{
	}

	public void Ads_Suc(AdImpressionData data)
	{
		int count = 0;
		DB.Track.AdUnitDisplayCount.TryGetValue(data.AdUnitId, out count);
		DB.Track.SetAdUnitDisplayCount(data.AdUnitId, count + 1);
		Track("Ads_Suc")
			.Add("AdsType", data.Placement)
			.Add("adNetwork", data.Mediation)
			.Add("precision", data.Precision)
			.Add("value", data.Value)
			.Add("currency", data.CurrencyCode)
			.CommonAttr(false)
			.Add("OpenUi", SceneMgr.sCurSceneName)
			.Send(true);
		AF_AdRewardAD();
		Ads_UnitRevenue(data);
	}

	public void Ads_Interstitial(INTER_PLACE place)
	{
	}

	private string _Get_Interstitial_Place(string value)
	{
		if (!Enum.IsDefined(typeof(INTER_PLACE), value))
		{
			value = _inter_place.ToString();
		}
		return value;
	}

	public void Ads_Interstitial_Click(INTER_PLACE placement)
	{
		_inter_place = placement;
	}

	public void Ads_Interstitial(AdImpressionData data)
	{
		int count = 0;
		DB.Track.AdUnitDisplayCount.TryGetValue(data.AdUnitId, out count);
		DB.Track.SetAdUnitDisplayCount(data.AdUnitId, count + 1);
		DB.Other.InterstitialSucNum++;
		Track("Ads_Interstitial")
			.Add("AdsType", data.Placement)
			.Add("adNetwork", data.Mediation)
			.Add("precision", data.Precision)
			.Add("value", data.Value)
			.Add("currency", data.CurrencyCode)
			.CommonAttr(false)
			.Add("Times", DB.Other.InterstitialSucNum)
			.Send(true);
		AF_InsertionADs();
		Ads_UnitRevenue(data);
	}

	public void Ads_UnitRevenue(AdImpressionData data)
	{
		DB.Track.AdsRevenue += data.Value;
		if (DB.Track.AdsRevenueMax == 0.41f)
		{
			return;
		}
		if (DB.Track.AdsRevenueMax == 0f)
		{
			DB.Track.AdsRevenueMax = fUnitRevenueValue[0];
		}
		if (DB.Track.AdsRevenueMax <= DB.Track.AdsRevenue)
		{
			for (int num = fUnitRevenueValue.Length - 1; num >= 0; num--)
			{
				if (fUnitRevenueValue[num] <= DB.Track.AdsRevenue)
				{
					string key = "ads_revenue_" + sUnitRevenueValue[num];
					float num2 = fUnitRevenueValue[num];
					DB.Track.AdsRevenueMax = ((num != fUnitRevenueValue.Length - 1) ? fUnitRevenueValue[num + 1] : 0.41f);
					if (num2 > 0f)
					{
						Track(key).Add("value", num2).Add("currency", data.CurrencyCode).SendAF();
					}
					break;
				}
			}
		}
	}

	public void Ads_NoAdsOpen(string place)
	{
		Track("NoAds_Open").Add("Type", place).CommonAttr(true).Send(false);
	}

	public LogEvent Ads_CommonAttr(REWARD_PLACE placement)
	{
		Add("AdsType", placement.ToString());
		Add("OpenUI", SceneMgr.sCurSceneName);
		IsNew();
		return this;
	}

	public void Ads_Impression_FireBase(AdImpressionData data)
	{
		Ads_Total_Revenue_FireBase(data);
		if (data != null)
		{
			_firebaseImpressionParam.Clear();
			_firebaseImpressionParam.Add("ad_platform", "AppLovinMax");
			_firebaseImpressionParam.Add("ad_source", data.Mediation);
			_firebaseImpressionParam.Add("ad_unit_name", data.AdCreativeId);
			_firebaseImpressionParam.Add("ad_format", data.AdsType);
			_firebaseImpressionParam.Add("value", data.Value);
			_firebaseImpressionParam.Add("currency", "USD");
			_firebaseImpressionParam.Add("precision", data.Precision);
			FirebaseAdImpressionMultiple(data.Value, _firebaseImpressionParam);
			FirebaseAdTotalValueMultiple(data.Value, _firebaseImpressionParam);
			Firebase_RevenueAds(data);
			Firebase_PurchaseAds(data.Value, false);
			Firebase_RevenueTiDuAds(data);
		}
	}

	public void Ads_Impression_ThinkingAnalytics(AdImpressionData data)
	{
		if (data != null)
		{
			Track("Max_sdk_postbacks_local")
				.Add("ad_source", data.Mediation)
				.Add("ad_unit_name", data.AdCreativeId)
				.Add("ad_format", data.AdsType)
				.Add("value", data.Value)
				.Add("currency", "USD")
				.Add("precision", data.Precision)
				.Send(false);
		}
	}

	public void Ads_Total_Revenue_FireBase(AdImpressionData data)
	{
		if (data != null)
		{
			DB.Track.FirebaseTotalAdsRevenue += data.Value;
			if (!(DB.Track.FirebaseTotalAdsRevenue < 0.01f))
			{
				_keyValuePairs.Clear();
				_keyValuePairs.Add("value", DB.Track.FirebaseTotalAdsRevenue);
				_keyValuePairs.Add("currency", "USD");
				SendFireBase("Total_Ads_Revenue_001", _keyValuePairs);
				DB.Track.FirebaseTotalAdsRevenue = 0f;
			}
		}
	}

	public void Ads_Request(AdImpressionData data)
	{
		Track("Ads_Request")
			.Add("ad_format", data.AdsType)
			.Add("Mediation_Platform", data.GetAdPlatform())
			.Add("AdsType", data.Placement)
			.Add("ad_unit_id", data.AdUnitId)
			.Add("max_country_code", data.Country)
			.CommonAttr(false)
			.Send(false);
	}

	public void Ads_Response(AdImpressionData data)
	{
		Track("Ads_Response")
			.Add("ad_format", data.AdsType)
			.Add("Mediation_Platform", data.GetAdPlatform())
			.Add("ad_source", data.Mediation)
			.Add("value", data.Value)
			.Add("currency", data.CurrencyCode)
			.Add("ad_unit_id", data.AdUnitId)
			.Add("max_country_code", data.Country)
			.Add("Ads_Duration", data.LoadTime)
			.Add("precision", data.Precision)
			.Add("OpenUi", SceneMgr.sCurSceneName)
			.CommonAttr(false)
			.Send(false);
	}

	public void Click_ads(AdImpressionData data)
	{
		Track("Click_ads")
			.Add("ad_format", data.AdsType)
			.Add("Mediation_Platform", data.GetAdPlatform())
			.Add("AdsType", data.Placement)
			.Add("ad_source", data.Mediation)
			.Add("ad_unit_id", data.AdUnitId)
			.CommonAttr(false)
			.Send(false);
	}

	public void Ads_Fail(AdImpressionData data)
	{
		Track("Ads_Fail")
			.Add("ad_format", data.AdsType)
			.Add("ad_unit_id", data.AdUnitId)
			.Add("Error_Info", data.Error)
			.Add("max_country_code", data.Country)
			.Add("Ads_Duration", data.LoadTime)
			.Add("OpenUi", SceneMgr.sCurSceneName)
			.CommonAttr(false)
			.Send(false);
	}

	public void Ads_GetPrice(AdImpressionData data)
	{
		Track("Ads_GetPrice")
			.Add("ad_format", data.AdsType)
			.Add("ad_unit_id", data.AdUnitId)
			.Add("ad_source", data.Mediation)
			.Add("value", data.Value)
			.Add("currency", data.CurrencyCode)
			.CommonAttr(false)
			.Send(false);
	}

	public void Ads_ClearCache(AdImpressionData data)
	{
		Track("Ads_ClearCache")
			.Add("ad_format", data.AdsType)
			.Add("ad_unit_id", data.AdUnitId)
			.Add("ad_source", data.Mediation)
			.Add("value", data.Value)
			.Add("currency", data.CurrencyCode)
			.CommonAttr(false)
			.Send(false);
	}

	public void Ads_Native(AdImpressionData data)
	{
		int count = 0;
		DB.Track.AdUnitDisplayCount.TryGetValue(data.AdUnitId, out count);
		Track("Ads_Native")
			.Add("ad_format", data.AdsType)
			.Add("AdsType", data.Placement)
			.Add("AdFormat", data.PlayAdType.ToString())
			.Add("Ctype", data.PlayMode.ToString())
			.Add("unit_display_count", count)
			.Add("max_country_code", data.Country)
			.Add("ad_source", data.Mediation)
			.Add("precision", data.Precision)
			.Add("currency", data.CurrencyCode)
			.Add("value", data.Value)
			.Add("ad_unit_id", data.AdUnitId)
			.Add("ad_AspectRatio", data.AdAspectRatio)
			.CommonAttr(false)
			.Send(false);
	}

	public bool IsLogChannel()
	{
		return PlayerPrefs.GetInt("IsLogChannel", 0) == 1;
	}

	public void SetLogChannel()
	{
		PlayerPrefs.SetInt("IsLogChannel", 1);
	}

	public void SetChannelStartTime()
	{
		_logChannelStartTime = DateTime.Now;
		PlayerPrefs.SetInt("LogChannelOpenTimes", PlayerPrefs.GetInt("LogChannelOpenTimes", 0) + 1);
	}

	public void AddChannelWaitTime()
	{
		if (!IsLogChannel())
		{
			float waitTime = PlayerPrefs.GetFloat("ChannelWaitTime", 0f);
			PlayerPrefs.SetFloat("ChannelWaitTime", (float)((DateTime.Now - _logChannelStartTime).TotalSeconds + (double)waitTime));
		}
	}

	private int GetChannelWaitTime()
	{
		float waitTime = PlayerPrefs.GetFloat("ChannelWaitTime", 0f);
		return (int)((DateTime.Now - _logChannelStartTime).TotalSeconds + (double)waitTime);
	}

	private int GetNowChannelWaitTime()
	{
		return (int)(DateTime.Now - _logChannelStartTime).TotalSeconds;
	}

	private int GetChannelOpenTimes()
	{
		return PlayerPrefs.GetInt("LogChannelOpenTimes", 0);
	}

	public void LogChannel(string channel, string afId, string getType)
	{
		Track("Channel_Get")
			.Add("GetTiming", GetNowChannelWaitTime())
			.Add("GetTotalTiming", GetChannelWaitTime())
			.Add("GetStartTime", GetChannelOpenTimes())
			.Add("GetMediaSource", channel)
			.Add("AFid", afId)
			.Add("GetType", getType)
			.CommonAttr(false)
			.Send(false);
	}

	public bool IsLogCampaign()
	{
		return PlayerPrefs.GetInt("IsLogCampaign", 0) == 1;
	}

	public void SetLogCampaign()
	{
		PlayerPrefs.SetInt("IsLogCampaign", 1);
	}

	public void LogCampaign(string campaign, string afId, string getType)
	{
		Track("Campaign_Get")
			.Add("GetTiming", GetNowChannelWaitTime())
			.Add("GetTotalTiming", GetChannelWaitTime())
			.Add("GetStartTime", GetChannelOpenTimes())
			.Add("GetCampaign", campaign)
			.Add("AFid", afId)
			.Add("GetType", getType)
			.CommonAttr(false)
			.Send(false);
	}

	public void FaceBook_AdValueReport(AdImpressionData data)
	{
		if (data != null)
		{
			float value = data.Value;
			Facebook_AdAndIapValueReport(value);
			Facebook_AdValue2Iap(value);
			Facebook_AdImpression(data);
			string country;
			List<float> values = GetCountryAndValues(true, out country);
			if (values != null)
			{
				AdValueCheckSend(country.ToLower(), value, values);
			}
		}
	}

	public void Facebook_AdAndIapValueReport(float value)
	{
		string country;
		List<float> list = GetCountryAndValues(false, out country);
		if (list != null)
		{
			DB.FacebookAdValue.AdAndPuechaseCurrentValue += value;
			foreach (float num in list)
			{
				if (num <= DB.FacebookAdValue.AdAndPuechaseCurrentValue)
				{
					string text = string.Concat(country.ToLower(), "_iaa_iap_revenue", GetValueString(num));
					if (!DB.FacebookAdValue.CacheValue.Contains(text))
					{
						DB.FacebookAdValue.CacheValue.Add(text);
						UnityEngine.Debug.Log("FacebookLog_AdAndIapValueReport: " + text);
						FacebookSDK.LogAppEvent(text);
						AppsflyerSDK.SendEvent(text, _keyNullValuePairs);
					}
				}
			}
		}
	}

	private void AdValueCheckSend(string country, float addValue, List<float> values)
	{
		DB.FacebookAdValue.AdCurrentValue += addValue;
		for (int i = 0; i < values.Count; i++)
		{
			if (values[i] <= DB.FacebookAdValue.AdCurrentValue)
			{
				string text = string.Concat(country, "_ad_revenue", GetValueString(values[i]));
				if (!DB.FacebookAdValue.CacheValue.Contains(text))
				{
					DB.FacebookAdValue.CacheValue.Add(text);
					UnityEngine.Debug.Log(string.Format("FacebookLog_AdValueReport: {0} allValue: {1} addValue: {2}", text, DB.FacebookAdValue.AdCurrentValue, addValue));
					FacebookSDK.LogAppEvent(text);
					AppsflyerSDK.SendEvent(text, _keyNullValuePairs);
				}
			}
		}
	}

	private List<float> GetCountryAndValues(bool isAdValue, out string country)
	{
		country = GetRegion();
		string cur = country;
		FacebookAdValue facebookAdValue = DataCfg.FacebookAdValues.Find(x => x.country == cur);
		if (facebookAdValue == null)
		{
			UnityEngine.Debug.Log(string.Concat("FacebookLog_AdConfig [", isAdValue ? "adValue" : "adIapValue", "] no find this country: ", country));
			return null;
		}
		return isAdValue ? facebookAdValue.ad_values : facebookAdValue.ad_iap_values;
	}

	private string GetValueString(float value)
	{
		return value.ToString().Replace(".", "");
	}

	public void Facebook_AdValue2Iap(float addValue)
	{
		Unity_AdToIapValue(addValue);
		List<PointKey> list = new List<PointKey>();
		foreach (string text in cf_facebookAdValue2Iap_const.PointKey)
		{
			if (!string.IsNullOrEmpty(text))
			{
				string[] array = text.Split('|');
				if (array.Length == 2)
				{
					PointKey pointKey = new PointKey();
					pointKey.key = (float)Math.Round(float.Parse(array[0], CultureInfo.InvariantCulture), 1);
					pointKey.value = (float)Math.Round(float.Parse(array[1], CultureInfo.InvariantCulture), 1);
					UnityEngine.Debug.Log(string.Format("key:{0} va:{1}", pointKey.key, pointKey.key));
					list.Add(pointKey);
				}
			}
		}
		if (list.Count == 0)
		{
			UnityEngine.Debug.LogError(" Facebook_AdValue2IapConst PointKey is empty ");
			return;
		}
		int ver = (int)cf_facebookAdValue2Iap_const.fbAdPurchaseVersionNumber;
		if (DB.FacebookAdValue.FbLogVersion == ver)
		{
			if (DB.FacebookAdValue.Ad2IapValueKey <= 0f)
			{
				DB.FacebookAdValue.Ad2IapValueKey = list[0].key;
				DB.FacebookAdValue.Ad2IapValueDelta = list[0].value;
			}
		}
		else
		{
			DB.FacebookAdValue.FbLogVersion = ver;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].key > DB.FacebookAdValue.Ad2IapValue)
				{
					DB.FacebookAdValue.Ad2IapValueKey = list[i].key;
					DB.FacebookAdValue.Ad2IapValueDelta = list[i].value;
					break;
				}
			}
		}
		DB.FacebookAdValue.Ad2IapValue += addValue;
		if (DB.FacebookAdValue.Ad2IapValue >= DB.FacebookAdValue.Ad2IapValueKey)
		{
			FacebookSDK.LogAdValue2Purchase(DB.FacebookAdValue.Ad2IapValueKey, DB.FacebookAdValue.Ad2IapValue * DB.FacebookAdValue.Ad2IapValueDelta);
			UnityEngine.Debug.Log(string.Format("Facebook_AdValue2Iap Ad2IapValueKey:{0} value:{1}", DB.FacebookAdValue.Ad2IapValueKey, DB.FacebookAdValue.Ad2IapValue * DB.FacebookAdValue.Ad2IapValueDelta));
			DB.FacebookAdValue.Ad2IapValue = 0f;
			int num = list.Select(x => x.key).ToList().IndexOf(DB.FacebookAdValue.Ad2IapValueKey);
			if (num < 0)
			{
				DB.FacebookAdValue.Ad2IapValueKey += cf_facebookAdValue2Iap_const.KeyDelta;
				DB.FacebookAdValue.Ad2IapValueDelta = cf_facebookAdValue2Iap_const.valueDelta;
			}
			else if (num + 1 < cf_facebookAdValue2Iap_const.PointKey.Count)
			{
				DB.FacebookAdValue.Ad2IapValueKey = list[num + 1].key;
				DB.FacebookAdValue.Ad2IapValueDelta = list[num + 1].value;
			}
			else
			{
				DB.FacebookAdValue.Ad2IapValueKey += cf_facebookAdValue2Iap_const.KeyDelta;
				DB.FacebookAdValue.Ad2IapValueDelta = cf_facebookAdValue2Iap_const.valueDelta;
			}
			UnityEngine.Debug.Log(string.Format("Facebook_AdValue2Iap new value:{0}", DB.FacebookAdValue.Ad2IapValueKey));
		}
	}

	public void Facebook_AdImpression(AdImpressionData impressionData)
	{
		if (IsNewUserByVersion(cf_facebookAdValue2Iap_const.fbAdImpressionVerisonNumber))
		{
			DB.FacebookAdValue.AdImpression += impressionData.Value;
			if (DB.FacebookAdValue.AdImpression >= cf_facebookAdValue2Iap_const.fbAdImpressionResetValue)
			{
				float adImpression = DB.FacebookAdValue.AdImpression;
				DB.FacebookAdValue.AdImpression = 0f;
				FacebookSDK.LogAdValueImpression(adImpression);
			}
		}
	}

	public void Facebook_Purchase_Impression(cfbt_Stores cfg)
	{
		if (IsNewUserByVersion(cf_facebookAdValue2Iap_const.fbAdImpressionVerisonNumber))
		{
			FacebookSDK.LogAdValueImpression(cfg.usd);
		}
	}

	public void Firebase_Purchase(float usd)
	{
		FirebasePurchaseMultiple(usd);
		Firebase_PurchaseAds(usd, true);
	}

	public void FirebasePurchaseMultiple(float value)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		foreach (string text in cf_firebase_const.purchase_multiple)
		{
			string[] array = text.Split('|');
			dictionary["value"] = float.Parse(array[1]) * value;
			dictionary["currency"] = "USD";
			SendFireBase(array[0], dictionary);
		}
	}

	public void FirebaseAdImpressionMultiple(float value, Dictionary<string, object> param)
	{
		foreach (string text in cf_firebase_const.ad_impression_multiple)
		{
			string[] array = text.Split('|');
			param["value"] = float.Parse(array[1]) * value;
			SendFireBase(array[0], param);
		}
	}

	public void FirebaseAdTotalValueMultiple(float value, Dictionary<string, object> param)
	{
		foreach (string text in cf_firebase_const.ad_total_value_multiple)
		{
			string[] array = text.Split('|');
			float cur = 0f;
			DB.FirebaseAdValue.KeyAdTotalValue.TryGetValue(array[0], out cur);
			cur += value;
			if (cur >= float.Parse(array[1]))
			{
				param["value"] = float.Parse(array[2]) * cur;
				SendFireBase(array[0], param);
				DB.FirebaseAdValue.SetKeyAdTotalValue(array[0], 0f);
			}
			else
			{
				DB.FirebaseAdValue.SetKeyAdTotalValue(array[0], cur);
			}
		}
	}

	public void Firebase_RevenueAds(AdImpressionData impressionData)
	{
		if (IsAnyOfMediaSource(cf_firebase_const.ad_revenue_source_media))
		{
			foreach (KeyValuePair<string, float> item in cf_firebase_const.ad_revenue_main)
			{
				float cur = 0f;
				DB.FirebaseAdValue.KeyAdRevenueValue.TryGetValue(item.Key, out cur);
				cur += impressionData.Value;
				if (cur >= item.Value)
				{
					Dictionary<string, object> param = new Dictionary<string, object>
					{
						{ "currency", "USD" },
						{ "value", cur }
					};
					SendFireBase(item.Key, param);
					DB.FirebaseAdValue.SetKeyAdRevenueValue(item.Key, 0f);
				}
				else
				{
					DB.FirebaseAdValue.SetKeyAdRevenueValue(item.Key, cur);
				}
			}
		}
	}

	public void Firebase_PurchaseAds(float usd, bool isPurchased)
	{
		if (IsAnyOfMediaSource(cf_firebase_const.ad_purchase_revenue_source_media))
		{
			foreach (KeyValuePair<string, float> item in cf_firebase_const.ad_purchase_revenue_main)
			{
				float cur = 0f;
				DB.FirebaseAdValue.KeyAdPurchaseRevenueValue.TryGetValue(item.Key, out cur);
				cur += usd;
				if (cur >= item.Value)
				{
					Dictionary<string, object> param = new Dictionary<string, object>
					{
						{ "currency", "USD" },
						{ "value", cur }
					};
					SendFireBase(item.Key, param);
					DB.FirebaseAdValue.SetKeyAdPurchaseRevenueValue(item.Key, 0f);
				}
				else
				{
					DB.FirebaseAdValue.SetKeyAdPurchaseRevenueValue(item.Key, cur);
				}
			}
		}
	}

	public void Firebase_RevenueTiDuAds(AdImpressionData impressionData)
	{
		if (IsNewUserByVersion(cf_firebase_const.ad_revenue_tidu_version))
		{
			Firebase_RevenueTiDuAds001(impressionData);
			Firebase_RevenueTiDuAds03(impressionData);
		}
	}

	private void Firebase_RevenueTiDuAds001(AdImpressionData impressionData)
	{
		DB.FirebaseAdValue.KeyAdRevenueTiDu001Value += impressionData.Value;
		if (DB.FirebaseAdValue.KeyAdRevenueTiDu001Value >= cf_firebase_const.ad_revenue_001_tidu_001_start + cf_firebase_const.ad_revenue_001_tidu_001_interval * (float)DB.FirebaseAdValue.KeyAdRevenueTiDu001Index)
		{
			Dictionary<string, object> param = new Dictionary<string, object>
			{
				{ "currency", "USD" },
				{ "value", DB.FirebaseAdValue.KeyAdRevenueTiDu001Value }
			};
			SendFireBase(_firebaseRevenueTidu001Key, param);
			DB.FirebaseAdValue.KeyAdRevenueTiDu001Index++;
			DB.FirebaseAdValue.KeyAdRevenueTiDu001Value = 0f;
		}
	}

	private void Firebase_RevenueTiDuAds03(AdImpressionData impressionData)
	{
		DB.FirebaseAdValue.KeyAdRevenueTiDu03Value += impressionData.Value;
		if (DB.FirebaseAdValue.KeyAdRevenueTiDu03Value >= cf_firebase_const.ad_revenue_01_tidu_03_start + cf_firebase_const.ad_revenue_01_tidu_03_interval * (float)DB.FirebaseAdValue.KeyAdRevenueTiDu03Index)
		{
			Dictionary<string, object> param = new Dictionary<string, object>
			{
				{ "currency", "USD" },
				{ "value", DB.FirebaseAdValue.KeyAdRevenueTiDu03Value }
			};
			SendFireBase(_firebaseRevenueTidu03Key, param);
			DB.FirebaseAdValue.KeyAdRevenueTiDu03Index++;
			DB.FirebaseAdValue.KeyAdRevenueTiDu03Value = 0f;
		}
	}

	private bool IsAnyOfMediaSource(List<string> mediaSourceList)
	{
		string media = FQDev.UserLabel.UserLabelGetters.GetMediaSource ?? "";
		return mediaSourceList.Any(x => media.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0);
	}

	public void HomeButtonClick(string type)
	{
		Track("Home_Button_Click").Add("Type", type).CommonAttr(true).Send(false);
	}

	public void HomeIconClick(string type)
	{
		Track("Home_Icon_Click").Add("Type", type).CommonAttr(true).Send(false);
	}

	public void HomeFrameOpen(string name, string openType)
	{
		Track("Home_Frame_Open").Add("Name", name).Add("OpenType", openType).CommonAttr(true).Send(false);
	}

	public void Level_Start(LevelType type, bool isRepeat, bool isPinnacleLevel)
	{
		string text = (isRepeat ? "Continue" : "New");
		if (type == LevelType.BakingMaster)
		{
			text = "Cakechallenge";
		}
		if (isPinnacleLevel)
		{
			text = "SummitShowdown";
		}
		Track("Level_Start")
			.Add("LevelType", text)
			.Add("DifficultyLevel", DB.User.DynamicDifficultyLevel)
			.Add("WinStreak", DB.Track.ConsecutiveVictory)
			.Add("IsPinnacleLevel", isPinnacleLevel)
			.Add("IsCakeLevel", type == LevelType.BakingMaster)
			.CommonAttr(false)
			.Send(true);
		TimeEvent("Level_Quit");
		TimeEvent("Level_Victory");
	}

	public void Level_Quit(LevelType type, bool isPinnacleLevel)
	{
		Track("Level_Quit")
			.Add("DifficultyLevel", DB.User.DynamicDifficultyLevel)
			.Add("WinStreak", DB.Track.ConsecutiveVictory)
			.Add("IsPinnacleLevel", isPinnacleLevel)
			.Add("IsCakeLevel", type == LevelType.BakingMaster)
			.CommonAttr(false)
			.Send(false);
	}

	public void Level_Victory(LevelType type, int ModelNum, int ModelResidual, bool isPinnacleLevel)
	{
		int time = GameTimeTriggerMono.I.GetTime("Level");
		GameTimeTriggerMono.I.Clear("Level");
		Track("Level_Victory")
			.Add("ModelNum", ModelNum)
			.Add("ModelResidual", ModelResidual)
			.Add("MeanTime", (float)time / (float)ModelNum)
			.Add("StarNum", DB.User.SumStar)
			.Add("ItemHint", DB.Bag.GetUseNum((PropID)10))
			.Add("ItemFreeze", DB.Bag.GetUseNum((PropID)11))
			.Add("ItemBack", DB.Bag.GetUseNum((PropID)14))
			.Add("ItemRefresh", DB.Bag.GetUseNum((PropID)15))
			.Add("DifficultyLevel", DB.User.DynamicDifficultyLevel)
			.Add("WinStreak", DB.Track.ConsecutiveVictory)
			.Add("IsPinnacleLevel", isPinnacleLevel)
			.Add("IsCakeLevel", type == LevelType.BakingMaster)
			.CommonAttr(false)
			.Send(false);
		Track("").Add("MaxLevel", DB.User.Maxlv + 1);
		UserSet();
	}

	public void Level_Fail(LevelType type, int RemainTile, int ModelNum, int ModelResidual, bool isTimeOut, bool isPinnacleLevel)
	{
		int time = GameTimeTriggerMono.I.GetTime("Level");
		Track("Level_Fail")
			.Add("Type", isTimeOut ? "Time" : "Box")
			.Add("duration", time)
			.Add("RemainTile", RemainTile)
			.Add("ModelNum", ModelNum)
			.Add("ModelResidual", ModelResidual)
			.Add("ItemHint", DB.Bag.GetUseNum((PropID)10))
			.Add("ItemFreeze", DB.Bag.GetUseNum((PropID)11))
			.Add("ItemBack", DB.Bag.GetUseNum((PropID)14))
			.Add("ItemRefresh", DB.Bag.GetUseNum((PropID)15))
			.Add("DifficultyLevel", DB.User.DynamicDifficultyLevel)
			.Add("WinStreak", DB.Track.ConsecutiveVictory)
			.Add("IsPinnacleLevel", isPinnacleLevel)
			.Add("IsCakeLevel", type == LevelType.BakingMaster)
			.CommonAttr(false)
			.Send(false);
	}

	public void LevelBox_Reward(string rewardType, int boxLevel, string item)
	{
		Track("LevelBox_Reward")
			.Add("RewardType", rewardType)
			.Add("BoxLevel", boxLevel)
			.Add("TaskItem", item)
			.CommonAttr(false)
			.Send(false);
	}

	public void ModelBox_Reward(string rewardType, int boxModel, string item)
	{
		Track("ModelBox_Reward")
			.Add("RewardType", rewardType)
			.Add("BoxModel", boxModel)
			.Add("TaskItem", item)
			.CommonAttr(false)
			.Send(false);
	}

	public void Deduct_Energy(int value)
	{
		Track("Deduct_Energy").Add("Remaining_energy", value).CommonAttr(false).Send(false);
	}

	public void NoAds_Banner_Buy()
	{
		Track("NoAds_Banner_Buy").CommonAttr(false).Send(false);
	}

	public void Level_Revive()
	{
		Track("Level_Revive").Add("DifficultyLevel", DB.User.DynamicDifficultyLevel).CommonAttr(false).Send(false);
	}

	public void Level_GamePause()
	{
		Track("Game_Pause").CommonAttr(false).Send(false);
	}

	public void Level_GamePauseClose(string s_param)
	{
		Track("Game_Pause_Close").Add("Type", s_param).CommonAttr(false).Send(false);
	}

	public LogEvent CommonAttr(bool b_max = false)
	{
		Add("Level_ID", (b_max ? DB.User.Maxlv : DB.User.Curlv) + 1);
		Add("IsNew", DB.Track.OpenAppCount == 1);
		return _instance;
	}

	public LogEvent Level(bool b_max = false)
	{
		Add("Level_ID", (b_max ? DB.User.Maxlv : DB.User.Curlv) + 1);
		return _instance;
	}

	public LogEvent IsNew()
	{
		Add("IsNew", DB.Track.OpenAppCount == 1);
		return _instance;
	}

	public void Peak_Match_Event_Open()
	{
		RegionRankActivityData selfData = RegionRankActivityManager.ins.GetSelfData();
		Track("Peak_Match_Event_Open")
			.Add("LevelGroup", selfData.LevelGroup)
			.Add("LevelOrder", selfData.LevelOrderIndex + 1)
			.Add("Issue", selfData.Issue)
			.Add("Region", RegionRankActivityManager.ins.GetRegionCode())
			.CommonAttr(false)
			.Send(false);
	}

	public void Peak_Match_Leaderboard_Display(RegionRankType rankType, int rank)
	{
		RegionRankActivityData selfData = RegionRankActivityManager.ins.GetSelfData();
		Track("Peak_Match_Leaderboard_Display")
			.Add("LevelGroup", selfData.LevelGroup)
			.Add("LevelOrder", selfData.LevelOrderIndex + 1)
			.Add("Issue", selfData.Issue)
			.Add("Region", RegionRankActivityManager.ins.GetRegionCode())
			.Add("Score", selfData.AddScore)
			.Add("NowWinStreak", selfData.WinningStreak)
			.Add("HistorieWinStreak", selfData.WinningStreakBest)
			.Add("LeaderboardType", (int)rankType)
			.Add("Rank", rank)
			.CommonAttr(false)
			.Send(false);
	}

	public void Peak_Match_Event_End(int personalRank, int regionalRank)
	{
		RegionRankActivityData selfData = RegionRankActivityManager.ins.GetSelfData();
		Track("Peak_Match_Event_End")
			.Add("LevelGroup", selfData.LevelGroup)
			.Add("LevelOrder", selfData.LevelOrderIndex + 1)
			.Add("Issue", selfData.Issue)
			.Add("Region", RegionRankActivityManager.ins.GetRegionCode())
			.Add("Score", selfData.AddScore)
			.Add("NowWinStreak", selfData.WinningStreak)
			.Add("HistorieWinStreak", selfData.WinningStreakBest)
			.Add("PersonalRank", personalRank)
			.Add("RegionalRank", regionalRank)
			.CommonAttr(false)
			.Send(false);
	}

	private void _SmallGames_Start(int modelNum, string targetNum, string gameType)
	{
		Track("SmallGames_Start")
			.Add("GameType", gameType)
			.Add("ModelNum", modelNum)
			.Add("TargetNum", targetNum)
			.CommonAttr(false)
			.Send(false);
	}

	public void SmallGames_Start_RubikCube(int modelNum)
	{
		_SmallGames_Start(modelNum, "", "Rubik'sCube");
	}

	public void SmallGames_Start_LinkDots(List<TargetData> targets)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (targets.Count >= 1)
		{
			for (int i = 0; i < targets.Count; i++)
			{
				if (i != 0)
				{
					stringBuilder.Append("|");
				}
				stringBuilder.Append(targets[i].ModelId);
				stringBuilder.Append(string.Format(",{0}", targets[i].Count));
			}
		}
		_SmallGames_Start(0, stringBuilder.ToString(), "LinkDots");
	}

	private void _SmallGames_End(string track, string type, int modelNum, int modelResidual, string targetNum, string targetResidual, string gameType)
	{
		Track(track)
			.Add("GameType", gameType)
			.Add("Type", type)
			.Add("ModelNum", modelNum)
			.Add("ModelResidual", modelResidual)
			.Add("TargetNum", targetNum)
			.Add("TargetResidual", targetResidual)
			.CommonAttr(false)
			.Send(false);
	}

	private void _SmallGames_SetTargetResidual(IReadOnlyList<TargetData> targets, int cur, ref StringBuilder sbTarget, ref StringBuilder sbResidual)
	{
		if (targets.Count < 1)
		{
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			if (i != 0)
			{
				sbTarget.Append("|");
			}
			sbTarget.Append(targets[i].ModelId);
			sbTarget.Append(string.Format(",{0}", targets[i].Count));
			if (cur < i)
			{
				sbResidual.Append("|");
			}
			if (cur <= i)
			{
				sbResidual.Append(targets[i].ModelId);
				sbResidual.Append(string.Format(",{0}", targets[i].Count));
			}
		}
	}

	public void SmallGames_Quit_RubikCube(int modelNum, int modelResidual, string type)
	{
		_SmallGames_End("SmallGames_Quit", type, modelNum, modelResidual, "", "", "Rubik'sCube");
	}

	public void SmallGames_Quit_LinkDots(List<TargetData> targets, int cur, string type)
	{
		StringBuilder sbTarget = new StringBuilder();
		StringBuilder sbResidual = new StringBuilder();
		_SmallGames_SetTargetResidual(targets, cur, ref sbTarget, ref sbResidual);
		_SmallGames_End("SmallGames_Quit", type, 0, 0, sbTarget.ToString(), sbResidual.ToString(), "LinkDots");
	}

	public void SmallGames_Fail_RubikCube(int modelNum, int modelResidual, string type)
	{
		_SmallGames_End("SmallGames_Fail", type, modelNum, modelResidual, "", "", "Rubik'sCube");
	}

	public void SmallGames_Fail_LinkDots(List<TargetData> targets, int cur, string type)
	{
		StringBuilder sbTarget = new StringBuilder();
		StringBuilder sbResidual = new StringBuilder();
		_SmallGames_SetTargetResidual(targets, cur, ref sbTarget, ref sbResidual);
		_SmallGames_End("SmallGames_Fail", type, 0, 0, sbTarget.ToString(), sbResidual.ToString(), "LinkDots");
	}

	public void SetTeamPosition(TeamPosition position = TeamPosition.None)
	{
		if (position != 0)
		{
			DB.Track.TeamPosition = position.ToString();
			Track("").Add("TeamStatus", DB.Track.TeamPosition);
			UserSet();
		}
	}

	public void TeamCreate(string teamId)
	{
		Track("Team_Create").Add("Team_ID", teamId).CommonAttr(true).Send(false);
	}

	public void TeamIn(string teamId, string type)
	{
		Track("Team_In").Add("Team_ID", teamId).Add("Type", type).CommonAttr(true).Send(false);
	}

	public void TeamLifeRequest(string teamId)
	{
		Track("Life_request").Add("Team_ID", teamId).Add("LifeNum", DB.Bag.GetProp(PropID.Energy)).CommonAttr(true).Send(false);
	}

	public void TeamLifeHelp(string teamId, int HelpNum)
	{
		Track("Life_Help").Add("Team_ID", teamId).Add("LifeNum", DB.Bag.GetProp(PropID.Energy)).Add("HelpNum", HelpNum).CommonAttr(true).Send(false);
	}

	public void TeamLifeReceive(string teamId)
	{
		DB.Track.TeamLifeReceive++;
		Track("Life_Receive").Add("Team_ID", teamId).Add("LifeNum", DB.Bag.GetProp(PropID.Energy)).Add("ReceiveNum", DB.Track.TeamLifeReceive).CommonAttr(true).Send(false);
	}

	public void AdLuckySpin_Show()
	{
		Track("AdLuckySpin_Show").CommonAttr(true).Send(false);
	}

	public void AdLuckySpin_Reward(int[,] rewards)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < rewards.GetLength(0); i++)
		{
			stringBuilder.Append(string.Format("{0}|{1},", rewards[i, 0], rewards[i, 1]));
		}
		Track("AdLuckySpin_Reward")
			.Add("TaskItem", stringBuilder.ToString())
			.Add("SpinNum", TurntableManager.I.RewardCount)
			.CommonAttr(true)
			.Send(false);
	}

	public void AdRewards_Show()
	{
		Track("AdRewards_Show").CommonAttr(true).Send(false);
	}

	public void AdRewards_Reward(int[,] rewards)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < rewards.GetLength(0); i++)
		{
			stringBuilder.Append(string.Format("{0}|{1},", rewards[i, 0], rewards[i, 1]));
		}
		Track("AdRewards_Reward")
			.Add("TaskItem", stringBuilder.ToString())
			.Add("AdNum", RewardAdView.RewardAdViewManager.I.GetCurrentCount())
			.CommonAttr(true)
			.Send(false);
	}

	private void Unity_AdToIapValue(float revenue)
	{
		if (DB.UnityAdValue.Ad2IapValueKey <= 0f)
		{
			DB.UnityAdValue.Ad2IapValueKey = cf_facebookAdValue2Iap_const.Unity_PointKey[0];
		}
		DB.UnityAdValue.Ad2IapValue += revenue;
		if (DB.UnityAdValue.Ad2IapValue < DB.UnityAdValue.Ad2IapValueKey)
		{
			return;
		}
		string text = string.Concat("[{\"id\": \"", DB.UnityAdValue.Ad2IapValueKey.ToString(), "\", \"quantity\": 1, \"item_price\": ", DB.UnityAdValue.Ad2IapValue.ToString(), "}]");
		Track("Unity_IAA_purchase")
			.Add("content_type", "ad_product")
			.Add("currency", "USD")
			.Add("value", DB.UnityAdValue.Ad2IapValue)
			.Add("content", text)
			.Send(false);
		UnityEngine.Debug.Log(string.Format("Unity_IAA_purchase Key: {0} value: {1}", DB.UnityAdValue.Ad2IapValueKey, DB.UnityAdValue.Ad2IapValue));
		DB.UnityAdValue.Ad2IapValue = 0f;
		int num = -1;
		float num2 = float.MaxValue;
		for (int i = 0; i < cf_facebookAdValue2Iap_const.Unity_PointKey.Count; i++)
		{
			float num3 = Mathf.Abs(DB.UnityAdValue.Ad2IapValueKey - cf_facebookAdValue2Iap_const.Unity_PointKey[i]);
			if (num3 < num2)
			{
				num2 = num3;
				num = i;
			}
		}
		if (num < 0 || num + 1 >= cf_facebookAdValue2Iap_const.Unity_PointKey.Count)
		{
			DB.UnityAdValue.Ad2IapValueKey += cf_facebookAdValue2Iap_const.Unity_KeyDelta;
		}
		else
		{
			DB.UnityAdValue.Ad2IapValueKey = cf_facebookAdValue2Iap_const.Unity_PointKey[num + 1];
		}
	}

	public void Custom_Attpop_Show()
	{
		RegionRankActivityManager.ins.GetSelfData();
		Track("Custom_Attpop_Show").Add("region", UserLabelGetCountry.Nation).CommonAttr(false).Send(false);
	}

	public void Custom_Attpop(string msg)
	{
		RegionRankActivityManager.ins.GetSelfData();
		Track("Custom_Attpop").Add("DataType", msg).CommonAttr(false).Send(false);
	}
}
