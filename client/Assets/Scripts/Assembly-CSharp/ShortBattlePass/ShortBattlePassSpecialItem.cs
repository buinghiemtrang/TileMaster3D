using UnityEngine;

namespace ShortBattlePass
{
	public class ShortBattlePassSpecialItem : ShortBattlePassItem
	{
		[SerializeField]
		private GameObject hookObj;

		[SerializeField]
		private GameObject lockObj;

		[SerializeField]
		private GameObject receiveButtonObj;

		private bool _isReceived;

		private bool _isCanReceive;

		private void Awake()
		{
			DispatcherMgr.On("ShortBattlePass_Unlock", _Unlock);
		}

		private void OnDestroy()
		{
			DispatcherMgr.Off("ShortBattlePass_Unlock", _Unlock);
		}

		public override void Init(int ordinaryRewardsNum, RewardInfo info)
		{
			base.Init(ordinaryRewardsNum, info);
			_Refresh();
		}

		private void _Refresh()
		{
			_isReceived = Info.State == RewardState.All;
			_isCanReceive = !_isReceived && Info.IsPaid && Info.IsSatisfyCondition;
			hookObj.SetActive(_isReceived);
			receiveButtonObj.SetActive(_isCanReceive);
			lockObj.SetActive(!Info.IsPaid);
		}

		public void ClickReceive(MButton btn)
		{
			if (_isReceived || !_isCanReceive)
			{
				return;
			}
			btn.interactable = false;
			RewardState state = Singleton<ShortBattlePassMgr>.I.Receive(RewardItemType.Special, Info.Config.id, true);
			Global.CreateReward(Info.Config.CostReward, RewardType.RoyalPassActivity, REWARD_PLACE.None, "LittlePassBuyReward").SetChestSkin("08").SetComplete(delegate
			{
				Info.State = state;
				_Refresh();
				btn.interactable = true;
			});
		}

		private void _Unlock()
		{
			if (base.gameObject.activeSelf && Info != null)
			{
				Info.IsPaid = true;
				_Refresh();
			}
		}
	}
}
