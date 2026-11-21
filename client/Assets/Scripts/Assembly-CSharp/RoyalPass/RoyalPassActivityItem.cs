using UnityEngine;

namespace RoyalPass
{
	public class RoyalPassActivityItem : MonoBehaviour
	{
		public float height = -1f;

		protected RoyalPassActivityRewardInfo info;

		protected int ordinaryRewardsNum;

		public virtual void Init(int ordinaryRewardsNum, RoyalPassActivityRewardInfo info)
		{
			this.info = info;
			this.ordinaryRewardsNum = ordinaryRewardsNum;
		}
	}
}
