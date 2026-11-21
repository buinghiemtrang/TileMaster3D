using System;

namespace _Game._Functions.LuckyPrize.Scripts
{
	public class LuckyPrizeMgr : Singleton<LuckyPrizeMgr>
	{
		public void CheckRefreshReward()
		{
			DateTime now = DateTime.Now;
			DateTime refreshTime = DateTimeExtend.ParseFull(now, DB.Other.LuckyPrizeRefreshTime, true);
			if (now < refreshTime)
			{
				return;
			}
			DB.Other.LuckyPrizeRefreshTime = DateTimeExtend.ToStringFull(now.AddDays(1.0));
			DB.Other.LuckyPrizeRewardIndex = 0;
		}

		public int GetRefreshTime()
		{
			DateTime now = DateTime.Now;
			DateTime refreshTime = DateTimeExtend.ParseFull(now, DB.Other.LuckyPrizeRefreshTime, true);
			return (int)(refreshTime - now).TotalSeconds;
		}
	}
}
