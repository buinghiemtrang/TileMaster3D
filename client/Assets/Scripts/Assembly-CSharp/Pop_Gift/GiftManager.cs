using System;
using System.Collections.Generic;
using System.Linq;
using DataCenter;
using UnityEngine;

namespace Pop_Gift
{
	public class GiftManager
	{
		private static GiftManager _instance;

		public static GiftManager Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new GiftManager();
				}
				return _instance;
			}
		}

		public GiftBuyType GetBuyType(cfbt_Gift config)
		{
			return GiftBuyType.OriginalPrice;
		}

		public int GetBuyCostGoldCoin(cfbt_Gift config)
		{
			return 0;
		}

		public void AddInterstitialPlayCount(int count)
		{
			GiftNoAdsManager.Instance.AddInterstitialPlayCount(count);
		}

		public bool CheckHasRemainingBuyCount(cfbt_Gift config)
		{
			return true;
		}

		public void Init()
		{
			GiftNoAdsManager.Instance.Init();
		}

		public void RefreshData()
		{
			GiftNoAdsManager.Instance.RefreshData();
		}

		public void PopGift(GiftData data, Action<bool> callback)
		{
			if (data == null)
			{
				if (callback != null)
				{
					callback(false);
				}
			}
			else
			{
				CreateDialog(false, data, callback);
			}
		}

		public bool TryAutoPopGift(GiftTriggerPosition position)
		{
			return AutoPopGift(position, delegate
			{
			});
		}

		public bool TryAutoPopGift(GiftTriggerPosition position, Action callback)
		{
			return AutoPopGift(position, delegate
			{
				if (callback != null)
				{
					callback();
				}
			});
		}

		public bool TryAutoPopGift(GiftTriggerPosition position, Action<bool> callback)
		{
			return AutoPopGift(position, delegate(bool isBought, bool _)
			{
				if (callback != null)
				{
					callback(isBought);
				}
			});
		}

		public bool TryAutoPopGift(GiftTriggerPosition position, Action<bool, bool> callback, bool isHaveNoAdsGift)
		{
			return AutoPopGift(position, callback, isHaveNoAdsGift);
		}

		private bool AutoPopGift(GiftTriggerPosition position, Action<bool, bool> callback, bool isHaveNoAdsGift = true)
		{
			if (isHaveNoAdsGift && GiftNoAdsManager.Instance.TryAutoPopGift(position, callback, true))
			{
				return true;
			}
			return false;
		}

		public List<GiftData> GetFixedTypeGiftsData(GiftTypeInGift typeInGift)
		{
			List<GiftData> list = new List<GiftData>();
			foreach (cfbt_Gift item in GetGiftConfig(typeInGift))
			{
				int[] array = item.storeIndex.ToInt(',');
				if (array.Length != 0)
				{
					int storeId = array[0];
					cfbt_Stores cfbt_Stores = DataCfg.cfbt_Storess.FirstOrDefault((cfbt_Stores cfg) => cfg.index == storeId);
					if (cfbt_Stores != null)
					{
						GiftData item2 = new GiftData
						{
							BuyType = GiftBuyType.OriginalPrice,
							BuyCostGoldCoin = 0,
							IsShowContinueTime = false,
							StartShowTimeInMainScene = 0L,
							ContinueTime = 0L,
							StoresConfig = cfbt_Stores,
							GiftConfig = item
						};
						list.Add(item2);
					}
				}
			}
			return list;
		}

		private IEnumerable<cfbt_Gift> GetGiftConfig(GiftTypeInGift typeInGift)
		{
			List<cfbt_Gift> list = new List<cfbt_Gift>();
			GiftTriggerType triggerType = GetTriggerType(typeInGift);
			int stalls = DB.GiftStalls.GetStalls();
			List<int> list2 = new List<int>();
			foreach (cfbt_GiftStalls cfbt_GiftStalls in DataCfg.cfbt_GiftStallss)
			{
				if (cfbt_GiftStalls.stalls == stalls && cfbt_GiftStalls.triggerType == (int)triggerType && !string.IsNullOrEmpty(cfbt_GiftStalls.giftIds))
				{
					list2.AddRange(cfbt_GiftStalls.giftIds.ToInt('|'));
				}
			}
			for (int i = 0; i < list2.Count; i++)
			{
				int id = list2[i];
				cfbt_Gift cfbt_Gift = DataCfg.cfbt_Gifts.FirstOrDefault((cfbt_Gift cfg) => cfg.id == id);
				if (cfbt_Gift != null)
				{
					list.Add(cfbt_Gift);
				}
			}
			if (list.Count == 0)
			{
				Debug.LogError($"get gift config error. no find gift config. GiftType:{typeInGift}");
			}
			return list;
		}

		public void CreateDialog(bool isAutoPop, GiftData data, Action<bool> callback)
		{
			SceneMgr.AddTopNode<BaseGiftDialog>("Prefabs/Gift/NoAdGiftDialog").Init(isAutoPop, data, callback);
		}

		private GiftTriggerType GetTriggerType(GiftTypeInGift typeInGift)
		{
			if (typeInGift != GiftTypeInGift.NoAds)
			{
				Debug.LogError($"get gift trigger type error. GiftTypeInGift:{typeInGift}");
			}
			return GiftTriggerType.ImmeTrotting;
		}
	}
}
