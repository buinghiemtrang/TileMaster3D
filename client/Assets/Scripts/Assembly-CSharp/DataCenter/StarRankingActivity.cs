using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class StarRankingActivity : IStarRankingActivity
	{
		private int[,] _grading = new int[5, 2]
		{
			{ 1, 5 },
			{ 6, 9 },
			{ 10, 12 },
			{ 13, 14 },
			{ 15, 15 }
		};

		public void GetDanGrading(out int dan, out int grading)
		{
			LevelToDanGrading(level + 1, out dan, out grading);
		}

		public void LevelToDanGrading(int lv, out int dan, out int grading)
		{
			int length = _grading.GetLength(0);
			int num = _grading[0, 0];
			if (_grading[0, 0] <= lv)
			{
				num = _grading[length - 1, 1];
				if (lv <= num)
				{
					num = lv;
				}
			}
			dan = 1;
			grading = 1;
			for (int i = 0; i < length; i++)
			{
				if (_grading[i, 0] <= num && num <= _grading[i, 1])
				{
					dan = i + 1;
					grading = num - _grading[i, 0] + 1;
					return;
				}
			}
		}

		public void ModifyLevel(int value)
		{
			int num = level + value;
			int num2 = _grading[_grading.GetLength(0) - 1, 1] - 1;
			if (num >= num2)
			{
				num = num2;
			}
			if (num < 0)
			{
				num = 0;
			}
			Level = num;
		}

		public int DanGradingConditionBy(int ranking)
		{
			if (ranking <= cf_const.activityRankingStarUpDownCondition[0])
			{
				return 1;
			}
			if (ranking >= cf_const.activityRankingStarUpDownCondition[1])
			{
				return -1;
			}
			return 0;
		}

		public string GetDanGradingPath(int dan)
		{
			return string.Format("DanGrading/Level-{0}", dan);
		}
	}
}
