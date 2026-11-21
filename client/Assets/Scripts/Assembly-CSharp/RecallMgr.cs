using System;
using UnityEngine;

public class RecallMgr
{
	public static RecallMgr I;

	static RecallMgr()
	{
		I = new RecallMgr();
	}

	public void Init()
	{
		int num = _OfflineDays();
		if (DataCfg.cfgRecallRewards != null)
		{
			for (int i = DataCfg.cfgRecallRewards.Count - 1; i >= 0; i--)
			{
				cfgRecallReward cfgRecallReward = DataCfg.cfgRecallRewards[i];
				if (num >= cfgRecallReward.condition)
				{
					if (cfgRecallReward.gear > DB.Recall.SatisfiedGear)
					{
						DB.Recall.SatisfiedGear = cfgRecallReward.gear;
					}
					break;
				}
			}
			DB.Recall.OfflineStartTime = DateTime.Today.AddDays(1.0).ToString();
		}
	}

	public bool JudgeAutoEject()
	{
		return DB.Recall.SatisfiedGear > 0;
	}

	public void Receive()
	{
		DB.Recall.SatisfiedGear = 0;
	}

	public cfgRecallReward GetRewards()
	{
		if (DataCfg.cfgRecallRewards == null)
		{
			return null;
		}
		for (int i = 0; i < DataCfg.cfgRecallRewards.Count; i++)
		{
			if (DB.Recall.SatisfiedGear == DataCfg.cfgRecallRewards[i].gear)
			{
				return DataCfg.cfgRecallRewards[i];
			}
		}
		return null;
	}

	private int _OfflineDays()
	{
		if (string.IsNullOrEmpty(DB.Recall.OfflineStartTime))
		{
			return 0;
		}
		DateTime d = MDateTime.Parse(DB.Recall.OfflineStartTime, DateTime.Today);
		return Mathf.Max(0, (DateTime.Today - d).Days);
	}
}
