using UnityEngine;

namespace GameScene_Level
{
	public class WaveControl
	{
		public void WaveDivision(LevelData data, bool isHas)
		{
			if (isHas)
			{
				int count = data.Models.Count;
				int num = (int)(cf_const.WavePercentage * (float)count);
				int num2 = Mathf.Min(num, count);
				if (num < count && num2 > 0)
				{
					data.Wave.Add(num2);
				}
			}
		}
	}
}
