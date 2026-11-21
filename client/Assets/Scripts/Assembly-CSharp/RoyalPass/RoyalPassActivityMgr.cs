using System;
using System.Collections.Generic;
using System.Linq;
using DataCenter;
using Festival;
using UnityEngine;
using UnityEngine.UI;

namespace RoyalPass
{
	public class RoyalPassActivityMgr : Singleton<RoyalPassActivityMgr>
	{
		public bool IsCloseNextIssue = true;

		public int CacheScore { get; set; }

		public void ShowDialog(ActivityState state)
		{
			_RemoveState(state);
		}

		public int JudgeAutoEject()
		{
			if (JudgeOpen())
			{
				if (JudgeHasState(ActivityState.ActivityDialog) || JudgeHasState(ActivityState.WaitingEnd))
				{
					return 1;
				}
				return (GetCanReceiveRewards().Count > 0) ? 1 : 0;
			}
			return 0;
		}

		public bool JudgeOpen()
		{
			if (DataManager.I.bLogined && DB.User.Maxlv + 1 > cf_const.royalPassActivityUnlockLv && !JudgeHasState(ActivityState.Ended) && !JudgeHasState(ActivityState.EarlyEnded))
			{
				return JudgeHasState(ActivityState.Started);
			}
			return false;
		}

		public bool JudgeHasState(ActivityState state)
		{
			return (DB.RoyalPassActivity.ActivityState & (int)state) != 0;
		}

		public int SetScore(int passLvNum)
		{
			if (JudgeOpen() && !JudgeHasState(ActivityState.WaitingEnd))
			{
				int num = cf_const.royalPassActivityUnitScore * passLvNum;
				DB.RoyalPassActivity.SaveScore(DB.RoyalPassActivity.Issue, num);
				CacheScore += num;
				return num;
			}
			return 0;
		}

		public void SetGrade(int grade, int exp)
		{
			DB.RoyalPassActivity.CurGrade = grade;
			DB.RoyalPassActivity.CurExp = exp;
		}

		public int GetEndTime()
		{
			if (JudgeOpen() && !JudgeHasState(ActivityState.WaitingEnd))
			{
				string endTime = DB.RoyalPassActivity.EndTime;
				if (!string.IsNullOrEmpty(endTime))
				{
					TimeSpan timeSpan = MDateTime.Parse(endTime) - DateTime.Now;
					double totalSeconds = timeSpan.TotalSeconds;
					if (totalSeconds >= 0.0)
					{
						int num = Convert.ToInt32(totalSeconds);
						if (num > 0)
						{
							return num + 2;
						}
						return num;
					}
				}
			}
			return 0;
		}

		public void ShowBuySuccessRewardsDialog(Action cBack)
		{
			int[,] array = RiceivePaidReward();
			if (array.GetLength(0) != 0)
			{
				RoyalPassActivityBuySuccessRewardsDialog royalPassActivityBuySuccessRewardsDialog = FestivalMgr.I.AddTopNode<RoyalPassActivityBuySuccessRewardsDialog>("Prefabs/Activity/RoyalPass/{0}_RoyalPassActivityBuySuccessRewardsDialog");
				royalPassActivityBuySuccessRewardsDialog.receiveCallback = cBack;
				royalPassActivityBuySuccessRewardsDialog.Show(array);
			}
		}

		public int[,] RiceivePaidReward()
		{
			int[,] result = new int[0, 2];
			if (!JudgeHasState(ActivityState.PaidSuccess) || !JudgeHasState(ActivityState.WaitingReceivePaidReward))
			{
				return result;
			}
			_RemoveState(ActivityState.WaitingReceivePaidReward);
			result = GetPaidReward();
			DBList<int> rewardsState = GetRewardsState();
			rewardsState[0] = ((rewardsState[0] == 1) ? 3 : 2);
			_End();
			int grade;
			GetGrade(out grade);
			LogEvent.I.Track("Master_Pass_Reward").Add("GradeNum", grade).Add("ClaimNum", 0)
				.Add("ActivityType", "Buy")
				.CommonAttr(true)
				.Send(false);
			return result;
		}

		public RewardState Receive(RewardItemType type, int id, bool isFree = true)
		{
			if (!isFree && !JudgeHasState(ActivityState.PaidSuccess))
			{
				return RewardState.None;
			}
			DBList<int> rewardsState = GetRewardsState();
			int num = 0;
			cfgRoyalPassActivityReward cfgRoyalPassActivityReward = null;
			foreach (KeyValuePair<int, cfgRoyalPassActivityReward> canReceiveReward in GetCanReceiveRewards())
			{
				num = rewardsState[canReceiveReward.Key];
				if (num == 3 || canReceiveReward.Value.id != id)
				{
					continue;
				}
				if (type == RewardItemType.Special)
				{
					rewardsState[canReceiveReward.Key] = 3;
					num = 3;
					cfgRoyalPassActivityReward = canReceiveReward.Value;
				}
				else
				{
					if (num == (isFree ? 1 : 2))
					{
						break;
					}
					num = ((!isFree) ? ((num == 1) ? 3 : 2) : ((num == 2) ? 3 : 1));
					rewardsState[canReceiveReward.Key] = num;
					cfgRoyalPassActivityReward = canReceiveReward.Value;
				}
				break;
			}
			_End();
			if (cfgRoyalPassActivityReward == null)
			{
				return (RewardState)num;
			}
			int grade;
			GetGrade(out grade);
			LogEvent.I.Track("Master_Pass_Reward").Add("GradeNum", grade).Add("ClaimNum", cfgRoyalPassActivityReward.grade)
				.Add("ActivityType", isFree ? "Free" : "Buy")
				.CommonAttr(true)
				.Send(false);
			return (RewardState)num;
		}

		private void _End()
		{
			if (JudgeHasState(ActivityState.WaitingEnd))
			{
				if (GetCanReceiveRewards().Count != 0)
				{
					return;
				}
				_AddState(ActivityState.Ended);
			}
			else
			{
				bool allReceived = true;
				GetRewardsState().Get().ForEach(delegate(int fe)
				{
					if (fe != 3)
					{
						allReceived = false;
					}
				});
				if (!allReceived)
				{
					return;
				}
				_AddState(ActivityState.EarlyEnded);
			}
			_SendGetActivitySuccEvent();
		}

		public List<RoyalPassActivityRewardInfo> GetActivityRewardInfo()
		{
			List<RoyalPassActivityRewardInfo> list = new List<RoyalPassActivityRewardInfo>();
			list.Add(new RoyalPassActivityRewardInfo
			{
				isSatisfyCondition = true,
				type = RewardItemType.Title
			});
			int grade;
			GetGrade(out grade);
			List<cfgRoyalPassActivityReward> cfgRoyalPassActivityRewards = DataCfg.cfgRoyalPassActivityRewards;
			DBList<int> rewardsState = GetRewardsState();
			bool isUnlockPaidReward = JudgeHasState(ActivityState.PaidSuccess) && !JudgeHasState(ActivityState.WaitingReceivePaidReward);
			for (int i = 0; i < cfgRoyalPassActivityRewards.Count; i++)
			{
				list.Add(new RoyalPassActivityRewardInfo
				{
					isSatisfyCondition = cfgRoyalPassActivityRewards[i].grade <= grade,
					isUnlockPaidReward = isUnlockPaidReward,
					state = (RewardState)rewardsState[i],
					type = (RewardItemType)cfgRoyalPassActivityRewards[i].type,
					cfg = cfgRoyalPassActivityRewards[i]
				});
			}
			return list;
		}

		public int GetGrade(out int grade)
		{
			grade = 0;
			int num = DB.RoyalPassActivity.GetScore(DB.RoyalPassActivity.Issue);
			List<cfgRoyalPassActivityReward> list = DataCfg.cfgRoyalPassActivityRewards.Where((cfgRoyalPassActivityReward w) => w.type == 2).ToList();
			int num2 = list.Count - 1;
			if (num2 < 1)
			{
				return 0;
			}
			for (int i = 0; i < num2; i++)
			{
				if (num < list[i].exp)
				{
					return num;
				}
				grade++;
				num -= list[i].exp;
			}
			return num;
		}

		public cfgRoyalPassActivityReward GetRewardCfg(int grade)
		{
			return DataCfg.cfgRoyalPassActivityRewards.Where((cfgRoyalPassActivityReward w) => w.type == 2).ToList().SingleOrDefault((cfgRoyalPassActivityReward sd) => sd.grade == grade);
		}

		public Dictionary<int, cfgRoyalPassActivityReward> GetCanReceiveRewards()
		{
			Dictionary<int, cfgRoyalPassActivityReward> dictionary = new Dictionary<int, cfgRoyalPassActivityReward>();
			int grade;
			GetGrade(out grade);
			List<cfgRoyalPassActivityReward> cfgRoyalPassActivityRewards = DataCfg.cfgRoyalPassActivityRewards;
			DBList<int> rewardsState = GetRewardsState();
			bool flag = JudgeHasState(ActivityState.PaidSuccess);
			for (int i = 0; i < cfgRoyalPassActivityRewards.Count; i++)
			{
				if (grade < cfgRoyalPassActivityRewards[i].grade)
				{
					return dictionary;
				}
				if (rewardsState[i] != 3)
				{
					if (flag)
					{
						dictionary[i] = cfgRoyalPassActivityRewards[i];
					}
					else if (rewardsState[i] != 1 && (cfgRoyalPassActivityRewards[i].type != 3 || rewardsState[i] != 0))
					{
						dictionary[i] = cfgRoyalPassActivityRewards[i];
					}
				}
			}
			return dictionary;
		}

		public DBList<int> GetRewardsState()
		{
			DBList<int> rewardsState = DB.RoyalPassActivity.RewardsState;
			int count = DataCfg.cfgRoyalPassActivityRewards.Count;
			int count2 = rewardsState.Count;
			if (count2 != count)
			{
				if (count < count2)
				{
					for (int num = count2 - 1; num >= count; num--)
					{
						rewardsState.RemoveAt(num, false);
					}
				}
				else
				{
					for (int i = count2 - 1; i < count; i++)
					{
						rewardsState.Add(0, false);
					}
				}
				rewardsState.Save();
			}
			return rewardsState;
		}

		public void PaidSuccess()
		{
			_AddState(ActivityState.PaidSuccess);
			_AddState(ActivityState.WaitingReceivePaidReward);
		}

		public void SendReward()
		{
			_AddState(ActivityState.SentRewards);
		}

		public void SentRewardFail()
		{
			_RemoveState(ActivityState.SentRewards);
		}

		public void ActivityData(RoyalPassActivityInfo info)
		{
			if (info == null)
			{
				return;
			}
			string issue = DB.RoyalPassActivity.Issue;
			bool flag = string.IsNullOrEmpty(issue) || JudgeHasState(ActivityState.Ended);
			bool flag2 = info.issue.Equals(issue);
			long endTimeSpan;
			if (flag && !flag2)
			{
				if (IsCloseNextIssue)
				{
					return;
				}
				_RemoveState(ActivityState.None);
				_AddState(ActivityState.ResetRewards);
				_AddState(ActivityState.ActivityDialog);
				_ResetBaseData(info);
				_ResetRewards();
				_SendGetActivitySuccEvent();
				endTimeSpan = info.endTimeSpan;
			}
			else
			{
				if (flag)
				{
					return;
				}
				if (!flag2 || info.endTimeSpan < 1)
				{
					_AddState(ActivityState.Started);
					if (GetCanReceiveRewards().Count == 0)
					{
						_AddState(ActivityState.Ended);
						DB.Bag.RestoreEnergyRecoverUpLimit();
					}
					else if (!JudgeHasState(ActivityState.WaitingEnd))
					{
						_AddState(ActivityState.WaitingEnd);
						DB.Bag.RestoreEnergyRecoverUpLimit();
					}
					_SendGetActivitySuccEvent();
					endTimeSpan = 0L;
				}
				else
				{
					if (JudgeHasState(ActivityState.EarlyEnded))
					{
						return;
					}
					_ResetBaseData(info);
					_SendGetActivitySuccEvent();
					endTimeSpan = info.endTimeSpan;
				}
			}
			_SendServerTimeEvent(endTimeSpan);
		}

		private void _ResetBaseData(RoyalPassActivityInfo info)
		{
			DB.RoyalPassActivity.Issue = info.issue;
			DB.RoyalPassActivity.StartTime = info.startTime;
			DB.RoyalPassActivity.EndTime = info.endTime;
			if (info.available)
			{
				_AddState(ActivityState.Started);
			}
			else
			{
				_RemoveState(ActivityState.Started);
			}
		}

		private void _ResetRewards()
		{
			if (JudgeHasState(ActivityState.ResetRewards))
			{
				_RemoveState(ActivityState.ResetRewards);
				DataCenter.RoyalPassActivity data = DB.RoyalPassActivity;
				List<cfgRoyalPassActivityReward> cfgRoyalPassActivityRewards = DataCfg.cfgRoyalPassActivityRewards;
				data.RewardsState.Clear(false);
				cfgRoyalPassActivityRewards.ForEach(delegate
				{
					data.RewardsState.Add(0, false);
				});
				data.RewardsState.Save();
			}
		}

		private void _SendGetActivitySuccEvent()
		{
			DispatcherMgr.Emit("RoyalPasHttp_GetActivity_Succ");
		}

		private void _SendServerTimeEvent(long endTimeSpan)
		{
			DispatcherMgr.Emit("RoyalPasHttp_GetServerTime", (int)new TimeSpan(endTimeSpan).TotalSeconds);
		}

		private void _AddState(ActivityState state)
		{
			if (state != ActivityState.None)
			{
				int activityState = DB.RoyalPassActivity.ActivityState;
				if ((activityState & (int)state) == 0)
				{
					DB.RoyalPassActivity.ActivityState = activityState + (int)state;
				}
			}
		}

		private void _RemoveState(ActivityState state)
		{
			if (state == ActivityState.None)
			{
				DB.RoyalPassActivity.ActivityState = 0;
				return;
			}
			int activityState = DB.RoyalPassActivity.ActivityState;
			if ((activityState & (int)state) != 0)
			{
				DB.RoyalPassActivity.ActivityState = activityState - (int)state;
			}
		}

		public void SetPaidRewardUI(Image medalImg, Text energyUpLimitTex, Image rewardBoxImg, int[,] rewards = null)
		{
			Sprite sprite = null;
			string text = null;
			if (rewards == null)
			{
				rewards = GetPaidReward();
			}
			for (int i = 0; i < rewards.GetLength(0); i++)
			{
				int num = rewards[i, 0];
				if (num == 23)
				{
					if (medalImg != null)
					{
						Global.GetContents(PropID.Medal, rewards[i, 1], out sprite, out text);
						medalImg.sprite = sprite;
					}
				}
				else if (num == 24)
				{
					if (energyUpLimitTex != null)
					{
						energyUpLimitTex.text = rewards[i, 1].ToString();
					}
				}
				else if (rewardBoxImg != null)
				{
					rewardBoxImg.sprite = FestivalMgr.I.LoadAsset<Sprite>("Festival/RoyalPassActivity/{0}/RewardBox");
				}
			}
		}

		public int[,] GetPaidReward()
		{
			cfgRoyalPassActivityReward cfgRoyalPassActivityReward = DataCfg.cfgRoyalPassActivityRewards.SingleOrDefault((cfgRoyalPassActivityReward sd) => sd.grade == 0);
			if (cfgRoyalPassActivityReward == null)
			{
				return new int[0, 2];
			}
			return cfgRoyalPassActivityReward.paidReward;
		}

		public List<cfgRoyalPassActivityReward> GetActivityRewards()
		{
			return DataCfg.cfgRoyalPassActivityRewards;
		}

		public void CheckReissue()
		{
			List<cfgRoyalPassActivityReward> cfgRoyalPassActivityRewards = DataCfg.cfgRoyalPassActivityRewards;
			DBList<int> rewardsState = GetRewardsState();
			for (int i = 0; i < cfgRoyalPassActivityRewards.Count; i++)
			{
				if (rewardsState[i] == 0)
				{
					continue;
				}
				int type = cfgRoyalPassActivityRewards[i].type;
				if (rewardsState[i] != 2)
				{
					if (rewardsState[i] == 1)
					{
						_ReissueMedal(cfgRoyalPassActivityRewards[i].freeReward);
						continue;
					}
					if (type == 2)
					{
						_ReissueMedal(cfgRoyalPassActivityRewards[i].freeReward);
					}
				}
				if (JudgeHasState(ActivityState.PaidSuccess))
				{
					_ReissueMedal(cfgRoyalPassActivityRewards[i].paidReward);
				}
			}
		}

		private void _ReissueMedal(int[,] rewards)
		{
			if (rewards == null)
			{
				return;
			}
			for (int i = 0; i < rewards.GetLength(0); i++)
			{
				if (rewards[i, 0] == 23)
				{
					DB.Bag.ReissueMedal(rewards[i, 1]);
				}
			}
		}
	}
}
