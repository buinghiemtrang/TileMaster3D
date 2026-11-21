using UnityEngine;
using UnityEngine.UI;

namespace ShortBattlePass
{
	public class ShortBattlePassEntranceIcon : MonoBehaviour
	{
		[SerializeField]
		private Image bar;

		[SerializeField]
		private Text valueTex;

		private void Start()
		{
			if (!Singleton<ShortBattlePassMgr>.I.JudgeHasState(ActivityState.Started))
			{
				return;
			}
			int grade = DB.ShortBattlePass.Data.Grade;
			int residualExp = DB.ShortBattlePass.Data.ResidualExp;
			cfgShortBattlePassReward rewardConfigByGrade = Singleton<ShortBattlePassMgr>.I.GetRewardConfigByGrade(grade);
			if (rewardConfigByGrade != null)
			{
				valueTex.text = ((rewardConfigByGrade.Exp > 0) ? string.Format("{0}/{1}", residualExp, rewardConfigByGrade.Exp) : "Full");
				bar.fillAmount = ((rewardConfigByGrade.Exp == 0) ? 1f : ((float)residualExp / (float)rewardConfigByGrade.Exp));
			}
		}
	}
}
