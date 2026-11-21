using System;

namespace Function_Reward_Video
{
	public abstract class BaseRewardVideoFeature : IRewardVideoCDFeature
	{
		protected cfgLevelRewardVideo _config;

		public abstract RewardCDType Type { get; }

		public abstract void UpdateData();

		public virtual void TryToReset()
		{
			DateTime today = DateTime.Today;
			RewardVideosData rewardVideosData = DB.RewardVideo.RewardVideosData;
			if (string.IsNullOrEmpty(rewardVideosData.LastRefreshTime))
			{
				ResetData();
			}
			if ((today - today.ParseFull(rewardVideosData.LastRefreshTime)).Days != 0)
			{
				ResetData();
			}
		}

		protected void ResetData()
		{
			RewardVideosData rewardVideosData = DB.RewardVideo.RewardVideosData;
			rewardVideosData.RewardVideosGear.Clear();
			rewardVideosData.RewardVideosLookedCount.Clear();
			rewardVideosData.RewardVideosCoolingStartTime.Clear();
			rewardVideosData.LastRefreshTime = DateTime.Today.ToStringFull();
			rewardVideosData.RewardVideosGear[_config.PointType] = 0;
			rewardVideosData.RewardVideosLookedCount[_config.PointType] = 0;
			rewardVideosData.SingleLevel_CurLevelFailedCount = 0;
			rewardVideosData.SingleLevel_DailyCurrentLevelLookAdCount.Clear();
			rewardVideosData.SingleLevel_PerEnterLookAdCount.Clear();
			DB.RewardVideo.SaveData();
		}

		public abstract RewardVideoData GetData();

		public virtual void AddCount(int count)
		{
			int value = 0;
			int pointType = _config.PointType;
			RewardVideosData rewardVideosData = DB.RewardVideo.RewardVideosData;
			rewardVideosData.RewardVideosLookedCount.TryGetValue(pointType, out value);
			value += count;
			rewardVideosData.RewardVideosLookedCount[pointType] = value;
			DB.RewardVideo.SaveData();
		}

		public void SetConfig(cfgLevelRewardVideo config)
		{
			_config = config;
		}

		public bool IsOpen()
		{
			return _config.IsOpen == 1;
		}

		public bool IsUnlock()
		{
			if (_config.CooldownType == (int)Type)
			{
				if (_config.IsOpen == 0)
				{
					return false;
				}
				if (IsLevelUnlock() && !IsPaidLimit())
				{
					return !IsPlayDaysLimit();
				}
			}
			return false;
		}

		public bool IsLevelUnlock()
		{
			return _config.UnlockLevel <= DB.User.Maxlv + 1;
		}

		private bool IsPaidLimit()
		{
			if (_config.IsPaid == 1 && DB.User.IsPayingUsers)
			{
				return false;
			}
			if (_config.IsPaid == 0 && !DB.User.IsPayingUsers)
			{
				return false;
			}
			return _config.IsPaid >= 0;
		}

		private bool IsPlayDaysLimit()
		{
			if (_config.UnlockPlayDaysRange == null || _config.UnlockPlayDaysRange.Count < 2)
			{
				return false;
			}
			int playDays = DB.Track.PlayDays;
			int num = _config.UnlockPlayDaysRange[0];
			int num2 = _config.UnlockPlayDaysRange[1];
			if (playDays < num)
			{
				return true;
			}
			if (num2 == -1)
			{
				return false;
			}
			return num2 < playDays;
		}
	}
}
