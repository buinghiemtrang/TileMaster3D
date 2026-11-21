using System;
using UnityEngine;
using UnityEngine.UI;

namespace ShortBattlePass
{
	public class ShortBattlePassOrdinaryItem : ShortBattlePassItem
	{
		[SerializeField]
		private ShortBattlePassOrdinaryItemRewardUI[] rewardUI;

		[SerializeField]
		private Text yellowGradeTex;

		[SerializeField]
		private GameObject yellowGradeObj;

		[SerializeField]
		private Text blueGradeTex;

		[SerializeField]
		private GameObject blueGradeObj;

		[SerializeField]
		private GameObject lineObj;

		public override void Init(int ordinaryRewardsNum, RewardInfo info)
		{
			base.Init(ordinaryRewardsNum, info);
			int grade = info.Config.Grade;
			yellowGradeTex.text = grade.ToString();
			blueGradeTex.text = grade.ToString();
			int grade2 = DB.ShortBattlePass.Data.Grade;
			yellowGradeObj.SetActive(grade <= grade2);
			blueGradeObj.SetActive(grade2 < grade);
			bool active = false;
			if (grade2 < ordinaryRewardsNum)
			{
				active = grade2 == grade || grade2 + 1 == grade;
			}
			lineObj.SetActive(active);
			Array.ForEach(rewardUI, delegate(ShortBattlePassOrdinaryItemRewardUI fe)
			{
				fe.Init(info);
			});
		}
	}
}
