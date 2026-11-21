using System;
using UnityEngine;
using UnityEngine.UI;

namespace Pop_Gift
{
	public class NoAdGiftDialog : BaseGiftDialog
	{
		[SerializeField]
		private GiftPopItem _cellItem;

		[SerializeField]
		private Text _titleTex;

		[SerializeField]
		private MButton _closeButton;

		[SerializeField]
		private Text _priceTex;

		[SerializeField]
		private MButton _originalPriceBuyButton;

		private GiftData _giftData;

		private Action<bool> _onCloseCallback;

		protected override void Awake()
		{
			base.Awake();
			_closeButton.onClick.AddListener(ClickClose);
			_originalPriceBuyButton.onClick.AddListener(ClickOriginalPriceBuy);
		}

		public override void Init(bool isAutoPop, GiftData data, Action<bool> onCloseCallback)
		{
			_giftData = data;
			_onCloseCallback = onCloseCallback;
			if (data != null && data.StoresConfig != null)
			{
				int[,] array = data.StoresConfig.item.ToInt(2, ',', '|');
				if (array.Length < 1)
				{
					_cellItem.gameObject.SetActive(false);
				}
				else
				{
					_cellItem.gameObject.SetActive(true);
					for (int i = 0; i < array.GetLength(0); i++)
					{
						if (array[i, 0] == 0)
						{
							_cellItem.Init(0, array[i, 1]);
							break;
						}
					}
				}
				if (data.StoresConfig.price != null)
				{
					if (data.StoresConfig.price == "")
					{
						_priceTex.text = string.Format("${0}", data.StoresConfig.usd);
					}
					else
					{
						_priceTex.text = data.StoresConfig.price;
					}
					_Open(delegate
					{
					});
					if (Global.pStores != null)
					{
						string productId = Global.pStores.ProductId(data.StoresConfig.id);
						LogEvent.I.Track("Gift_Show").Add("Commodity_ID", productId).CommonAttr(true).Send();
					}
				}
			}
		}

		private void ClickClose(MButton button)
		{
			CloseDialog(false);
		}

		private void ClickOriginalPriceBuy(MButton button)
		{
			CloseDialog(true);
			SceneMgr.OpenWaiting(30f, 0f);
			if (_giftData != null && _giftData.StoresConfig != null)
			{
				string productId = Global.pStores.ProductId(_giftData.StoresConfig.id);
				LogEvent.I.Track("Gift_Click").Add("Commodity_ID", productId).Add("Type", "Activity").CommonAttr(true).Send();
				if (IAPManager.instance != null)
				{
					if (IAPManager.instance.Buy(_giftData.StoresConfig))
					{
						LogEvent.I.Track("Gift_Buy").Add("Commodity_ID", productId).CommonAttr(true).Send();
					}
					else
					{
						SceneMgr.CloseWaiting();
					}
				}
			}
		}

		private void CloseDialog(bool isBought)
		{
			_onCloseCallback?.Invoke(isBought);
			_Close();
		}
	}
}
