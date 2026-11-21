using System.Collections.Generic;

public class AchiveMgr
{
	private static AchiveMgr _ins = new AchiveMgr();

	private List<int> tipDatas;

	private List<int> canReceiveDatas;

	private cfg_achiveScore curScoreCfg;

	private bool bShowTips;

	public static AchiveMgr I => _ins;

	public void Init()
	{
		tipDatas = new List<int>();
		canReceiveDatas = new List<int>();
	}

	public cfg_achive GetAchiveCfg(int cfgId)
	{
		return DataCfg.cfg_achives.Find((cfg_achive a) => a.id == cfgId);
	}

	public cfg_achive GetAchiveCfgByType(AchieveType type)
	{
		return DataCfg.cfg_achives.Find((cfg_achive a) => a.Type == (int)type);
	}

	public cfg_achiveScore GetAchiveScoreCfg(int cfgId)
	{
		return DataCfg.cfg_achiveScores.Find((cfg_achiveScore a) => a.id == cfgId);
	}

	public cfg_achiveScore GetAchiveScoreCfgByReachCount(int reachCnt)
	{
		for (int num = DataCfg.cfg_achiveScores.Count - 1; num >= 0; num--)
		{
			cfg_achiveScore cfg_achiveScore = DataCfg.cfg_achiveScores[num];
			if (cfg_achiveScore.Count <= reachCnt)
			{
				return cfg_achiveScore;
			}
		}
		return DataCfg.cfg_achiveScores[0];
	}

	public int GetCurAchieveReachCount()
	{
		int num = 0;
		foreach (KeyValuePair<string, int> achieveData in DB.Achieve.AchieveDatas)
		{
			num += achieveData.Value;
		}
		return num;
	}

	public cfg_achiveScore GetCurAchiveScoreCfg()
	{
		return GetAchiveScoreCfgByReachCount(GetCurAchieveReachCount());
	}

	public int GetReachCount(AchieveType type)
	{
		return DB.Achieve.GetReachData(type);
	}

	public void AddTaskReachCount(AchieveType type, int count, bool bIsUpdate = false)
	{
	}

	private void SetTaskReachCount(AchieveType type, int count, bool bIsNotAdd = false)
	{
		int reachData = DB.Achieve.GetReachData(type);
		int value = (bIsNotAdd ? 0 : reachData) + count;
		DB.Achieve.SetReachData(type, value);
		cfg_achive achiveCfgByType = GetAchiveCfgByType(type);
		if (CheckAchieveCfg(achiveCfgByType))
		{
			AddTipsData(achiveCfgByType);
			UpdateCanReceiveData(achiveCfgByType);
			ShowAchiveTips();
		}
	}

	public int GetAchieveLevel(int cfgId)
	{
		return DB.Achieve.GetAchiveData(cfgId);
	}

	public void SetAchieveLevel(int cfgId, int value)
	{
		DB.Achieve.SetAchieveData(cfgId, value);
	}

	public void InitDatas()
	{
	}

	public bool IsShowTipsByCfg(cfg_achive cfg)
	{
		return GetStatusByCfg(cfg) == AchiveStatus.Finished && !DB.Achieve.GetTipsData(cfg.id);
	}

	public void UpdateCanReceiveData(cfg_achive cfg)
	{
		if (!CheckAchieveCfg(cfg))
		{
			return;
		}
		int count = canReceiveDatas.Count;
		AchiveStatus statusByCfg = GetStatusByCfg(cfg);
		if (!canReceiveDatas.Contains(cfg.id))
		{
			if (statusByCfg == AchiveStatus.Finished)
			{
				canReceiveDatas.Add(cfg.id);
			}
		}
		else if (statusByCfg != AchiveStatus.Finished)
		{
			canReceiveDatas.Remove(cfg.id);
		}
		if (count != canReceiveDatas.Count)
		{
			DispatcherMgr.Emit("Update_Achive_Red");
		}
	}

	public void AddTipsData(cfg_achive cfg)
	{
		if (CheckAchieveCfg(cfg) && IsShowTipsByCfg(cfg))
		{
			tipDatas.Add(cfg.id);
		}
	}

	public void ShowAchiveTips()
	{
		if (bShowTips || tipDatas == null || tipDatas.Count <= 0)
		{
			return;
		}
		int num = tipDatas.Pop();
		cfg_achive achiveCfg = GetAchiveCfg(num);
		if (CheckAchieveCfg(achiveCfg))
		{
			bShowTips = true;
			int length = achiveCfg.Values.ToInt('|').Length;
			int achieveLevel = GetAchieveLevel(num);
			SetAchiveTipsData(achiveCfg.id, true);
			AchiveFinishedTips achiveFinishedTips = SceneMgr.AddSuperNode<AchiveFinishedTips>("Prefabs/Achive/AchiveFinishedTips");
			achiveFinishedTips.InitInfo(achiveCfg, achieveLevel, length);
			achiveFinishedTips.completeCallback = delegate
			{
				bShowTips = false;
				ShowAchiveTips();
			};
		}
	}

	public void SetAchiveTipsData(int cfgId, bool bIsNotShowTips)
	{
		DB.Achieve.SetTipsData(cfgId, bIsNotShowTips);
	}

	public bool IsCanGetAchiveReward()
	{
		return canReceiveDatas != null && canReceiveDatas.Count > 0;
	}

	public void UpdateCurScoreCfg()
	{
		cfg_achiveScore achiveScoreCfgByReachCount = GetAchiveScoreCfgByReachCount(GetCurAchieveReachCount());
		if (achiveScoreCfgByReachCount != null && (curScoreCfg == null || achiveScoreCfgByReachCount.id != curScoreCfg.id))
		{
			curScoreCfg = achiveScoreCfgByReachCount;
			DispatcherMgr.Emit("Update_Achive_Title");
		}
	}

	public void SortAchieveCfg()
	{
		DataCfg.cfg_achives.Sort((cfg_achive a, cfg_achive b) => (GetStatusByCfg(a) < GetStatusByCfg(b)) ? 1 : -1);
	}

	public AchiveStatus GetStatusByCfg(cfg_achive cfg)
	{
		int[] array = cfg.Values.ToInt('|');
		int length = array.Length;
		int achieveLevel = GetAchieveLevel(cfg.id);
		int reachCount = GetReachCount((AchieveType)cfg.Type);
		int num = (achieveLevel < length) ? achieveLevel : (achieveLevel - 1);
		int num2 = (achieveLevel < length) ? 2 : 0;
		if (reachCount < array[num])
		{
			num2 = 1;
		}
		return (AchiveStatus)num2;
	}

	public bool CheckAchieveCfg(cfg_achive cfg)
	{
		return cfg != null && cfg.id != 0 && cfg.Type != 0 && !cfg.Values.Equals("");
	}
}
