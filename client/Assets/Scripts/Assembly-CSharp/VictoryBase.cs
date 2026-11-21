using UnityEngine;

public class VictoryBase : MonoBehaviour
{
	protected void ModifySumStar(int count)
	{
		DB.User.SumStar += count;
		MedalMgr.I.CheckUpMedal();
		DB.Chest.AddStar(count);
		DB.Bag.Modify(PropID.Star, count, b_update: true, "");
		DB.User.AddMonthIssueSumStar(count);
		Singleton<StarRankingActivityMgr>.I.SetStarNum(count);
	}
}
