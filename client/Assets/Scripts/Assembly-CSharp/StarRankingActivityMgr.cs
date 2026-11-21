using System;
using System.Collections.Generic;
using System.Linq;
using DataCenter;
using UnityEngine;

public class StarRankingActivityMgr : Singleton<StarRankingActivityMgr>
{
	private const int _aINum = 59;

	private StarRankingCfg starRankingCfg;

	public void Init()
	{
		starRankingCfg = LoadMgr.LoadJson<StarRankingCfg>("Json/cfgStarRankingActivityAIStar");
		ReplenishAICount();
	}

	public StarRankingActivityState GetActivityState()
	{
		return RefreshData();
	}

	public int JudgeAutoEject()
	{
		if (RefreshData() == StarRankingActivityState.During && !DB.StarRankingActivity.IsAutoEjected)
		{
			return 1;
		}
		if (JudgeHaveNoReceive())
		{
			return 2;
		}
		return 0;
	}

	public List<cfgStarRankingActivityReward> GetRewards(int dan, int grading)
	{
		return DataCfg.cfgStarRankingActivityRewards[dan.ToString()].FindAll(sd => sd.grading == grading);
	}

	public List<StarRankingData> GetPlayerData(out StarRankingData ownData)
	{
		List<StarRankingData> list = new List<StarRankingData>();
		for (int i = 0; i < DB.StarRankingActivity.AIStars.Count; i++)
		{
			StarRankingData starRankingData = new StarRankingData();
			starRankingData.isOwn = false;
			starRankingData.ranking = -1;
			starRankingData.name = DB.StarRankingActivity.AINames[i];
			starRankingData.score = DB.StarRankingActivity.AIStars[i];
			starRankingData.dataType = StarRankingDataType.Normal;
			starRankingData.danGrading = DB.StarRankingActivity.Level + 1;
			list.Add(starRankingData);
		}
		ownData = new StarRankingData();
		ownData.isOwn = true;
		ownData.ranking = -1;
		ownData.name = DB.Account.GetName();
		ownData.score = DB.StarRankingActivity.SumStar;
		ownData.danGrading = DB.StarRankingActivity.Level + 1;
		list.Add(ownData);
		list.Sort((a, b) => b.score - a.score);
		if (list.Count > 0)
		{
			bool flag = false;
			for (int j = list.Count - 1; j >= 0; j--)
			{
				int num = flag ? 59 : 60;
				if (j < num)
				{
					break;
				}
				if (list[j].isOwn)
				{
					flag = true;
				}
				list.RemoveAt(j);
			}
			if (flag)
			{
				list.Add(ownData);
			}
			for (int k = 0; k < list.Count; k++)
			{
				list[k].ranking = k + 1;
			}
		}
		return list;
	}

	public int GetActivityEndTime()
	{
		if (!string.IsNullOrEmpty(DB.StarRankingActivity.EndTime))
		{
			double totalSeconds = (MDateTime.Parse(DB.StarRankingActivity.EndTime) - DateTime.Now).TotalSeconds;
			if (totalSeconds >= 0.0)
			{
				int num = Convert.ToInt32(totalSeconds);
				if (num < 1)
				{
					return num;
				}
				return num + 2;
			}
		}
		return 0;
	}

	public void Receive()
	{
		if (DB.StarRankingActivity.IsCanReceive)
		{
			SetStarNum(0, true);
			DB.StarRankingActivity.IsReceived = true;
		}
	}

	public void SetStarNum(int va, bool isClear = false)
	{
		if (isClear)
		{
			DB.StarRankingActivity.SumStar = va;
			return;
		}
		int durDays;
		if (_JudgeDuringActivity(out durDays) != -1)
		{
			DB.StarRankingActivity.SumStar += va;
		}
	}

	public bool JudgeHaveNoReceive()
	{
		return DB.StarRankingActivity.IsCanReceive && !DB.StarRankingActivity.IsReceived;
	}

	private void _RefreshAIStarByHours(int hours)
	{
		if (hours > 0)
		{
			int activityRankingStarRefreshOnceTime = cf_const.activityRankingStarRefreshOnceTime;
			int num = (activityRankingStarRefreshOnceTime != 0) ? (hours / activityRankingStarRefreshOnceTime) : 0;
			DB.StarRankingActivity.LastRefreshTime = DateTime.Now.ToString();
			_RefreshAIStar(num);
		}
	}

	private StarRankingActivityState RefreshData()
	{
		if (JudgeHaveNoReceive())
		{
			return StarRankingActivityState.NoStart;
		}
		DateTime now = DateTime.Now;
		DateTime defaultTime = MDateTime.Parse(DB.StarRankingActivity.StartTime, default(DateTime));
		DateTime t = MDateTime.Parse(DB.StarRankingActivity.EndTime, default(DateTime));
		DateTime d = MDateTime.Parse(DB.StarRankingActivity.LastRefreshTime, defaultTime);
		if (now > t)
		{
			if (!DB.StarRankingActivity.IsCanReceive)
			{
				DB.StarRankingActivity.IsCanReceive = true;
				int hours = (int)((t - d).TotalHours - 1.0);
				_RefreshAIStarByHours(hours);
				DB.StarRankingActivity.LastRefreshTime = string.Empty;
			}
			DB.StarRankingActivity.EndTime = string.Empty;
		}
		else if (now > defaultTime)
		{
			int hours2 = (int)(now - d).TotalHours;
			_RefreshAIStarByHours(hours2);
		}
		int durDays;
		int num = _JudgeDuringActivity(out durDays);
		if (string.IsNullOrEmpty(DB.StarRankingActivity.EndTime) && num != -1)
		{
			if (DB.StarRankingActivity.IsReceived)
			{
				_InitAIStar();
				DB.StarRankingActivity.IsReceived = false;
				DB.StarRankingActivity.IsCanReceive = false;
				DB.StarRankingActivity.IsAutoEjected = false;
				DB.StarRankingActivity.StartTime = DateTime.Today.AddDays(-num).ToString();
				DB.StarRankingActivity.EndTime = DateTime.Today.AddDays(durDays - num).ToString();
			}
		}
		if (num == -1)
		{
			return StarRankingActivityState.NoStart;
		}
		return StarRankingActivityState.During;
	}

	private int _JudgeDuringActivity(out int durDays)
	{
		durDays = 0;
		int dayOfWeek = (int)DateTime.Now.DayOfWeek;
		string[] array = cf_const.activityRankingStarShowTime.Split('|');
		for (int i = 0; i < array.Length; i++)
		{
			int[] array2 = array[i].ToInt(',');
			durDays = array2.Length;
			int num = Array.FindIndex(array2, fi => fi == dayOfWeek);
			if (num != -1)
			{
				return num;
			}
		}
		return -1;
	}

	private void _SetAIData(int start, int end)
	{
		int num = end - start;
		if (num <= 0)
		{
			return;
		}
		for (int i = start; i < end; i++)
		{
			int item = starRankingCfg.stars[i] + UnityEngine.Random.Range(0, 501);
			DB.StarRankingActivity.AIStars.Add(item);
		}
		DB.StarRankingActivity.AIStars.Save();
		List<string> list = new List<string>();
		if (DB.StarRankingActivity.AINames.Count == 0)
		{
			list.AddRange(starRankingCfg.names);
		}
		else
		{
			list.AddRange(starRankingCfg.names.Where(name => !DB.StarRankingActivity.AINames.Contains(name)));
		}
		while (num > 0)
		{
			int index = UnityEngine.Random.Range(0, list.Count);
			DB.StarRankingActivity.AINames.Add(list[index]);
			list.RemoveAt(index);
			num--;
		}
		DB.StarRankingActivity.AINames.Save();
	}

	private void _InitAIStar()
	{
		if (starRankingCfg.names.Length < 59 && starRankingCfg.stars.Length < 59)
		{
			return;
		}
		DB.StarRankingActivity.AIStars.Clear(true);
		DB.StarRankingActivity.AINames.Clear(true);
		_SetAIData(0, 59);
	}

	public void ReplenishAICount()
	{
		_SetAIData(DB.StarRankingActivity.AINames.Count, 59);
	}

	private void _RefreshAIStar(int num)
	{
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < DB.StarRankingActivity.AIStars.Count; j++)
				{
					int num2 = starRankingCfg.condition.Length - 1;
					while (num2 >= 0)
					{
						if (DB.StarRankingActivity.AIStars[j] >= starRankingCfg.condition[num2])
						{
							break;
						}
						num2--;
					}
					if (num2 >= 0 && MMath.Random() < (double)starRankingCfg.probability[num2])
					{
						int min = starRankingCfg.min[num2];
						int num3 = starRankingCfg.max[num2];
						int num4 = MMath.Random(min, num3 + 1);
						DB.StarRankingActivity.AIStars[j] += num4;
					}
				}
			}
		}
		DB.StarRankingActivity.AIStars.Save();
	}

	private void _ResetReward()
	{
	}
}
