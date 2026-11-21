using Com.TheFallenGames.OSA.Core;

namespace Recharge
{
	public class RechargeIvh : BaseItemViewsHolder
	{
		public RewardItem PItem;

		public override void CollectViews()
		{
			base.CollectViews();
			PItem = root.GetComponent<RewardItem>();
		}
	}
}
