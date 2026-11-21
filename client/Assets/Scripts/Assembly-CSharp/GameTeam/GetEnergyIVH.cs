using Com.TheFallenGames.OSA.Core;
using frame8.Logic.Misc.Other.Extensions;

namespace GameTeam
{
	public class GetEnergyIVH : BaseItemViewsHolder
	{
		public GetEnergyCellItem pItem;

		public override void CollectViews()
		{
			base.CollectViews();
			pItem = root.GetComponentAtPath<GetEnergyCellItem>("Root");
		}
	}
}
