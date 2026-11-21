using Com.TheFallenGames.OSA.Core;
using frame8.Logic.Misc.Other.Extensions;

namespace GameTeam
{
	public class MyTeamIVH : BaseItemViewsHolder
	{
		public MyTeamMemberItem pItem;

		public override void CollectViews()
		{
			base.CollectViews();
			pItem = root.GetComponentAtPath<MyTeamMemberItem>("Root");
		}
	}
}
