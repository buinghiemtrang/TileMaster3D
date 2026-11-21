using UnityEngine;

namespace GameScene_Level
{
	public class DynamicDifficultyControl
	{
		internal void Victory(int lv, bool isNewLv, int residueTime, int totalTime)
		{
			if (lv < cf_const.OpenDynamicDifficultyLevel)
			{
				return;
			}
			if (!isNewLv)
			{
				System.Collections.Generic.Dictionary<int, int> pointsChangeValue = DB.User.DynamicDifficultyPointsChangeValue;
				if (!pointsChangeValue.ContainsKey(lv))
				{
					return;
				}
				int num = pointsChangeValue[lv];
				if (num >= 0)
				{
					return;
				}
				cfgLevelDifficulty configByPoints = GetConfigByPoints(GetCurrentPoints());
				num = pointsChangeValue[lv];
				int currentPoints = GetCurrentPoints();
				bool isAdd = true;
				if (currentPoints + num < configByPoints.PointsRange[0])
				{
					int dynamicDifficultyDownshiftNeedLevelNumber = cf_const.DynamicDifficultyDownshiftNeedLevelNumber;
					int num2 = 0;
					int num3 = 0;
					int key = lv;
					while (num2 < dynamicDifficultyDownshiftNeedLevelNumber && pointsChangeValue.TryGetValue(key, out var value))
					{
						num2++;
						key--;
						if (value < 0)
						{
							num3++;
						}
					}
					int num4 = Mathf.Clamp(Mathf.RoundToInt((float)num3 / (float)num2 * 100f), 0, 100);
					if (num4 < Mathf.RoundToInt(cf_const.DynamicDifficultyDownshiftThreshold * 100f))
					{
						num = configByPoints.PointsRange[0];
						isAdd = false;
					}
				}
				ModifyPoints(num, isAdd);
				return;
			}
			int num5 = Mathf.Clamp(Mathf.RoundToInt((float)residueTime / (float)totalTime * 100f), 0, 100);
			float[,] thresholds = cf_const.DynamicDifficultyIncreasedPointsByVictoryRemainingTimeThreshold;
			int length = thresholds.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				if (Mathf.RoundToInt(thresholds[i, 0] * 100f) <= num5)
				{
					int num6 = (int)thresholds[i, 1];
					ModifyPoints(num6, isAdd: true);
					DB.User.SetDynamicDifficultyPointsChangeValue(lv, num6);
					break;
				}
			}
		}

		internal void Fail(int lv, bool isTimeOut, int residualModelCount, int totalModelCount)
		{
			if (lv < cf_const.OpenDynamicDifficultyLevel)
			{
				return;
			}
			int num = Mathf.Clamp(Mathf.RoundToInt((float)residualModelCount / (float)totalModelCount * 100f), 0, 100);
			float[,] array = (isTimeOut ? cf_const.DynamicDifficultyReducedPointsByTimeoutFailResidualModelThreshold : cf_const.DynamicDifficultyReducedPointsByFullFailResidualModelThreshold);
			int length = array.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				if (num <= Mathf.RoundToInt(array[i, 0] * 100f))
				{
					int num2 = (int)array[i, 1];
					if (DB.User.DynamicDifficultyPointsChangeValue.TryGetValue(lv, out var value) && value <= -num2)
					{
						break;
					}
					DB.User.SetDynamicDifficultyPointsChangeValue(lv, -num2);
					break;
				}
			}
		}

		public cfgLevelDifficulty GetConfig()
		{
			return GetConfigByPoints(GetCurrentPoints());
		}

		public int GetCurrentPoints()
		{
			if (DB.User.DynamicDifficultyPoints < 0)
			{
				ModifyPoints(cf_const.DynamicDifficultyDefaultPoints, isAdd: false);
			}
			if (DB.User.DynamicDifficultyPoints < cf_const.DynamicDifficultyMinPoints || cf_const.DynamicDifficultyMaxPoints < DB.User.DynamicDifficultyPoints)
			{
				ModifyPoints(0, isAdd: true);
			}
			return DB.User.DynamicDifficultyPoints;
		}

		private void ModifyPoints(int points, bool isAdd)
		{
			if (isAdd)
			{
				points = Mathf.Clamp(DB.User.DynamicDifficultyPoints + points, cf_const.DynamicDifficultyMinPoints, cf_const.DynamicDifficultyMaxPoints);
			}
			DB.User.DynamicDifficultyPoints = points;
			cfgLevelDifficulty configByPoints = GetConfigByPoints(DB.User.DynamicDifficultyPoints);
			if (DB.User.DynamicDifficultyLevel != configByPoints.Id)
			{
				DB.User.DynamicDifficultyLevel = configByPoints.Id;
			}
		}

		private cfgLevelDifficulty GetConfigByPoints(int points)
		{
			foreach (cfgLevelDifficulty cfgLevelDifficulty in DataCfg.cfgLevelDifficultys)
			{
				if (cfgLevelDifficulty.PointsRange.Length == 2 && cfgLevelDifficulty.PointsRange[0] <= points && points <= cfgLevelDifficulty.PointsRange[1])
				{
					return cfgLevelDifficulty;
				}
			}
			Debug.LogError(string.Format("no find dynamic difficulty config. Points:{0}", points));
			return new cfgLevelDifficulty
			{
				Id = 0,
				PointsRange = new int[2] { 0, 20 },
				FixedLevelModelTypeProportion = 1f,
				RandomLevelGroupId = 1,
				// UNVERIFIED: hai hằng NormalTimeK/RapidlyTimeK suy từ fmov 0x41200000 (10f), decompiler làm mất
				NormalTimeK = 10f,
				RapidlyTimeK = 10f
			};
		}
	}
}
