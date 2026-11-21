using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Fail.Banner
{
	public class ReviveGiftFailBannerItem : FailBannerItem
	{
		[SerializeField]
		private GameObject _timeRoot;

		[SerializeField]
		private Text _addTimeTex;

		[SerializeField]
		private GameObject _tipRoot;

		[SerializeField]
		private MButton _buyButton;

		[SerializeField]
		private Text _priceTex;

		[SerializeField]
		private ItemControl _itemControl;

		private ReviveGiftFailBanner _banner;

		private void Awake()
		{
			_buyButton.onClick.AddListener(OnClickBuyButton);
		}

		internal override void Init(FailBanner banner)
		{
			_banner = banner as ReviveGiftFailBanner;
			_timeRoot.SetActive(_banner.ReviveAfterAddTime > 0);
			_tipRoot.SetActive(!_timeRoot.activeSelf);
			if (_timeRoot.activeSelf)
			{
				_addTimeTex.text = string.Format("+{0}S", _banner.ReviveAfterAddTime);
			}
			if (_banner.Config.price == "")
			{
				_priceTex.text = string.Format("${0}", _banner.Config.usd);
			}
			else
			{
				_priceTex.text = _banner.Config.price;
			}
			_itemControl.InitData(_banner.Config.item, _banner.Config.itemOther);
			SetOffTag(_banner.Config.discount);
		}

		private void OnClickBuyButton(MButton button)
		{
			_banner.ClickBuyCallback?.Invoke();
			SceneMgr.OpenWaiting(30f, 0f);
			if (!IAPManager.instance.Buy(_banner.Config))
			{
				SceneMgr.CloseWaiting();
				return;
			}
			string value = Global.pStores.ProductId(_banner.Config.id);
			LogEvent.I.Track("Gift_Buy").Add("Commodity_ID", value).CommonAttr(true).Send();
		}
	}
}
