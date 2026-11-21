using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Recharge
{
	public class RewardItem : MonoBehaviour
	{
		[SerializeField]
		private Text conditionValueTex;

		[SerializeField]
		private GameObject notReceivedObj;

		[SerializeField]
		private GameObject receivedObj;

		[SerializeField]
		private GameObject rightObj;

		[SerializeField]
		private Text differenceValueTex;

		[SerializeField]
		private Text buttonValueTex;

		[SerializeField]
		private ItemUI[] notReceivedItems;

		[SerializeField]
		private ItemUI[] receivedItems;

		[SerializeField]
		private MButton button;

		private RewardInfo _info;

		private void Awake()
		{
			button.onClick.AddListener(_ClickButton);
		}

		public void Init(RewardInfo info)
		{
			_info = info;
			conditionValueTex.SetLBy(string.Format("${0}", info.Cfg.Condition));
			_RefreshUI();
		}

		private void _RefreshUI()
		{
			receivedObj.SetActive(_info.IsReceived);
			notReceivedObj.SetActive(!_info.IsReceived);
			rightObj.SetActive(!_info.IsReceived);
			if (_info.IsReceived)
			{
				_SetReward(receivedItems);
				return;
			}
			_SetReward(notReceivedItems);
			if (!_info.IsSatisfyReceivingCondition)
			{
				differenceValueTex.gameObject.SetActive(true);
				differenceValueTex.SetLBy(string.Format("${0:#0.00}", _info.Difference));
				buttonValueTex.SetLKey("Language429");
			}
			else
			{
				differenceValueTex.gameObject.SetActive(false);
				buttonValueTex.SetLKey("Language318");
			}
		}

		private void _SetReward(IReadOnlyList<ItemUI> items)
		{
			int length = _info.Cfg.Reward.GetLength(0);
			for (int i = 0; i < items.Count; i++)
			{
				ItemUI itemUI = items[i];
				if (i < length)
				{
					Sprite sprite;
					string text;
					Global.GetContents((PropID)_info.Cfg.Reward[i, 0], _info.Cfg.Reward[i, 1], out sprite, out text);
					if (itemUI.valueTex != null)
					{
						itemUI.valueTex.text = text;
					}
					itemUI.iconImg.sprite = sprite;
				}
				else
				{
					itemUI.obj.SetActive(false);
				}
			}
		}

		private void _ClickButton(MButton btn)
		{
			if (_info.IsReceived)
			{
				return;
			}
			if (_info.IsSatisfyReceivingCondition)
			{
				if (!RechargeActivityMgr.I.Receive(_info.Cfg))
				{
					return;
				}
				_info.IsReceived = true;
				Global.ReceiveReward(_info.Cfg.Reward, "Top-up_Reward");
				_RefreshUI();
				LogEvent.I.Track("Top-up_Reward_Receive")
					.Add("ReceiveId", _info.Cfg.Id)
					.Add("Reward", _info.Cfg.Reward.ToStr(',', '|'))
					.CommonAttr(true)
					.Send(false);
			}
			else
			{
				DispatcherMgr.Emit("RechargeActivity_ClickJumpStoreButton");
			}
		}
	}
}
