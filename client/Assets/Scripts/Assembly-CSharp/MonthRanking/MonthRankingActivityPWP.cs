using System;
using Com.TheFallenGames.OSA.CustomParams;

namespace MonthRanking
{
	[Serializable]
	public class MonthRankingActivityPWP : BaseParamsWithPrefab
	{
		public float DefaultItemHeight
		{
			set
			{
				base.DefaultItemSize = value;
			}
		}
	}
}
