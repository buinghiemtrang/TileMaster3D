using System;
using UnityEngine;

namespace GameScene_Level
{
	public class PeriodicityDifficultyControl
	{
		public void ControlModelNumber(LevelData data)
		{
			if (!cf_const.IsOpenPeriodicityControlDifficulty || !JudgeSatisfyLevelCondition(data))
			{
				return;
			}
			int periodicityControlDifficultyModelChangeNumber = cf_const.PeriodicityControlDifficultyModelChangeNumber;
			if (periodicityControlDifficultyModelChangeNumber != 0)
			{
				if (periodicityControlDifficultyModelChangeNumber < 0)
				{
					data.RemoveFixedNumberModels(data.Models, periodicityControlDifficultyModelChangeNumber);
				}
				else
				{
					data.AddFixedNumberModelsByModelGroupConfig(data.Models, cf_const.PeriodicityControlDifficultyUseModels, periodicityControlDifficultyModelChangeNumber);
				}
			}
		}

		public void ControlTime(LevelData data)
		{
			if (cf_const.IsOpenPeriodicityControlDifficulty && JudgeSatisfyLevelCondition(data))
			{
				data.Time = Mathf.Max(cf_const.LvTimeMin, data.Time + cf_const.PeriodicityControlDifficultyTimeChangeNumber);
			}
		}

		private bool JudgeSatisfyLevelCondition(LevelData data)
		{
			if (data.Lv == 1 && data.Type == LevelType.Ordinary)
			{
				return false;
			}
			int remainder = data.Lv % 10;
			return Array.FindIndex(cf_const.PeriodicityControlDifficultyLevelCondition, (int f) => remainder == f) != -1;
		}
	}
}
