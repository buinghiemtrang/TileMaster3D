using System;
using UnityEngine;

namespace ShortBattlePass
{
	public class ShortBattlePassBuySuccessRewardsDialog : DialogBase
	{
		[SerializeField]
		private MButton receiveButton;

		public Action ReceivedCallback;

		private int[,] _rewards;

		protected override void Awake()
		{
			base.Awake();
			receiveButton.onClick.AddListener(_ClickReceive);
		}

		public void Show(int[,] rewards)
		{
			_rewards = rewards;
			DispatcherMgr.Emit("GamePause", true, "ShortBattlePassReward");
			_Open();
		}

		private void _ClickReceive(MButton btn)
		{
			btn.interactable = false;
			Singleton<ShortBattlePassMgr>.I.SendTeamReward();
			int length = _rewards.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				if (_rewards[i, 0] == 24)
				{
					DB.Bag.ModifyEnergyRecoverUpLimit(_rewards[i, 1]);
				}
			}
			DispatcherMgr.Emit("GamePause", false, "ShortBattlePassReward");
			_Close(delegate
			{
				ReceivedCallback.InvokeSafe();
				Singleton<LoginMgr>.I.Cost();
			});
		}

		public static ShortBattlePassBuySuccessRewardsDialog Create(int[,] rewards)
		{
			ShortBattlePassBuySuccessRewardsDialog shortBattlePassBuySuccessRewardsDialog = SceneMgr.AddTopNode<ShortBattlePassBuySuccessRewardsDialog>("Prefabs/Activity/ShortBattlePass/ShortBattlePassBuySuccessRewardsDialog");
			shortBattlePassBuySuccessRewardsDialog.Show(rewards);
			return shortBattlePassBuySuccessRewardsDialog;
		}
	}
}
