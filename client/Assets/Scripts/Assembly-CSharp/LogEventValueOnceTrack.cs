using System;
using System.Collections.Generic;
using UnityEngine;

public class LogEventValueOnceTrack : Singleton<LogEventValueOnceTrack>
{
	private const int FacebookType = 1;

	private const int FirebaseType = 2;

	private const int AppsflyerType = 3;

	private Version _saveVersion;

	public int LoginDay;

	private List<CfgValueOnceTrack> _fbPoints = new List<CfgValueOnceTrack>();

	private List<CfgValueOnceTrack> _firebasePoints = new List<CfgValueOnceTrack>();

	private List<CfgValueOnceTrack> _afPoints = new List<CfgValueOnceTrack>();

	private Dictionary<string, object> _keyValuePairs = new Dictionary<string, object>();

	public void InitData()
	{
		if (DB.Track.OpenAppCount == 1)
		{
			Debug.LogError("Set LogVersion");
			DB.ValueOnceTrack.LogVersion = Application.version;
		}
		_saveVersion = new Version(DB.ValueOnceTrack.LogVersion);
		CalculateLoginDays();
		Debug.LogError(string.Format("OpenAppCount: {0}, LogVersion:{1}", DB.Track.OpenAppCount, DB.ValueOnceTrack.LogVersion));
		foreach (CfgValueOnceTrack cfgValueOnceTrack in DataCfg.CfgValueOnceTracks)
		{
			if ((string.IsNullOrEmpty(cfgValueOnceTrack.Version) || !(_saveVersion < new Version(cfgValueOnceTrack.Version))) && (cfgValueOnceTrack.DayBefore < 0 || LoginDay <= cfgValueOnceTrack.DayBefore) && cfgValueOnceTrack.Platform < 2)
			{
				if (cfgValueOnceTrack.SdkType == 1)
				{
					if (!DB.ValueOnceTrack.FbLogNames.ContainsKey(cfgValueOnceTrack.EventName))
					{
						_fbPoints.Add(cfgValueOnceTrack);
					}
				}
				else if (cfgValueOnceTrack.SdkType == 2)
				{
					if (!DB.ValueOnceTrack.FirebaseLogNames.ContainsKey(cfgValueOnceTrack.EventName))
					{
						_firebasePoints.Add(cfgValueOnceTrack);
					}
				}
				else if (cfgValueOnceTrack.SdkType == 3 && !DB.ValueOnceTrack.AfLogNames.ContainsKey(cfgValueOnceTrack.EventName))
				{
					_afPoints.Add(cfgValueOnceTrack);
				}
			}
		}
		CheckAll();
	}

	private void CalculateLoginDays()
	{
		if (string.IsNullOrEmpty(DB.ValueOnceTrack.FirstLoginTime))
		{
			DB.ValueOnceTrack.FirstLoginTime = DateTime.Now.ToStringFull();
			LoginDay = 0;
			return;
		}
		DateTime byString = MDateTime.GetByString(DB.ValueOnceTrack.FirstLoginTime, "yyyyMMddHHmmss", true);
		int num = (int)(DateTime.Now.TotalDays() - byString.TotalDays());
		LoginDay = num;
		Debug.Log(string.Format("CalculateLoginDays, LoginDays:{0}", num));
	}

	public void Trigger(ValueOnceTrackTriggerType type, float value)
	{
		if (type == ValueOnceTrackTriggerType.LevelFail)
		{
			DB.ValueOnceTrack.LevelFailCount++;
		}
		else if (type == ValueOnceTrackTriggerType.AdReward)
		{
			DB.ValueOnceTrack.AdRewardCount++;
			DB.ValueOnceTrack.IaaValue += value;
		}
		else if (type == ValueOnceTrackTriggerType.AdInter)
		{
			DB.ValueOnceTrack.AdInterCount++;
			DB.ValueOnceTrack.IaaValue += value;
		}
		else if (type == ValueOnceTrackTriggerType.AdBanner)
		{
			DB.ValueOnceTrack.AdBannerCount++;
			DB.ValueOnceTrack.IaaValue += value;
		}
		CheckAll();
	}

	private void CheckAll()
	{
		CalculateLoginDays();
		Check(ref _fbPoints, 1);
		Check(ref _firebasePoints, 2);
		Check(ref _afPoints, 3);
	}

	private void Check(ref List<CfgValueOnceTrack> points, int sdkType)
	{
		float payTotalMoney = DB.Track.PayTotalMoney;
		float iaaValue = DB.ValueOnceTrack.IaaValue;
		int curlv = DB.User.Curlv;
		int levelFailCount = DB.ValueOnceTrack.LevelFailCount;
		int adRewardCount = DB.ValueOnceTrack.AdRewardCount;
		int adInterCount = DB.ValueOnceTrack.AdInterCount;
		int adBannerCount = DB.ValueOnceTrack.AdBannerCount;
		for (int i = points.Count - 1; i >= 0; i--)
		{
			CfgValueOnceTrack cfgValueOnceTrack = points[i];
			if ((cfgValueOnceTrack.DayBefore < 0 || LoginDay <= cfgValueOnceTrack.DayBefore) && cfgValueOnceTrack.IapValue <= payTotalMoney && cfgValueOnceTrack.IaaValue <= iaaValue && cfgValueOnceTrack.IapIaaValue <= payTotalMoney + iaaValue && cfgValueOnceTrack.Level <= curlv && cfgValueOnceTrack.LevelFailCount <= levelFailCount && cfgValueOnceTrack.AdReward <= adRewardCount && cfgValueOnceTrack.AdInter <= adInterCount && cfgValueOnceTrack.AdBanner <= adBannerCount && cfgValueOnceTrack.AdReward_Inter_Banner <= adBannerCount + adInterCount + adRewardCount && cfgValueOnceTrack.AdReward_Inter <= adInterCount + adRewardCount && cfgValueOnceTrack.AdReward_Banner <= adBannerCount + adRewardCount && cfgValueOnceTrack.AdInter_Banner <= adBannerCount + adInterCount)
			{
				string eventName = cfgValueOnceTrack.EventName;
				if (sdkType == 3)
				{
					AppsflyerSDK.SendEvent(eventName, _keyValuePairs);
					DB.ValueOnceTrack.SetAfLogNames(eventName, 1);
				}
				else if (sdkType == 2)
				{
					FirebaseSDK.LogEvent(eventName, _keyValuePairs);
					DB.ValueOnceTrack.SetFirebaseLogNames(eventName, 1);
				}
				else if (sdkType == 1)
				{
					FacebookSDK.LogAppEvent(eventName);
					DB.ValueOnceTrack.SetFbLogNames(eventName, 1);
				}
				points.RemoveAt(i);
			}
		}
	}
}
