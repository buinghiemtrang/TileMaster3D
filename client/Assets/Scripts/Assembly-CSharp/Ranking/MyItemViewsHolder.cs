using Com.TheFallenGames.OSA.Core;
using frame8.Logic.Misc.Other.Extensions;

namespace Ranking
{
	public class MyItemViewsHolder : BaseItemViewsHolder
	{
		public RankingCellItem pItem;

		public override void CollectViews()
		{
			base.CollectViews();
			pItem = root.GetComponentAtPath<RankingCellItem>("Root");
		}
	}
}
