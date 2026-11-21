using System.Collections.Generic;
using GameScene;

namespace Function_Reward_Video
{
	public class RewardVideoManager : Singleton<RewardVideoManager>
	{
		private int CurrentLevelAdCount;

		private List<IRewardVideoCDFeature> _features;

		public void InitData()
		{
			_features = new List<IRewardVideoCDFeature>();
			_features.Add(new CDLimitFeature());
			_features.Add(new SingleLevelCountLimitFeature());
		}

		private void ClearListen()
		{
			DispatcherMgr.Off<GameStatus>("GameStatus_Change", OnGameStatusChange);
		}

		public void AddListen()
		{
			ClearListen();
			DispatcherMgr.On<GameStatus>("GameStatus_Change", OnGameStatusChange);
		}

		public void ResetPreEnterData()
		{
			RewardVideosData rewardVideosData = DB.RewardVideo.RewardVideosData;
			rewardVideosData.SingleLevel_PerEnterLookAdCount.Clear();
			rewardVideosData.SingleLevel_CurLevelFailedCount = 0;
			DB.RewardVideo.SaveData();
		}

		private void OnGameStatusChange(GameStatus status)
		{
			RewardVideosData rewardVideosData = DB.RewardVideo.RewardVideosData;
			switch (status)
			{
			case GameStatus.Victory:
				rewardVideosData.SingleLevel_CurLevelFailedCount = 0;
				rewardVideosData.SingleLevel_DailyCurrentLevelLookAdCount.Clear();
				break;
			case GameStatus.Failure:
				rewardVideosData.SingleLevel_CurLevelFailedCount++;
				break;
			}
			DB.RewardVideo.SaveData();
		}

		private IRewardVideoCDFeature GetFeature(cfgLevelRewardVideo config)
		{
			return _features.Find((IRewardVideoCDFeature feature) => feature.Type == (RewardCDType)config.CooldownType);
		}

		public bool IsOpen(PointType type)
		{
			List<cfgLevelRewardVideo> configs = GetConfigs(type);
			for (int i = 0; i < configs.Count; i++)
			{
				IRewardVideoCDFeature feature = GetFeature(configs[i]);
				if (feature == null)
				{
					return false;
				}
				feature.SetConfig(configs[i]);
				if (feature.IsUnlock())
				{
					return true;
				}
			}
			return false;
		}

		public RewardVideoData GetAdData(PointType type)
		{
			IRewardVideoCDFeature handle = GetHandle(type);
			if (handle == null)
			{
				return new RewardVideoData(RewardVideoState.Closed, 0, 0);
			}
			handle.TryToReset();
			handle.UpdateData();
			return handle.GetData();
		}

		private List<cfgLevelRewardVideo> GetConfigs(PointType type)
		{
			List<cfgLevelRewardVideo> list = new List<cfgLevelRewardVideo>();
			foreach (cfgLevelRewardVideo cfgLevelRewardVideo in DataCfg.cfgLevelRewardVideos)
			{
				if (cfgLevelRewardVideo.PointType == (int)type)
				{
					list.Add(cfgLevelRewardVideo);
				}
			}
			if (list.Count > 0)
			{
				list.Sort((cfgLevelRewardVideo a, cfgLevelRewardVideo b) => b.Priority - a.Priority);
			}
			return list;
		}

		private IRewardVideoCDFeature GetHandle(PointType type)
		{
			List<cfgLevelRewardVideo> configs = GetConfigs(type);
			for (int i = 0; i < configs.Count; i++)
			{
				IRewardVideoCDFeature feature = GetFeature(configs[i]);
				if (feature == null)
				{
					return null;
				}
				feature.SetConfig(configs[i]);
				if (feature.IsUnlock())
				{
					return feature;
				}
			}
			return null;
		}

		public void AddLookCount(PointType type, int count = 1)
		{
			GetHandle(type)?.AddCount(count);
		}

		public PointType GetTypeByPropID(PropID prop)
		{
			switch (prop)
			{
			case PropID.Hint:
				return PointType.Skill1;
			case PropID.Back:
				return PointType.Skill2;
			case PropID.Freeze:
				return PointType.Skill3;
			case PropID.Refresh:
				return PointType.Skill4;
			default:
				return PointType.None;
			}
		}
	}
}
