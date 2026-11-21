using System.Collections.Generic;
using System.Linq;

public class UnlockMgr
{
	private static UnlockMgr _instance;

	private Dictionary<int, List<cfgUnlockNewItem>> cfgUnlockNewItemDict;

	public List<int> curGameUnlockModelIDs;

	public static UnlockMgr Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new UnlockMgr();
				_instance.Init();
			}
			return _instance;
		}
	}

	private UnlockMgr()
	{
	}

	private void Init()
	{
		curGameUnlockModelIDs = new List<int>();
		cfgUnlockNewItemDict = new Dictionary<int, List<cfgUnlockNewItem>>();
		foreach (cfgUnlockNewItem cfgUnlockNewItem in DataCfg.cfgUnlockNewItems)
		{
			List<cfgUnlockNewItem> list;
			if (cfgUnlockNewItemDict.ContainsKey(cfgUnlockNewItem.level))
			{
				list = cfgUnlockNewItemDict[cfgUnlockNewItem.level];
			}
			else
			{
				list = new List<cfgUnlockNewItem>();
				cfgUnlockNewItemDict[cfgUnlockNewItem.level] = list;
			}
			list.Add(cfgUnlockNewItem);
		}
	}

	public List<cfgUnlockNewItem> GetLevelUnLockData(int level)
	{
		List<cfgUnlockNewItem> result = new List<cfgUnlockNewItem>();
		if (cfgUnlockNewItemDict.ContainsKey(level))
		{
			result = cfgUnlockNewItemDict[level];
		}
		return result;
	}

	public bool HaveUnlockItem(int level)
	{
		return curGameUnlockModelIDs != null && curGameUnlockModelIDs.Count > 0;
	}

	public bool SubFunctionIsUnlock(SUB_FUNCTION_ID subID)
	{
		foreach (KeyValuePair<int, List<cfgUnlockNewItem>> item in cfgUnlockNewItemDict)
		{
			if (item.Value.Where((cfgUnlockNewItem cfg) => cfg != null && cfg.unlockid == (int)subID && cfg.level <= DB.User.Curlv).ToList().Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	public void ClearUnlockModeIDs()
	{
		curGameUnlockModelIDs.Clear();
	}
}
