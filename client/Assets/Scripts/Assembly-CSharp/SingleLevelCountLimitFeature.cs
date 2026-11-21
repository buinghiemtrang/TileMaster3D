using System;
using Function_Reward_Video;

public class SingleLevelCountLimitFeature : BaseRewardVideoFeature
{
	public override RewardCDType Type => RewardCDType.Counter;

	public override void UpdateData()
	{
		RewardVideosData rewardVideosData = DB.RewardVideo.RewardVideosData;
		if (_config != null && rewardVideosData != null && !rewardVideosData.RewardVideosLookedCount.ContainsKey(_config.id))
		{
			rewardVideosData.RewardVideosLookedCount[_config.id] = 0;
			rewardVideosData.RewardVideosGear[_config.id] = 0;
			DB.RewardVideo.SaveData();
		}
	}

	public override RewardVideoData GetData()
	{
		int value = 0;
		int id = _config.id;
		RewardVideosData rewardVideosData = DB.RewardVideo.RewardVideosData;
		rewardVideosData.RewardVideosLookedCount.TryGetValue(id, out value);
		int preEnterAdCountLimit = (int)Math.Min((uint)_config.SingleLevel_PreEnterAdCountLimit, 2147483647u);
		int failCountLimit = (int)Math.Min((uint)_config.SingleLevel_FailCountLimit, 2147483647u);
		int adCountLimit = (int)Math.Min((uint)_config.SingleLevel_AdCountLimit, 2147483647u);
		int lookCountDailyMax = (int)Math.Min((uint)_config.LookCountDailyMax, 2147483647u);
		if (!rewardVideosData.SingleLevel_DailyCurrentLevelLookAdCount.ContainsKey(id))
		{
			rewardVideosData.SingleLevel_DailyCurrentLevelLookAdCount.Add(id, 0);
		}
		if (!rewardVideosData.SingleLevel_PerEnterLookAdCount.ContainsKey(id))
		{
			rewardVideosData.SingleLevel_PerEnterLookAdCount.Add(id, 0);
		}
		if (rewardVideosData.SingleLevel_CurLevelFailedCount > failCountLimit || rewardVideosData.SingleLevel_DailyCurrentLevelLookAdCount[id] >= adCountLimit || rewardVideosData.SingleLevel_PerEnterLookAdCount[id] >= preEnterAdCountLimit || value >= lookCountDailyMax)
		{
			return new RewardVideoData(RewardVideoState.Closed, 0, 0);
		}
		int num = ((_config.LookCountDailyMax >= 1) ? Math.Max(_config.LookCountDailyMax - value, 0) : (-1));
		if (_config.SingleLevel_AdCountLimit >= 1)
		{
			num = CheckCount(num, _config.SingleLevel_AdCountLimit - rewardVideosData.SingleLevel_DailyCurrentLevelLookAdCount[id]);
		}
		if (_config.SingleLevel_PreEnterAdCountLimit >= 1)
		{
			num = CheckCount(num, _config.SingleLevel_PreEnterAdCountLimit - rewardVideosData.SingleLevel_PerEnterLookAdCount[id]);
		}
		return new RewardVideoData(RewardVideoState.Open, 0, (num > 0) ? num : (-1));
	}

	private int CheckCount(int count, int value)
	{
		if (value > 0)
		{
			if (count >= 0)
			{
				return Math.Min(count, value);
			}
			return value;
		}
		return count;
	}

	public override void AddCount(int count)
	{
		base.AddCount(count);
		if (_config != null)
		{
			int id = _config.id;
			RewardVideosData rewardVideosData = DB.RewardVideo.RewardVideosData;
			int value = 0;
			rewardVideosData.SingleLevel_PerEnterLookAdCount.TryGetValue(id, out value);
			value++;
			rewardVideosData.SingleLevel_PerEnterLookAdCount[id] = value;
			rewardVideosData.SingleLevel_DailyCurrentLevelLookAdCount.TryGetValue(id, out value);
			value++;
			rewardVideosData.SingleLevel_DailyCurrentLevelLookAdCount[id] = value;
			DB.RewardVideo.SaveData();
		}
	}
}
