using UnityEngine;

namespace GameScene_Level
{
	public class PayingUserDifficultyControl
	{
		public void ControlModelNumber(LevelData data)
		{
			if (!Global.pStores.PayingUsers || data.Lv - DB.User.FirstConsumeLv - 1 <= cf_const.PayingUsersAfterLevelIncrease)
			{
				return;
			}
			int num = (Global.pStores.AdFree ? Random.Range(cf_const.BuyNoAdsAddModelsMinNum, cf_const.BuyNoAdsAddModelsMaxNum) : 0);
			float num2 = DB.User.TotalConsume;
			int[,] payingUsersIncreaseValue = cf_const.PayingUsersIncreaseValue;
			for (int num3 = payingUsersIncreaseValue.GetLength(0) - 1; num3 >= 0; num3--)
			{
				if (!(num2 < (float)payingUsersIncreaseValue[num3, 0]))
				{
					num += Random.Range(payingUsersIncreaseValue[num3, 1], payingUsersIncreaseValue[num3, 2]);
					break;
				}
			}
			if (num != 0)
			{
				if (num < 1)
				{
					data.RemoveFixedNumberModels(data.Models, num);
					return;
				}
				int num4 = data.AddFixedNumberModels(data.Models, num, data.RemainingModels, isOutputLogError: false);
				if (num4 >= 1)
				{
					data.AddRepeatModels(data.Models, num4);
				}
			}
		}
	}
}
