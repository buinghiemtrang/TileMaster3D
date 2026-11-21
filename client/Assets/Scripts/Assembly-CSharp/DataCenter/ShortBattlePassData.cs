using System.Collections.Generic;

namespace DataCenter
{
	public class ShortBattlePassData
	{
		public string Issue = string.Empty;

		public string StartTime = string.Empty;

		public string EndTime = string.Empty;

		public string CoolingEndTime = string.Empty;

		public int ActivityState;

		public List<int> RewardsState = new List<int>();

		public int RewardGroupId;

		public int Score;

		public int Grade = 1;

		public int ResidualExp;

		public int Duration;
	}
}
