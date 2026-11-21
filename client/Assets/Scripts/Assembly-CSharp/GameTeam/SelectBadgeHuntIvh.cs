using Com.TheFallenGames.OSA.Core;
using frame8.Logic.Misc.Other.Extensions;

namespace GameTeam
{
	public class SelectBadgeHuntIvh : BaseItemViewsHolder
	{
		public TeamBadgeCellItem PItem;

		public override void CollectViews()
		{
			base.CollectViews();
			PItem = root.GetComponentAtPath<TeamBadgeCellItem>("Root");
		}
	}
}
