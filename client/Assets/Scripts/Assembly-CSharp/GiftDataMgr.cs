using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GiftDataMgr
{
	private static GiftDataMgr _ins;

	public List<cfbt_Gift> buttonShowGifts;

	private List<cfbt_Gift> popGifts;

	private bool isShowPopGift;

	public static GiftDataMgr ins
	{
		get
		{
			if (_ins == null)
			{
				_ins = new GiftDataMgr();
				_ins.buttonShowGifts = new List<cfbt_Gift>();
				_ins.popGifts = new List<cfbt_Gift>();
			}
			return _ins;
		}
	}

	public GiftDataMgr()
	{
		isShowPopGift = true;
	}

	public void CleanCache()
	{
		buttonShowGifts.Clear();
		popGifts.Clear();
	}

	public bool TryPopGift()
	{
		if (!isShowPopGift)
		{
			return false;
		}
		isShowPopGift = false;
		if (buttonShowGifts.Count == 0)
		{
			return false;
		}
		cfbt_Gift cfg;
		if (popGifts.Count == 0)
		{
			int num = MMath.Random(buttonShowGifts.Count);
			Debug.Log(string.Format("pop_gift_index:{0}", num));
			cfg = buttonShowGifts[num];
		}
		else
		{
			cfg = popGifts[0];
			Debug.Log(string.Format("pop_gift_cfgId:{0}", cfg.id));
			popGifts.RemoveAt(0);
			DB.GiftStalls.SetGiftDataPop(cfg);
		}
		int[] array = cfg.storeIndex.ToInt(',');
		if (array.Length == 1)
		{
			GiftData giftDataById = DB.GiftStalls.GetGiftDataById(cfg.id);
			cfbt_Stores storeCfg = GetStoreCfg(cfg);
			GiftPop.pop(cfg, giftDataById, storeCfg);
		}
		else
		{
			GiftData giftDataById2 = DB.GiftStalls.GetGiftDataById(cfg.id);
			GiftPropDialog.pop(cfg, giftDataById2);
		}
		CheckResetOfflineDays(cfg);
		return true;
	}

	public cfbt_Gift GetShowGiftInMainScene()
	{
		if (buttonShowGifts.Count == 0)
		{
			return null;
		}
		int index = MMath.Random(buttonShowGifts.Count);
		return buttonShowGifts[index];
	}

	public void CheckResetOfflineDays(cfbt_Gift cfg)
	{
		if (GetAllGiftCfgIdsByType(GiftTriggerType.Recall).Contains(cfg.id))
		{
			LogEvent.I.Track("Recall_Gift_Open")
				.Add("RecallTime", DB.GiftStalls.OfflineDays)
				.Add("Level_ID", DB.User.Maxlv + 1)
				.Add("Gift_ID", cfg.id)
				.Send();
			DB.GiftStalls.OfflineDays = 0;
		}
	}

	public void GetGiftCfg(GiftTriggerType type, out cfbt_Gift giftCfg, out cfbt_Stores storeCfg)
	{
		giftCfg = null;
		storeCfg = null;
		if (IAPManager.instance.isInitialized)
		{
			giftCfg = GetPopGiftImmediatelyCfg(type);
			if (giftCfg != null)
			{
				storeCfg = GetStoreCfg(giftCfg);
				if (storeCfg == null)
				{
					Debug.LogError("gift data error, no find store cfg: " + giftCfg.storeIndex);
				}
			}
		}
	}

	public bool TryPopGiftImmediately(GiftTriggerType k_type, Action callback = null, bool isPause = true)
	{
		if (!IAPManager.instance.isInitialized)
		{
			if (callback != null)
			{
				callback();
			}
			return false;
		}
		cfbt_Gift popGiftImmediatelyCfg = GetPopGiftImmediatelyCfg(k_type);
		if (popGiftImmediatelyCfg == null)
		{
			if (callback != null)
			{
				callback();
			}
			return false;
		}
		if (k_type == GiftTriggerType.ImmeTrotting)
		{
			LogEvent.I.HomeFrameOpen("RemoveAD", "Click");
		}
		if (isPause)
		{
			DispatcherMgr.Emit("GamePause", new object[] { true, "GiftData" });
		}
		GiftData giftDataById = DB.GiftStalls.GetGiftDataById(popGiftImmediatelyCfg.id);
		int[] array = popGiftImmediatelyCfg.storeIndex.ToInt(',');
		if (array.Length == 1)
		{
			cfbt_Stores storeCfg = GetStoreCfg(popGiftImmediatelyCfg);
			GiftPop.pop(popGiftImmediatelyCfg, giftDataById, storeCfg);
			GiftPop.ins.func_close = delegate(bool isNoBuy)
			{
				if (isPause)
				{
					DispatcherMgr.Emit("GamePause", new object[] { false, "GiftData" });
				}
				if (callback != null)
				{
					callback();
				}
			};
			return true;
		}
		else
		{
			GiftData giftDataById2 = DB.GiftStalls.GetGiftDataById(popGiftImmediatelyCfg.id);
			GiftPropDialog.pop(popGiftImmediatelyCfg, giftDataById2);
			GiftPropDialog.ins.func_close = delegate(bool isNoBuy)
			{
				if (isPause)
				{
					DispatcherMgr.Emit("GamePause", new object[] { false, "GiftData" });
				}
				if (callback != null)
				{
					callback();
				}
			};
			return true;
		}
	}

	public void GetTriggerMain()
	{
		DB.GiftStalls.CheckOfflineTime();
		buttonShowGifts.Clear();
		popGifts.Clear();
		RefreshHomeGiftDataByRecord();
		if (!isShowPopGift)
		{
			return;
		}
		if (buttonShowGifts.Count >= cf_const.showGiftButtonNum)
		{
			return;
		}
		Dictionary<GiftTriggerType, int> dictionary = new Dictionary<GiftTriggerType, int>();
		dictionary.Add(GiftTriggerType.Recall, 1);
		dictionary.Add(GiftTriggerType.Cheap, 1);
		dictionary.Add(GiftTriggerType.Expensive, 2);
		dictionary.Add(GiftTriggerType.Random, 2);
		List<GiftTriggerType> list = dictionary.Keys.ToList();
		Debug.Log("-----------------------refresh gift loop start----------------------------");
		for (int i = 0; i < list.Count; i++)
		{
			int count = buttonShowGifts.Count;
			int num = cf_const.showGiftButtonNum - count;
			GiftTriggerType giftTriggerType = list[i];
			Debug.Log(string.Format("type:{0},sumContainsCount:{1},sumLeftCount:{2}", giftTriggerType, count, num));
			if (num < 1)
			{
				break;
			}
			if (giftTriggerType == GiftTriggerType.Random)
			{
				Debug.Log(string.Format("n_show_count:{0}", num));
				RefreshButtonShowGiftCfgs(giftTriggerType, num, giftTriggerType == GiftTriggerType.Recall);
				Debug.Log(string.Format("buttonShowGiftsCount:{0},popGiftsCount:{1}", buttonShowGifts.Count, popGifts.Count));
			}
			else
			{
				int num2 = dictionary[giftTriggerType];
				int containsCount = GetContainsCount(giftTriggerType);
				Debug.Log(string.Format("maxCountOfCurType:{0},containsCountOfCurType:{1}", num2, containsCount));
				int num3 = num2 - containsCount;
				if (num3 > 0)
				{
					if (num3 <= num)
					{
						num = num3;
					}
					Debug.Log(string.Format("n_show_count:{0}", num));
					RefreshButtonShowGiftCfgs(giftTriggerType, num, giftTriggerType == GiftTriggerType.Recall);
					Debug.Log(string.Format("buttonShowGiftsCount:{0},popGiftsCount:{1}", buttonShowGifts.Count, popGifts.Count));
				}
			}
		}
		Debug.Log("-----------------------refresh gift loop end----------------------------");
		if (popGifts.Count == 0 && buttonShowGifts.Count < cf_const.showGiftButtonNum)
		{
			ResetRandomGiftData();
			RefreshButtonShowGiftCfgs(GiftTriggerType.Random, cf_const.showGiftButtonNum - buttonShowGifts.Count, false);
		}
	}

	private void ResetRandomGiftData()
	{
		List<cfbt_Gift> giftCfgByType = GetGiftCfgByType(GiftTriggerType.Random);
		for (int i = 0; i < giftCfgByType.Count; i++)
		{
			cfbt_Gift cfbt_Gift = giftCfgByType[i];
			if (!buttonShowGifts.Contains(cfbt_Gift))
			{
				DB.GiftStalls.FindAndRefresh(cfbt_Gift, true);
			}
		}
	}

	public cfbt_Stores GetStoreCfg(cfbt_Gift gift)
	{
		return GetStoreCfgByIndex(gift.storeIndex.ToInt(',')[0]);
	}

	public cfbt_Stores GetStoreCfgByIndex(int storeIndex)
	{
		return DataCfg.cfbt_Storess.Find((cfbt_Stores cfg) => cfg.index == storeIndex);
	}

	private bool JudgeIAPIsInitSuccessed(cfbt_Gift cfg)
	{
		Logs.Common.Info(string.Format("giftId:{0} storeIndex:{1}", cfg.id, cfg.storeIndex), Array.Empty<object>());
		int[] array = cfg.storeIndex.ToInt(',');
		for (int i = 0; i < array.Length; i++)
		{
			int num = array[i];
			cfbt_Stores storeCfgByIndex = GetStoreCfgByIndex(num);
			Logs.Common.Info(string.Format("storeIndex:{0} price:{1}", num, storeCfgByIndex.price), Array.Empty<object>());
			if (storeCfgByIndex.price == "")
			{
				return false;
			}
		}
		return true;
	}

	private bool JudgeButtonGiftData(cfbt_Gift cfg, GiftData data)
	{
		if (data == null)
		{
			return true;
		}
		if (cfg.continueTime != -1 && data.timer <= DateTime.Now)
		{
			return false;
		}
		return !data.isBuy;
	}

	private bool JudgeImmediatelyGiftData(cfbt_Gift cfg, GiftData data)
	{
		if (data == null)
		{
			return true;
		}
		if (cfg.continueTime != -1 && data.timer <= DateTime.Now)
		{
			return false;
		}
		return data.nPop != 0;
	}

	private bool JudgeConfigCondition(string conditionStr, bool bIsRecord = false)
	{
		string[] array = conditionStr.Split('|');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(',');
			if (array2.Length > 1)
			{
				int num = int.Parse(array2[0]);
				int num2 = int.Parse(array2[1]);
				if (num < 5)
				{
					if (num != 2 && num != 3 && num != 4)
					{
						if (num != 1)
						{
							return false;
						}
						if (DB.User.Maxlv < num2)
						{
							return false;
						}
					}
				}
				else if (num == 5)
				{
					if (DB.Track.PlayDays < num2 || DB.User.TotalConsume != 0f)
					{
						return false;
					}
				}
				else
				{
					if (num != 6)
					{
						return false;
					}
					if (!bIsRecord && DB.GiftStalls.OfflineDays < num2)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public List<cfbt_Gift> GetGiftCfgByType(GiftTriggerType type)
	{
		int stalls = DB.GiftStalls.Stalls;
		Debug.Log(string.Format("curStallls:{0}", stalls));
		return GetGiftCfgByStallsAndType(stalls, type);
	}

	public List<cfbt_Gift> GetGiftCfgByStallsAndType(int stalls, GiftTriggerType type)
	{
		List<cfbt_Gift> list = new List<cfbt_Gift>();
		cfbt_GiftStalls giftStallsCfgByStallsAndType = GetGiftStallsCfgByStallsAndType(stalls, type);
		if (giftStallsCfgByStallsAndType != null)
		{
			int[] array = giftStallsCfgByStallsAndType.giftIds.ToInt('|');
			for (int i = 0; i < array.Length; i++)
			{
				cfbt_Gift giftCfgById = GetGiftCfgById(array[i]);
				if (giftCfgById != null)
				{
					list.Add(giftCfgById);
				}
			}
		}
		return list;
	}

	public cfbt_GiftStalls GetGiftStallsCfgByStallsAndType(int stalls, GiftTriggerType type)
	{
		foreach (cfbt_GiftStalls cfbt_GiftStalls in DataCfg.cfbt_GiftStallss)
		{
			if (cfbt_GiftStalls.stalls == stalls && cfbt_GiftStalls.triggerType == (int)type)
			{
				return cfbt_GiftStalls;
			}
		}
		return null;
	}

	private List<int> GetAllGiftCfgIdsByType(GiftTriggerType type)
	{
		List<int> list = new List<int>();
		foreach (cfbt_GiftStalls cfbt_GiftStalls in DataCfg.cfbt_GiftStallss)
		{
			if (cfbt_GiftStalls.triggerType == (int)type)
			{
				list.AddRange(cfbt_GiftStalls.giftIds.ToInt('|').ToList());
			}
		}
		return list;
	}

	public cfbt_Gift GetGiftCfgById(int id)
	{
		return DataCfg.cfbt_Gifts.Find((cfbt_Gift cfg) => cfg.id == id);
	}

	private void RefreshHomeGiftDataByRecord()
	{
		DateTime now = DateTime.Now;
		for (int i = 0; i < cf_const.showGiftButtonNum; i++)
		{
			int showGiftButtonDataByIndex = DB.GiftStalls.GetShowGiftButtonDataByIndex(i);
			Debug.Log(string.Format("RefreshHomeGiftDataByRecord start i:{0},giftCfgId:{1}", i, showGiftButtonDataByIndex));
			if (showGiftButtonDataByIndex != 0)
			{
				cfbt_Gift giftCfgById = GetGiftCfgById(showGiftButtonDataByIndex);
				GiftData giftDataById = DB.GiftStalls.GetGiftDataById(showGiftButtonDataByIndex);
				if (giftDataById == null)
				{
					DB.GiftStalls.SetShowGiftButtonDataByIndex(i, 0);
					DB.GiftStalls.SetShowGiftButtonTypeDataByIndex(i, 0);
				}
				else
				{
					if (giftCfgById.continueTime != -1 && giftDataById.timer <= now)
					{
						DB.GiftStalls.SetShowGiftButtonDataByIndex(i, 0);
						DB.GiftStalls.SetShowGiftButtonTypeDataByIndex(i, 0);
					}
					else
					{
						bool flag = JudgeConfigCondition(giftCfgById.popConditions, true);
						GiftData giftData = DB.GiftStalls.FindAndRefresh(giftCfgById, false);
						if (flag)
						{
							if (buttonShowGifts.Count < cf_const.showGiftButtonNum && JudgeButtonGiftData(giftCfgById, giftData))
							{
								buttonShowGifts.Add(giftCfgById);
								if (giftData == null || giftData.nPop > 0)
								{
									popGifts.Add(giftCfgById);
								}
								Debug.Log(string.Format("RefreshHomeGiftDataByRecord success giftId: {0}", showGiftButtonDataByIndex));
							}
							else
							{
								DB.GiftStalls.SetShowGiftButtonDataByIndex(i, 0);
								DB.GiftStalls.SetShowGiftButtonTypeDataByIndex(i, 0);
								Debug.Log(string.Format("RefreshHomeGiftDataByRecord fail giftId: {0}", showGiftButtonDataByIndex));
							}
						}
						else
						{
							DB.GiftStalls.SetShowGiftButtonDataByIndex(i, 0);
							DB.GiftStalls.SetShowGiftButtonTypeDataByIndex(i, 0);
							Debug.Log(string.Format("RefreshHomeGiftDataByRecord fail giftId: {0}", showGiftButtonDataByIndex));
						}
					}
				}
			}
		}
	}

	private int GetShowButtonFreeIndex()
	{
		for (int i = 0; i < cf_const.showGiftButtonNum; i++)
		{
			if (DB.GiftStalls.GetShowGiftButtonDataByIndex(i) == 0)
			{
				return i;
			}
		}
		return -1;
	}

	private int GetContainsCount(GiftTriggerType type)
	{
		int num = 0;
		for (int i = cf_const.showGiftButtonNum - 1; i >= 0; i--)
		{
			if (DB.GiftStalls.GetShowButtonTypeDataByIndex(i) == (int)type)
			{
				num++;
			}
		}
		return num;
	}

	public void ResetShowGiftButtonData(int giftCfgId)
	{
		for (int i = cf_const.showGiftButtonNum - 1; i >= 0; i--)
		{
			if (DB.GiftStalls.GetShowGiftButtonDataByIndex(i) == giftCfgId)
			{
				DB.GiftStalls.SetShowGiftButtonDataByIndex(i, 0);
				DB.GiftStalls.SetShowGiftButtonTypeDataByIndex(i, 0);
			}
		}
	}

	private void PrintAllGiftsPrice()
	{
		foreach (cfbt_Gift gift in DataCfg.cfbt_Gifts)
		{
			cfbt_Stores storeCfg = GetStoreCfg(gift);
			Debug.Log(string.Format("giftId:{0} storeIndex:{1} price:{2}", gift.id, gift.storeIndex, storeCfg.price));
		}
	}

	private bool IsValidTime(cfbt_Gift cfg)
	{
		DayOfWeek dayOfWeek = DateTime.Now.DayOfWeek;
		int[] array = cfg.showWeek.ToInt('|');
		if (array.Length != 0 && (array[0] == -1 || array.Contains((int)dayOfWeek)))
		{
			string[] array2 = cfg.showTime.Split('|');
			for (int i = 0; i < array2.Length; i++)
			{
				string[] array3 = array2[i].Split('-');
				DateTime now = DateTime.Now;
				if (now >= MDateTime.Parse(array3[0], DateTime.MaxValue) && now < MDateTime.Parse(array3[1], DateTime.MinValue))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void RefreshButtonShowGiftCfgs(GiftTriggerType type, int nShowCount, bool bIsNotLoop = false)
	{
		List<cfbt_Gift> giftCfgByType = GetGiftCfgByType(type);
		int giftTypeIndexData = DB.GiftStalls.GetGiftTypeIndexData(type);
		Debug.Log(string.Format("cfgsCount:{0},typeIndex:{1}", giftCfgByType.Count, giftTypeIndexData));
		if (bIsNotLoop || giftCfgByType.Count - 1 <= giftTypeIndexData)
		{
			giftTypeIndexData = -1;
		}
		Debug.Log(string.Format("typeIndex:{0}", giftTypeIndexData));
		int num = 0;
		for (int i = 0; i < giftCfgByType.Count; i++)
		{
			if (i > giftTypeIndexData)
			{
				cfbt_Gift cfbt_Gift = giftCfgByType[i];
				Debug.Log(string.Format("giftCfgId:{0}", cfbt_Gift.id));
				if (CheckButtonGiftAllConditions(cfbt_Gift))
				{
					if (CanAddButtonShowCfg(cfbt_Gift, type, i))
					{
						num++;
						if (num >= nShowCount)
						{
							break;
						}
					}
				}
			}
		}
		if (num < nShowCount && giftTypeIndexData != -1 && giftTypeIndexData > -1)
		{
			for (int j = 0; j <= giftTypeIndexData; j++)
			{
				cfbt_Gift cfbt_Gift2 = giftCfgByType[j];
				Debug.Log(string.Format("giftCfgId:{0}", cfbt_Gift2.id));
				if (CheckButtonGiftAllConditions(cfbt_Gift2))
				{
					if (CanAddButtonShowCfg(cfbt_Gift2, type, j))
					{
						num++;
						if (num >= nShowCount)
						{
							break;
						}
					}
				}
			}
		}
	}

	private bool CheckButtonGiftAllConditions(cfbt_Gift cfg)
	{
		if (!JudgeIAPIsInitSuccessed(cfg) || !IsValidTime(cfg))
		{
			return false;
		}
		if (!JudgeConfigCondition(cfg.popConditions, false))
		{
			return false;
		}
		GiftData data = DB.GiftStalls.FindAndRefresh(cfg, false);
		return JudgeButtonGiftData(cfg, data);
	}

	private bool CanAddButtonShowCfg(cfbt_Gift cfg, GiftTriggerType type, int index)
	{
		int showButtonFreeIndex = GetShowButtonFreeIndex();
		Debug.Log(string.Format("GetShowButtonFreeIndex:{0}", showButtonFreeIndex));
		if (showButtonFreeIndex == -1)
		{
			return false;
		}
		GiftData giftData = DB.GiftStalls.FindAndRefresh(cfg, false);
		if (giftData == null)
		{
			DB.GiftStalls.AddGiftData(cfg);
			buttonShowGifts.Add(cfg);
			popGifts.Add(cfg);
		}
		else
		{
			buttonShowGifts.Add(cfg);
			if (giftData.nPop > 0)
			{
				popGifts.Add(cfg);
			}
		}
		DB.GiftStalls.SetShowGiftButtonDataByIndex(showButtonFreeIndex, cfg.id);
		DB.GiftStalls.SetShowGiftButtonTypeDataByIndex(showButtonFreeIndex, (int)type);
		DB.GiftStalls.SetGiftTypeIndexData(((int)type).ToString(), index);
		if (!cfg.isUnlockAll)
		{
			DB.GiftStalls.SetGiftUnlockData(cfg.id.ToString(), 0);
		}
		else
		{
			DB.GiftStalls.RemoveGiftBuyData(cfg.id);
		}
		return true;
	}

	private cfbt_Gift GetPopGiftImmediatelyCfg(GiftTriggerType type)
	{
		List<cfbt_Gift> giftCfgByType = GetGiftCfgByType(type);
		int giftTypeIndexData = DB.GiftStalls.GetGiftTypeIndexData(type);
		Debug.Log(string.Format("GetPopGiftImmediatelyCfg type:{0},cfgsCount:{1},typeIndex:{2}", type, giftCfgByType.Count, giftTypeIndexData));
		if (giftCfgByType.Count - 1 <= giftTypeIndexData)
		{
			giftTypeIndexData = -1;
		}
		Debug.Log(string.Format("typeIndex:{0}", giftTypeIndexData));
		for (int i = 0; i < giftCfgByType.Count; i++)
		{
			if (i > giftTypeIndexData)
			{
				cfbt_Gift cfbt_Gift = giftCfgByType[i];
				if (CheckImmediatelyCfg(cfbt_Gift))
				{
					DB.GiftStalls.SetGiftDataPop(cfbt_Gift);
					DB.GiftStalls.SetGiftTypeIndexData(((int)type).ToString(), i);
					return cfbt_Gift;
				}
			}
		}
		if (giftTypeIndexData > -1)
		{
			for (int j = 0; j <= giftTypeIndexData; j++)
			{
				cfbt_Gift cfbt_Gift2 = giftCfgByType[j];
				if (CheckImmediatelyCfg(cfbt_Gift2))
				{
					DB.GiftStalls.SetGiftDataPop(cfbt_Gift2);
					DB.GiftStalls.SetGiftTypeIndexData(((int)type).ToString(), j);
					return cfbt_Gift2;
				}
			}
		}
		return null;
	}

	private bool CheckImmediatelyCfg(cfbt_Gift cfg)
	{
		if (!JudgeIAPIsInitSuccessed(cfg) || !IsValidTime(cfg))
		{
			return false;
		}
		if (!JudgeConfigCondition(cfg.popConditions, false))
		{
			return false;
		}
		GiftData giftData = DB.GiftStalls.FindAndRefresh(cfg, false);
		if (giftData == null)
		{
			giftData = DB.GiftStalls.AddGiftData(cfg);
		}
		return JudgeImmediatelyGiftData(cfg, giftData);
	}

	public void ResetShowPopStatus(bool bShow = true)
	{
		isShowPopGift = bShow;
	}

	public void DealBuyGift(cfbt_Stores storeCfg)
	{
		foreach (cfbt_Gift cfbt_Gift in DataCfg.cfbt_Gifts)
		{
			int[] array = cfbt_Gift.storeIndex.ToInt(',');
			int num = Array.IndexOf(array, storeCfg.index);
			if (num != -1)
			{
				int length = array.Length;
				if (length == 1)
				{
					DB.GiftStalls.SetGiftDataBuy(cfbt_Gift);
					ResetShowGiftButtonData(cfbt_Gift.id);
					buttonShowGifts.Remove(cfbt_Gift);
				}
				else
				{
					if (!cfbt_Gift.isUnlockAll)
					{
						DB.GiftStalls.SetGiftUnlockData(cfbt_Gift.id.ToString(), num + 1);
						if (length <= num + 1)
						{
							DB.GiftStalls.SetGiftDataBuy(cfbt_Gift);
							ResetShowGiftButtonData(cfbt_Gift.id);
							buttonShowGifts.Remove(cfbt_Gift);
						}
					}
					else
					{
						DB.GiftStalls.AddGiftBuyData(cfbt_Gift.id, storeCfg.index);
						string text = cfbt_Gift.id.ToString();
						if (DB.GiftStalls.GiftBuyData[text].ToInt(',').Length == length)
						{
							DB.GiftStalls.SetGiftDataBuy(cfbt_Gift);
							ResetShowGiftButtonData(cfbt_Gift.id);
							buttonShowGifts.Remove(cfbt_Gift);
						}
					}
					CheckUpStallsDirectly(storeCfg);
				}
				break;
			}
		}
	}

	private void CheckUpStallsDirectly(cfbt_Stores storeCfg)
	{
		Logs.Common.Info(string.Format("CheckUpStallsDirectly usd:{0}", storeCfg.usd), Array.Empty<object>());
		int[,] array = cf_const.giftStallsUpDirectly.ToInt(2, '|', ',');
		int length = array.GetLength(0);
		for (int i = 0; i < length; i++)
		{
			if ((float)array[i, 0] < storeCfg.usd)
			{
				int stalls = DB.GiftStalls.Stalls;
				int num = array[i, 1];
				if (stalls < num)
				{
					DB.GiftStalls.Stalls = num;
					return;
				}
			}
		}
	}

	public string[] MoneySplit(string str)
	{
		Logs.Common.Info("MoneyStrSale str.Length:" + str.Length, Array.Empty<object>());
		int index = 0;
		while (index < str.Length)
		{
			char c = str[index];
			int num = Convert.ToInt32(c);
			if ((uint)(num - 58) > 4294967285U)
			{
				break;
			}
			index++;
		}
		string text = str.Substring(0, index);
		string text2 = str.Substring(index, str.Length - index);
		return new string[] { text, text2 };
	}

	private static string _LocalMoneyStrSale(string str, float sale)
	{
		System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(str, @"\d[\d.,\s\u00a0]*\d|\d");
		if (!m.Success)
		{
			return str;
		}
		string digits = System.Text.RegularExpressions.Regex.Replace(m.Value, @"[^\d]", "");
		double value;
		if (!double.TryParse(digits, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out value))
		{
			return str;
		}
		value /= 1.0 - sale * 0.01;
		string sep = m.Value.Contains(".") ? "." : (m.Value.Contains(",") ? "," : ".");
		string num = Math.Round(value).ToString("#,0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", sep);
		return str.Substring(0, m.Index) + num + str.Substring(m.Index + m.Length);
	}

	public string MoneyStrSale(string str, float sale)
	{
		string[] array = MoneySplit(str);
		string text = array[0];
		string text2 = array[1];
		Logs.Common.Info("MoneyStrSale symbol:" + text + " value:" + text2 + " sale:" + sale, Array.Empty<object>());
		float num;
		if (!float.TryParse(text2, out num))
		{
			// [rebuild] Giá nội tệ của Google Play (vd "160.000 ₫"): gốc float.Parse ném FormatException → caller
			// (StoreItem.Init…) dừng giữa chừng. Đọc phần số nguyên (bỏ dấu nhóm), giữ đuôi ký hiệu tiền.
			return _LocalMoneyStrSale(str, sale);
		}
		num /= 1f - sale * 0.01f;
		string text3 = text + string.Format("{0:F2}", num);
		Logs.Common.Info("MoneyStrSale rs:" + text3, Array.Empty<object>());
		return text3;
	}
}
