using System;
using System.Collections.Generic;
using System.Linq;
using GameScene_Record;
using UnityEngine;

public class RegionRankActivityManager : ActivityManagerBase<RegionRankActivityManager>
{
	public Action OnActivityEnd;

	private int _maxLevelGroup;

	private Dictionary<int, List<cfgLevelFixed_RegionRank>> _groupLevels;

	private Dictionary<string, cfgRegionRankRegions> _regions;

	private Dictionary<int, List<cfgRegionRankReward>> _rewards;

	private Dictionary<int, int> _winningStreakGenerateModel;

	public int CacheScore { get; set; }

	public override void Setup()
	{
		_groupLevels = new Dictionary<int, List<cfgLevelFixed_RegionRank>>();
		foreach (cfgLevelFixed_RegionRank item in LoadMgr.LoadJson<List<cfgLevelFixed_RegionRank>>("Json/cfgLevelFixed_RegionRank"))
		{
			if (!_groupLevels.ContainsKey(item.RegionRank_Group))
			{
				_groupLevels[item.RegionRank_Group] = new List<cfgLevelFixed_RegionRank>();
			}
			_groupLevels[item.RegionRank_Group].Add(item);
		}
		_maxLevelGroup = _groupLevels.Keys.Max();
		foreach (KeyValuePair<int, List<cfgLevelFixed_RegionRank>> groupLevel in _groupLevels)
		{
			groupLevel.Value.Sort((cfgLevelFixed_RegionRank a, cfgLevelFixed_RegionRank b) => a.RegionRank_Order.CompareTo(b.RegionRank_Order));
		}
		_regions = new Dictionary<string, cfgRegionRankRegions>();
		foreach (cfgRegionRankRegions cfgRegionRankRegionss in DataCfg.cfgRegionRankRegionss)
		{
			_regions[cfgRegionRankRegionss.RegionCode.ToLower()] = cfgRegionRankRegionss;
		}
		_rewards = new Dictionary<int, List<cfgRegionRankReward>>();
		foreach (cfgRegionRankReward cfgRegionRankReward in DataCfg.cfgRegionRankRewards)
		{
			if (!_rewards.ContainsKey(cfgRegionRankReward.RankType))
			{
				_rewards[cfgRegionRankReward.RankType] = new List<cfgRegionRankReward>();
			}
			_rewards[cfgRegionRankReward.RankType].Add(cfgRegionRankReward);
		}
		_winningStreakGenerateModel = new Dictionary<int, int>();
		foreach (string item2 in cf_regionRank_const.WinningStreakGenerateModel)
		{
			int[] array = item2.ToInt('|');
			_winningStreakGenerateModel[array[0]] = array[1];
		}
		OnActivityStateEnding += delegate
		{
			Singleton<RecordMgr>.I.Clear(isForce: true);
		};
	}

	public bool IsUnknownRegion()
	{
		return GetRegionCode().Equals(RegionRankActivityData.UnknownRegionCode);
	}

	public string GetRegionCode()
	{
		string regionCode = GetSelfData().RegionCode;
		if (regionCode != RegionRankActivityData.UnknownRegionCode)
		{
			if (_regions.ContainsKey(regionCode))
			{
				return regionCode;
			}
			GetSelfData().RegionCode = RegionRankActivityData.UnknownRegionCode;
			SaveData();
		}
		return RegionRankActivityData.UnknownRegionCode;
	}

	public cfgRegionRankRegions GetRegionConfig()
	{
		RegionRankActivityData selfData = GetSelfData();
		if (!_regions.ContainsKey(selfData.RegionCode))
		{
			return null;
		}
		return _regions[selfData.RegionCode];
	}

	public cfgRegionRankRegions GetRegionConfig(string regionCode)
	{
		if (!_regions.ContainsKey(regionCode))
		{
			return null;
		}
		return _regions[regionCode];
	}

	public cfgRegionRankReward GetRankReward(int ranking, RegionRankType type)
	{
		if (!_rewards.ContainsKey((int)type))
		{
			return null;
		}
		List<cfgRegionRankReward> list = _rewards[(int)type];
		for (int i = 0; i < list.Count; i++)
		{
			if (ranking <= list[i].Ranking)
			{
				return list[i];
			}
		}
		return null;
	}

	public RegionRankActivityData GetSelfData()
	{
		return DB.RegionRankActivity.GetData();
	}

	public int GetChallengeCount()
	{
		ActivityStateBase activityState = GetActivityState();
		int num = ((activityState == ActivityStateBase.During) ? GetSelfData().ChallengeCount : 0);
		Debug.Log(string.Format("GetChallengeCount: {0} state: {1}", num, activityState));
		return num;
	}

	public int GetWinningStreak()
	{
		int num = GetSelfData().WinningStreak;
		if (!_winningStreakGenerateModel.ContainsKey(num))
		{
			num = _winningStreakGenerateModel.Keys.Max();
		}
		return num;
	}

	public bool TryGetLevelConfig(out cfgLevelFixed_RegionRank config, out int[] crownModels)
	{
		config = null;
		crownModels = null;
		if (GetChallengeCount() < 1)
		{
			return false;
		}
		RegionRankActivityData selfData = GetSelfData();
		if (GetActivityStateWithRefresh() != ActivityStateBase.During)
		{
			return false;
		}
		if (!_groupLevels.ContainsKey(selfData.LevelGroup))
		{
			selfData.LevelGroup = _groupLevels.Keys.First();
			SaveData();
		}
		List<cfgLevelFixed_RegionRank> list = _groupLevels[selfData.LevelGroup];
		if (selfData.LevelOrderIndex >= list.Count)
		{
			selfData.LevelOrderIndex = 0;
			SaveData();
		}
		int winningStreak = GetWinningStreak();
		int num = _winningStreakGenerateModel[winningStreak];
		crownModels = new int[num];
		for (int i = 0; i < num; i++)
		{
			crownModels[i] = cf_regionRank_const.CrownModelId;
		}
		config = list[selfData.LevelOrderIndex];
		Debug.LogError(string.Concat(new string[5]
		{
			string.Format("RegionRankActivity GetLevelConfig group: {0} ", config.RegionRank_Group),
			string.Format("orderIndex: {0} ", selfData.LevelOrderIndex),
			string.Format("order: {0} ", config.RegionRank_Order),
			string.Format("winningStreak: {0} ", winningStreak),
			string.Format("crownModels: {0}", crownModels.Length)
		}));
		return true;
	}

	public int GetStartLevelGroup()
	{
		return GetStartLevelGroup(DB.RegionRankActivity.LastRanking);
	}

	public int GetStartLevelGroup(int lastRanking)
	{
		int num = ((lastRanking != 0) ? lastRanking : 1001);
		int num2 = DB.RegionRankActivity.LastGroup;
		List<string> rankingAddGroup = cf_regionRank_const.RankingAddGroup;
		for (int i = 0; i < rankingAddGroup.Count; i++)
		{
			int[] array = rankingAddGroup[i].ToInt('|');
			if (array[0] >= num)
			{
				Debug.LogError(string.Format("ranking: {0} addGroup: {1}", num, array[1]));
				num2 += array[1];
				break;
			}
		}
		return Mathf.Clamp(num2, 1, _maxLevelGroup);
	}

	public void GMAddOrderIndex(int index)
	{
	}

	public void ResetWinningStreak()
	{
		RegionRankActivityData selfData = GetSelfData();
		selfData.WinningStreak = 1;
		selfData.ChallengeCount = Math.Max(selfData.ChallengeCount - 1, 0);
		selfData.LevelOrderIndex = Math.Max(selfData.LevelOrderIndex - cf_regionRank_const.FailReduceLevelOrder, 0);
		SaveData();
		Debug.LogError(string.Concat(string.Format("ResetWinningStreak WinningStreak: {0} ", selfData.WinningStreak), string.Format("ChallengeCount: {0} ", selfData.ChallengeCount), string.Format("LevelOrderIndex: {0} ", selfData.LevelOrderIndex)));
	}

	public void LevelVictory(int score, bool isGMAddScore = false)
	{
		if (GetActivityState() != ActivityStateBase.During)
		{
			return;
		}
		if (GetActivityEndSeconds() >= 600)
		{
			RegionRankActivityData selfData = GetSelfData();
			selfData.Score += score;
			selfData.AddScore += score;
			selfData.AddScoreCount++;
			CacheScore += score;
			if (!isGMAddScore)
			{
				selfData.WinningStreak++;
				if (selfData.WinningStreakBest < selfData.WinningStreak)
				{
					selfData.WinningStreakBest = selfData.WinningStreak;
				}
				selfData.LevelOrderIndex++;
				if (selfData.LevelOrderIndex >= _groupLevels[selfData.LevelGroup].Count)
				{
					selfData.LevelOrderIndex = 0;
				}
				Debug.LogError(string.Concat(string.Format("RegionRankActivity LevelVictory group: {0} ", selfData.LevelGroup), string.Format("orderIndex: {0} ", selfData.LevelOrderIndex)));
			}
			SaveData();
		}
		else
		{
			Debug.LogError(string.Format("服务器结算时间，最后10分钟不再加分: {0}", score));
			if (GetSelfData().AddScore < 1)
			{
				return;
			}
		}
		Singleton<RegionRankServerManager>.I.UploadSelfScore(UpScoreCallback);
	}

	public void ResetAddScoreCount()
	{
		GetSelfData().AddScoreCount = 0;
		SaveData();
	}

	public void UpScoreCallback(bool succ)
	{
		if (succ)
		{
			RegionRankActivityData selfData = GetSelfData();
			selfData.AddScore = 0;
			selfData.AddScoreCount = 0;
			SaveData();
		}
	}

	public void SetPreparePopTrue()
	{
		GetSelfData().PreparePop = true;
		SaveData();
	}

	public void SetStartPopTrue()
	{
		GetData().StartPop = true;
		SaveData();
	}

	public void ActivityEnd(int personalRanking)
	{
		DB.RegionRankActivity.LastRanking = personalRanking;
		DB.RegionRankActivity.LastGroup = GetSelfData().LevelGroup;
		Singleton<RegionRankServerManager>.I.ClearCache();
		GetData().State = ActivityStateBase.NoStart;
		SaveData();
		if (OnActivityEnd != null)
		{
			OnActivityEnd();
		}
	}

	protected override void ActivityOpenPrepare(int issue)
	{
		RegionRankActivityData selfData = GetSelfData();
		selfData.Issue = issue;
		selfData.State = ActivityStateBase.StartPrepare;
		SaveData();
	}

	public bool IsPreview()
	{
		return DB.User.Maxlv + 1 < cf_regionRank_const.UnlockLevel;
	}

	protected override bool PrepareOrDuringPop(bool isForcePop, Action<bool> endCall)
	{
		RegionRankActivityData selfData = GetSelfData();
		ActivityStateBase activityState = GetActivityState();
		bool flag = IsPreview();
		if (isForcePop)
		{
			if (activityState == ActivityStateBase.StartPrepare)
			{
				if (flag)
				{
					RegionRankPreparePanel.Create(delegate
					{
						endCall?.Invoke(true);
					});
				}
				else
				{
					RegionRankStartPanel.Create(delegate
					{
						endCall?.Invoke(true);
					});
				}
			}
			else
			{
				RegionRankPanel.Create(delegate
				{
					endCall?.Invoke(true);
				});
			}
			return true;
		}
		if (activityState == ActivityStateBase.StartPrepare)
		{
			if (flag)
			{
				if (selfData.PreparePop)
				{
					return false;
				}
				RegionRankPreparePanel.Create(delegate
				{
					endCall?.Invoke(true);
				});
			}
			else
			{
				if (selfData.StartPop)
				{
					return false;
				}
				RegionRankStartPanel.Create(delegate
				{
					endCall?.Invoke(true);
				});
			}
			return true;
		}
		endCall?.Invoke(false);
		return false;
	}

	protected override bool EndingPop(bool isForcePop, Action<bool> endCall)
	{
		RegionRankEndPanel.Create(delegate
		{
			endCall?.Invoke(true);
		});
		return true;
	}

	protected override ActivityDataBase GetData()
	{
		return DB.RegionRankActivity.GetData();
	}

	protected override void SaveData()
	{
		DB.RegionRankActivity.SaveData();
	}

	protected override void NewActivityStart(int issue)
	{
		Singleton<RegionRankServerManager>.I.ClearCache();
		GetActivityInfo(out var startTimestamp, out var endTimestamp, out var _);
		RegionRankActivityData selfData = GetSelfData();
		selfData.PreparePop = false;
		selfData.LevelOrderIndex = 0;
		selfData.LevelGroup = GetStartLevelGroup();
		selfData.WinningStreak = 1;
		selfData.WinningStreakBest = 1;
		selfData.AddScore = 0;
		selfData.AddScoreCount = 0;
		selfData.ChallengeCount = cf_regionRank_const.ChallengeCount;
		string text = NativePlatform.Helper.GetCountryCode().ToLower();
		selfData.RegionCode = (_regions.ContainsKey(text) ? _regions[text].RegionCode : RegionRankActivityData.UnknownRegionCode);
		selfData.Issue = issue;
		selfData.Score = 0;
		selfData.StartPop = false;
		selfData.AiLastRefreshTime = startTimestamp;
		selfData.StartTime = startTimestamp;
		selfData.EndTime = endTimestamp;
		selfData.State = ActivityStateBase.During;
		SaveData();
		Singleton<RecordMgr>.I.Clear(isForce: true);
	}

	protected override bool JudgeDuring()
	{
		return true;
	}

	protected override bool JudgeEndExtra()
	{
		return ServerDateTimeMgr.I.IsServerTimeSynced || CanUseLocalTime();
	}

	protected override int JudgeNewActivity()
	{
		int issue = 0;
		if (GetActivityInfo(out var startTimestamp, out var _, out issue))
		{
			RegionRankActivityData selfData = GetSelfData();
			if (selfData.StartTime != startTimestamp)
			{
				Debug.LogError(string.Format("RegionRankActivityManager StartTime: {0} new startTime: {1}", selfData.StartTime, startTimestamp));
				selfData.StartTime = startTimestamp;
				selfData.StartPop = false;
				selfData.PreparePop = false;
				SaveData();
			}
		}
		else
		{
			issue = -1;
		}
		return issue;
	}

	private bool GetActivityInfo(out long startTimestamp, out long endTimestamp, out int issue)
	{
		startTimestamp = 0L;
		endTimestamp = 0L;
		issue = 0;
		if ((ServerDateTimeMgr.I.IsServerTimeSynced || CanUseLocalTime()) && IsActivityOpenControl() && cf_regionRank_const.PreviewLevel <= DB.User.Maxlv + 1)
		{
			DateTime dateTime = (CanUseLocalTime() ? DateTime.UtcNow : ServerDateTimeMgr.I.NetTimeUtc);
			DateTime dateTime2 = dateTime.ParseFull(cf_regionRank_const.FirstStartTime);
			if (!(dateTime < dateTime2))
			{
				int num = cf_regionRank_const.ContinueTime + cf_regionRank_const.CoolTime;
				issue = (int)((dateTime - dateTime2).TotalSeconds / (double)num);
				DateTime dateTime3 = dateTime2.AddSeconds(issue * num);
				DateTime dateTime4 = dateTime3.AddSeconds(cf_regionRank_const.ContinueTime);
				startTimestamp = GetTimeStamp(dateTime3);
				endTimestamp = GetTimeStamp(dateTime4);
				return dateTime3 <= dateTime && dateTime < dateTime4;
			}
		}
		return false;
	}

	protected override bool IsActivityOpenControl()
	{
		return cf_regionRank_const.IsOpen == 1;
	}

	public override long GetTimeStamp()
	{
		DateTime dateTime = (CanUseLocalTime() ? DateTime.UtcNow : ServerDateTimeMgr.I.NetTimeUtc);
		return (dateTime.Ticks - 621355968000000000L) / 10000000;
	}

	public long GetTimeStamp(DateTime time)
	{
		return (time.Ticks - 621355968000000000L) / 10000000;
	}

	private bool CanUseLocalTime()
	{
		return PlayerPrefs.GetInt("Key_RegionRankLocalTime", 0) == 1;
	}
}
