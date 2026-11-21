using Com.TheFallenGames.OSA.Core;
using frame8.Logic.Misc.Other.Extensions;

namespace GameTeam
{
	public class TeamSearchIVH : BaseItemViewsHolder
	{
		public TeamItem pItem;

		public override void CollectViews()
		{
			base.CollectViews();
			root.GetComponentAtPath("Root", out pItem);
		}
	}
}
