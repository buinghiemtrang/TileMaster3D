using Com.TheFallenGames.OSA.Core;
using frame8.Logic.Misc.Other.Extensions;

namespace TreasureHunt
{
	public class TreasureHuntIVH : BaseItemViewsHolder
	{
		public TreasureHuntActivityItem pItem;

		public override void CollectViews()
		{
			base.CollectViews();
			pItem = root.GetComponentAtPath<TreasureHuntActivityItem>("Root");
		}
	}
}
