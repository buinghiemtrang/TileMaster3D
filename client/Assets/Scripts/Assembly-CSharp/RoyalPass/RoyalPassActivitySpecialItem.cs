using Spine.Unity;
using UnityEngine;

namespace RoyalPass
{
	public class RoyalPassActivitySpecialItem : RoyalPassActivityItem
	{
		[SerializeField]
		private GameObject hookObj;

		[SerializeField]
		private GameObject receiveBtnObj;

		[SerializeField]
		private SkeletonGraphic lockAnim;

		private bool isReceived;

		private bool isCanReceive;

		private void Awake()
		{
			DispatcherMgr.On("RoyalPassActivity_Unlock", _OnUnlock);
		}

		public override void Init(int ordinaryRewardsNum, RoyalPassActivityRewardInfo info)
		{
			base.info = info;
			base.ordinaryRewardsNum = ordinaryRewardsNum;
			isReceived = info.state == RewardState.ReceivedAll;
			isCanReceive = info.state != RewardState.ReceivedAll && info.isUnlockPaidReward && info.isSatisfyCondition;
			hookObj.SetActive(isReceived);
			receiveBtnObj.SetActive(isCanReceive);
			lockAnim.gameObject.SetActive(!info.isUnlockPaidReward);
			if (!info.isUnlockPaidReward)
			{
				lockAnim.AnimationState.SetAnimation(0, "lock_idle", true);
			}
		}

		public void ClickReceive(MButton btn)
		{
			if (isReceived || !isCanReceive)
			{
				return;
			}
			btn.interactable = false;
			RewardState state = Singleton<RoyalPassActivityMgr>.I.Receive(RewardItemType.Special, info.cfg.id);
			Global.CreateReward(info.cfg.paidReward, RewardType.RoyalPassActivity, REWARD_PLACE.None, "PassBuyReward").SetChestSkin("08").SetComplete(delegate
			{
				info.state = state;
				Init(ordinaryRewardsNum, info);
				btn.interactable = true;
			});
		}

		private void _OnUnlock()
		{
			if (base.gameObject.activeSelf && info != null)
			{
				info.isUnlockPaidReward = true;
				if (lockAnim != null)
				{
					lockAnim.AnimationState.SetAnimation(0, "unlock", false);
					lockAnim.AnimationState.Complete += delegate
					{
						Init(ordinaryRewardsNum, info);
					};
				}
			}
		}

		private void OnDestroy()
		{
			DispatcherMgr.Off("RoyalPassActivity_Unlock", _OnUnlock);
		}
	}
}
