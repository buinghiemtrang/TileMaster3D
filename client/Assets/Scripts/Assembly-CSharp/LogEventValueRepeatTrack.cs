using System;
using System.Collections.Generic;
using UnityEngine;

public class LogEventValueRepeatTrack : Singleton<LogEventValueRepeatTrack>
{
	private const int FacebookType = 1;

	private const int FirebaseType = 2;

	private const int AppsflyerType = 3;

	private Version _saveVersion;

	public int LoginDay;

	private List<CfgValueRepeatTrack> _fbPoints = new List<CfgValueRepeatTrack>();

	private List<CfgValueRepeatTrack> _firebasePoints = new List<CfgValueRepeatTrack>();

	private List<CfgValueRepeatTrack> _afPoints = new List<CfgValueRepeatTrack>();

	private Dictionary<string, object> _keyValuePairs = new Dictionary<string, object>();

	private bool _canTrackCum = true;

	public void InitData()
	{
		if (DB.Track.OpenAppCount == 1)
		{
			Debug.LogError("RepeatTrack Set LogVersion");
			DB.ValueRepeatTrack.LogVersion = Application.version;
		}
		_saveVersion = new Version(DB.ValueRepeatTrack.LogVersion);
		if (DB.ValueRepeatTrack.CumRevenueValue <= 0f)
		{
			DB.ValueRepeatTrack.CumRevenueValue = cf_firebase_const.cum_rev_startValue;
		}
		if (!string.IsNullOrEmpty(cf_firebase_const.cum_version) && _saveVersion < new Version(cf_firebase_const.cum_version))
		{
			_canTrackCum = false;
		}
		CalculateLoginDays();
		Debug.LogError(string.Format("RepeatTrack OpenAppCount: {0}, LogVersion:{1}", DB.Track.OpenAppCount, DB.ValueRepeatTrack.LogVersion));
		foreach (CfgValueRepeatTrack cfgValueRepeatTrack in DataCfg.CfgValueRepeatTracks)
		{
			if ((string.IsNullOrEmpty(cfgValueRepeatTrack.Version) || !(_saveVersion < new Version(cfgValueRepeatTrack.Version))) && (cfgValueRepeatTrack.FirstSevenDays <= 0 || LoginDay <= 6 || DB.ValueRepeatTrack.SevenPlayDays >= cfgValueRepeatTrack.FirstSevenDays) && cfgValueRepeatTrack.Platform < 2)
			{
				if (cfgValueRepeatTrack.SdkType == 1)
				{
					_fbPoints.Add(cfgValueRepeatTrack);
				}
				else if (cfgValueRepeatTrack.SdkType == 3)
				{
					_afPoints.Add(cfgValueRepeatTrack);
				}
				else if (cfgValueRepeatTrack.SdkType == 2)
				{
					_firebasePoints.Add(cfgValueRepeatTrack);
				}
			}
		}
		CheckAll(ValueTrackTriggerType.SevenPlayDays, 0f);
	}

	private void CalculateLoginDays()
	{
		if (string.IsNullOrEmpty(DB.ValueRepeatTrack.FirstLoginTime))
		{
			DB.ValueRepeatTrack.FirstLoginTime = DateTime.Now.ToStringFull();
			DB.ValueRepeatTrack.SevenPlayDays++;
			DB.ValueRepeatTrack.BeforePlayDay = 0;
			LoginDay = 0;
			return;
		}
		DateTime byString = MDateTime.GetByString(DB.ValueRepeatTrack.FirstLoginTime, "yyyyMMddHHmmss", true);
		LoginDay = (int)(DateTime.Now.TotalDays() - byString.TotalDays());
		if (DB.ValueRepeatTrack.BeforePlayDay != LoginDay)
		{
			DB.ValueRepeatTrack.BeforePlayDay = LoginDay;
			if (LoginDay < 8)
			{
				DB.ValueRepeatTrack.SevenPlayDays++;
			}
		}
		Debug.Log(string.Format("RepeatTrack CalculateLoginDays, SevenPlayDays:{0}", DB.ValueRepeatTrack.SevenPlayDays));
	}

	public void Trigger(ValueTrackTriggerType type, float value)
	{
		if (type == ValueTrackTriggerType.LevelPass)
		{
			DB.ValueRepeatTrack.LevelPassCount = DB.User.Curlv;
		}
		else if (type == ValueTrackTriggerType.AdReward)
		{
			DB.ValueRepeatTrack.AdRewardCount++;
			DB.ValueRepeatTrack.IaaValue += value;
		}
		else if (type == ValueTrackTriggerType.AdInter)
		{
			DB.ValueRepeatTrack.AdInterCount++;
			DB.ValueRepeatTrack.IaaValue += value;
		}
		else if (type == ValueTrackTriggerType.AdBanner)
		{
			DB.ValueRepeatTrack.AdBannerCount++;
			DB.ValueRepeatTrack.IaaValue += value;
		}
		CheckAll(type, value);
	}

	private void CheckAll(ValueTrackTriggerType type, float value)
	{
		CalculateLoginDays();
		Check(type, value, _fbPoints, 1);
		Check(type, value, _firebasePoints, 2);
		Check(type, value, _afPoints, 3);
		CheckCum_revenue(type, value);
	}

	private void Check(ValueTrackTriggerType triggerType, float value, List<CfgValueRepeatTrack> points, int sdkType)
	{
		float iaaValue = DB.ValueRepeatTrack.IaaValue;
		int level = DB.ValueRepeatTrack.LevelPassCount;
		int sevenPlayDays = DB.ValueRepeatTrack.SevenPlayDays;
		int adReward = DB.ValueRepeatTrack.AdRewardCount;
		int adInter = DB.ValueRepeatTrack.AdInterCount;
		int adBanner = DB.ValueRepeatTrack.AdBannerCount;
		for (int i = points.Count - 1; i >= 0; i--)
		{
			CfgValueRepeatTrack cfg = points[i];
			_keyValuePairs.Clear();
			_keyValuePairs.Add("currency", "USD");
			if (triggerType != ValueTrackTriggerType.Purchase || cfg.IsPurchaseTrack == 1)
			{
				string eventName = cfg.EventName;
				if (sdkType == 3)
				{
					if (triggerType != ValueTrackTriggerType.Purchase)
					{
						float currentVal = 0f;
						DB.ValueRepeatTrack.AfLogNames.TryGetValue(eventName, out currentVal);
						currentVal += value;
						if (CheckCondition(cfg, currentVal, sevenPlayDays, level, adReward, adInter, adBanner))
						{
							AppsflyerSDK.SendEvent(eventName, _keyValuePairs);
							DB.ValueRepeatTrack.SetAfLogNames(eventName, 0f);
						}
						else
						{
							DB.ValueRepeatTrack.SetAfLogNames(eventName, currentVal);
						}
					}
					else
					{
						AppsflyerSDK.SendEvent(eventName, _keyValuePairs);
					}
				}
				else if (sdkType == 2)
				{
					if (triggerType != ValueTrackTriggerType.Purchase)
					{
						float currentVal2 = 0f;
						DB.ValueRepeatTrack.FirebaseLogNames.TryGetValue(eventName, out currentVal2);
						currentVal2 += value;
						if (CheckCondition(cfg, currentVal2, sevenPlayDays, level, adReward, adInter, adBanner))
						{
							_keyValuePairs.Add("value", currentVal2);
							FirebaseSDK.LogEvent(eventName, _keyValuePairs);
							DB.ValueRepeatTrack.SetFirebaseLogNames(eventName, 0f);
						}
						else
						{
							DB.ValueRepeatTrack.SetFirebaseLogNames(eventName, currentVal2);
						}
					}
					else
					{
						_keyValuePairs.Add("value", value);
						FirebaseSDK.LogEvent(eventName, _keyValuePairs);
					}
				}
				else if (sdkType == 1)
				{
					float currentVal3 = 0f;
					DB.ValueRepeatTrack.FbLogNames.TryGetValue(eventName, out currentVal3);
					currentVal3 += value;
					if (CheckCondition(cfg, currentVal3, sevenPlayDays, level, adReward, adInter, adBanner))
					{
						FacebookSDK.LogAppEvent(eventName);
						DB.ValueRepeatTrack.SetFbLogNames(eventName, 0f);
					}
					else
					{
						DB.ValueRepeatTrack.SetFbLogNames(eventName, currentVal3);
					}
				}
			}
		}
	}

	private bool CheckCondition(CfgValueRepeatTrack cfg, float value, int sevenPlayDays, int level, int adReward, int adInter, int adBanner)
	{
		return cfg.IaaValue <= value && (cfg.FirstSevenDays < 0 || cfg.FirstSevenDays <= sevenPlayDays) && cfg.Level <= level && cfg.AdReward <= adReward && cfg.AdInter <= adInter && cfg.AdBanner <= adBanner;
	}

	private void CheckCum_revenue(ValueTrackTriggerType type, float value)
	{
		if (!_canTrackCum)
		{
			return;
		}
		if (type != ValueTrackTriggerType.Purchase && type != ValueTrackTriggerType.AdReward && type != ValueTrackTriggerType.AdInter && type != ValueTrackTriggerType.AdBanner)
		{
			return;
		}
		if (DB.ValueRepeatTrack.CumRevenueValue > cf_firebase_const.cum_rev_max)
		{
			return;
		}
		string cum_log_name = cf_firebase_const.cum_log_name;
		_keyValuePairs.Clear();
		_keyValuePairs.Add("currency", "USD");
		if (type == ValueTrackTriggerType.Purchase)
		{
			_keyValuePairs.Add("value", value);
			AppsflyerSDK.SendEvent(cum_log_name, _keyValuePairs);
			FirebaseSDK.LogEvent(cum_log_name, _keyValuePairs);
			return;
		}
		if (DB.ValueRepeatTrack.IaaValue <= DB.ValueRepeatTrack.CumRevenueValue)
		{
			return;
		}
		_keyValuePairs.Add("value", DB.ValueRepeatTrack.IaaValue);
		AppsflyerSDK.SendEvent(cum_log_name, _keyValuePairs);
		FirebaseSDK.LogEvent(cum_log_name, _keyValuePairs);
		DB.ValueRepeatTrack.IaaValue = 0f;
		if (DB.ValueRepeatTrack.CumRevenueValue >= cf_firebase_const.cum_rev_changeValue)
		{
			if (DB.ValueRepeatTrack.CumRevenueValue < cf_firebase_const.cum_rev_max)
			{
				DB.ValueRepeatTrack.CumRevenueValue += cf_firebase_const.cum_rev_changeAddValue;
				if (DB.ValueRepeatTrack.CumRevenueValue >= cf_firebase_const.cum_rev_max)
				{
					DB.ValueRepeatTrack.CumRevenueValue = cf_firebase_const.cum_rev_max;
				}
			}
			else
			{
				DB.ValueRepeatTrack.CumRevenueValue += cf_firebase_const.cum_rev_changeAddValue;
			}
		}
		else
		{
			DB.ValueRepeatTrack.CumRevenueValue += cf_firebase_const.cum_rev_addValue;
		}
	}
}
