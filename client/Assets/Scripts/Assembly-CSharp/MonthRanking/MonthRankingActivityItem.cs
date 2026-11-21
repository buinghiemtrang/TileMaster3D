using RoyalPass;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace MonthRanking
{
	public class MonthRankingActivityItem : MonoBehaviour
	{
		[SerializeField]
		private Text rankingTex;

		[SerializeField]
		private Image rankingImg;

		[SerializeField]
		private Sprite[] rankingSprites;

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
		private Sprite[] _spStarBgs;

		[SerializeField]
		private Image _imageStarBg;

		private RankingData _rankingData;

		public void Init(RankingData info)
		{
			_rankingData = info;
			scoreTex.text = info.score.ToString();
			nameTex.text = info.name;
			if (bgSp != null && bgSp.Length >= 2 && bgImg != null)
			{
				bgImg.sprite = bgSp[info.isOwn ? 0 : 1];
			}
			if (_spStarBgs != null && _spStarBgs.Length >= 2 && _imageStarBg != null)
			{
				_imageStarBg.sprite = _spStarBgs[info.isOwn ? 0 : 1];
			}
			rankingTex.gameObject.SetActive(info.ranking > 2);
			rankingImg.gameObject.SetActive(info.ranking < 3);
			if (info.ranking <= 2)
			{
				rankingImg.sprite = rankingSprites[info.ranking];
			}
			else if (info.ranking < cf_const.monthRankingActivityLimit)
			{
				rankingTex.text = string.Format("{0}", info.ranking + 1);
			}
			else
			{
				rankingTex.text = string.Format("{0}+", cf_const.monthRankingActivityLimit);
			}
			cfgMonthRankingActivityReward rewardsCfg = MonthRankingActivityMgr.I.GetRewardsCfg(info.ranking);
			string text = rewardsCfg != null ? rewardsCfg.skinName : string.Empty;
			chestAnim.gameObject.SetActive(!string.IsNullOrEmpty(text));
			if (!string.IsNullOrEmpty(text))
			{
				chestAnim.transform.localScale = Vector3.one * (GetChestScale(info.ranking) * 0.36f);
				chestAnim.Skeleton.SetSkin(text);
				chestAnim.Skeleton.SetSlotsToSetupPose();
				chestAnim.AnimationState.SetAnimation(0, "idle", true);
			}
		}

		private float GetChestScale(int ranking)
		{
			if ((uint)ranking < 3)
			{
				switch (ranking)
				{
				case 0:
					return 1f;
				case 1:
					return 0.95f;
				case 2:
					return 0.9f;
				}
			}
			return 0.85f;
		}

		public void ClickShowChest(MButton button)
		{
			if (_rankingData == null)
			{
				return;
			}
			cfgMonthRankingActivityReward rewardsCfg = MonthRankingActivityMgr.I.GetRewardsCfg(_rankingData.ranking);
			if (chestAnim == null || rewardsCfg == null || rewardsCfg.reward == null)
			{
				return;
			}
			RoyalPassActivityRewardTip tip = SceneMgr.AddTopNode<RoyalPassActivityRewardTip>("Prefabs/Activity/RoyalPass/RoyalPassActivityRewardTip");
			tip.yOffset = -50f;
			tip.xOffset = 0f;
			tip.ShowReward(chestAnim.transform, rewardsCfg.reward);
		}
	}
}
