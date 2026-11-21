using System;
using System.Collections.Generic;
using System.Linq;
using FQDev.StoreKit;
using Recharge;
using UnityEngine;

public class StoresMgr
{
	public StoreMultipleCtrl storeMultipleCtrl;

	private readonly Dictionary<string, string> _initProductPrices;

	private bool _isInit;

	public bool AdFree
	{
		get
		{
			return DB.User.AdFree;
		}
		set
		{
			DB.User.AdFree = value;
			if (DB.User.AdFree)
			{
				Global.HideBanner();
			}
		}
	}

	public bool PayingUsers
	{
		get
		{
			return DB.User.IsPayingUsers;
		}
		set
		{
			DB.User.IsPayingUsers = value;
		}
	}

	public StoresMgr()
	{
		_initProductPrices = new Dictionary<string, string>();
		storeMultipleCtrl = new StoreMultipleCtrl();
	}

	public void Init()
	{
		IAPManager.instance.initCallback = InitCallBack;
		IAPManager.instance.buyCall = BuyCallBack;
		List<ProductInfo> list = new List<ProductInfo>();
		for (int i = 0; i < DataCfg.cfbt_Storess.Count; i++)
		{
			cfbt_Stores cfbt_Stores = DataCfg.cfbt_Storess[i];
			ProductInfo item = default(ProductInfo);
			item.productId = cfbt_Stores.id;
			item.type = (UnityEngine.Purchasing.ProductType)cfbt_Stores.productType;
			list.Add(item);
		}
		IAPManager.instance.Init(list);
	}

	public void InitCallBack(bool isSuc, List<string> productIDs, string err)
	{
		if (isSuc)
		{
			_isInit = true;
			_initProductPrices.Clear();
			using (List<string>.Enumerator enumerator = productIDs.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string item = enumerator.Current;
					cfbt_Stores cfbt_Stores = DataCfg.cfbt_Storess.Find((cfbt_Stores a) => ProductId(a.id) == item);
					if (cfbt_Stores != null)
					{
						Product product = StoreUtility.Instance.GetProduct(item);
						if (!string.IsNullOrEmpty(product.Price))
						{
							cfbt_Stores.price = product.Price;
						}
						_initProductPrices.Add(item, cfbt_Stores.price);
					}
				}
			}
			StoreUtility.Instance.QueryIncompletePurchase();
			DispatcherMgr.Emit("IAPInitFinishedCallBack");
		}
		else
		{
			Debug.LogError("IAP 初始化错误: " + err);
		}
	}

	public void ResetStorePrice()
	{
		if (!_isInit || _initProductPrices.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<string, string> initProductPrice in _initProductPrices)
		{
			cfbt_Stores cfbt_Stores = DataCfg.cfbt_Storess.Find((cfbt_Stores a) => ProductId(a.id) == initProductPrice.Key);
			if (cfbt_Stores != null)
			{
				cfbt_Stores.price = initProductPrice.Value;
			}
		}
	}

	public void BuyCallBack(bool isSuc, string productId, string transactionID, string err)
	{
		SceneMgr.CloseWaiting();
		AdvertisingMgr.I.DisableWaiting();
		Global.IsShowAdOrPurchase = false;
		cfbt_Stores cfbt_Stores = DataCfg.cfbt_Storess.Find((cfbt_Stores a) => ProductId(a.id) == productId);
		if (cfbt_Stores == null)
		{
			Debug.LogError("IAP PRODUCTID:" + productId + " no find");
			return;
		}
		if (isSuc)
		{
			if (!PayingUsers)
			{
				PayingUsers = true;
				DB.User.FirstConsumeLv = DB.User.Curlv;
			}
			if (DB.GiftStalls.IsRecordStoreIndex(cfbt_Stores.index))
			{
				DB.User.TotalConsume += cfbt_Stores.usd;
				DB.GiftStalls.AddBuyCount();
			}
			Product product = StoreUtility.Instance.GetProduct(productId);
			if (cfbt_Stores.productType != 1 || (product != null && productId.Equals(product.ID)))
			{
				Singleton<RechargeActivityMgr>.I.AddRechargeValue(cfbt_Stores.usd);
			}
			_deal(transactionID, cfbt_Stores);
			DispatcherMgr.Emit("BuySuccessCallBack", cfbt_Stores.gType);
			DispatcherMgr.Emit("BuySuccessEvent", cfbt_Stores);
		}
		else
		{
			Debug.LogError("IAP buy error: " + err);
			DispatcherMgr.Emit("BuyFailCallBack", cfbt_Stores.gType);
		}
		DB.GiftStalls.RemoveStoreIndex(cfbt_Stores.index);
	}

	public List<cfbt_Stores> GetCfgs(BuySpotType type)
	{
		return DataCfg.cfbt_Storess.Where((cfbt_Stores a) => a.sType == (int)type).ToList();
	}

	public cfbt_Stores GetConfig(int id)
	{
		return DataCfg.cfbt_Storess.FirstOrDefault((cfbt_Stores a) => a.index == id);
	}

	public List<cfbt_Stores> GetCfgs(BuySpotType bType, GiftType gType)
	{
		return DataCfg.cfbt_Storess.Where((cfbt_Stores a) => a.sType == (int)bType && a.gType == (int)gType).ToList();
	}

	public cfbt_Stores GetStoreInfoByProductID(string productID)
	{
		return DataCfg.cfbt_Storess.Find((cfbt_Stores a) => ProductId(a.id) == productID);
	}

	public string ProductId(string id)
	{
		return id.Split('|')[0];
	}

	private void _deal(string transactionID, cfbt_Stores cfg)
	{
		int prop = DB.Bag.GetProp(PropID.Money);
		int prop2 = DB.Bag.GetProp(PropID.Hint);
		int prop3 = DB.Bag.GetProp(PropID.Freeze);
		int prop4 = DB.Bag.GetProp(PropID.Back);
		int prop5 = DB.Bag.GetProp(PropID.Refresh);
		int vipMultiple = 0;
		string item = cfg.item;
		switch (cfg.sType)
		{
		case 1:
			_reward(cfg, item, out vipMultiple);
			break;
		case 2:
			item = string.Format("0|{0}", DB.Other.PigBankGold);
			_reward(cfg, item, out vipMultiple);
			break;
		case 3:
			GiftDataMgr.ins.DealBuyGift(cfg);
			_reward(cfg, item, out vipMultiple);
			break;
		case 4:
			break;
		case 5:
			Singleton<RoyalPass.RoyalPassActivityMgr>.I.PaidSuccess();
			break;
		case 6:
		case 7:
			_reward(cfg, item, out vipMultiple);
			break;
		case 8:
			Singleton<ShortBattlePass.ShortBattlePassMgr>.I.PaidSuccess();
			break;
		case 9:
			_reward(cfg, item, out vipMultiple);
			break;
		default:
			Debug.LogError("pay error: type no find!");
			return;
		}
		if (string.IsNullOrEmpty(transactionID))
		{
			return;
		}
		DB.Track.BuyNum++;
		DateTime today = DateTime.Today;
		DateTime d = (string.IsNullOrEmpty(DB.Track.BuyInterval) ? today : MDateTime.Parse(DB.Track.BuyInterval, DateTime.Today));
		int days = (today - d).Days;
		DB.Track.BuyInterval = today.ToString();
		if (transactionID != null && !string.IsNullOrEmpty(transactionID.Trim()))
		{
			DB.Track.PayTotalMoney += cfg.usd;
			Singleton<LogEventValueOnceTrack>.I.Trigger(0, cfg.usd);
			Singleton<LogEventValueRepeatTrack>.I.Trigger(0, cfg.usd);
			LogEvent.I.Facebook_AdAndIapValueReport(cfg.usd);
			LogEvent.I.Facebook_Purchase_Impression(cfg);
			LogEvent.I.Firebase_Purchase(cfg.usd);
			LogEvent.I.Track("Shoping_Commodity_Reward")
				.Add("Commodity_ID", ProductId(cfg.id))
				.Add("Commodity_State", (vipMultiple < 1) ? "Normal" : "Doubled")
				.Add("BuyNum", DB.Track.BuyNum)
				.Add("RepeatTime", days)
				.Add("CommodityItem", item)
				.Add("Dill_ID", transactionID)
				.Add("Current_Money", prop)
				.Add("Current_Hint", prop2)
				.Add("Current_Frozen", prop3)
				.Add("Current_Back", prop4)
				.Add("Current_Refresh", prop5)
				.CommonAttr(true)
				.Send(true);
			LogEvent.I.Track("")
				.Add("PayNum", 1)
				.UserAdd();
			LogEvent.I.Track("")
				.Add("PayMoney", cfg.usd)
				.UserAdd();
			LogEvent.I.AF_Purchase(cfg);
		}
	}

	private void _reward(cfbt_Stores cfg, string items, out int vipMultiple)
	{
		vipMultiple = 0;
		if (storeMultipleCtrl.buyPlace == BuyPlace.Store && storeMultipleCtrl.HasBuyCount(cfg))
		{
			vipMultiple = cf_const.storeMultiple;
			storeMultipleCtrl.DealBuyCount(cfg.index);
			storeMultipleCtrl.buyPlace = BuyPlace.BuyProp;
		}
		int[,] array = items.ToInt(2, ',', '|');
		bool flag = false;
		for (int i = 0; i < array.GetLength(0); i++)
		{
			if (array[i, 0] == 20)
			{
				flag = true;
			}
			else
			{
				array[i, 1] += array[i, 1] * vipMultiple;
			}
		}
		if (cfg.sType == 2)
		{
			int oldNum = DB.Bag.Modify(PropID.Money, array[0, 1], false, BuySpotType.Pig.ToString());
			PigAnimDialog pigAnimDialog = SceneMgr.AddTopNode<PigAnimDialog>("Prefabs/PigAnimDialog");
			pigAnimDialog.Show(oldNum, array[0, 1]);
		}
		else
		{
			RewardUnifyDialog rewardUnifyDialog = Global.CreateReward(array, RewardType.None, REWARD_PLACE.None, ((BuySpotType)cfg.sType).ToString(), cfg.gType == 2, false);
			rewardUnifyDialog.SetComplete(delegate
			{
				Singleton<LoginMgr>.I.Cost();
				if (cfg.sType == 3)
				{
					GiftButtonsBox.BuyCallBack(cfg);
				}
			});
		}
		if (flag && !AdFree)
		{
			AdFree = true;
			LogEvent.I.Track("af_NoADS").Add("NoADS", "").SendAF();
		}
	}

	public void PopStoreDialog(GiftTriggerType type, bool isPause = true)
	{
		if (isPause)
		{
			DispatcherMgr.Emit("GamePause", true, "Store");
		}
		int buyNum = DB.Track.BuyNum;
		StoreDialog storeDialog = SceneMgr.AddTopNode<StoreDialog>("Prefabs/StorePopDialog");
		storeDialog.func_callback_close = delegate
		{
			if (isPause)
			{
				DispatcherMgr.Emit("GamePause", false, "Store");
			}
			if (buyNum == DB.Track.BuyNum)
			{
				GiftDataMgr.ins.TryPopGiftImmediately(type, null, isPause);
			}
		};
	}

	public List<cfbt_Stores> GetCfgsByTypeAndPropID(BuySpotType type, PropID propId)
	{
		return DataCfg.cfbt_Storess.Where((cfbt_Stores a) => a.sType == (int)type && a.gType == (int)propId).ToList();
	}
}
