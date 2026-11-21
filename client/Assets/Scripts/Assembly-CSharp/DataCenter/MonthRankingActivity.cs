using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class MonthRankingActivity : IMonthRankingActivity
	{
		public void UpdateRewardCfgGroupId()
		{
			if (RewardCfgGroupId < DataCfg.cfgMonthRankingActivityRewardGroups.Count)
			{
				RewardCfgGroupId++;
			}
			else
			{
				RewardCfgGroupId = 1;
			}
		}
	}
}
