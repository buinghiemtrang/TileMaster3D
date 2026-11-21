using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public static class RegionRankServerUtils
{
	public static void UpdatePersonalData(ref RegionRankClientData data, RegionRankClientItemData selfData)
	{
		if (selfData.Score == 0)
		{
			return;
		}
		RegionRankClientItemData regionRankClientItemData = data.List.Find((RegionRankClientItemData a) => a.Key == selfData.Key);
		if (regionRankClientItemData != null)
		{
			regionRankClientItemData.Score = selfData.Score;
			data.List.Sort((RegionRankClientItemData a, RegionRankClientItemData b) => -a.Score.CompareTo(b.Score));
			data.SelfRank = data.List.IndexOf(regionRankClientItemData) + 1;
		}
	}

	public static void UpdateNationalData(ref RegionRankClientData data, RegionRankClientItemData selfData)
	{
		if (selfData.Score == 0)
		{
			return;
		}
		RegionRankClientItemData regionRankClientItemData = data.List.Find((RegionRankClientItemData a) => a.Key == selfData.Key);
		if (regionRankClientItemData != null)
		{
			regionRankClientItemData.Score += selfData.Score;
			data.List.Sort((RegionRankClientItemData a, RegionRankClientItemData b) => -a.Score.CompareTo(b.Score));
			data.SelfRank = data.List.IndexOf(regionRankClientItemData) + 1;
		}
	}

	public static RegionRankClientData ParseData(RegionRankServerData serverData)
	{
		RegionRankClientData regionRankClientData = new RegionRankClientData();
		regionRankClientData.List = new List<RegionRankClientItemData>();
		regionRankClientData.NextRefreshAt = serverData.nextRefreshAt;
		regionRankClientData.SelfRank = serverData.selfRank;
		regionRankClientData.Total = serverData.total;
		foreach (RegionRankServerItemData item in serverData.list)
		{
			RegionRankClientItemData regionRankClientItemData = new RegionRankClientItemData();
			regionRankClientItemData.Key = item.key;
			regionRankClientItemData.Score = item.score;
			if (!string.IsNullOrEmpty(item.data))
			{
				regionRankClientItemData.Data = JsonConvert.DeserializeObject<RegionRankExtraData>(item.data);
			}
			regionRankClientData.List.Add(regionRankClientItemData);
		}
		return regionRankClientData;
	}

	public static void GMClearServerData()
	{
		DateTime dateTime = ServerDateTimeMgr.I.NetTimeUtc;
		dateTime = dateTime.AddDays(-31.0);
		DateTime time = dateTime.AddSeconds(-1.0);
		Debug.LogError("GMClearServerData: " + time.ToString());
		// UNVERIFIED: the original probably passed GetTimeStamp(time) to GMSetExpire (empty body, call removed by the compiler); only the GetTimeStamp call is visible in the machine code.
		GSingleton<RegionRankActivityManager>.ins.GetTimeStamp(time);
	}

	private static void GMSetExpire(string rankType, long expireAt)
	{
	}

	public static void GMUploadScore(int num, int scoreMax)
	{
	}
}
