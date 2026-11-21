using System;
using Function_Reward_Video;

public class CDLimitFeature : BaseRewardVideoFeature
{
	public override RewardCDType Type => RewardCDType.Timer;

	private int GetIndex(int gear)
	{
		return Math.Min(gear, _config.CD_GearAdCount.Count - 1);
	}

	public override void UpdateData()
	{
		var rewardVideosData = DB.RewardVideo.RewardVideosData;
		int pointType = _config.PointType;
		bool flag = false;
		if (!rewardVideosData.RewardVideosLookedCount.ContainsKey(pointType))
		{
			rewardVideosData.RewardVideosLookedCount[pointType] = 0;
			rewardVideosData.RewardVideosGear[pointType] = 0;
			flag = true;
		}
		int value = 0;
		rewardVideosData.RewardVideosGear.TryGetValue(pointType, out value);
		int value2 = 0;
		rewardVideosData.RewardVideosLookedCount.TryGetValue(pointType, out value2);
		int index = GetIndex(value);
		if (_config.CD_GearAdCount[index] - value2 < 1)
		{
			if (!rewardVideosData.RewardVideosCoolingStartTime.ContainsKey(pointType))
			{
				rewardVideosData.RewardVideosCoolingStartTime[pointType] = DateTime.Now.ToStringFull();
				flag = true;
			}
			int remainingTime = GetRemainingTime(_config, index);
			if (remainingTime <= 0)
			{
				rewardVideosData.RewardVideosGear[pointType]++;
				rewardVideosData.RewardVideosLookedCount[pointType] = 0;
				rewardVideosData.RewardVideosCoolingStartTime.Remove(pointType);
				flag = true;
			}
		}
		else if (rewardVideosData.RewardVideosCoolingStartTime.ContainsKey(pointType))
		{
			rewardVideosData.RewardVideosCoolingStartTime.Remove(pointType);
			flag = true;
		}
		if (flag)
		{
			DB.RewardVideo.SaveData();
		}
	}

	public override void AddCount(int count)
	{
		base.AddCount(count);
		if (_config != null)
		{
			int pointType = _config.PointType;
			var rewardVideosData = DB.RewardVideo.RewardVideosData;
			int value = 0;
			rewardVideosData.RewardVideosGear.TryGetValue(pointType, out value);
			int index = GetIndex(value);
			int value2 = 0;
			rewardVideosData.RewardVideosLookedCount.TryGetValue(pointType, out value2);
			if (_config.CD_GearAdCount[index] <= value2)
			{
				rewardVideosData.RewardVideosCoolingStartTime[pointType] = DateTime.Now.ToStringFull();
			}
			DB.RewardVideo.SaveData();
		}
	}

	public override RewardVideoData GetData()
	{
		int pointType = _config.PointType;
		var rewardVideosData = DB.RewardVideo.RewardVideosData;
		int value = 0;
		rewardVideosData.RewardVideosLookedCount.TryGetValue(pointType, out value);
		int value2 = 0;
		rewardVideosData.RewardVideosGear.TryGetValue(pointType, out value2);
		int index = GetIndex(value2);
		int num = _config.CD_GearAdCount[index];
		if (num < 1)
		{
			return new RewardVideoData(RewardVideoState.Closed, 0, 0);
		}
		int num2 = num - value;
		if (num2 < 1)
		{
			int remainingTime = GetRemainingTime(_config, index);
			RewardVideoState state = ((remainingTime < 1) ? RewardVideoState.Open : RewardVideoState.Cooling);
			return new RewardVideoData(state, remainingTime, 0);
		}
		return new RewardVideoData(RewardVideoState.Open, 0, Math.Max(0, num2));
	}

	private int GetRemainingTime(cfgLevelRewardVideo config, int coolingTimeIndex)
	{
		DateTime now = DateTime.Now;
		var rewardVideosData = DB.RewardVideo.RewardVideosData;
		string timeInfo = null;
		rewardVideosData.RewardVideosCoolingStartTime.TryGetValue(config.PointType, out timeInfo);
		DateTime dateTime = now.ParseFull(timeInfo, true);
		DateTime dateTime2 = dateTime.AddSeconds(config.CD_GearCD[coolingTimeIndex]);
		DateTime dateTime3 = DateTime.Today.AddDays(1.0);
		if (dateTime2 <= dateTime3)
		{
			dateTime3 = dateTime2;
		}
		return Convert.ToInt32((dateTime3 - now).TotalSeconds);
	}
}
