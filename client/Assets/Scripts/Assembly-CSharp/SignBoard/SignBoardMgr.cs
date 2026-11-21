using System;
using UnityEngine;

namespace SignBoard
{
	public class SignBoardMgr
	{
		public static readonly SignBoardMgr I = new SignBoardMgr();

		public void Init()
		{
		}

		public bool JudgeAutoEject()
		{
			if (DB.User.Maxlv + 1 > cf_const.signBoardUnlockLv)
			{
				string lastReceiveTime = DB.SignBoard.LastReceiveTime;
				if (string.IsNullOrEmpty(lastReceiveTime))
				{
					return true;
				}
				return Days() > 0;
			}
			return false;
		}

		public cfgSignBoardReward GetRewardCfg()
		{
			int index = UnityEngine.Random.Range(0, DataCfg.cfgSignBoardRewards.Count);
			return DataCfg.cfgSignBoardRewards[index];
		}

		public void ResetData()
		{
			DB.SignBoard.LastReceiveTime = DateTime.Today.ToString();
		}

		private int Days()
		{
			string lastReceiveTime = DB.SignBoard.LastReceiveTime;
			if (string.IsNullOrEmpty(lastReceiveTime))
			{
				return 0;
			}
			if (!DateTime.TryParse(DB.SignBoard.LastReceiveTime, out DateTime result))
			{
				ResetData();
				result = DateTime.Today;
			}
			int days = (DateTime.Today - result).Days;
			if (days < 0)
			{
				return 0;
			}
			return days;
		}
	}
}
