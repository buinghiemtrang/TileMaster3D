using UnityEngine;

namespace ShortBattlePass
{
	public class ShortBattlePassItem : MonoBehaviour
	{
		public float height = -1f;

		protected RewardInfo Info;

		protected int OrdinaryRewardsNum;

		public virtual void Init(int ordinaryRewardsNum, RewardInfo info)
		{
			Info = info;
			OrdinaryRewardsNum = ordinaryRewardsNum;
		}
	}
}
