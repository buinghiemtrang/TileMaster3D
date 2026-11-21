using System.Collections.Generic;
using GameScene_Record;
using UnityEngine;

namespace GameScene_Level
{
	public static class LevelMgr
	{
		public static LevelData Data;

		private static Dictionary<int, int> _allUnlockModelsAndLevels;

		public static Dictionary<int, int> AllUnlockModelsAndLevels
		{
			get
			{
				if (_allUnlockModelsAndLevels == null)
				{
					_allUnlockModelsAndLevels = new Dictionary<int, int>();
					foreach (cfgLevelUnlock cfgLevelUnlock in DataCfg.cfgLevelUnlocks)
					{
						foreach (int unlockId in cfgLevelUnlock.unlockIds)
						{
							_allUnlockModelsAndLevels.Add(unlockId, cfgLevelUnlock.id);
						}
					}
				}
				return _allUnlockModelsAndLevels;
			}
		}

		public static void GetLevelStatus(out bool isRapidly, out bool isRegenerate)
		{
			int num = DB.User.Maxlv + 1;
			DataCfg.cfgLevelFixeds.TryGetValue(num, out var value);
			if (GSingleton<RegionRankActivityManager>.ins.TryGetLevelConfig(out var config, out var _))
			{
				value = config;
			}
			if (value == null)
			{
				isRapidly = RandomLevelControl.RandomOrdinaryLevelType(num) == OrdinaryLevelType.Rapidly;
				isRegenerate = RandomLevelControl.JudgeWaveDivision(num);
			}
			else
			{
				isRapidly = value.IsRapidlyLevel;
				isRegenerate = value.IsWaveDivision;
			}
		}

		public static int GetLevelHardLevel()
		{
			if (GSingleton<RegionRankActivityManager>.ins.TryGetLevelConfig(out var _, out var _))
			{
				return 0;
			}
			int maxlv = DB.User.Maxlv;
			DataCfg.cfgLevelFixeds.TryGetValue(maxlv + 1, out var value);
			if (value != null)
			{
				return value.HardLevel;
			}
			return RandomLevelControl.RandomLevelHardLevel(maxlv + 1);
		}

		public static void SetLevelData(LevelType levelType = LevelType.Ordinary, int groupId = -1, int lv = -1)
		{
			if (levelType == LevelType.BakingMaster)
			{
				Data = new BakingMasterLevel();
				List<cfgBakingMasterLevel> list = DataCfg.cfgBakingMasterLevels[groupId];
				int num = ((lv >= 0) ? Mathf.Min(lv, list.Count - 1) : 0);
				DB.User.Curlv = num;
				cfgBakingMasterLevel cfgBakingMasterLevel = list[num];
				Data.Init(num + 1, new cfgLevelFixed
				{
					IsRapidlyLevel = false,
					IsWaveDivision = false,
					RandomModels = cfgBakingMasterLevel.RandomModels,
					Models = cfgBakingMasterLevel.Models,
					Time = cfgBakingMasterLevel.Time
				});
			}
			else
			{
				Data = new OrdinaryLevel();
				Singleton<RecordMgr>.I.LoadStart();
				if (lv == -1)
				{
					lv = DB.User.Maxlv;
				}
				DB.User.Curlv = lv;
				if (!Singleton<RecordMgr>.I.IsLoading)
				{
					cfgLevelFixed config = (DataCfg.cfgLevelFixeds.ContainsKey(lv + 1) ? DataCfg.cfgLevelFixeds[lv + 1] : null);
					Data.Init(lv + 1, config);
				}
				else
				{
					Data.RestoreRecords();
				}
				Singleton<RecordMgr>.I.SaveFixed();
			}
			GameTimeTriggerMono.I.Create("Level");
		}
	}
}
