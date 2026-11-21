using System;
using System.Collections.Generic;
using System.Linq;
using DataCenter;

namespace TreasureHunt
{
	public class TreasureHuntActivityMgr : Singleton<TreasureHuntActivityMgr>
	{
		public void ShowDialog(ActivityState state)
		{
			_RemoveState(state);
		}

		public int JudgeAutoEject()
		{
			if (JudgeOpen())
			{
				if (JudgeHasState(ActivityState.WaitingEnd))
				{
					return 2;
				}
				if (JudgeHasState(ActivityState.PosterDialog))
				{
					return 1;
				}
				List<cfgTreasureHuntActivityReward> canReciveActivityRewards = _GetCanReciveActivityRewards();
				return (canReciveActivityRewards.Count > 0) ? 2 : 0;
			}
			return 0;
		}

		public bool JudgeOpen()
		{
			if (DataManager.I.bLogined && JudgeUnlock() && !JudgeHasState(ActivityState.Ended))
			{
				return JudgeHasState(ActivityState.Started);
			}
			return false;
		}

		public bool JudgeHasState(ActivityState state)
		{
			return (DB.TreasureHuntActivity.ActivityState & (int)state) != 0;
		}

		public int SetScore(int modelNum)
		{
			if (JudgeOpen() && JudgeHasState(ActivityState.JoinedTeam) && !JudgeHasState(ActivityState.WaitingEnd))
			{
				int num = cf_const.treasureHuntActivityUnitScore * modelNum;
				DB.TreasureHuntActivity.SaveScore(DB.TreasureHuntActivity.TeamId, DB.TreasureHuntActivity.Issue, num);
				return num;
			}
			return 0;
		}

		public int GetEndTime()
		{
			if (JudgeOpen() && !JudgeHasState(ActivityState.WaitingEnd))
			{
				string endTime = DB.TreasureHuntActivity.EndTime;
				if (!string.IsNullOrEmpty(endTime))
				{
					DateTime dateTime = MDateTime.Parse(endTime);
					double totalSeconds = (dateTime - DateTime.Now).TotalSeconds;
					if (totalSeconds >= 0.0)
					{
						int num = Convert.ToInt32(totalSeconds);
						return (num < 1) ? num : (num + 2);
					}
				}
			}
			return 0;
		}

		public void LevelTeam()
		{
			if (!JudgeOpen() || JudgeHasState(ActivityState.WaitingEnd) || !JudgeHasState(ActivityState.JoinedTeam))
			{
				return;
			}
			bool flag = TreasureHuntHttpMgr.I.AbortGetActivityRequest();
			_RemoveState(ActivityState.JoinedTeam);
			DB.TreasureHuntActivity.TeamId = string.Empty;
			if (flag)
			{
				TreasureHuntHttpMgr.I.GetActivity();
			}
		}

		public void JoinTeam(string teamId)
		{
			if (!JudgeOpen() || JudgeHasState(ActivityState.WaitingEnd) || JudgeHasState(ActivityState.JoinedTeam))
			{
				return;
			}
			bool flag = TreasureHuntHttpMgr.I.AbortGetActivityRequest();
			_AddState(ActivityState.JoinedTeam);
			DB.TreasureHuntActivity.TeamId = teamId;
			if (flag)
			{
				TreasureHuntHttpMgr.I.GetActivity();
			}
		}

		public List<MemberInfo> GetMembers()
		{
			if (!JudgeOpen() || JudgeHasState(ActivityState.WaitingEnd) || !JudgeHasState(ActivityState.JoinedTeam))
			{
				return null;
			}
			string teamId = DB.TreasureHuntActivity.TeamId;
			string issue = DB.TreasureHuntActivity.Issue;
			List<MemberInfo> list = DB.TreasureHuntActivity.MembersInfo.Get();
			MemberInfo memberInfo = list.FirstOrDefault((MemberInfo fd) => fd.isOwn);
			if (memberInfo == null)
			{
				memberInfo = new MemberInfo();
				list.Add(memberInfo);
			}
			memberInfo.isOwn = true;
			memberInfo.name = DB.Account.GetName();
			int num = memberInfo.score;
			int score = DB.TreasureHuntActivity.GetScore(teamId, issue);
			if (num <= score)
			{
				num = score;
			}
			memberInfo.score = num;
			list.Sort((MemberInfo a, MemberInfo b) => b.score - a.score);
			return list;
		}

		public DBList<bool> GetActivityRewardsReceivedState(out List<cfgTreasureHuntActivityReward> cfgs)
		{
			DBList<bool> isReceivedActivityReward = DB.TreasureHuntActivity.IsReceivedActivityReward;
			cfgs = GetActivityRewards();
			if (cfgs != null && isReceivedActivityReward != null)
			{
				int count = cfgs.Count;
				int count2 = isReceivedActivityReward.Count;
				if (count2 != count)
				{
					if (count2 < count)
					{
						int num = count - count2;
						for (int i = 0; i < num; i++)
						{
							isReceivedActivityReward.Add(false);
						}
					}
					else
					{
						for (int num2 = count2 - 1; num2 >= count; num2--)
						{
							isReceivedActivityReward.RemoveAt(num2);
						}
					}
				}
				return isReceivedActivityReward;
			}
			return null;
		}

		public List<cfgTreasureHuntActivityReward> GetActivityRewards()
		{
			int gId = DB.TreasureHuntActivity.ActivityRewardGroupId;
			if (!DataCfg.cfgTreasureHuntActivityRewards.Exists((cfgTreasureHuntActivityReward e) => e.groupId == gId))
			{
				gId = _ResetActivityRewardGroupId();
			}
			return DataCfg.cfgTreasureHuntActivityRewards.Where((cfgTreasureHuntActivityReward w) => w.groupId == gId).ToList();
		}

		public List<cfgTreasureHuntActivityRankingReward> GetActivityRankingRewards()
		{
			int gId = DB.TreasureHuntActivity.ActivityRankingRewardGroupId;
			if (!DataCfg.cfgTreasureHuntActivityRankingRewards.Exists((cfgTreasureHuntActivityRankingReward e) => e.groupId == gId))
			{
				gId = _ResetActivityRankingRewardGroupId();
			}
			return DataCfg.cfgTreasureHuntActivityRankingRewards.Where((cfgTreasureHuntActivityRankingReward w) => w.groupId == gId).ToList();
		}

		public int[,] ReceiveActivityReward(int id)
		{
			int[,] result = new int[0, 2];
			List<cfgTreasureHuntActivityReward> cfgs;
			DBList<bool> activityRewardsReceivedState = GetActivityRewardsReceivedState(out cfgs);
			if (cfgs != null)
			{
				for (int i = 0; i < cfgs.Count; i++)
				{
					if (cfgs[i].id == id)
					{
						if (!activityRewardsReceivedState[i])
						{
							activityRewardsReceivedState[i] = true;
							return cfgs[i].reward;
						}
						break;
					}
				}
			}
			return result;
		}

		public bool End(out int[,] rewards)
		{
			rewards = new int[0, 2];
			if (JudgeHasState(ActivityState.WaitingEnd) && !JudgeHasState(ActivityState.Ended))
			{
				_AddState(ActivityState.Ended);
				_SendGetActivitySuccEvent();
				List<cfgTreasureHuntActivityReward> canReciveActivityRewards = _GetCanReciveActivityRewards();
				if (canReciveActivityRewards != null)
				{
					for (int i = 0; i < canReciveActivityRewards.Count; i++)
					{
						rewards = Util.Helper.MergerReward(rewards, canReciveActivityRewards[i].reward);
					}
				}
				int totalScore = 0;
				int min = 0;
				List<MemberInfo> members = GetMembers();
				if (members != null)
				{
					members.ForEach(delegate(MemberInfo fe)
					{
						totalScore += fe.score;
					});
					List<cfgTreasureHuntActivityReward> activityRewards = GetActivityRewards();
					activityRewards.ForEach(delegate(cfgTreasureHuntActivityReward fe)
					{
						if (min == 0)
						{
							min = fe.condition;
						}
					});
					if (min <= totalScore)
					{
						int num = members.FindIndex((MemberInfo fi) => fi.isOwn);
						List<cfgTreasureHuntActivityRankingReward> activityRankingRewards = GetActivityRankingRewards();
						if (activityRankingRewards != null)
						{
							for (int j = 0; j < activityRankingRewards.Count; j++)
							{
								if (activityRankingRewards[j].ranking == num + 1)
								{
									rewards = Util.Helper.MergerReward(rewards, activityRankingRewards[j].reward);
									return true;
								}
							}
						}
					}
				}
				return true;
			}
			return false;
		}

		public void ActivityData(TreasureHuntActivityInfo info)
		{
			string issue = DB.TreasureHuntActivity.Issue;
			bool flag = string.IsNullOrEmpty(issue) || JudgeHasState(ActivityState.Ended);
			if (info == null || info.issue == null)
			{
				return;
			}
			bool flag2 = info.issue.Equals(issue);
			if (flag && !flag2)
			{
				_RemoveState(ActivityState.None);
				_AddState(ActivityState.ResetRewards);
				_AddState(ActivityState.PosterDialog);
				_ResetBaseData(info);
				_ResetRewards();
				_ResetJoinedTeamData(info);
				_SendGetActivitySuccEvent();
				_SendServerTimeEvent(info.endTimeSpan);
				DB.TreasureHuntActivity.MembersInfo.Clear();
				return;
			}
			if (flag)
			{
				return;
			}
			if (flag2 && info.endTimeSpan > 0)
			{
				_ResetBaseData(info);
				_ResetJoinedTeamData(info);
				_SendGetActivitySuccEvent();
				_SendServerTimeEvent(info.endTimeSpan);
				return;
			}
			_AddState(ActivityState.Started);
			if (JudgeHasState(ActivityState.JoinedTeam))
			{
				if (!JudgeHasState(ActivityState.WaitingEnd))
				{
					_AddState(ActivityState.WaitingEnd);
				}
			}
			else
			{
				_AddState(ActivityState.Ended);
			}
			_SendGetActivitySuccEvent();
			_SendServerTimeEvent(0L);
		}

		public void MembersData(string teamId, string issue, List<TreasureHuntMemberInfo> datas)
		{
			_ResetMembersInfo();
			datas.ForEach(delegate(TreasureHuntMemberInfo fe)
			{
				MemberInfo memberInfo = new MemberInfo();
				DB.TreasureHuntActivity.MembersInfo.Add(memberInfo, false);
				bool flag = DB.Account.Userid.Equals(fe.userId);
				memberInfo.isOwn = flag;
				if (flag)
				{
					memberInfo.name = DB.Account.GetName();
					int score = DB.TreasureHuntActivity.GetScore(teamId, issue);
					int num = fe.projection.TreasureHuntActivity.allScores[teamId][issue];
					if (num <= score)
					{
						num = score;
					}
					memberInfo.score = num;
				}
				else
				{
					memberInfo.name = fe.projection.Account.name;
					memberInfo.score = fe.projection.TreasureHuntActivity.allScores[teamId][issue];
				}
			});
			DB.TreasureHuntActivity.MembersInfo.Save();
			_SendGetTeamMembersSuccEvent();
		}

		public bool JudgeUnlock()
		{
			return cf_const.treasureHuntActivityUnlockLv <= DB.User.Curlv + 1;
		}

		private List<cfgTreasureHuntActivityReward> _GetCanReciveActivityRewards()
		{
			List<cfgTreasureHuntActivityReward> list = new List<cfgTreasureHuntActivityReward>();
			int totalScore = 0;
			List<MemberInfo> members = GetMembers();
			if (members != null)
			{
				members.ForEach(delegate(MemberInfo fe)
				{
					totalScore += fe.score;
				});
				List<cfgTreasureHuntActivityReward> cfgs;
				DBList<bool> activityRewardsReceivedState = GetActivityRewardsReceivedState(out cfgs);
				if (cfgs != null)
				{
					for (int i = 0; i < cfgs.Count; i++)
					{
						if (!activityRewardsReceivedState[i] && cfgs[i].condition <= totalScore)
						{
							list.Add(cfgs[i]);
						}
					}
				}
			}
			return list;
		}

		private void _ResetBaseData(TreasureHuntActivityInfo info)
		{
			DB.TreasureHuntActivity.Issue = info.issue;
			DB.TreasureHuntActivity.StartTime = info.startTime;
			DB.TreasureHuntActivity.EndTime = info.endTime;
			if (info.available)
			{
				_AddState(ActivityState.Started);
			}
			else
			{
				_RemoveState(ActivityState.Started);
			}
		}

		private void _ResetMembersInfo()
		{
			DB.TreasureHuntActivity.MembersInfo.Clear();
		}

		private void _ResetJoinedTeamData(TreasureHuntActivityInfo info)
		{
			if (info.myTeam == null)
			{
				DB.TreasureHuntActivity.TeamId = string.Empty;
				_RemoveState(ActivityState.JoinedTeam);
			}
			else
			{
				DB.TreasureHuntActivity.TeamId = info.myTeam.id;
				_AddState(ActivityState.JoinedTeam);
			}
		}

		private void _ResetRewards()
		{
			if (JudgeHasState(ActivityState.ResetRewards))
			{
				_RemoveState(ActivityState.ResetRewards);
				_ResetActivityRewardGroupId();
				List<cfgTreasureHuntActivityReward> activityRewards = GetActivityRewards();
				DB.TreasureHuntActivity.IsReceivedActivityReward.Clear();
				for (int i = 0; i < activityRewards.Count; i++)
				{
					DB.TreasureHuntActivity.IsReceivedActivityReward.Add(false);
				}
				_ResetActivityRankingRewardGroupId();
			}
		}

		private int _ResetActivityRewardGroupId()
		{
			int gId = DB.TreasureHuntActivity.ActivityRewardGroupId;
			int num = (DataCfg.cfgTreasureHuntActivityRewards.Exists((cfgTreasureHuntActivityReward e) => e.groupId == gId + 1) ? (gId + 1) : 1);
			DB.TreasureHuntActivity.ActivityRewardGroupId = num;
			return num;
		}

		private int _ResetActivityRankingRewardGroupId()
		{
			int gId = DB.TreasureHuntActivity.ActivityRankingRewardGroupId;
			int num = (DataCfg.cfgTreasureHuntActivityRankingRewards.Exists((cfgTreasureHuntActivityRankingReward e) => e.groupId == gId + 1) ? (gId + 1) : 1);
			DB.TreasureHuntActivity.ActivityRankingRewardGroupId = num;
			return num;
		}

		private void _SendGetActivitySuccEvent()
		{
			DispatcherMgr.Emit("TreasureHuntHttp_GetActivity_Succ");
		}

		private void _SendGetTeamMembersSuccEvent()
		{
			DispatcherMgr.Emit("TreasureHuntHttp_GetTeamMembers_Succ");
		}

		private void _SendServerTimeEvent(long endTimeSpan)
		{
			TimeSpan timeSpan = new TimeSpan(endTimeSpan);
			DispatcherMgr.Emit("TreasureHuntActivity_GetServerTime", (int)timeSpan.TotalSeconds);
		}

		private void _AddState(ActivityState state)
		{
			if (state != 0)
			{
				int activityState = DB.TreasureHuntActivity.ActivityState;
				if ((activityState & (int)state) == 0)
				{
					DB.TreasureHuntActivity.ActivityState = activityState + (int)state;
				}
			}
		}

		private void _RemoveState(ActivityState state)
		{
			if (state == ActivityState.None)
			{
				DB.TreasureHuntActivity.ActivityState = 0;
				return;
			}
			int activityState = DB.TreasureHuntActivity.ActivityState;
			if ((activityState & (int)state) != 0)
			{
				DB.TreasureHuntActivity.ActivityState = activityState - (int)state;
			}
		}
	}
}
