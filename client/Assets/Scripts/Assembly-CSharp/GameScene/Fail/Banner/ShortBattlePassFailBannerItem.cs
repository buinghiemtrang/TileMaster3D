using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Fail.Banner
{
	public class ShortBattlePassFailBannerItem : FailBannerItem
	{
		[SerializeField]
		private MButton _buyButton;

		[SerializeField]
		private Text _priceTex;

		[SerializeField]
		private GameObject _hasMedalRewardsRoot;

		[SerializeField]
		private GiftPopItem _item;

		[SerializeField]
		private GameObject _noHasMedalRewardsRoot;

		private ShortBattlePassFailBanner _banner;

		private void Awake()
		{
			_buyButton.onClick.AddListener(OnClickBuyButton);
		}

		internal override void Init(FailBanner banner)
		{
			_banner = banner as ShortBattlePassFailBanner;
			if (_banner.Config.price == "")
			{
				_priceTex.text = string.Format("${0}", _banner.Config.usd);
			}
			else
			{
				_priceTex.text = _banner.Config.price;
			}
			int[,] array = _banner.Config.item.ToInt();
			bool flag = false;
			if (array != null)
			{
				for (int i = 0; i < array.GetLength(0); i++)
				{
					if (array[i, 0] == 23)
					{
						flag = true;
						_item.Init(23, array[i, 1]);
						break;
					}
				}
				_hasMedalRewardsRoot.SetActive(flag);
				_noHasMedalRewardsRoot.SetActive(!flag);
				SetOffTag(_banner.Config.discount);
			}
		}

		private void OnClickBuyButton(MButton button)
		{
			_banner.ClickBuyCallback?.Invoke();
			SceneMgr.OpenWaiting(30f, 0f);
			LogEvent.I.Track("Liitle_Master_Pass_Click").Add("GradeNum", DB.ShortBattlePass.Data.Grade).Add("Type", "banner")
				.CommonAttr(true)
				.Send();
			if (!IAPManager.instance.Buy(_banner.Config))
			{
				SceneMgr.CloseWaiting();
				return;
			}
			LogEvent.I.Track("Little_Master_Pass_Buy").Add("GradeNum", DB.ShortBattlePass.Data.Grade).Add("Type", "banner")
				.CommonAttr(true)
				.Send();
		}
	}
}
