using System;

public class StoreMultipleCtrl
{
	public BuyPlace buyPlace = BuyPlace.BuyProp;

	public bool ActivityIsOpen()
	{
		if (!cf_const.storeMultipleSwitch)
		{
			return false;
		}
		if (!ServerDateTimeMgr.I.IsServerTimeSynced)
		{
			return false;
		}
		int[,] array = cf_const.storeMultiplePeriod.ToInt(6, '|', ',');
		if (array.GetLength(0) != 2)
		{
			return false;
		}
		DateTime t = new DateTime(array[0, 0], array[0, 1], array[0, 2], array[0, 3], array[0, 4], array[0, 5]);
		DateTime t2 = new DateTime(array[1, 0], array[1, 1], array[1, 2], array[1, 3], array[1, 4], array[1, 5]);
		DateTime netTime = ServerDateTimeMgr.I.NetTime;
		if (netTime < t)
		{
			return false;
		}
		if (netTime >= t2)
		{
			return false;
		}
		return true;
	}

	public bool IsCanMultiple(cfbt_Stores cfg)
	{
		if (cfg.sType != 1)
		{
			if (cfg.sType == 6)
			{
				return cfg.productType != 1;
			}
			return false;
		}
		return true;
	}

	public bool IsInStore()
	{
		return buyPlace == BuyPlace.Store;
	}

	public bool HasBuyCount(cfbt_Stores cfg)
	{
		if (!ActivityIsOpen())
		{
			return false;
		}
		if (cfg.sType != 1 && (cfg.sType != 6 || cfg.productType == 1))
		{
			return false;
		}
		if (DB.StoreMultipleActivity.StoreItemBuyCount.TryGetValue(cfg.index, out var value))
		{
			return value < cf_const.storeMultipleBuyTimes;
		}
		return true;
	}

	public string GetStartTime()
	{
		string[] array = cf_const.storeMultiplePeriod.Split('|');
		if (array.Length != 2)
		{
			return "";
		}
		return array[0];
	}

	public void JudgeIsNewActivity()
	{
		if (ActivityIsOpen())
		{
			string startTime = GetStartTime();
			if (DB.StoreMultipleActivity.LastActivityTime != startTime)
			{
				ResetData();
				DB.StoreMultipleActivity.LastActivityTime = startTime;
			}
		}
	}

	public void DealBuyCount(int id)
	{
		if (DB.StoreMultipleActivity.StoreItemBuyCount.TryGetValue(id, out var value))
		{
			DB.StoreMultipleActivity.SetStoreItemBuyCount(id, value + 1);
		}
		else
		{
			DB.StoreMultipleActivity.SetStoreItemBuyCount(id, 1);
		}
	}

	public void ResetData()
	{
		DB.StoreMultipleActivity.StoreItemBuyCount.Clear();
		DB.StoreMultipleActivity.IsActivityPushed = false;
	}
}
