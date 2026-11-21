using System;
using System.Collections.Generic;
using System.Linq;
using ShortBattlePass;
using UnityEngine;

namespace GameScene.Fail.Banner
{
	public class FailBannerControl : MonoBehaviour
	{
		[SerializeField]
		private GameObject _root;

		[SerializeField]
		private FailBannerScroll _scroll;

		[SerializeField]
		private DotControl _dotControl;

		private bool _isTimeOut;

		private int _reviveAfterAddTime;

		private bool _isAutoMove;

		private float _waitAutoMoveDuration;

		private bool _isBought;

		private bool _goldIsEnough;

		private FailureControl _failureControl;

		private void Awake()
		{
			_scroll.OnSelectedValueChanged += OnSelectedValueChanged;
			DispatcherMgr.On("FailBannerCountdownEnd", OnCountdownCompleted);
			DispatcherMgr.On<cfbt_Stores>("BuySuccessEvent", OnIAPBuySuccess);
		}

		private void OnDestroy()
		{
			_scroll.OnSelectedValueChanged -= OnSelectedValueChanged;
			DispatcherMgr.Off("FailBannerCountdownEnd", OnCountdownCompleted);
			DispatcherMgr.Off<cfbt_Stores>("BuySuccessEvent", OnIAPBuySuccess);
		}

		private void Update()
		{
			if (_isAutoMove)
			{
				_waitAutoMoveDuration += Time.deltaTime;
				if ((float)cf_fail_dialog_const.FailBannerAutoMoveIntervalTime <= _waitAutoMoveDuration)
				{
					_waitAutoMoveDuration = 0f;
					_scroll.ScrollToNextPage();
				}
			}
		}

		public bool Init(int lv, bool isTimeOut, int reviveAfterAddTime, bool goldIsEnough, FailureControl failureControl)
		{
			_isBought = false;
			_root.SetActive(cf_fail_dialog_const.FailBannerOpenLevel <= lv);
			bool activeSelf = _root.activeSelf;
			if (activeSelf)
			{
				_reviveAfterAddTime = reviveAfterAddTime;
				_isTimeOut = isTimeOut;
				_goldIsEnough = goldIsEnough;
				_failureControl = failureControl;
				RefreshItem();
				_waitAutoMoveDuration = 0f;
				_isAutoMove = true;
			}
			return activeSelf;
		}

		private void RefreshItem()
		{
			List<FailBanner> banners = GetBanners();
			foreach (FailBanner item in banners)
			{
				item.ClickBuyCallback = OnClickBuy;
				item.AdCountUsedUpCallback = OnAdCountUsedUpCallback;
			}
			_scroll.RefreshItems(banners);
			_dotControl.Refresh(banners.Count);
			_dotControl.Show(_scroll.SelectedValue);
		}

		private void OnClickBuy()
		{
			_isBought = true;
		}

		private void OnAdCountUsedUpCallback()
		{
			RefreshItem();
		}

		private void OnSelectedValueChanged(int index)
		{
			_waitAutoMoveDuration = 0f;
			_dotControl.Show(_scroll.SelectedValue);
		}

		private void OnCountdownCompleted()
		{
			RefreshItem();
		}

		private void OnIAPBuySuccess(cfbt_Stores storeConfig)
		{
			if (!_isBought)
			{
				return;
			}
			_isBought = false;
			foreach (cfbt_Gift cfbt_Gifts in DataCfg.cfbt_Gifts)
			{
				int[] array = cfbt_Gifts.storeIndex.ToInt(',');
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] != storeConfig.index)
					{
						continue;
					}
					foreach (cfbt_GiftStalls cfbt_GiftStallss in DataCfg.cfbt_GiftStallss)
					{
						if (cfbt_GiftStallss.triggerType != 14)
						{
							continue;
						}
						int[] array2 = cfbt_GiftStallss.giftIds.ToInt('|');
						for (int j = 0; j < array2.Length; j++)
						{
							if (array2[j] == cfbt_Gifts.id)
							{
								_failureControl.Revive();
								return;
							}
						}
					}
				}
			}
			if (storeConfig.gType != 22)
			{
				RefreshItem();
				return;
			}
			Singleton<ShortBattlePassMgr>.I.ShowBuySuccessRewardsDialog(RefreshItem);
			LogEvent.I.Track("Little_Master_Pass_Callback").Add("GradeNum", DB.ShortBattlePass.Data.Grade).Add("Type", "Banner")
				.CommonAttr(true)
				.Send();
		}

		private List<FailBanner> GetBanners()
		{
			List<FailBanner> list = new List<FailBanner>();
			if (cf_fail_dialog_const.FailBannerMaxShowCount <= 0)
			{
				return list;
			}
			foreach (int item in _goldIsEnough ? cf_fail_dialog_const.FailBannerOrderCoinFull : cf_fail_dialog_const.FailBannerOrder)
			{
				switch (item)
				{
				case 1:
				{
					List<cfbt_Gift> giftCfgByType = GiftDataMgr.ins.GetGiftCfgByType(GiftTriggerType.Revive);
					if (giftCfgByType.Count < 1)
					{
						continue;
					}
					cfbt_Gift gift = giftCfgByType[0];
					list.Add(new ReviveGiftFailBanner
					{
						Config = GiftDataMgr.ins.GetStoreCfg(gift),
						IsTimeOut = _isTimeOut,
						ReviveAfterAddTime = _reviveAfterAddTime
					});
					break;
				}
				case 2:
				{
					int num = 1;
					foreach (cfbt_Gift buttonShowGift in GiftDataMgr.ins.buttonShowGifts)
					{
						if (buttonShowGift.storeIndex.ToInt(',').Length != 1)
						{
							continue;
						}
						GiftData giftDataById = DB.GiftStalls.GetGiftDataById(buttonShowGift.id);
						if (giftDataById == null || (int)(giftDataById.timer - DateTime.Now).TotalSeconds < 1)
						{
							continue;
						}
						cfbt_Stores storeCfg = GiftDataMgr.ins.GetStoreCfg(buttonShowGift);
						list.Add(new TimeLimitGiftFailBanner
						{
							Data = giftDataById,
							StoreConfig = storeCfg,
							Order = num
						});
						num++;
						if (list.Count >= cf_fail_dialog_const.FailBannerMaxShowCount)
						{
							return list;
						}
					}
					continue;
				}
				case 3:
				{
					if (cf_fail_dialog_const.StoreGiftMaxShowCount < 1)
					{
						continue;
					}
					int stall = DB.GiftStalls.GetStalls();
					DirectPurchaseStall directPurchaseStall = DataCfg.DirectPurchaseStalls.FirstOrDefault((DirectPurchaseStall fd) => fd.stall == stall);
					if (directPurchaseStall == null || directPurchaseStall.failBannerGiftIds == null || directPurchaseStall.failBannerGiftIds.Count < 1)
					{
						continue;
					}
					int num2 = 0;
					foreach (int failBannerGiftId in directPurchaseStall.failBannerGiftIds)
					{
						int id = failBannerGiftId;
						DirectPurchase directPurchase = DataCfg.DirectPurchases.FirstOrDefault((DirectPurchase fd) => fd.id == id);
						if (directPurchase == null)
						{
							continue;
						}
						list.Add(new StoreGiftFailBanner
						{
							Config = Global.pStores.GetConfig(directPurchase.storeId)
						});
						if (list.Count >= cf_fail_dialog_const.FailBannerMaxShowCount)
						{
							return list;
						}
						num2++;
						if (num2 >= cf_fail_dialog_const.StoreGiftMaxShowCount)
						{
							break;
						}
					}
					continue;
				}
				case 4:
				{
					if (!Singleton<ShortBattlePassMgr>.I.JudgeOpen() || Singleton<ShortBattlePassMgr>.I.JudgeHasState(ShortBattlePass.ActivityState.CostSuccess))
					{
						continue;
					}
					List<cfbt_Stores> cfgs = Global.pStores.GetCfgs(BuySpotType.ShortBattlePass);
					if (cfgs.Count < 1)
					{
						continue;
					}
					list.Add(new ShortBattlePassFailBanner
					{
						Config = cfgs[0]
					});
					break;
				}
				case 5:
					if (DB.User.SurFreeGoldNum <= 0)
					{
						continue;
					}
					list.Add(new FreeGoldFailBanner
					{
						LookAdGetGoldReward = cf_const.freeGoldReward
					});
					break;
				default:
					continue;
				}
				if (list.Count >= cf_fail_dialog_const.FailBannerMaxShowCount)
				{
					return list;
				}
			}
			return list;
		}
	}
}
