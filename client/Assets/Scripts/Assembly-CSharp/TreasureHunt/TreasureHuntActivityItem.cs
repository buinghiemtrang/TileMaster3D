using RoyalPass;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace TreasureHunt
{
	public class TreasureHuntActivityItem : MonoBehaviour
	{
		[SerializeField]
		private Text rankingTex;

		[SerializeField]
		private GameObject rankingObj;

		[SerializeField]
		private Image rankingImg;

		[SerializeField]
		private Sprite[] medals;

		[SerializeField]
		private Text scoreTex;

		[SerializeField]
		private Text nameTex;

		[SerializeField]
		private SkeletonGraphic chestAnim;

		[SerializeField]
		private Sprite[] bgSp;

		[SerializeField]
		private Image bgImg;

		[SerializeField]
		private Sprite[] nameBgSp;

		[SerializeField]
		private Image nameBgImg;

		[HideInInspector]
		public string skinName;

		[HideInInspector]
		public int ranking;

		private cfgTreasureHuntActivityRankingReward cfg;

		public void Init(cfgTreasureHuntActivityRankingReward cfg, int ranking, MemberInfo info)
		{
			this.cfg = cfg;
			skinName = string.Empty;
			this.ranking = ranking;
			if (cfg != null)
			{
				skinName = cfg.skinName;
			}
			if (info != null)
			{
				scoreTex.text = info.score.ToString();
				nameTex.text = info.name;
				bgImg.sprite = bgSp[info.isOwn ? 0 : 1];
				nameBgImg.sprite = nameBgSp[info.isOwn ? 0 : 1];
				rankingTex.gameObject.SetActive(ranking > 3);
				rankingObj.SetActive(ranking <= 3);
				if (ranking <= 3)
				{
					rankingImg.sprite = medals[ranking - 1];
				}
				else
				{
					rankingTex.text = ranking.ToString();
				}
				bool flag = !string.IsNullOrEmpty(skinName);
				chestAnim.gameObject.SetActive(flag);
				if (flag)
				{
					chestAnim.Skeleton.SetSkin(skinName);
					chestAnim.Skeleton.SetSlotsToSetupPose();
					chestAnim.AnimationState.SetAnimation(0, "idle", true);
				}
			}
		}

		public void ClickShow(MButton btn)
		{
			if (cfg != null && cfg.reward != null)
			{
				RoyalPassActivityRewardTip royalPassActivityRewardTip = SceneMgr.AddTopNode<RoyalPassActivityRewardTip>("Prefabs/Activity/RoyalPass/RoyalPassActivityRewardTip");
				if (royalPassActivityRewardTip != null)
				{
					royalPassActivityRewardTip.yOffset = -50f;
					royalPassActivityRewardTip.xOffset = 0f;
					royalPassActivityRewardTip.ShowReward(scoreTex.transform, cfg.reward);
				}
			}
		}
	}
}
