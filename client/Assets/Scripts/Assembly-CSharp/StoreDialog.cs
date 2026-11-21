using System;
using System.Collections.Generic;
using RoyalPass;
using ShortBattlePass;
using UnityEngine;
using UnityEngine.UI;

public class StoreDialog : MonoBehaviour
{
	[SerializeField]
	private StoreItem pRoyalPassActivityGiftItem;

	[SerializeField]
	private StoreItem shortBattlePassGiftItem;

	[SerializeField]
	private StoreItem[] pStorePropGiftItems;

	[SerializeField]
	private GameObject objCardBoxRootNode;

	[SerializeField]
	private GameObject btnResumePurchase;

	[SerializeField]
	private Transform tfMoreBtn;

	[SerializeField]
	private StoreItem adsStoreItem;

	private List<StoreItem> _items;

	[SerializeField]
	private ScrollRect scroll;

	[SerializeField]
	private Sprite[] spBigIcon;

	[SerializeField]
	private StoreItem bigPrefab;

	[SerializeField]
	private StoreItem smallPrefab;

	public Action func_callback_close;

	private DialogShow _pDialog;

	private List<cfbt_Stores> _cfgs;

	private float vipMultiple;

	private bool _isMore;

	protected void OnDestroy()
	{
		DispatcherMgr.Off<GiftType>("BuySuccessCallBack", _buySucc);
		DispatcherMgr.Off("IAPInitFinishedCallBack", _iAPInitSuc);
	}

	protected void Awake()
	{
		btnResumePurchase.SetActive(false);
		_pDialog = GetComponent<DialogShow>();
		_items = new List<StoreItem>();
		_cfgs = new List<cfbt_Stores>();
		adsStoreItem.gameObject.SetActive(false);
		objCardBoxRootNode.SetActive(false);
		pRoyalPassActivityGiftItem.SetActive(false);
		shortBattlePassGiftItem.SetActive(false);
		DispatcherMgr.On<GiftType>("BuySuccessCallBack", _buySucc);
		DispatcherMgr.On("IAPInitFinishedCallBack", _iAPInitSuc);
	}

	private void Start()
	{
		LogEvent.I.Track("Shoping_Open").Add("OpenType", "Icon").CommonAttr(true).Send(false);
		for (int i = 0; i < pStorePropGiftItems.Length; i++)
		{
			pStorePropGiftItems[i].transform.SetParent(scroll.content, false);
		}
		List<cfbt_Stores> list = new List<cfbt_Stores>();
		_refreshPropGiftItems(list);
		_cfgs.Clear();
		List<cfbt_Stores> cfgs = Global.pStores.GetCfgs(BuySpotType.Stores, GiftType.Big);
		int num = Math.Max(0, pStorePropGiftItems.Length - list.Count);
		for (int j = 0; j < num; j++)
		{
			_cfgs.Add(cfgs[j]);
		}
		List<cfbt_Stores> cfgs2 = Global.pStores.GetCfgs(BuySpotType.Stores, GiftType.Small);
		if (list.Count == 2)
		{
			int num2 = cfgs2.FindLastIndex((cfbt_Stores w) => w.usd <= list[0].usd);
			int num3 = cfgs2.FindLastIndex((cfbt_Stores w) => w.usd <= list[1].usd);
			if (num2 < num3)
			{
				_cfgs.Add(cfgs2[num3]);
				_cfgs.Add(cfgs2[num2]);
			}
			else if (num2 > num3)
			{
				_cfgs.Add(cfgs2[num2]);
				_cfgs.Add(cfgs2[num3]);
			}
			else
			{
				int num4 = (num2 < cfgs2.Count - 1) ? num2 : (cfgs2.Count - 2);
				_cfgs.Add(cfgs2[num4 + 1]);
				_cfgs.Add(cfgs2[num4]);
			}
		}
		else if (list.Count == 1)
		{
			int num5 = cfgs2.FindLastIndex((cfbt_Stores w) => w.usd <= list[0].usd);
			int num6 = (num5 < 0) ? 0 : ((num5 < cfgs2.Count - 1) ? num5 : (cfgs2.Count - 2));
			_cfgs.Add(cfgs2[num6 + 1]);
			_cfgs.Add(cfgs2[num6]);
		}
		else if (list.Count == 0)
		{
			cfbt_Stores cfbt_Stores = cfgs2.Find((cfbt_Stores w) => w.name == "BUCKET OF GOLDS");
			if (cfbt_Stores != null)
			{
				_cfgs.Add(cfbt_Stores);
			}
			cfbt_Stores cfbt_Stores2 = cfgs2.Find((cfbt_Stores w) => w.name == "FISTFUL OF GOLDS");
			if (cfbt_Stores2 != null)
			{
				_cfgs.Add(cfbt_Stores2);
			}
		}
		_InitItem();
		_refreshItems();
		tfMoreBtn.SetParent(scroll.content, false);
	}

	private void _InitItem()
	{
		int num = 0;
		for (int i = 0; i < _cfgs.Count; i++)
		{
			cfbt_Stores cfbt_Stores = _cfgs[i];
			GiftType gType = (GiftType)cfbt_Stores.gType;
			StoreItem storeItem;
			if (gType == GiftType.Big)
			{
				storeItem = UnityEngine.Object.Instantiate(bigPrefab, scroll.content, false);
				storeItem.LoadItemIcon(spBigIcon[num++]);
			}
			else if (gType == GiftType.Small || gType == GiftType.PropHint || gType == GiftType.PropFreeze || gType == GiftType.PropMagnet || gType == GiftType.PropBack || gType == GiftType.PropRefresh || gType == GiftType.PropEnergyTime)
			{
				storeItem = UnityEngine.Object.Instantiate(smallPrefab, scroll.content, false);
			}
			else if (gType == GiftType.Ads)
			{
				adsStoreItem.gameObject.SetActive(!Global.pStores.AdFree);
				storeItem = adsStoreItem;
			}
			else
			{
				continue;
			}
			storeItem.transform.SetParent(scroll.content, false);
			storeItem.refreshCallback = _refresh;
			_items.Add(storeItem);
		}
	}

	private void _buySucc(GiftType type)
	{
		if ((1 << (int)type & 0x20DC17) != 0)
		{
			if (_cfgs != null)
			{
				_refreshItems();
				if (_isMore)
				{
					_refreshRoyalPassItems();
					_RefreshShortBattlePassItem();
				}
				_refreshPropGiftItems();
			}
		}
		else if (type == GiftType.BattleToken)
		{
			Singleton<RoyalPassActivityMgr>.I.ShowBuySuccessRewardsDialog(_refresh);
			LogEvent.I.Track("Master_Pass_Callback")
				.Add("GradeNum", DB.RoyalPassActivity.CurGrade)
				.Add("Type", "Shop")
				.CommonAttr(true)
				.Send(false);
		}
		else if (type == GiftType.ShortBattlePass)
		{
			Singleton<ShortBattlePassMgr>.I.ShowBuySuccessRewardsDialog(_refresh);
			LogEvent.I.Track("Little_Master_Pass_Callback")
				.Add("GradeNum", DB.ShortBattlePass.Data.Grade)
				.Add("Type", "Shop")
				.CommonAttr(true)
				.Send(false);
		}
	}

	private void _iAPInitSuc()
	{
		_refresh();
	}

	public void ClickClose(MButton btn)
	{
		btn.interactable = false;
		_pDialog._Close(func_callback_close);
	}

	public void ClickBtnResumePurchase(MButton button)
	{
		if (IAPManager.instance.isInitialized)
		{
			SceneMgr.OpenWaiting(30f);
			IAPManager.instance.RestorePurchases(() => SceneMgr.CloseWaiting());
		}
		else
		{
			Global.CreateMsgBoxDialog("Language54");
		}
	}

	public void ClickMoreItem(MButton button)
	{
		_isMore = true;
		foreach (StoreItem item in _items)
		{
			item.DestroySelf();
		}
		_items.Clear();
		tfMoreBtn.DestroySelf();
		objCardBoxRootNode.transform.SetParent(scroll.content, false);
		_cfgs.Clear();
		_cfgs.AddRange(Global.pStores.GetCfgs(BuySpotType.Stores));
		_cfgs.AddRange(Global.pStores.GetCfgs(BuySpotType.BuyProp));
		_InitItem();
		_refreshCardBoxItems();
		_refresh();
		btnResumePurchase.transform.SetParent(scroll.content, false);
		pRoyalPassActivityGiftItem.transform.SetParent(scroll.content, false);
		pRoyalPassActivityGiftItem.transform.SetSiblingIndex(1);
		shortBattlePassGiftItem.transform.SetParent(scroll.content, false);
		shortBattlePassGiftItem.transform.SetSiblingIndex(2);
	}

	private void _refresh()
	{
		if (_cfgs != null)
		{
			_refreshItems();
			if (_isMore)
			{
				_refreshRoyalPassItems();
				_RefreshShortBattlePassItem();
			}
			_refreshPropGiftItems();
		}
	}

	private void _refreshItems()
	{
		if (_cfgs == null)
		{
			return;
		}
		for (int i = 0; i < _cfgs.Count; i++)
		{
			StoreItem storeItem = _items[i];
			cfbt_Stores cfbt_Stores = _cfgs[i];
			GiftType gType = (GiftType)cfbt_Stores.gType;
			if (gType == GiftType.Big || gType == GiftType.Small)
			{
				storeItem.Init(cfbt_Stores, null, vipMultiple);
			}
			else if (gType == GiftType.Ads)
			{
				if (!Global.pStores.AdFree)
				{
					storeItem.Init(cfbt_Stores, null, vipMultiple);
				}
			}
			else if (gType == GiftType.PropHint || gType == GiftType.PropFreeze || gType == GiftType.PropMagnet || gType == GiftType.PropBack || gType == GiftType.PropRefresh || gType == GiftType.PropEnergyTime)
			{
				storeItem.LoadItemIcon(string.Format("Props/prop{0}", (int)gType));
				storeItem.Init(cfbt_Stores, null, vipMultiple);
			}
		}
	}

	private void _refreshRoyalPassItems()
	{
		bool flag = Singleton<RoyalPassActivityMgr>.I.JudgeOpen() && !Singleton<RoyalPassActivityMgr>.I.JudgeHasState(RoyalPass.ActivityState.PaidSuccess);
		pRoyalPassActivityGiftItem.gameObject.SetActive(flag);
		if (flag)
		{
			List<cfbt_Stores> cfgs = Global.pStores.GetCfgs(BuySpotType.BattleToken, GiftType.BattleToken);
			if (cfgs.Count == 0)
			{
				pRoyalPassActivityGiftItem.gameObject.SetActive(false);
			}
			else
			{
				pRoyalPassActivityGiftItem.Init(cfgs[0], null, vipMultiple);
			}
		}
	}

	private void _RefreshShortBattlePassItem()
	{
		bool flag = Singleton<ShortBattlePassMgr>.I.JudgeOpen() && !Singleton<ShortBattlePassMgr>.I.JudgeHasState(ShortBattlePass.ActivityState.CostSuccess);
		shortBattlePassGiftItem.gameObject.SetActive(flag);
		if (flag)
		{
			List<cfbt_Stores> cfgs = Global.pStores.GetCfgs(BuySpotType.ShortBattlePass, GiftType.ShortBattlePass);
			if (cfgs.Count == 0)
			{
				shortBattlePassGiftItem.gameObject.SetActive(false);
			}
			else
			{
				shortBattlePassGiftItem.Init(cfgs[0], null, vipMultiple);
			}
		}
	}

	private void _refreshPropGiftItems(ICollection<cfbt_Stores> cfgs = null)
	{
		List<cfbt_Gift> list = new List<cfbt_Gift>();
		GiftDataMgr ins = GiftDataMgr.ins;
		if (ins != null && ins.buttonShowGifts != null)
		{
			foreach (cfbt_Gift buttonShowGift in ins.buttonShowGifts)
			{
				if (buttonShowGift.storeIndex.ToInt(',').Length == 1)
				{
					list.Add(buttonShowGift);
				}
			}
			if (pStorePropGiftItems != null)
			{
				for (int i = pStorePropGiftItems.Length - 1; i >= 0; i--)
				{
					if (i < list.Count)
					{
						cfbt_Stores storeCfg = GiftDataMgr.ins.GetStoreCfg(list[i]);
						if (cfgs != null)
						{
							cfgs.Add(storeCfg);
						}
						pStorePropGiftItems[i].Init(storeCfg, list[i], vipMultiple);
					}
					else
					{
						pStorePropGiftItems[i].gameObject.SetActive(false);
					}
				}
			}
		}
	}

	private void _refreshCardBoxItems()
	{
		if (objCardBoxRootNode == null)
		{
			return;
		}
		if (cf_const.shopCardBoxQuality != null && cf_const.shopCardBoxQuality.GetLength(0) != 0 && DB.User.Maxlv >= cf_const.cardSystemUnlockLevel)
		{
			objCardBoxRootNode.SetActive(true);
			StoreCardBoxItem[] componentsInChildren = objCardBoxRootNode.GetComponentsInChildren<StoreCardBoxItem>(true);
			if (componentsInChildren.Length < 1)
			{
				return;
			}
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (i < cf_const.shopCardBoxQuality.GetLength(0))
				{
					componentsInChildren[i].InitData(cf_const.shopCardBoxQuality[i, 0], cf_const.shopCardBoxQuality[i, 1], cf_const.shopCardBoxQuality[i, 2]);
				}
				else
				{
					componentsInChildren[i].gameObject.SetActive(false);
				}
			}
		}
		else
		{
			objCardBoxRootNode.SetActive(false);
		}
	}
}
