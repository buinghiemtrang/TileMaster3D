using System.Collections.Generic;

public class MedalMgr
{
	private static MedalMgr _ins = new MedalMgr();

	private bool bShowTips;

	public static MedalMgr I => _ins;

	public int CurSelectId { get; set; }

	public bool IsShowFirstStepGuide { get; set; }

	public MedalMgr()
	{
		CurSelectId = DB.User.Medal;
	}

	public int GetMaxMedalLv(MedalType type = MedalType.Normall)
	{
		List<cfg_medal> cfgsByType = GetCfgsByType(type);
		return cfgsByType[cfgsByType.Count - 1].Lv;
	}

	public void CheckUpMedal()
	{
		cfg_medal medalCfgByStar = GetMedalCfgByStar(DB.User.SumStar);
		if (medalCfgByStar.Lv == DB.User.LvMedal)
		{
			return;
		}
		DB.User.LvMedal = medalCfgByStar.Lv;
		if (!DB.User.IsChangedMedal)
		{
			DB.User.Medal = medalCfgByStar.ID;
			DispatcherMgr.Emit("Update_User_Medal");
		}
		else
		{
			DB.Bag.NewGetMedals.Add(medalCfgByStar.ID);
			DispatcherMgr.Emit("Update_User_MedalRed");
		}
		ShowReachTips(medalCfgByStar);
	}

	public cfg_medal GetMedalCfgByStar(int starNum)
	{
		List<cfg_medal> cfgsByType = GetCfgsByType(MedalType.Normall);
		for (int num = cfgsByType.Count - 1; num >= 0; num--)
		{
			if (cfgsByType[num].Count <= starNum)
			{
				return cfgsByType[num];
			}
		}
		return cfgsByType[0];
	}

	private void ShowReachTips(cfg_medal cfg)
	{
		if (!bShowTips)
		{
			bShowTips = true;
			MedalFinishedTips medalFinishedTips = SceneMgr.AddSuperNode<MedalFinishedTips>("Prefabs/Medal/MedalFinishedTips");
			medalFinishedTips.InitInfo(cfg);
			medalFinishedTips.completeCallback = delegate
			{
				bShowTips = false;
			};
		}
	}

	public bool JudgeHasNewGetMedals(List<MedalType> types)
	{
		for (int i = 0; i < types.Count; i++)
		{
			List<cfg_medal> cfgsByType = GetCfgsByType(types[i]);
			for (int j = 0; j < cfgsByType.Count; j++)
			{
				if (DB.Bag.NewGetMedals.Contains(cfgsByType[j].ID))
				{
					return true;
				}
			}
		}
		return false;
	}

	public List<cfg_medal> GetCfgsByType(MedalType type)
	{
		List<cfg_medal> list = new List<cfg_medal>();
		foreach (cfg_medal cfg_medal in DataCfg.cfg_medals)
		{
			if (cfg_medal.Type == (int)type)
			{
				list.Add(cfg_medal);
			}
		}
		return list;
	}

	public void SortSpecialMedals(List<cfg_medal> cfgs)
	{
		DataCenter.DBList<int> unlockMedals = DB.Bag.Medal;
		cfgs.Sort(delegate(cfg_medal x, cfg_medal y)
		{
			bool flag = unlockMedals.Contains(x.ID);
			bool flag2 = unlockMedals.Contains(y.ID);
			int num = x.ID.CompareTo(y.ID);
			if (flag)
			{
				if (!flag2)
				{
					return -1;
				}
				return num;
			}
			if (flag2)
			{
				return 1;
			}
			return num;
		});
	}

	public cfg_medal GetCfgById(int id)
	{
		return DataCfg.cfg_medals.Find((cfg_medal cfg) => cfg.ID == id);
	}
}
