using System;
using System.Collections.Generic;
using DataCenter;
using GameScene;
using GameScene_Element;
using GameScene_Level;

public class SpecialBlockRewardManager : Singleton<SpecialBlockRewardManager>
{
	private List<cfgSpecialBlockReward> _listSpecialBlockRewards;

	private Dictionary<int, cfgSpecialBlockReward> _fixedRewards;

	private float _timingStarTime;

	private int _collectCount;

	private int _createTypeIndex;

	private bool _assimilationBlockSwitch;

	private bool _isAddMonitor;

	private int _coexistingCount
	{
		get
		{
			return DB.SpecialBlockData.CoexistingCount;
		}
		set
		{
			DB.SpecialBlockData.CoexistingCount = value;
		}
	}

	private int _levelCreateCount
	{
		get
		{
			return DB.SpecialBlockData.LevelCreateCount;
		}
		set
		{
			DB.SpecialBlockData.LevelCreateCount = value;
		}
	}

	public void InitData()
	{
		_listSpecialBlockRewards = new List<cfgSpecialBlockReward>();
		_fixedRewards = new Dictionary<int, cfgSpecialBlockReward>();
		foreach (cfgSpecialBlockReward cfgSpecialBlockReward in DataCfg.cfgSpecialBlockRewards)
		{
			if (cfgSpecialBlockReward.Id > cfgSpecialBlockReward_const.FixedRewardStage)
			{
				_listSpecialBlockRewards.Add(cfgSpecialBlockReward);
			}
			else
			{
				_fixedRewards.Add(cfgSpecialBlockReward.Id, cfgSpecialBlockReward);
			}
		}
		_timingStarTime = 0f;
	}

	private void OnGameTimeChange(float time)
	{
		if (_assimilationBlockSwitch)
		{
			_timingStarTime += time;
			CheckCreateSpecialBlock();
		}
	}

	public void LevelInit(bool hasRecord)
	{
		if (!_isAddMonitor)
		{
			DispatcherMgr.OnG<float>("GameTime_Change", OnGameTimeChange);
			_isAddMonitor = true;
		}
		if (!hasRecord)
		{
			_coexistingCount = 0;
			_levelCreateCount = 0;
		}
		_createTypeIndex = 0;
		_assimilationBlockSwitch = IsUnlockAssimilationBlock();
	}

	public void OnSpecialBlockCollect(Action callBack)
	{
		_coexistingCount--;
		_timingStarTime = 0f;
		_collectCount = 0;
		DB.SpecialBlockData.RewardCount++;
		cfgSpecialBlockReward cfgSpecialBlockReward = ((DB.SpecialBlockData.RewardCount <= cfgSpecialBlockReward_const.FixedRewardStage) ? _fixedRewards[DB.SpecialBlockData.RewardCount] : _listSpecialBlockRewards.Random());
		if (cfgSpecialBlockReward == null)
		{
			if (callBack != null)
			{
				callBack.InvokeSafe();
			}
			return;
		}
		DB.SpecialBlockData.AvailableDailyCount++;
		if (!Singleton<UserLevelRewardManager>.I.IsLevelUpStage)
		{
			SpecialBlockRewardDialog.Create(cfgSpecialBlockReward.Rewards, callBack);
			return;
		}
		int[,] array = cfgSpecialBlockReward.Rewards.ToInt(2, ',', '|');
		if (array.Length < 1)
		{
			if (callBack != null)
			{
				callBack.InvokeSafe();
			}
		}
		else
		{
			int num = array[0, 0];
			int num2 = array[0, 1];
			DB.Bag.Modify((PropID)num, num2, true, "LevelRewardItem");
			DispatcherMgr.Emit("GetRewardByUserLevel", num, num2, callBack);
		}
	}

	public void OnNormalBlockCollect()
	{
		if (_assimilationBlockSwitch)
		{
			_collectCount++;
			CheckCreateSpecialBlock();
		}
	}

	private void CheckCreateSpecialBlock()
	{
		if (GameMgr.I.GameStatus == GameStatus.Playing && Singleton<AdManager>.I.IsRewardVideoOk(false) && DB.SpecialBlockData.AvailableDailyCount < cfgSpecialBlockReward_const.AvailableDailyLimit && _levelCreateCount < cfgSpecialBlockReward_const.LevelCreateLimit && _coexistingCount < cfgSpecialBlockReward_const.CoexistingLimit && _timingStarTime >= (float)cfgSpecialBlockReward_const.CreateCD && _collectCount >= cfgSpecialBlockReward_const.CollectTimes && _assimilationBlockSwitch)
		{
			CreateAssimilationBlock();
		}
	}

	public void ResetCd()
	{
		_timingStarTime = 0f;
		_collectCount = 0;
	}

	private bool IsUnlockAssimilationBlock()
	{
		if (!cfgSpecialBlockReward_const.AssimilationBlockSwitch)
		{
			return false;
		}
		if (DB.User.Maxlv < cfgSpecialBlockReward_const.AssimilationBlockUnlockLevel)
		{
			return false;
		}
		return LevelMgr.Data.Type != LevelType.BakingMaster;
	}

	public void CreateAssimilationBlock()
	{
		if (ElementsMgr.I.AssimilateElements(cfgSpecialBlockReward_const.AssimilationBlockId))
		{
			_levelCreateCount++;
			_coexistingCount++;
			_timingStarTime = 0f;
			_collectCount = 0;
		}
	}
}
