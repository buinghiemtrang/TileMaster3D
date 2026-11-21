using Com.TheFallenGames.OSA.Core;
using frame8.Logic.Misc.Other.Extensions;

namespace Achive
{
	public class MyItemViewsHolder : BaseItemViewsHolder
	{
		public AchiveCellItem pItem;

		public override void CollectViews()
		{
			base.CollectViews();
			pItem = root.GetComponentAtPath<AchiveCellItem>("Root");
		}
	}
}
