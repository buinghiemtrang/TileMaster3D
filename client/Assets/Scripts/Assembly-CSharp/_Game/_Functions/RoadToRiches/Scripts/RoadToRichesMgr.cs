using System;
using System.Collections.Generic;

namespace _Game._Functions.RoadToRiches.Scripts
{
	public class RoadToRichesMgr : Singleton<RoadToRichesMgr>
	{
		public DateTime EndTime;

		public void Initialized()
		{
			EndTime = default(DateTime);
			if (DB.User.Maxlv < cf_const.RoadToRichesUnlockLv)
			{
				return;
			}
			DateTime now = DateTime.Now;
			DateTime dbEndTime = DateTimeExtend.ParseDate(now, DB.Other.RoadToRichesEndTime, true);
			EndTime = dbEndTime;
			if (now < dbEndTime)
			{
				return;
			}
			string[] openTime = cf_const.RoadToRichesOpenTime;
			DateTime configStartDate = DateTimeExtend.ParseDate(now, openTime[0], true);
			if (now < configStartDate)
			{
				EndTime = default(DateTime);
				return;
			}
			if (!int.TryParse(openTime[1], out int cycleDays))
			{
				cycleDays = 3;
			}
			int days = (now - configStartDate).Days;
			DateTime today = DateTime.Today;
			int cycles = (cycleDays != 0) ? (days / cycleDays) : 0;
			DateTime newEndTime = today.AddDays(cycleDays + (cycles * cycleDays - days));
			EndTime = newEndTime;
			DB.Other.RoadToRichesEndTime = DateTimeExtend.ToStringDate(newEndTime);
			DB.Other.RoadToRichesCount = 0;
		}

		public int[,] GetReward()
		{
			List<cfgRoadToRichesReward> fixedList = DataCfg.cfgRoadToRichesRewards["fixed"];
			int count = DB.Other.RoadToRichesCount;
			cfgRoadToRichesReward rewardItem;
			if (count / 3 < fixedList.Count)
			{
				rewardItem = fixedList[count / 3];
			}
			else
			{
				rewardItem = DataCfg.cfgRoadToRichesRewards["random"].Random();
			}
			return rewardItem.reward;
		}

		public void ClaimReward(Action callback)
		{
			int[,] reward = GetReward();
			RewardUnifyDialog rewardUnifyDialog = Global.CreateReward(reward, 0, 0, "Road_To_Wealth", false, false);
			rewardUnifyDialog.SetComplete(delegate
			{
				callback?.Invoke();
			});
			LogEvent.I.Track("Road_To_Wealth_Receive")
				.Add("Road_ID", DB.Other.RoadToRichesCount)
				.Add("Reward", reward)
				.CommonAttr(true)
				.Send(false);
		}
	}
}
