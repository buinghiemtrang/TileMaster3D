using System;
using System.Collections.Generic;
using DataCenter;
using UnityEngine;

public class UserLevelRewardManager : Singleton<UserLevelRewardManager>
{
	private Dictionary<int, int> _fixedLevelInfo;

	private List<int> _randomLevelInfo;

	private cfgUserLevel _nextLevelInfo;

	private bool _isLevelUpStage;

	private int _addScore;

	private string _rewardInfo;

	public bool IsLevelUpStage => _isLevelUpStage;

	public cfgUserLevel NextLevelInfo => _nextLevelInfo;

	public string RewardInfo => _rewardInfo;

	public void InitDate()
	{
		if (cfgUserLevel_const.FunctionStage < 1)
		{
			return;
		}
		if (_fixedLevelInfo == null)
		{
			_fixedLevelInfo = new Dictionary<int, int>();
		}
		_fixedLevelInfo.Clear();
		if (_randomLevelInfo == null)
		{
			_randomLevelInfo = new List<int>();
		}
		_randomLevelInfo.Clear();
		foreach (KeyValuePair<int, cfgUserLevel> cfgUserLevel in DataCfg.cfgUserLevels)
		{
			if (cfgUserLevel.Value.UserLevelId == -1)
			{
				_randomLevelInfo.Add(cfgUserLevel.Key);
			}
			else
			{
				_fixedLevelInfo.Add(cfgUserLevel.Value.UserLevelId, cfgUserLevel.Key);
			}
		}
		if (DB.UserLevelReward.NextLevelId == -1 || !DataCfg.cfgUserLevels.ContainsKey(DB.UserLevelReward.NextLevelId))
		{
			UpdateNextLevelInfo();
		}
		else
		{
			_nextLevelInfo = DataCfg.cfgUserLevels[DB.UserLevelReward.NextLevelId];
		}
	}

	public void AddScore()
	{
		if (cfgUserLevel_const.FunctionStage >= 1)
		{
			_addScore = cfgUserLevel_const.HighScore;
			DB.UserLevelReward.UserScore += _addScore;
			DispatcherMgr.Emit("AddUseLevelScore");
		}
	}

	public void AddScore(int score)
	{
		DB.UserLevelReward.UserScore += score;
		DispatcherMgr.Emit("AddUseLevelScore");
	}

	public bool CheckLevelUp()
	{
		if (cfgUserLevel_const.FunctionStage < 1)
		{
			return false;
		}
		if (_nextLevelInfo == null)
		{
			UpdateNextLevelInfo();
		}
		return DB.UserLevelReward.UserScore >= _nextLevelInfo.NeedScore;
	}

	public void LevelUp(Action action)
	{
		_rewardInfo = _nextLevelInfo.Rewards;
		DB.UserLevelReward.UserLevel++;
		DB.UserLevelReward.UserScore = 0;
		UpdateNextLevelInfo();
		DispatcherMgr.Emit("UseLevelUp", action);
	}

	public void UpdateStage()
	{
		_isLevelUpStage = true;
		GSingleton<GTimer>.ins.removeDelay(Tick);
		GSingleton<GTimer>.ins.addDelay(Tick, cfgUserLevel_const.LevelUpCD);
		void Tick()
		{
			_isLevelUpStage = false;
		}
	}

	private void UpdateNextLevelInfo()
	{
		if (_fixedLevelInfo.TryGetValue(DB.UserLevelReward.UserLevel + 1, out var value))
		{
			DB.UserLevelReward.NextLevelId = value;
			_nextLevelInfo = DataCfg.cfgUserLevels[value];
		}
		else
		{
			int index = UnityEngine.Random.Range(0, _randomLevelInfo.Count - 1);
			DB.UserLevelReward.NextLevelId = _randomLevelInfo[index];
			_nextLevelInfo = DataCfg.cfgUserLevels[_randomLevelInfo[index]];
		}
	}
}
