using System.Collections.Generic;
using UnityEngine;

namespace GameScene_Level
{
	public abstract class LevelData
	{
		public int Lv;

		public int Time;

		public LevelType Type;

		public OrdinaryLevelType OrdLevelType;

		public readonly List<int> Models = new List<int>();

		public readonly List<int> Wave = new List<int>();

		public readonly List<cfgModel> ModelConfigs = new List<cfgModel>();

		public readonly List<int> RemainingModels = new List<int>();

		public readonly LevelDifficulty Difficulty = new LevelDifficulty();

		public bool IsRegionRankLevel;

		public int RegionRankScore;

		public bool EnableDynamicDifficulty = true;

		public abstract void RestoreRecords();

		public abstract void Init(int lv, cfgLevelFixed config);

		public virtual void Restart()
		{
		}

		public void AddModelsByConfig(List<int> models, IEnumerable<int> data)
		{
			AddUnlockedModels(models, data);
		}

		public List<int> AddModelsByModelGroupConfig(List<int> models, int[,] data)
		{
			List<int> list = new List<int>();
			if (data != null)
			{
				int length = data.GetLength(0);
				for (int i = 0; i < length; i++)
				{
					list.AddRange(AddFixedNumberModelsByModelGroupConfig(models, data[i, 0], data[i, 1]));
				}
			}
			return list;
		}

		public void AddModelsEliminateSpecialModels(List<int> models, List<int> data)
		{
			for (int i = 0; i < data.Count; i++)
			{
				int item = data[i];
				if (DataCfg.cfgModels.TryGetValue(item, out var value) && value.type == 1)
				{
					models.Add(item);
				}
			}
		}

		public List<int> AddFixedNumberModelsByModelGroupConfig(List<int> models, int groupId, int number)
		{
			List<int> list = new List<int>();
			cfgSynthetic modelGroupConfig = GetModelGroupConfig(groupId);
			if (modelGroupConfig != null)
			{
				AddUnlockedModels(list, modelGroupConfig.table);
				AddFixedNumberModels(models, number, list, isOutputLogError: false);
			}
			return list;
		}

		public List<int> GetModelsByModelGroupConfig(int groupId)
		{
			List<int> list = new List<int>();
			cfgSynthetic modelGroupConfig = GetModelGroupConfig(groupId);
			if (modelGroupConfig != null)
			{
				AddUnlockedModels(list, modelGroupConfig.table);
			}
			return list;
		}

		private cfgSynthetic GetModelGroupConfig(int groupId)
		{
			if (DataCfg.cfgSynthetics.ContainsKey(groupId))
			{
				return DataCfg.cfgSynthetics[groupId];
			}
			return null;
		}

		public void AddUnlockedModels(List<int> models, IEnumerable<int> data)
		{
			if (data == null)
			{
				return;
			}
			foreach (int datum in data)
			{
				if (!LevelMgr.AllUnlockModelsAndLevels.TryGetValue(datum, out var value))
				{
					models.Add(datum);
				}
				else if (value < Lv)
				{
					models.Add(datum);
				}
			}
		}

		public int AddFixedNumberModels(List<int> models, int number, List<int> data, bool isOutputLogError = true)
		{
			if (data != null && number > 0)
			{
				do
				{
					int item;
					do
					{
						if (data.Count < 1)
						{
							return number;
						}
						int index = Random.Range(0, data.Count);
						item = data[index];
						data.RemoveAt(index);
					}
					while (models.Contains(item));
					models.Add(item);
					number--;
				}
				while (number > 0);
			}
			return number;
		}

		public void RemoveModelsByProportion(List<int> models, float proportion)
		{
			if (models == null || models.Count == 0)
			{
				return;
			}
			int count = models.Count;
			float num = count;
			float num2 = (int)(num * proportion);
			if (num <= num2)
			{
				if (num < num2 && 0f < num2 - num)
				{
					int num3 = 1;
					int num4 = 0;
					float num5;
					do
					{
						models.Add(models[num4]);
						num5 = num3;
						num3++;
						num4 = ((num4 + 1 < count) ? (num4 + 1) : 0);
					}
					while (num5 < num2 - num);
				}
			}
			else
			{
				for (int num6 = count - 1; (float)num6 >= num2; num6--)
				{
					models.RemoveAt(num6);
				}
			}
		}

		public void RemoveFixedNumberModels(List<int> models, int number, int keepNumber = 0)
		{
			number = Mathf.Abs(number);
			List<int> list = new List<int>();
			for (int num = models.Count - 1; num >= keepNumber; num--)
			{
				int item = models[num];
				if (DataCfg.cfgModels.TryGetValue(item, out var value) && value.type != 1)
				{
					list.Add(item);
					models.RemoveAt(num);
				}
			}
			int count = models.Count;
			if (count - keepNumber > 0)
			{
				int count2 = list.Count;
				do
				{
					if (number < 1 || count + count2 < 11)
					{
						break;
					}
					int index = Random.Range(keepNumber, count);
					models.RemoveAt(index);
					count = models.Count;
					number--;
				}
				while (count - keepNumber > 0);
			}
			models.AddRange(list);
		}

		public (int, int) AddRepeatModels(List<int> models, int number, bool isOutputLogError = true)
		{
			if (models == null || number < 1)
			{
				return (number, 0);
			}
			List<int> list = new List<int>();
			for (int i = 0; i < models.Count; i++)
			{
				list.Add(i);
			}
			List<int> list2 = new List<int>();
			int num = 0;
			int num2 = number;
			while (num != number && list.Count > 0)
			{
				bool flag = false;
				while (true)
				{
					int index = Random.Range(0, list.Count);
					int item = models[list[index]];
					list.RemoveAt(index);
					if (DataCfg.cfgModels.TryGetValue(item, out var value) && value.type == 1)
					{
						list2.Add(item);
						num++;
						num2--;
						flag = true;
						break;
					}
					if (list.Count < 1)
					{
						break;
					}
				}
				if (!flag)
				{
					break;
				}
			}
			for (int j = 0; j < list2.Count; j++)
			{
				models.Add(list2[j]);
			}
			return (num2, num);
		}

		public void AddModelConfigs(List<cfgModel> modelConfigs, List<int> models)
		{
			for (int i = 0; i < models.Count; i++)
			{
				if (DataCfg.cfgModels.TryGetValue(models[i], out var value))
				{
					modelConfigs.Add(value);
				}
			}
		}
	}
}
