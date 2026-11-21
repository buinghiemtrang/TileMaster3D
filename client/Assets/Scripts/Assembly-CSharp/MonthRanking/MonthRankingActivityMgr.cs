using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MonthRanking
{
	public class MonthRankingActivityMgr : Singleton<MonthRankingActivityMgr>
	{
		public bool JudgeOpen()
		{
			if (cf_const.isOpenMonthRankingActivity && DataManager.I.bLogined && DB.MonthRankingActivity.State != 0)
			{
				return DB.MonthRankingActivity.IsAvailable;
			}
			return false;
		}

		public int JudgeAutoEject()
		{
			if (JudgeOpen())
			{
				if (DB.MonthRankingActivity.State == 2 && !DB.MonthRankingActivity.IsAutoEjected)
				{
					return 1;
				}
				if (DB.MonthRankingActivity.State == 4 && DB.MonthRankingActivity.IsCanReceive && !DB.MonthRankingActivity.IsReceived)
				{
					return 2;
				}
			}
			return 0;
		}

		public void SetReceivedState()
		{
			DB.MonthRankingActivity.IsReceived = true;
			DB.MonthRankingActivity.State = 0;
			DB.MonthRankingActivity.UpdateRewardCfgGroupId();
			MonthRankingHttpMgr.I.GetPersonalRankingActivity();
		}

		public void RefreshActivityInfo(MonthRankingActivityInfo info)
		{
			string monthRankingIssue = GetMonthRankingIssue(DB.MonthRankingActivity.StartTime);
			string monthRankingIssue2 = GetMonthRankingIssue(info.startTime);
			bool flag = string.IsNullOrEmpty(monthRankingIssue) || DB.MonthRankingActivity.State == 0;
			bool flag2 = monthRankingIssue2.Equals(monthRankingIssue);
			if (flag && !flag2)
			{
				DB.MonthRankingActivity.IsCanReceive = false;
				DB.MonthRankingActivity.IsReceived = false;
				DB.MonthRankingActivity.IsAutoEjected = false;
				DB.MonthRankingActivity.State = 2;
				_ResetActivityInfo(info);
				_SendGetActivityInfoSuccEvent();
				_SendServerTimeEvent(info.endTimeSpan);
			}
			else if (!flag)
			{
				if (flag2 && info.endTimeSpan >= 1)
				{
					_ResetActivityInfo(info);
					_SendGetActivityInfoSuccEvent();
					_SendServerTimeEvent(info.endTimeSpan);
				}
				else
				{
					DB.MonthRankingActivity.IsAvailable = true;
					DB.MonthRankingActivity.State = 4;
					if (!DB.MonthRankingActivity.IsCanReceive)
					{
						RequestRankingData();
					}
					_SendGetActivityInfoSuccEvent();
					_SendServerTimeEvent(0L);
				}
			}
		}

		private void _ResetActivityInfo(MonthRankingActivityInfo info)
		{
			DB.MonthRankingActivity.Issue = info.issue;
			DB.MonthRankingActivity.EndTime = info.endTime;
			DB.MonthRankingActivity.StartTime = info.startTime;
			DB.MonthRankingActivity.IsAvailable = info.available;
		}

		public void RequestRankingData()
		{
			string monthRankingIssue = GetMonthRankingIssue(DB.MonthRankingActivity.StartTime);
			MonthRankingHttpMgr.I.GetPersonalRankingData(monthRankingIssue);
		}

		public string GetMonthRankingIssue(string startTime)
		{
			if (string.IsNullOrEmpty(startTime))
			{
				return string.Empty;
			}
			DateTime dateTime = MDateTime.Parse(startTime);
			return string.Format("{0:yyyy}Y{1:MM}M", dateTime, dateTime);
		}

		public int GetLocalEndTimeSpan()
		{
			string endTime = DB.MonthRankingActivity.EndTime;
			if (string.IsNullOrEmpty(endTime))
			{
				return 0;
			}
			double totalSeconds = (MDateTime.Parse(endTime) - DateTime.Now).TotalSeconds;
			if (totalSeconds < 0.0)
			{
				return 0;
			}
			int num = Convert.ToInt32(totalSeconds);
			return (num < 1) ? num : (num + 2);
		}

		private void _SendServerTimeEvent(long endTimeSpan)
		{
			DispatcherMgr.Emit("MonthRankingActivity_GetServerTime", Mathf.CeilToInt((float)new TimeSpan(endTimeSpan).TotalSeconds));
		}

		private void _SendGetActivityInfoSuccEvent()
		{
			DispatcherMgr.Emit("MonthRanking_GetActivityInfo_Succ");
		}

		private void _SendGetRankingDataSuccEvent()
		{
			DispatcherMgr.Emit("MonthRanking_GetPersonalRankingData_Succ");
		}

		public void RefreshRankingData(List<PersonalMonthStarRankingData> datas)
		{
			DB.MonthRankingActivity.PersonalMonthStarRankingDatas.Clear(true);
			string monthRankingIssue = GetMonthRankingIssue(DB.MonthRankingActivity.StartTime);
			for (int i = 0; i < datas.Count; i++)
			{
				PersonalMonthStarRankingData personalMonthStarRankingData = datas[i];
				RankingData rankingData = new RankingData();
				rankingData.userID = personalMonthStarRankingData.userId;
				rankingData.isOwn = DB.Account.Userid.Equals(personalMonthStarRankingData.userId);
				string text;
				if (personalMonthStarRankingData.projection != null && personalMonthStarRankingData.projection.Account != null && !string.IsNullOrEmpty(personalMonthStarRankingData.projection.Account.name))
				{
					text = personalMonthStarRankingData.projection.Account.name;
				}
				else if (rankingData.isOwn)
				{
					text = DB.Account.GetName();
				}
				else
				{
					text = "Player" + personalMonthStarRankingData.userId.Substring(0, 8);
				}
				rankingData.name = text;
				rankingData.medal = personalMonthStarRankingData.projection.User.medal;
				int score = 0;
				if (personalMonthStarRankingData.projection.User.MonthIssueSumStar != null && personalMonthStarRankingData.projection.User.MonthIssueSumStar.ContainsKey(monthRankingIssue))
				{
					score = personalMonthStarRankingData.projection.User.MonthIssueSumStar[monthRankingIssue];
				}
				rankingData.score = score;
				rankingData.ranking = i;
				rankingData.sumScore = personalMonthStarRankingData.projection.User.sumStar;
				rankingData.rankingType = RankingType.PersonalMonth;
				rankingData.limit = cf_const.monthRankingActivityLimit;
				DB.MonthRankingActivity.PersonalMonthStarRankingDatas.Add(rankingData);
			}
			DB.MonthRankingActivity.PersonalMonthStarRankingDatas.Save();
			if (DB.MonthRankingActivity.State == 4 && !DB.MonthRankingActivity.IsCanReceive)
			{
				DB.MonthRankingActivity.IsCanReceive = true;
			}
			_SendGetRankingDataSuccEvent();
		}

		public List<RankingData> GetAllRankingData()
		{
			List<RankingData> list = new List<RankingData>(DB.MonthRankingActivity.PersonalMonthStarRankingDatas.Get());
			RankingData rankingData = list.FirstOrDefault((RankingData x) => x.isOwn);
			RankingData localOwnRankingData = GetLocalOwnRankingData();
			if (localOwnRankingData != null)
			{
				if (rankingData != null)
				{
					rankingData.score = Math.Max(rankingData.score, localOwnRankingData.score);
				}
				else
				{
					list.Add(localOwnRankingData);
				}
			}
			list.Sort((RankingData a, RankingData b) => b.score - a.score);
			if (list.Count > cf_const.monthRankingActivityLimit)
			{
				list.RemoveAt(list.Count - 1);
			}
			for (int i = 0; i < list.Count; i++)
			{
				list[i].ranking = i;
			}
			return list;
		}

		private RankingData GetLocalOwnRankingData()
		{
			RankingData rankingData = new RankingData();
			rankingData.userID = DB.Account.Userid;
			rankingData.name = DB.Account.GetName();
			rankingData.ranking = 0;
			rankingData.limit = cf_const.monthRankingActivityLimit;
			rankingData.sumScore = DB.User.SumStar;
			rankingData.medal = DB.User.Medal;
			rankingData.rankingType = RankingType.PersonalMonth;
			rankingData.isOwn = true;
			string monthRankingIssue = GetMonthRankingIssue(DB.MonthRankingActivity.StartTime);
			rankingData.score = DB.User.GetMonthIssueSumStar(monthRankingIssue);
			return rankingData;
		}

		public RankingData GetOwnRankingData()
		{
			List<RankingData> allRankingData = GetAllRankingData();
			RankingData rankingData = ((allRankingData != null) ? allRankingData.Find((RankingData x) => x.userID == DB.Account.Userid) : null);
			if (rankingData == null)
			{
				rankingData = GetLocalOwnRankingData();
				if (allRankingData == null)
				{
					rankingData.ranking = 0;
				}
			}
			return rankingData;
		}

		public cfgMonthRankingActivityReward GetRewardsCfg(int rankingIndex)
		{
			if (DB.MonthRankingActivity.RewardCfgGroupId > DataCfg.cfgMonthRankingActivityRewardGroups.Count)
			{
				DB.MonthRankingActivity.RewardCfgGroupId = 1;
			}
			List<cfgMonthRankingActivityReward> list = DataCfg.cfgMonthRankingActivityRewardGroups[DB.MonthRankingActivity.RewardCfgGroupId];
			int num = rankingIndex + 1;
			for (int i = 0; i < list.Count; i++)
			{
				if (num <= list[i].ranking)
				{
					return list[i];
				}
			}
			return null;
		}
	}
}
