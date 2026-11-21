using System;
using System.Collections.Generic;
using System.Linq;
using DataCenter;
using Util;

public class VictoriesActivityMgr
{
	public static VictoriesActivityMgr I = new VictoriesActivityMgr();

	public bool JudgeActivityDuring()
	{
		return RefreshData() == VictoriesActivityState.During;
	}

	public VictoriesActivityState GetActivityState()
	{
		return RefreshData();
	}

	public int JudgeAutoEject()
	{
		VictoriesActivityState state = RefreshData();
		if (state == VictoriesActivityState.BeforeStarting)
		{
			if (!DB.VictoriesActivity.IsAutoEjectedNoticeDialog)
			{
				return 1;
			}
		}
		else if (state == VictoriesActivityState.During)
		{
			if (!DB.VictoriesActivity.IsAutoEjectedActivityDialog)
			{
				return 2;
			}
		}
		return JudgeHaveNoReceive() ? 3 : 0;
	}

	public void SetData(bool isWin)
	{
		if (isWin)
		{
			if (RefreshData() != VictoriesActivityState.During)
			{
				return;
			}
			int num = DB.VictoriesActivity.CurVictoryNum + 1;
			if (num <= DB.VictoriesActivity.MaxVictoryNum)
			{
				DB.VictoriesActivity.CurVictoryNum = num;
				if (num > DB.VictoriesActivity.HighestVictoryNum)
				{
					DB.VictoriesActivity.HighestVictoryNum = num;
				}
			}
			LogEvent.I.Track("Activity_Victories_Victory")
				.Add("VictoryNum", DB.VictoriesActivity.CurVictoryNum)
				.Add("VictoryMaxNum", DB.VictoriesActivity.HighestVictoryNum)
				.CommonAttr(true)
				.Send();
		}
		else
		{
			RegionRankActivityManager.ins.ResetWinningStreak();
			if (RefreshData() != VictoriesActivityState.During)
			{
				return;
			}
			LogEvent.I.Track("Activity_Victories_Fail")
				.Add("VictoryNum", DB.VictoriesActivity.CurVictoryNum)
				.Add("VictoryMaxNum", DB.VictoriesActivity.HighestVictoryNum)
				.CommonAttr(true)
				.Send();
			DB.VictoriesActivity.CurVictoryNum = 0;
		}
	}

	public int GetActivityStartOrEndTime(bool isEnd)
	{
		string text = isEnd ? DB.VictoriesActivity.EndTime : DB.VictoriesActivity.StartTime;
		if (string.IsNullOrEmpty(text))
		{
			return 0;
		}
		double totalSeconds = (MDateTime.Parse(text) - DateTime.Now).TotalSeconds;
		if (totalSeconds < 0.0)
		{
			return 0;
		}
		int num = Convert.ToInt32(totalSeconds);
		return (num < 1) ? num : (num + 2);
	}

	public int[,] Receive(int id = -1)
	{
		int[,] result = new int[0, 2];
		foreach (KeyValuePair<int, VictoriesActivityReward> item in _GetNoReceiveRewardCfg())
		{
			if (id == -1)
			{
				result = Helper.MergerReward(result, item.Value.reward);
				DB.VictoriesActivity.ReceivedChests[item.Key] = 1;
				continue;
			}
			if (item.Value.id == id)
			{
				result = item.Value.reward;
				DB.VictoriesActivity.ReceivedChests[item.Key] = 1;
				break;
			}
		}
		return result;
	}

	public List<VictoriesActivityRewardInfo> GetRewardInfos()
	{
		List<VictoriesActivityRewardInfo> list = new List<VictoriesActivityRewardInfo>();
		int highestVictoryNum = DB.VictoriesActivity.HighestVictoryNum;
		DBList<int> receivedChests = DB.VictoriesActivity.ReceivedChests;
		List<VictoriesActivityReward> rewardsCfg = _GetRewardsCfg();
		if (rewardsCfg.Count == receivedChests.Count)
		{
			for (int i = 0; i < rewardsCfg.Count; i++)
			{
				VictoriesActivityRewardInfo victoriesActivityRewardInfo = new VictoriesActivityRewardInfo();
				victoriesActivityRewardInfo.cfg = rewardsCfg[i];
				if (highestVictoryNum < rewardsCfg[i].condition)
				{
					victoriesActivityRewardInfo.rewardState = 0;
				}
				else
				{
					victoriesActivityRewardInfo.rewardState = ((receivedChests[i] != 0) ? 2 : 1);
				}
				list.Add(victoriesActivityRewardInfo);
			}
		}
		return list;
	}

	public bool JudgeUnlock()
	{
		return DB.User.Curlv + 1 > cf_const.victoriesActivityUnlockLv;
	}

	public bool JudgeHaveNoReceive()
	{
		return _GetNoReceiveRewardCfg().Count > 0;
	}

	public VictoriesActivityState RefreshData()
	{
		if (!JudgeUnlock())
		{
			return VictoriesActivityState.NoStart;
		}
		string issue = _GetIssue(out var sTime, out var eTime, out var isStarting);
		if (string.IsNullOrEmpty(issue))
		{
			return VictoriesActivityState.NoStart;
		}
		if (string.IsNullOrEmpty(DB.VictoriesActivity.Issue))
		{
			_InitData(issue, sTime, eTime);
			return isStarting ? VictoriesActivityState.During : VictoriesActivityState.BeforeStarting;
		}
		if (issue.Equals(DB.VictoriesActivity.Issue))
		{
			if (DB.VictoriesActivity.HighestVictoryNum < DB.VictoriesActivity.MaxVictoryNum || JudgeHaveNoReceive())
			{
				return isStarting ? VictoriesActivityState.During : VictoriesActivityState.BeforeStarting;
			}
			return VictoriesActivityState.NoStart;
		}
		if (JudgeHaveNoReceive())
		{
			return VictoriesActivityState.NoStart;
		}
		_InitData(issue, sTime, eTime);
		return isStarting ? VictoriesActivityState.During : VictoriesActivityState.BeforeStarting;
	}

	private void _InitData(string issue, DateTime sTime, DateTime eTime)
	{
		DB.VictoriesActivity.Issue = issue;
		DB.VictoriesActivity.StartTime = sTime.ToString();
		DB.VictoriesActivity.EndTime = eTime.ToString();
		DB.VictoriesActivity.IsAutoEjectedActivityDialog = false;
		DB.VictoriesActivity.IsReceivedEnergy = false;
		DB.VictoriesActivity.IsAutoEjectedNoticeDialog = false;
		_ResetRewardId();
		DB.VictoriesActivity.CurVictoryNum = 0;
		DB.VictoriesActivity.HighestVictoryNum = 0;
		int maxVictoryNum = 0;
		DB.VictoriesActivity.ReceivedChests.Clear(true);
		_GetRewardsCfg().ForEach(delegate(VictoriesActivityReward a)
		{
			DB.VictoriesActivity.ReceivedChests.Add(0);
			if (a.condition > maxVictoryNum)
			{
				maxVictoryNum = a.condition;
			}
		});
		DB.VictoriesActivity.ReceivedChests.Save();
		DB.VictoriesActivity.MaxVictoryNum = maxVictoryNum;
	}

	private List<VictoriesActivityReward> _GetRewardsCfg()
	{
		return DataCfg.VictoriesActivityRewards.Where(a => a.groupId == DB.VictoriesActivity.RewardId).ToList();
	}

	private string _GetIssue(out DateTime sTime, out DateTime eTime, out bool isStarting)
	{
		isStarting = false;
		DateTime now = DateTime.Now;
		sTime = now;
		eTime = now;
		int num = (now.DayOfWeek == DayOfWeek.Sunday) ? 7 : ((int)now.DayOfWeek);
		int hour = now.Hour;
		int[,] array = cf_const.victoriesActivityDate.ToInt(2, '|', ',');
		int length = array.GetLength(0);
		for (int i = 0; i < length; i += 2)
		{
			if (array[i, 0] <= num && num <= array[i + 1, 0] && hour < array[i + 1, 1])
			{
				sTime = DateTime.Today.AddHours(array[i, 1]);
				eTime = DateTime.Today.AddHours(array[i + 1, 1]);
				string result = string.Format("{0}Y{1}M{2}D{3}H", now.Year, now.Month, now.Day, array[i, 1]);
				if (array[i, 1] <= hour)
				{
					isStarting = true;
				}
				return result;
			}
		}
		return null;
	}

	private void _ResetRewardId()
	{
		int rewardId = DB.VictoriesActivity.RewardId;
		if (DataCfg.VictoriesActivityRewards.Exists(a => a.groupId == rewardId + 1))
		{
			DB.VictoriesActivity.RewardId = rewardId + 1;
		}
		else
		{
			DB.VictoriesActivity.RewardId = 1;
		}
	}

	private Dictionary<int, VictoriesActivityReward> _GetNoReceiveRewardCfg()
	{
		Dictionary<int, VictoriesActivityReward> dictionary = new Dictionary<int, VictoriesActivityReward>();
		int highestVictoryNum = DB.VictoriesActivity.HighestVictoryNum;
		DBList<int> receivedChests = DB.VictoriesActivity.ReceivedChests;
		List<VictoriesActivityReward> rewardsCfg = _GetRewardsCfg();
		if (rewardsCfg.Count == receivedChests.Count)
		{
			for (int i = 0; i < rewardsCfg.Count; i++)
			{
				if (highestVictoryNum >= rewardsCfg[i].condition && receivedChests[i] != 1)
				{
					dictionary.Add(i, rewardsCfg[i]);
				}
			}
		}
		return dictionary;
	}
}
