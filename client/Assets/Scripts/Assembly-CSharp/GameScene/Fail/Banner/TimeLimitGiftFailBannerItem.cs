using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Fail.Banner
{
	public class TimeLimitGiftFailBannerItem : FailBannerItem
	{
		[SerializeField]
		private Text _nameTex;

		[SerializeField]
		private GameObject _timeRoot;

		[SerializeField]
		private Text _timeTex;

		[SerializeField]
		private MButton _buyButton;

		[SerializeField]
		private Text _priceTex;

		[SerializeField]
		private ItemControl _itemControl;

		private TimeLimitGiftFailBanner _banner;

		private void Awake()
		{
			_buyButton.onClick.AddListener(OnClickBuyButton);
		}

		private void OnDestroy()
		{
			_timeTex.DOKill();
		}

		internal override void Init(FailBanner banner)
		{
			_banner = banner as TimeLimitGiftFailBanner;
			_nameTex.text = _banner.StoreConfig.name;
			if (_banner.StoreConfig.price == "")
			{
				_priceTex.text = string.Format("${0}", _banner.StoreConfig.usd);
			}
			else
			{
				_priceTex.text = _banner.StoreConfig.price;
			}
			_timeTex.DOKill();
			_timeRoot.SetActive(true);
			if (_timeRoot.activeSelf)
			{
				int second = (int)(_banner.Data.timer - DateTime.Now).TotalSeconds + 2;
				CountdownFixedFormat(second, _timeTex, true);
			}
			_itemControl.InitData(_banner.StoreConfig.item, _banner.StoreConfig.itemOther);
			SetOffTag(_banner.StoreConfig.discount);
		}

		private void OnClickBuyButton(MButton button)
		{
			_banner.ClickBuyCallback?.Invoke();
			SceneMgr.OpenWaiting(30f, 0f);
			if (!IAPManager.instance.Buy(_banner.StoreConfig))
			{
				SceneMgr.CloseWaiting();
				return;
			}
			string value = Global.pStores.ProductId(_banner.StoreConfig.id);
			LogEvent.I.Track("Gift_Buy").Add("Commodity_ID", value).CommonAttr(true).Send();
		}
	}
}
