using System;
using System.Collections.Generic;
using System.Linq;

namespace _Game._Functions.PropIceBreakingGift
{
	public class PropIceBreakingGiftManager : Singleton<PropIceBreakingGiftManager>
	{
		public PropIceBreakingGiftManager()
		{
			DispatcherMgr.OnG<GiftType>("BuySuccessCallBack", PaidGiftBuySuccess);
		}

		public override void Destroy()
		{
			base.Destroy();
			DispatcherMgr.OffG<GiftType>("BuySuccessCallBack", PaidGiftBuySuccess);
		}

		public void TryPopGift(Action closeCallback)
		{
			IceBreakingGiftInfo info = default(IceBreakingGiftInfo);
			if (DB.User.Curlv >= cf_propIceBreakingGift_const.PropIceBreakingGiftUnlockLevel && cf_propIceBreakingGift_const.PropIceBreakingGiftIsOpen != 0 && (IsEndTimeCd() || IsEndPassLevelCd()))
			{
				int prop = DB.Bag.GetProp(PropID.Money);
				int propIceBreakingGiftCoinLimit = cf_propIceBreakingGift_const.PropIceBreakingGiftCoinLimit;
				IceBreakingGiftType giftType;
				int type;
				if (prop < propIceBreakingGiftCoinLimit)
				{
					giftType = IceBreakingGiftType.Paid;
					type = DB.PropIceBreakingGift.PaidGiftType;
				}
				else
				{
					giftType = IceBreakingGiftType.Coin;
					type = -1;
				}
				List<cfgPropIceBreakingGift> configs = GetConfigs(giftType, type);
				if (configs != null)
				{
					RemoveNotMetConditionsConfig(configs);
					if (configs.Count >= 2)
					{
						configs = configs.OrderByDescending((cfgPropIceBreakingGift a) => a.Priority).ToList();
					}
					else if (configs.Count == 0)
					{
						closeCallback.InvokeSafe();
						return;
					}
					cfgPropIceBreakingGift cfgPropIceBreakingGift2 = configs[0];
					cfbt_Stores storeConfig = ((cfgPropIceBreakingGift2.BundleId != -1) ? GetStoreConfig(cfgPropIceBreakingGift2.BundleId) : null);
					info.GiftType = giftType;
					info.IceBreakingGiftConfig = cfgPropIceBreakingGift2;
					info.StoreConfig = storeConfig;
					PropIceBreakingGiftDialog.Create(info, closeCallback);
					DB.PropIceBreakingGift.LastShowTime = DateTime.Now.ToStringFull();
					DB.PropIceBreakingGift.AfterShowPassLevelCount = 0;
					if (prop < propIceBreakingGiftCoinLimit && cfgPropIceBreakingGift2.Type != DB.PropIceBreakingGift.PaidGiftType)
					{
						DB.PropIceBreakingGift.PaidGiftType = cfgPropIceBreakingGift2.Type;
						DB.PropIceBreakingGift.PaidGiftBuyCount = 0;
					}
					return;
				}
			}
			closeCallback.InvokeSafe();
		}

		public void PopGiftDialog(int propIceBreakId)
		{
			cfgPropIceBreakingGift cfgPropIceBreakingGift2 = DataCfg.cfgPropIceBreakingGifts.Find((cfgPropIceBreakingGift a) => a.Id == propIceBreakId);
			if (cfgPropIceBreakingGift2 != null && cfgPropIceBreakingGift2.BundleId != -1)
			{
				cfbt_Stores storeConfig = GetStoreConfig(cfgPropIceBreakingGift2.BundleId);
				IceBreakingGiftInfo info = default(IceBreakingGiftInfo);
				info.GiftType = IceBreakingGiftType.Paid;
				info.IceBreakingGiftConfig = cfgPropIceBreakingGift2;
				info.StoreConfig = storeConfig;
				PropIceBreakingGiftDialog.Create(info, null);
			}
		}

		public void AddPassLevelCount(int value)
		{
			DB.PropIceBreakingGift.AfterShowPassLevelCount = DB.PropIceBreakingGift.AfterShowPassLevelCount + value;
		}

		public void CoinGiftBuySuccess()
		{
			DB.PropIceBreakingGift.CoinGiftBuyCount = DB.PropIceBreakingGift.CoinGiftBuyCount + 1;
		}

		private void PaidGiftBuySuccess(GiftType type)
		{
			if (type == GiftType.PropIceBreakingGift)
			{
				DB.PropIceBreakingGift.PaidGiftBuyCount = DB.PropIceBreakingGift.PaidGiftBuyCount + 1;
			}
		}

		private List<cfgPropIceBreakingGift> GetConfigs(IceBreakingGiftType giftType, int type)
		{
			int num = ((giftType == IceBreakingGiftType.Coin) ? DB.PropIceBreakingGift.CoinGiftBuyCount : DB.PropIceBreakingGift.PaidGiftBuyCount);
			while (true)
			{
				List<cfgPropIceBreakingGift> list = DataCfg.cfgPropIceBreakingGifts.Where((cfgPropIceBreakingGift w) => w.Type == type).ToList();
				if (list.Count == 0)
				{
					return null;
				}
				if (list[0].BuyCountLimit == -1)
				{
					return list;
				}
				if (num < list[0].BuyCountLimit)
				{
					return list;
				}
				type = list[0].TypeUpgrade;
				num = 0;
				if (type < 1)
				{
					break;
				}
			}
			return null;
		}

		private void RemoveNotMetConditionsConfig(IList<cfgPropIceBreakingGift> configs)
		{
			for (int num = configs.Count - 1; num >= 0; num--)
			{
				cfgPropIceBreakingGift cfgPropIceBreakingGift2 = configs[num];
				if (cfgPropIceBreakingGift2.PropCount < DB.Bag.GetProp((PropID)cfgPropIceBreakingGift2.PropId))
				{
					configs.RemoveAt(num);
				}
			}
		}

		private cfbt_Stores GetStoreConfig(int id)
		{
			return DataCfg.cfbt_Storess.FirstOrDefault((cfbt_Stores w) => w.index == id);
		}

		private bool IsEndTimeCd()
		{
			if (cf_propIceBreakingGift_const.PropIceBreakingGiftTimeCD == -1)
			{
				return false;
			}
			if (string.IsNullOrEmpty(DB.PropIceBreakingGift.LastShowTime))
			{
				return true;
			}
			DateTime now = DateTime.Now;
			return now >= now.ParseFull(DB.PropIceBreakingGift.LastShowTime).AddSeconds(cf_propIceBreakingGift_const.PropIceBreakingGiftTimeCD);
		}

		private bool IsEndPassLevelCd()
		{
			if (cf_propIceBreakingGift_const.PropIceBreakingGiftLevelCD == -1)
			{
				return false;
			}
			if (DB.PropIceBreakingGift.AfterShowPassLevelCount < 0)
			{
				return true;
			}
			return cf_propIceBreakingGift_const.PropIceBreakingGiftLevelCD <= DB.PropIceBreakingGift.AfterShowPassLevelCount;
		}
	}
}
