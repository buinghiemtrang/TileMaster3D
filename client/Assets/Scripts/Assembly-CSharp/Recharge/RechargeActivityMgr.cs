using System;
using System.Collections.Generic;

namespace Recharge
{
	public class RechargeActivityMgr : Singleton<RechargeActivityMgr>
	{
		public void RefreshActivityData()
		{
			string newIssue = _GetNewIssue();
			if (!newIssue.Equals(DB.RechargeActivity.Issue) && _JudgeActivityEnded() && !_JudgeHasCanReceiveRewards())
			{
				_ResetActivityData(newIssue);
			}
		}

		public bool JudgeInActivity()
		{
			DateTime now = DateTime.Now;
			DateTime t = now.ParseFull(DB.RechargeActivity.StartTime, true);
			DateTime t2 = now.ParseFull(DB.RechargeActivity.EndTime, true);
			return now > t && now < t2;
		}

		public bool JudgeUnlocked()
		{
			return DB.User.Maxlv + 1 > cf_const.RechargeActivityUnlockLv;
		}

		public bool JudgeForceClose()
		{
			return cf_const.IsForceCloseRechargeActivity;
		}

		public void AddRechargeValue(float value)
		{
			DB.RechargeActivity.CumulativeRecharge += value;
		}

		public List<RewardInfo> GetRewardInfos()
		{
			List<RewardInfo> list = new List<RewardInfo>();
			List<cfgRechargeActivityReward> rewardCfgs = _GetRewardCfgs();
			for (int i = 0; i < rewardCfgs.Count; i++)
			{
				cfgRechargeActivityReward cfg = rewardCfgs[i];
				RewardInfo rewardInfo = new RewardInfo();
				rewardInfo.Cfg = cfg;
				rewardInfo.IsReceived = _JudgeReceived(cfg);
				rewardInfo.IsSatisfyReceivingCondition = _JudgeSatisfyReceivingCondition(cfg);
				float num = cfg.Condition - DB.RechargeActivity.CumulativeRecharge;
				rewardInfo.Difference = ((num >= 0f) ? num : 0f);
				list.Add(rewardInfo);
			}
			return list;
		}

		public int JudgeAutoEject()
		{
			if (!cf_const.IsForceCloseRechargeActivity && JudgeUnlocked() && JudgeInActivity())
			{
				return _JudgeHasCanReceiveRewards() ? 1 : 0;
			}
			return 0;
		}

		public bool Receive(cfgRechargeActivityReward cfg)
		{
			if (cfg != null && !_JudgeReceived(cfg) && _JudgeSatisfyReceivingCondition(cfg))
			{
				DB.RechargeActivity.ReceivedRewards.Add(cfg.Id);
				return true;
			}
			return false;
		}

		public int GetEndTime()
		{
			if (!cf_const.IsForceCloseRechargeActivity && JudgeUnlocked() && JudgeInActivity())
			{
				DateTime now = DateTime.Now;
				int num = (int)(now.ParseFull(DB.RechargeActivity.EndTime, true) - now).TotalSeconds;
				if (num > 0)
				{
					return num + 2;
				}
			}
			return 0;
		}

		private bool _JudgeHasCanReceiveRewards()
		{
			List<cfgRechargeActivityReward> rewardCfgs = _GetRewardCfgs();
			for (int i = 0; i < rewardCfgs.Count; i++)
			{
				cfgRechargeActivityReward cfg = rewardCfgs[i];
				if (_JudgeReceived(cfg) && _JudgeSatisfyReceivingCondition(cfg))
				{
					return true;
				}
			}
			return false;
		}

		private bool _JudgeSatisfyReceivingCondition(cfgRechargeActivityReward cfg)
		{
			return DB.RechargeActivity.CumulativeRecharge >= cfg.Condition;
		}

		private bool _JudgeReceived(cfgRechargeActivityReward cfg)
		{
			return DB.RechargeActivity.ReceivedRewards.Contains(cfg.Id);
		}

		private void _ResetActivityData(string issue)
		{
			DB.RechargeActivity.ReceivedRewards.Clear();
			DB.RechargeActivity.Issue = issue;
			DB.RechargeActivity.CumulativeRecharge = 0f;
			DB.RechargeActivity.StartTime = cf_const.RechargeActivityDate[0];
			DB.RechargeActivity.EndTime = cf_const.RechargeActivityDate[1];
			DB.RechargeActivity.RewardGroupId = cf_const.RechargeActivityUsedRewardGroup;
		}

		private bool _JudgeActivityEnded()
		{
			DateTime now = DateTime.Now;
			return now.ParseFull(DB.RechargeActivity.EndTime, true) <= now;
		}

		private List<cfgRechargeActivityReward> _GetRewardCfgs()
		{
			int groupId = DB.RechargeActivity.RewardGroupId;
			return DataCfg.cfgRechargeActivityRewards.FindAll((cfgRechargeActivityReward f) => f.GroupId == groupId);
		}

		private string _GetNewIssue()
		{
			return string.Concat(cf_const.RechargeActivityDate[0], "-", cf_const.RechargeActivityDate[1]);
		}
	}
}
