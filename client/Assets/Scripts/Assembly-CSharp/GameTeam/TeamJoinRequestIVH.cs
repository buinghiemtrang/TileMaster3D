using Com.TheFallenGames.OSA.Core;
using frame8.Logic.Misc.Other.Extensions;

namespace GameTeam
{
	public class TeamJoinRequestIVH : BaseItemViewsHolder
	{
		public TeamJoinRequestItem pItem;

		public override void CollectViews()
		{
			base.CollectViews();
			root.GetComponentAtPath("Root", out pItem);
		}
	}
}
