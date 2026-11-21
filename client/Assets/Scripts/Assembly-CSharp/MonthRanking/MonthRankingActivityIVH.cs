using Com.TheFallenGames.OSA.Core;
using frame8.Logic.Misc.Other.Extensions;

namespace MonthRanking
{
	public class MonthRankingActivityIVH : BaseItemViewsHolder
	{
		public MonthRankingActivityItem pItem;

		public override void CollectViews()
		{
			base.CollectViews();
			pItem = root.GetComponentAtPath<MonthRankingActivityItem>("Root");
		}
	}
}
