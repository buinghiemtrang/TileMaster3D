using System;
using System.Collections.Generic;
using System.Linq;
using BestHTTP;
using DataCenter;
using Newtonsoft.Json;
using OwnHttp;
using RoyalPass;
using UnityEngine;

namespace ShortBattlePass
{
	public class ShortBattlePassMgr : Singleton<ShortBattlePassMgr>
	{
		public int CacheScore { get; set; }

		public void Init()
		{
			if (!cf_const.IsOpenShortBattlePass || !DataManager.I.bLogined || DB.User.Maxlv + 1 <= cf_const.ShortBattlePassUnlockLevel || Singleton<RoyalPassActivityMgr>.I.JudgeOpen() || !ServerDateTimeMgr.I.IsServerTimeSynced)
			{
				return;
			}
			DateTime currentUtcTime = _GetCurrentUtcTime();
			ShortBattlePassData data = DB.ShortBattlePass.Data;
			if (!string.IsNullOrEmpty(data.Issue) && JudgeHasState(ActivityState.Started))
			{
				if (currentUtcTime < DateTime.UtcNow.ParseFull(data.EndTime, true))
				{
					return;
				}
				if (JudgeHaveCanReceiveRewards())
				{
					if (JudgeHasState(ActivityState.WaitingReceiveReward))
					{
						return;
					}
					_AddState(ActivityState.WaitingReceiveReward);
					return;
				}
				_EndActivity();
			}
			if (JudgeHasState(ActivityState.Cooling))
			{
				if (currentUtcTime < DateTime.UtcNow.ParseFull(data.CoolingEndTime, true))
				{
					return;
				}
				_RemoveState(ActivityState.Cooling);
			}
			List<cfgShortBattlePass> list = new List<cfgShortBattlePass>();
			List<cfgShortBattlePass> cfgShortBattlePasss = DataCfg.cfgShortBattlePasss;
			bool flag = false;
			for (int i = 0; i < cfgShortBattlePasss.Count; i++)
			{
				cfgShortBattlePass cfgShortBattlePass = cfgShortBattlePasss[i];
				if (cfgShortBattlePass.Issue.Equals("default"))
				{
					list.Add(cfgShortBattlePass);
					continue;
				}
				DateTime dateTime = DateTime.UtcNow.ParseFull(cfgShortBattlePass.Issue, true);
				if (currentUtcTime < dateTime)
				{
					flag = true;
				}
				else if (!(currentUtcTime >= dateTime.AddDays(cfgShortBattlePass.KeepDuration)))
				{
					_RefreshData(currentUtcTime, cfgShortBattlePass);
					return;
				}
			}
			if (flag || list.Count <= 0)
			{
				return;
			}
			cfgShortBattlePass config = list[0];
			for (int j = 0; j < list.Count; j++)
			{
				if (data.Duration == list[j].KeepDuration)
				{
					config = list[j];
					break;
				}
			}
			_RefreshData(currentUtcTime, config);
		}

		public bool JudgeOpen()
		{
			return JudgeHasState(ActivityState.Started);
		}

		public bool JudgeTimeEnd()
		{
			if (!JudgeHasState(ActivityState.Started))
			{
				return true;
			}
			return JudgeHasState(ActivityState.WaitingReceiveReward);
		}

		public int JudgeAutoEject()
		{
			if (JudgeHasState(ActivityState.Started))
			{
				if (JudgeHasState(ActivityState.ActivityDialog))
				{
					return 1;
				}
				return JudgeHaveCanReceiveRewards() ? 1 : 0;
			}
			return 0;
		}

		public void ShowDialog()
		{
			_RemoveState(ActivityState.ActivityDialog);
		}

		public void SetScore(int clearanceNumber)
		{
			if (!JudgeTimeEnd())
			{
				int num = cf_const.ShortBattlePassUnitScore * clearanceNumber;
				ShortBattlePassData data = DB.ShortBattlePass.Data;
				data.Score += num;
				int grade;
				int residualExp = _GetGrade(out grade);
				data.Grade = grade;
				data.ResidualExp = residualExp;
				DB.ShortBattlePass.SaveData();
				CacheScore += num;
			}
		}

		public int GetEndTime()
		{
			if (!JudgeTimeEnd())
			{
				string endTime = DB.ShortBattlePass.Data.EndTime;
				if (!string.IsNullOrEmpty(endTime))
				{
					TimeSpan timeSpan = DateTime.UtcNow.ParseFull(endTime, true) - _GetCurrentUtcTime();
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

		public bool JudgeHasState(ActivityState state)
		{
			return (DB.ShortBattlePass.Data.ActivityState & (int)state) != 0;
		}

		public List<RewardInfo> GetRewardInfos()
		{
			List<RewardInfo> list = new List<RewardInfo>();
			list.Add(new RewardInfo
			{
				IsSatisfyCondition = true,
				Type = RewardItemType.Title
			});
			List<cfgShortBattlePassReward> list2 = _GetRewardConfigs();
			List<int> rewardsState = DB.ShortBattlePass.Data.RewardsState;
			int grade = DB.ShortBattlePass.Data.Grade;
			bool isPaid = JudgeHasState(ActivityState.CostSuccess);
			for (int i = 0; i < list2.Count; i++)
			{
				cfgShortBattlePassReward cfgShortBattlePassReward = list2[i];
				list.Add(new RewardInfo
				{
					IsSatisfyCondition = cfgShortBattlePassReward.Grade <= grade,
					IsPaid = isPaid,
					State = (RewardState)rewardsState[i],
					Type = (RewardItemType)cfgShortBattlePassReward.Type,
					Config = cfgShortBattlePassReward
				});
			}
			return list;
		}

		public cfgShortBattlePassReward GetRewardConfigByGrade(int grade)
		{
			List<cfgShortBattlePassReward> list = _GetRewardConfigs();
			cfgShortBattlePassReward cfgShortBattlePassReward = list.SingleOrDefault((cfgShortBattlePassReward sd) => sd.Type == 1 && sd.Grade == grade);
			if (cfgShortBattlePassReward == null)
			{
				cfgShortBattlePassReward = ((list.Count >= 1) ? list[list.Count - 1] : null);
			}
			return cfgShortBattlePassReward;
		}

		private List<cfgShortBattlePassReward> _GetRewardConfigs()
		{
			List<cfgShortBattlePassReward> value;
			DataCfg.cfgShortBattlePassRewards.TryGetValue(DB.ShortBattlePass.Data.RewardGroupId, out value);
			if (value == null)
			{
				foreach (KeyValuePair<int, List<cfgShortBattlePassReward>> cfgShortBattlePassReward in DataCfg.cfgShortBattlePassRewards)
				{
					DB.ShortBattlePass.Data.RewardGroupId = cfgShortBattlePassReward.Key;
					DB.ShortBattlePass.SaveData();
					return cfgShortBattlePassReward.Value;
				}
				return new List<cfgShortBattlePassReward>();
			}
			return value;
		}

		public void SendTeamReward()
		{
			if (!JudgeHasState(ActivityState.CostSuccess) || JudgeHasState(ActivityState.SentRewards))
			{
				return;
			}
			OwnSingleHttp ownSingleHttp = CheckRepeatRequestsMgr.I.Creat("ShortBattlePass_Send_Team_Reward", HTTPMethods.Post, "https://service.fuqiangame.com/team/CreateTeamBoard", delegate(ResultInfo info)
			{
				OwnHttp.ServerDataRootBase serverDataRootBase = ParsingServerData.SendInfo(info);
				if (serverDataRootBase != null && serverDataRootBase.code == 1)
				{
					_AddState(ActivityState.SentRewards);
				}
			});
			if (ownSingleHttp != null)
			{
				ownSingleHttp.AddHeader("Authorization", "Bearer " + DataManager.I.Token).AddHeader("Content-Type", "application/json");
				ownSingleHttp.AddParameter(JsonConvert.SerializeObject(new
				{
					UserName = DB.Account.GetName(),
					Medal = DB.User.Medal,
					Type = "ShortBattlePass",
					Content = "I got the Master Pass!",
					Extension = "{'1':'ShortBattlePass_Send_Team_Reward'}"
				})).AddCustomTimeout(15f).Send();
			}
		}

		public void PaidSuccess()
		{
			_AddState(ActivityState.CostSuccess);
			_AddState(ActivityState.WaitingReceivePaidReward);
		}

		public bool ShowBuySuccessRewardsDialog(Action callback)
		{
			int[,] array = _ReceivePaidReward();
			if (array.GetLength(0) != 0)
			{
				ShortBattlePassBuySuccessRewardsDialog.Create(array).ReceivedCallback = callback;
			}
			return array.GetLength(0) != 0;
		}

		public int[,] GetCostReward()
		{
			List<cfbt_Stores> cfgs = Global.pStores.GetCfgs(BuySpotType.ShortBattlePass, GiftType.ShortBattlePass);
			if (cfgs.Count < 1)
			{
				return new int[0, 2];
			}
			return cfgs[0].item.ToInt();
		}

		private int[,] _ReceivePaidReward()
		{
			int[,] result = new int[0, 2];
			if (!JudgeHasState(ActivityState.CostSuccess) || !JudgeHasState(ActivityState.WaitingReceivePaidReward))
			{
				return result;
			}
			_RemoveState(ActivityState.WaitingReceivePaidReward);
			_JudgeEnd();
			LogEvent.I.Track("Little_Master_Pass_Reward").Add("GradeNum", DB.ShortBattlePass.Data.Grade).Add("ClaimNum", 0).Add("ActivityType", "Buy")
				.CommonAttr(true)
				.Send(false);
			return GetCostReward();
		}

		public RewardState Receive(RewardItemType type, int id, bool isFree = true)
		{
			if (!isFree && !JudgeHasState(ActivityState.CostSuccess))
			{
				return RewardState.None;
			}
			List<int> rewardsState = DB.ShortBattlePass.Data.RewardsState;
			List<cfgShortBattlePassReward> list = _GetRewardConfigs();
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				cfgShortBattlePassReward cfgShortBattlePassReward = list[i];
				if (cfgShortBattlePassReward.id != id || cfgShortBattlePassReward.Type != (int)type)
				{
					continue;
				}
				int num2 = rewardsState[i];
				if (num2 == 3)
				{
					num = 3;
					continue;
				}
				if (type == RewardItemType.Special)
				{
					num2 = 3;
					rewardsState[i] = num2;
				}
				else
				{
					int num3 = (isFree ? 1 : 2);
					if (num2 == num3)
					{
						num = num3;
						continue;
					}
					num2 = ((!isFree) ? ((num2 == 1) ? 3 : 2) : ((num2 == 2) ? 3 : 1));
					rewardsState[i] = num2;
				}
				DB.ShortBattlePass.SaveData();
				_JudgeEnd();
				LogEvent.I.Track("Little_Master_Pass_Reward").Add("GradeNum", DB.ShortBattlePass.Data.Grade).Add("ClaimNum", cfgShortBattlePassReward.Grade).Add("ActivityType", isFree ? "Free" : "Buy")
					.CommonAttr(true)
					.Send(false);
				return (RewardState)num2;
			}
			_JudgeEnd();
			return (RewardState)num;
		}

		private void _JudgeEnd()
		{
			DispatcherMgr.Emit("ShortBattlePass_Received_Reward");
			if (!JudgeHasState(ActivityState.WaitingReceiveReward))
			{
				List<int> rewardsState = DB.ShortBattlePass.Data.RewardsState;
				for (int i = 0; i < rewardsState.Count; i++)
				{
					if (rewardsState[i] != 3)
					{
						return;
					}
				}
				_EndActivity();
			}
			else if (!JudgeHaveCanReceiveRewards())
			{
				_EndActivity();
			}
		}

		private void _EndActivity()
		{
			_RemoveState(ActivityState.Started);
			_AddState(ActivityState.Ended);
			_AddState(ActivityState.Cooling);
		}

		public bool JudgeHaveCanReceiveRewards()
		{
			List<cfgShortBattlePassReward> list = _GetRewardConfigs();
			List<int> rewardsState = DB.ShortBattlePass.Data.RewardsState;
			int grade = DB.ShortBattlePass.Data.Grade;
			bool flag = JudgeHasState(ActivityState.CostSuccess);
			for (int i = 0; i < list.Count; i++)
			{
				cfgShortBattlePassReward cfgShortBattlePassReward = list[i];
				if (grade < cfgShortBattlePassReward.Grade)
				{
					return false;
				}
				int num = rewardsState[i];
				if (num != 3)
				{
					if (flag)
					{
						return true;
					}
					if (num != 1 && (num != 0 || cfgShortBattlePassReward.Type != 2))
					{
						return true;
					}
				}
			}
			return false;
		}

		private void _RefreshData(DateTime curTime, cfgShortBattlePass config)
		{
			DB.ShortBattlePass.Data = new ShortBattlePassData();
			ShortBattlePassData data = DB.ShortBattlePass.Data;
			data.Issue = config.Issue;
			data.StartTime = curTime.ToStringFull();
			data.EndTime = curTime.AddDays(config.KeepDuration).ToStringFull();
			data.CoolingEndTime = curTime.AddDays(config.CoolDownTime + config.KeepDuration).ToStringFull();
			data.ActivityState = 3;
			data.RewardGroupId = config.Reward;
			data.Duration = config.KeepDuration;
			if (data.RewardsState == null)
			{
				data.RewardsState = new List<int>();
			}
			else
			{
				data.RewardsState.Clear();
			}
			_GetRewardConfigs().ForEach(delegate
			{
				data.RewardsState.Add(0);
			});
			DB.ShortBattlePass.SaveData();
		}

		private int _GetGrade(out int grade)
		{
			grade = 1;
			int num = DB.ShortBattlePass.Data.Score;
			List<cfgShortBattlePassReward> list = _GetRewardConfigs();
			int count = list.Count;
			int result = 0;
			for (int i = 0; i < count; i++)
			{
				cfgShortBattlePassReward cfgShortBattlePassReward = list[i];
				if (cfgShortBattlePassReward.Type == 1 && cfgShortBattlePassReward.Exp != 0)
				{
					result = num;
					if (num < cfgShortBattlePassReward.Exp)
					{
						break;
					}
					grade++;
					num -= cfgShortBattlePassReward.Exp;
					result = num;
				}
			}
			grade = Math.Min(grade, count - 1);
			return result;
		}

		private DateTime _GetCurrentUtcTime()
		{
			return ServerDateTimeMgr.I.NetTimeUtc;
		}

		private void _AddState(ActivityState state)
		{
			if (state != ActivityState.None)
			{
				ShortBattlePassData data = DB.ShortBattlePass.Data;
				if ((data.ActivityState & (int)state) == 0)
				{
					data.ActivityState += (int)state;
					DB.ShortBattlePass.SaveData();
				}
			}
		}

		private void _RemoveState(ActivityState state)
		{
			ShortBattlePassData data = DB.ShortBattlePass.Data;
			if (state == ActivityState.None)
			{
				data.ActivityState = 0;
			}
			else
			{
				if ((data.ActivityState & (int)state) == 0)
				{
					return;
				}
				data.ActivityState -= (int)state;
			}
			DB.ShortBattlePass.SaveData();
		}
	}
}
