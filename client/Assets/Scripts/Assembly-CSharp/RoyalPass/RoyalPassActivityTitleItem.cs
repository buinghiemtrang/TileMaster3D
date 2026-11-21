namespace RoyalPass
{
	public class RoyalPassActivityTitleItem : RoyalPassActivityItem
	{
		public override void Init(int ordinaryRewardsNum, RoyalPassActivityRewardInfo info)
		{
			base.info = info;
			base.ordinaryRewardsNum = ordinaryRewardsNum;
		}
	}
}
