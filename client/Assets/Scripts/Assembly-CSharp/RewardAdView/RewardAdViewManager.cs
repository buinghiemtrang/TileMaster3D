using System;

namespace RewardAdView
{
	public class RewardAdViewManager : Singleton<RewardAdViewManager>
	{
		public DateTime EndTime;

		public bool IsOpen => cf_accumulatedAd_const.IsOpen == 1 && IsUnlockLevel();

		public void RefreshData()
		{
			CheckSameDay();
		}

		public void CheckSameDay()
		{
			if (DB.RewardAdView.Data.LastDate > 0)
			{
				DateTime dateTimeBySeconds = MDateTime.GetDateTimeBySeconds(DB.RewardAdView.Data.LastDate);
				bool flag = DateTime.Now.Date == dateTimeBySeconds.Date;
				EndTime = DateTime.Today.AddSeconds(cf_accumulatedAd_const.ContinueTime);
				if (flag)
				{
					return;
				}
			}
			DB.RewardAdView.Data.SeenCount = 0;
			DB.RewardAdView.Data.HasDailyShowPop = false;
			DB.RewardAdView.Data.ReceivedRewardId.Clear();
			DB.RewardAdView.Data.LastDate = DateTime.Now.TotalSeconds();
			DB.RewardAdView.Save();
			LogEvent.I.AdRewards_Show();
		}

		public bool HasPop()
		{
			return DB.RewardAdView.Data.HasDailyShowPop;
		}

		public bool HasReward(int index)
		{
			if (index < 0 || index >= DataCfg.cfgAccumlatedAdRewards.Count)
			{
				return false;
			}
			return DB.RewardAdView.Data.ReceivedRewardId.Contains(DataCfg.cfgAccumlatedAdRewards[index].id);
		}

		public cfgAccumlatedAdReward GetReward(int index)
		{
			if (index >= 0 && index < DataCfg.cfgAccumlatedAdRewards.Count)
			{
				return DataCfg.cfgAccumlatedAdRewards[index];
			}
			return null;
		}

		public int[,] GetRewardByIndex(int index)
		{
			if (index < 0 || index >= DataCfg.cfgAccumlatedAdRewards.Count)
			{
				return new int[0, 0];
			}
			return DataCfg.cfgAccumlatedAdRewards[index].rewards;
		}

		public void PopDialog(Action callback = null)
		{
			RewardAdViewDialog.Create(callback);
		}

		public int GetCurrentCount()
		{
			return DB.RewardAdView.Data.SeenCount;
		}

		public int GetMaxCount()
		{
			return DataCfg.cfgAccumlatedAdRewards.Last().count;
		}

		private void AddCount()
		{
			DB.RewardAdView.Data.SeenCount++;
		}

		public void SeeRewardAd(Action<bool> callback)
		{
			Global.ShowRewardedVideo(delegate(string info)
			{
				if (info == "succ")
				{
					AddCount();
					TryToGetReward();
					callback?.Invoke(true);
				}
				else
				{
					Global.ShowMsgTip("Language135");
					callback?.Invoke(false);
				}
			}, REWARD_PLACE.ADDailyView);
		}

		public void TryToGetReward()
		{
			if (DB.RewardAdView.Data.ReceivedRewardId.Count >= DataCfg.cfgAccumlatedAdRewards.Count)
			{
				return;
			}
			int currentCount = GetCurrentCount();
			for (int i = 0; i < DataCfg.cfgAccumlatedAdRewards.Count; i++)
			{
				cfgAccumlatedAdReward cfgAccumlatedAdReward = DataCfg.cfgAccumlatedAdRewards[i];
				if (currentCount >= cfgAccumlatedAdReward.count && !DB.RewardAdView.Data.ReceivedRewardId.Contains(cfgAccumlatedAdReward.id))
				{
					DB.RewardAdView.Data.ReceivedRewardId.Add(cfgAccumlatedAdReward.id);
					Global.CreateReward(cfgAccumlatedAdReward.rewards, RewardType.None, REWARD_PLACE.ADDailyView, "", false, false);
					LogEvent.I.AdRewards_Reward(cfgAccumlatedAdReward.rewards);
					DB.RewardAdView.Save();
				}
			}
		}

		private bool IsUnlockLevel()
		{
			return DB.User.Maxlv + 1 >= cf_accumulatedAd_const.UnlockLevel;
		}

		public int GetLeftTime()
		{
			return (int)(EndTime - DateTime.Now).TotalSeconds;
		}
	}
}
