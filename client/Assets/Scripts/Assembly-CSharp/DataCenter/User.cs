using Newtonsoft.Json;
using UnityEngine;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class User : IUser
	{
		public LvTime GetLvTimeData(string key)
		{
			if (!lvTimeData.ContainsKey(key))
			{
				return null;
			}
			return lvTimeData[key];
		}

		public void SaveLvTimeData(string key)
		{
			if (lvTimeData.ContainsKey(key))
			{
				Asse.Save("lvTimeData." + key, lvTimeData[key]);
			}
		}

		public int GetMonthIssueSumStar(string key)
		{
			if (!MonthIssueSumStar.ContainsKey(key))
			{
				return 0;
			}
			return MonthIssueSumStar[key];
		}

		public void AddMonthIssueSumStar(int addCount)
		{
			if (!string.IsNullOrEmpty(MonthIssue))
			{
				string key = MonthIssue;
				SetMonthIssueSumStar_(key, GetMonthIssueSumStar(key) + addCount);
			}
		}

		public int GetMonthIssueLevel(string key)
		{
			if (!MonthIssueLevel.ContainsKey(key))
			{
				return 0;
			}
			return MonthIssueLevel[key];
		}

		public void AddMonthIssueLevel(int addLevelCount = 1)
		{
			if (!string.IsNullOrEmpty(MonthIssue))
			{
				string key = MonthIssue;
				SetMonthIssueLevel(key, GetMonthIssueLevel(key) + addLevelCount);
			}
		}

		public int GetGoldReviveNeedGold(int[] needGolds)
		{
			if (needGolds != null && needGolds.Length != 0)
			{
				return needGolds[Mathf.Clamp(useGoldReviveNumberTimes, 0, needGolds.Length - 1)];
			}
			return 0;
		}
	}
}
