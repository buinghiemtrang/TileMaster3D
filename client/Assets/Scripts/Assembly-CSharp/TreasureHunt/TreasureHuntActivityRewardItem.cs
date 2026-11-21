using RoyalPass;
using Spine.Unity;
using UnityEngine;

namespace TreasureHunt
{
	public class TreasureHuntActivityRewardItem : MonoBehaviour
	{
		[SerializeField]
		private SkeletonGraphic anim;

		[HideInInspector]
		public int state;

		[HideInInspector]
		public cfgTreasureHuntActivityReward cfg;

		public string skinName;

		public void Init(int state, cfgTreasureHuntActivityReward cfg)
		{
			this.state = state;
			this.cfg = cfg;
			_PlayChestAnimation();
		}

		private void _PlayChestAnimation()
		{
			string text;
			if (state == 0)
			{
				text = "idle";
			}
			else if (state == 1)
			{
				text = "close-idle";
			}
			else
			{
				text = "open-idle";
			}
			anim.AnimationState.SetAnimation(0, text, true);
		}

		public void ClickReceive(MButton btn)
		{
			if (state == 1)
			{
				int[,] array = Singleton<TreasureHuntActivityMgr>.I.ReceiveActivityReward(cfg.id);
				if (array == null || array.Length == 0)
				{
					return;
				}
				RewardUnifyDialog rewardUnifyDialog = Global.CreateReward(array, RewardType.ActivityOne, REWARD_PLACE.None, "TeamReward", false, false);
				if (rewardUnifyDialog != null)
				{
					rewardUnifyDialog.SetChestSkin(skinName);
					rewardUnifyDialog.SetComplete(delegate
					{
						state = 2;
						_PlayChestAnimation();
					});
					int teamModel = 0;
					int myModel = 0;
					Singleton<TreasureHuntActivityMgr>.I.GetMembers().ForEach(delegate(MemberInfo fe)
					{
						int num = fe.score / cf_const.treasureHuntActivityUnitScore;
						teamModel += num;
						if (fe.isOwn)
						{
							myModel = num;
						}
					});
					LogEvent.I.Track("Team_Treasure_Box_Reward")
						.Add("ActivityTime", DB.TreasureHuntActivity.Issue)
						.Add("Team_ID", DB.TreasureHuntActivity.TeamId)
						.Add("TeamBox", cfg.id)
						.Add("TeamModel", teamModel)
						.Add("MyModel", myModel)
						.CommonAttr(true)
						.Send();
				}
			}
			else if (state == 0)
			{
				RoyalPassActivityRewardTip royalPassActivityRewardTip = SceneMgr.AddTopNode<RoyalPassActivityRewardTip>("Prefabs/Activity/RoyalPass/RoyalPassActivityRewardTip");
				if (royalPassActivityRewardTip != null)
				{
					royalPassActivityRewardTip.yOffset = -50f;
					royalPassActivityRewardTip.xOffset = 0f;
					royalPassActivityRewardTip.ShowReward(transform, cfg.reward);
				}
			}
		}
	}
}
