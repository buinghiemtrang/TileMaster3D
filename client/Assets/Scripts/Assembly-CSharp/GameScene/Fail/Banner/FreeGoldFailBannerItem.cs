using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Fail.Banner
{
	public class FreeGoldFailBannerItem : FailBannerItem
	{
		[SerializeField]
		private MButton _adButton;

		[SerializeField]
		private Text _adCountTex;

		[SerializeField]
		private GiftPopItem _item;

		private FreeGoldFailBanner _banner;

		private void Awake()
		{
			_adButton.onClick.AddListener(OnClickAdButton);
		}

		internal override void Init(FailBanner banner)
		{
			_banner = banner as FreeGoldFailBanner;
			_item.Init(0, _banner.LookAdGetGoldReward);
			_adCountTex.text = DB.User.SurFreeGoldNum.ToString();
		}

		private void OnClickAdButton(MButton button)
		{
			Global.ShowRewardedVideo(delegate(string result)
			{
				if (!(result != "succ"))
				{
					int num = DB.User.SurFreeGoldNum - 1;
					DB.User.SurFreeGoldNum = num;
					_adCountTex.text = num.ToString();
					Global.CreateReward(string.Format("0|{0}", _banner.LookAdGetGoldReward), RewardType.None, REWARD_PLACE.None, "FreeGold").SetComplete(delegate
					{
						if (num <= 0)
						{
							_banner.AdCountUsedUpCallback?.Invoke();
						}
					});
				}
			}, REWARD_PLACE.AdGiveCoin);
		}
	}
}
