using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameScene_Level
{
	public class RandomLevelControl
	{
		public static bool JudgeWaveDivision(int lv)
		{
			// UNVERIFIED: điều kiện null kiểm RapidlyLevelCondition (0x298) nhưng mảng tìm là RegenerateLevelCondition (0x310), theo đúng offset trong pseudo-C
			if (cf_const.RapidlyLevelCondition == null)
			{
				return false;
			}
			int remainder = lv % 10;
			return Array.FindIndex(cf_const.RegenerateLevelCondition, (int f) => remainder == f) != -1;
		}

		public static OrdinaryLevelType RandomOrdinaryLevelType(int lv)
		{
			if (cf_const.RapidlyLevelCondition == null)
			{
				return OrdinaryLevelType.Normal;
			}
			int remainder = lv % 10;
			if (Array.FindIndex(cf_const.RapidlyLevelCondition, (int f) => remainder == f) != -1)
			{
				return OrdinaryLevelType.Rapidly;
			}
			return OrdinaryLevelType.Normal;
		}

		public List<int> AddRandomLevelModels(List<int> models, LevelData data)
		{
			List<int> list = new List<int>();
			if (data.Type != LevelType.Ordinary)
			{
				return list;
			}
			List<int> list2 = new List<int>();
			cfgLevelDifficulty config = data.Difficulty.DynamicDifficulty.GetConfig();
			List<cfgLevelRandom> randomLevelConfigs = GetRandomLevelConfigs(config.RandomLevelGroupId, data.OrdLevelType);
			for (int i = 0; i < randomLevelConfigs.Count; i++)
			{
				list2.Clear();
				cfgLevelRandom cfgLevelRandom = randomLevelConfigs[i];
				List<int> list3 = new List<int>(cfgLevelRandom.GroupIds);
				for (int j = 0; j < cfgLevelRandom.RandomGroupNumber; j++)
				{
					int index = Random.Range(0, list3.Count);
					List<int> modelsByModelGroupConfig = data.GetModelsByModelGroupConfig(list3[index]);
					if (modelsByModelGroupConfig.Count == 0)
					{
						Debug.LogError(string.Format("没有获取到模型Id，config:{0}, groupId:{1} ", cfgLevelRandom.Id, list3[index]));
					}
					list2.AddRange(modelsByModelGroupConfig);
					list3.RemoveAt(index);
				}
				if (list2.Count == 0)
				{
					Debug.LogError(string.Format("没有模型Id，config:{0} ,配置有问题", cfgLevelRandom.Id));
					continue;
				}
				list2.SortR();
				int num = Random.Range(cfgLevelRandom.Min, cfgLevelRandom.Max);
				float num2 = cfgLevelRandom.ModelComplexity;
				if (num2 <= 0f)
				{
					num2 = 1f;
				}
				int num3 = (int)(num2 * (float)num);
				int num4 = list2.Count - num3;
				if (num4 != 0 && num3 <= list2.Count)
				{
					list.AddRange(list2.GetRange(num3, num4));
					list2 = list2.GetRange(0, num3);
				}
				// UNVERIFIED: bản gốc vòng lặp mãi nếu list2 rỗng (num3 == 0), ở đây dừng khi rỗng
				int num5 = num;
				while (num5 != 0 && list2.Count > 0)
				{
					int num6 = 0;
					bool flag;
					do
					{
						models.Add(list2[num6]);
						flag = num5 - 1 != num6;
						num6++;
					}
					while (flag && num6 < list2.Count);
					num5 -= num6;
				}
			}
			return list;
		}

		public List<int> GetModelsByRandomLevelConfig(LevelData data)
		{
			List<int> list = new List<int>();
			cfgLevelDifficulty config = data.Difficulty.DynamicDifficulty.GetConfig();
			List<cfgLevelRandom> randomLevelConfigs = GetRandomLevelConfigs(config.RandomLevelGroupId, data.OrdLevelType);
			for (int i = 0; i < randomLevelConfigs.Count; i++)
			{
				cfgLevelRandom cfgLevelRandom = randomLevelConfigs[i];
				List<int> list2 = new List<int>(cfgLevelRandom.GroupIds);
				for (int j = 0; j < cfgLevelRandom.RandomGroupNumber; j++)
				{
					int index = Random.Range(0, list2.Count);
					list.AddRange(data.GetModelsByModelGroupConfig(list2[index]));
					list2.RemoveAt(index);
				}
			}
			return list;
		}

		private List<cfgLevelRandom> GetRandomLevelConfigs(int groupId, OrdinaryLevelType type)
		{
			List<cfgLevelRandom> list = new List<cfgLevelRandom>();
			for (int i = 0; i < DataCfg.cfgLevelRandoms.Count; i++)
			{
				cfgLevelRandom cfgLevelRandom = DataCfg.cfgLevelRandoms[i];
				if (cfgLevelRandom.GroupId == groupId && cfgLevelRandom.Type == (int)type)
				{
					list.Add(cfgLevelRandom);
				}
			}
			return list;
		}

		public static int RandomLevelHardLevel(int lv)
		{
			if (!string.IsNullOrWhiteSpace(cf_const.HardLevelIds) && cf_const.HardLevelIds.ToInt('|').Contains(lv))
			{
				return 1;
			}
			if (!string.IsNullOrWhiteSpace(cf_const.VeryHardLevelIds) && cf_const.VeryHardLevelIds.ToInt('|').Contains(lv))
			{
				return 2;
			}
			if (!string.IsNullOrWhiteSpace(cf_const.HardLevelNumberCondition) && cf_const.HardLevelNumberCondition.ToInt('|').Contains(lv % 10))
			{
				return 1;
			}
			if (!string.IsNullOrWhiteSpace(cf_const.VeryHardLevelNumberCondition) && cf_const.VeryHardLevelNumberCondition.ToInt('|').Contains(lv % 10))
			{
				return 2;
			}
			return 0;
		}
	}
}
