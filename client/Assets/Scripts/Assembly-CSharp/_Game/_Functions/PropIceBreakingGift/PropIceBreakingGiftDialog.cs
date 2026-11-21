using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Game._Functions.PropIceBreakingGift
{
	public class PropIceBreakingGiftDialog : DialogBase
	{
		[SerializeField]
		private MButton _closeButton;

		[SerializeField]
		private Text _describeTex;

		[SerializeField]
		private Text _titleTex;

		[SerializeField]
		private RewardItem[] _items;

		[SerializeField]
		private GameObject _discountRoot;

		[SerializeField]
		private Text _discountTex;

		[SerializeField]
		private GameObject _ordinaryButtonRoot;

		[SerializeField]
		private MButton _coinBuyButton;

		[SerializeField]
		private Text _needCoinTex;

		[SerializeField]
		private MButton _paidBuyButton;

		[SerializeField]
		private Text _priceTex;

		[SerializeField]
		private GameObject _tipButtonRoot;

		[SerializeField]
		private MButton _yesButton;

		[SerializeField]
		private MButton _noButton;

		[SerializeField]
		private GameObject _offTagObj;

		[SerializeField]
		private Text _offTagText;

		private IceBreakingGiftInfo _giftInfo;

		private Action _closeCallback;

		private bool _isTip;

		protected override void Awake()
		{
			base.Awake();
			_closeButton.onClick.AddListener(ClickClose);
			_coinBuyButton.onClick.AddListener(ClickCoinBuy);
			_paidBuyButton.onClick.AddListener(ClickPaidBuy);
			_yesButton.onClick.AddListener(ClickYes);
			_noButton.onClick.AddListener(ClickNo);
			DispatcherMgr.On<GiftType>("BuySuccessCallBack", IAPBuySuccess);
			DispatcherMgr.On("IAPInitFinishedCallBack", IAPInitFinished);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			DispatcherMgr.Off<GiftType>("BuySuccessCallBack", IAPBuySuccess);
			DispatcherMgr.Off("IAPInitFinishedCallBack", IAPInitFinished);
		}

		private void Start()
		{
			RefreshUI(false);
			int[,] array = ((_giftInfo.GiftType != IceBreakingGiftType.Paid) ? _giftInfo.IceBreakingGiftConfig.Rewards : _giftInfo.StoreConfig.item.ToInt(2, ',', '|'));
			int length = array.GetLength(0);
			for (int i = 0; i < _items.Length; i++)
			{
				RewardItem rewardItem = _items[i];
				rewardItem.Root.SetActive(i < length);
				if (rewardItem.Root.activeSelf)
				{
					rewardItem.Init((PropID)array[i, 0], array[i, 1]);
				}
			}
			SetOffTag(_giftInfo.StoreConfig);
			ShowMask();
			_Open();
		}

		private void RefreshUI(bool isTip)
		{
			_isTip = isTip;
			_closeButton.gameObject.SetActive(!isTip);
			_tipButtonRoot.SetActive(isTip);
			_ordinaryButtonRoot.SetActive(!isTip);
			if (_ordinaryButtonRoot.activeSelf)
			{
				_coinBuyButton.gameObject.SetActive(_giftInfo.GiftType == IceBreakingGiftType.Coin);
				if (_coinBuyButton.gameObject.activeSelf)
				{
					_needCoinTex.text = _giftInfo.IceBreakingGiftConfig.CoinPrice.ToString();
				}
				_paidBuyButton.gameObject.SetActive(_giftInfo.GiftType == IceBreakingGiftType.Paid);
				if (_paidBuyButton.gameObject.activeSelf)
				{
					// UNVERIFIED: định dạng giá khi store.price rỗng, nhãn SL____0__ có 5 giá trị ("\n{0}", " {0}", "+{0}", ":{0}", "_{0}"), chọn "_{0}"
					_priceTex.text = (string.IsNullOrEmpty(_giftInfo.StoreConfig.price) ? string.Format("_{0}", _giftInfo.StoreConfig.usd) : _giftInfo.StoreConfig.price);
				}
			}
			_titleTex.SetLKey("TM3DLanguage8");
			_describeTex.SetLKey(isTip ? "TM3DLanguage10" : "TM3DLanguage9");
			_discountRoot.SetActive(_giftInfo.StoreConfig != null);
			if (_discountRoot.activeSelf)
			{
				_discountTex.text = "";
			}
		}

		private void ClickClose(MButton button)
		{
			RefreshUI(true);
		}

		private void ClickNo(MButton button)
		{
			RefreshUI(false);
		}

		private void ClickCoinBuy(MButton button)
		{
			int coinPrice = _giftInfo.IceBreakingGiftConfig.CoinPrice;
			if (DB.Bag.GetProp(PropID.Money) < coinPrice)
			{
				return;
			}
			Singleton<PropIceBreakingGiftManager>.I.CoinGiftBuySuccess();
			DB.Bag.Modify(PropID.Money, -coinPrice, b_update: true, "PropIceBreakingGift");
			RewardUnifyDialog rewardUnifyDialog = Global.CreateReward(_giftInfo.IceBreakingGiftConfig.Rewards, RewardType.None, REWARD_PLACE.None, "PropIceBreakingGift");
			rewardUnifyDialog.SetComplete(delegate
			{
				_closeCallback.InvokeSafe();
			});
			_Close();
		}

		private void ClickPaidBuy(MButton button)
		{
			SceneMgr.OpenWaiting(30f, 0f);
			if (!IAPManager.instance.Buy(_giftInfo.StoreConfig))
			{
				SceneMgr.CloseWaiting();
			}
		}

		private void ClickYes(MButton button)
		{
			_Close(_closeCallback);
		}

		private void IAPInitFinished()
		{
			RefreshUI(_isTip);
		}

		private void IAPBuySuccess(GiftType type)
		{
			if (type == GiftType.PropIceBreakingGift)
			{
				_Close(_closeCallback);
			}
		}

		private void SetOffTag(cfbt_Stores config)
		{
			if (!(_offTagObj == null))
			{
				if (config == null)
				{
					_offTagObj.SetActive(false);
				}
				else
				{
					_offTagObj.SetActive(config.discount > 0);
					if (config.discount > 0)
					{
						// UNVERIFIED: nhãn SL___0____2 có 5 giá trị ("{0} ", "{0}%", "{0}+", "{0}-", "{0}_"), chọn "{0}%" theo ngữ cảnh giảm giá
						_offTagText.text = string.Format("{0}%", config.discount);
					}
				}
			}
		}

		public static void Create(IceBreakingGiftInfo info, Action closeCallback)
		{
			PropIceBreakingGiftDialog propIceBreakingGiftDialog = SceneMgr.AddTopNode<PropIceBreakingGiftDialog>("Prefabs/PropIceBreakingGiftDialog");
			propIceBreakingGiftDialog._giftInfo = info;
			propIceBreakingGiftDialog._closeCallback = closeCallback;
		}
	}
}
