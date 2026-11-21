using System;
using UnityEngine;
using UnityEngine.UI;

namespace RoyalPass
{
	public class RoyalPassActivityBuySuccessRewardsDialog : DialogBase
	{
		[SerializeField]
		private Text energyUpLimitText;

		[SerializeField]
		private Image medalImg;

		[SerializeField]
		private Image rewardBoxImg;

		public Action receiveCallback;

		private int[,] rewards;

		private bool isPause;

		public void Show(int[,] rewards)
		{
			this.rewards = rewards;
			DispatcherMgr.Emit("GamePause", true, "Reward");
			Singleton<RoyalPassActivityMgr>.I.SetPaidRewardUI(medalImg, energyUpLimitText, rewardBoxImg, rewards);
			_Open();
		}

		public void ClickReceive(MButton btn)
		{
			if (btn != null)
			{
				btn.interactable = false;
				RoyalPassHttpMgr.I.SendTeamReward();
				if (rewards != null)
				{
					int length = rewards.GetLength(0);
					for (int i = 0; i < length; i++)
					{
						int id = rewards[i, 0];
						int count = rewards[i, 1];
						if (id == (int)PropID.Medal)
						{
							DB.Bag.Modify(PropID.Medal, count, true, "PassBuyReward");
						}
						else if (id == (int)PropID.EnergyRecoverUpLimit)
						{
							DB.Bag.ModifyEnergyRecoverUpLimit(count);
						}
					}
					DispatcherMgr.Emit("GamePause", false, "Reward");
					_Close(delegate
					{
						receiveCallback.InvokeSafe();
						Singleton<LoginMgr>.I.Cost();
					});
				}
			}
		}
	}
}
