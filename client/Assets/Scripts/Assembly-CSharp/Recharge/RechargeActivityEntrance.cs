using DG.Tweening;
using FunctionEject;
using UnityEngine;
using UnityEngine.UI;

namespace Recharge
{
	public class RechargeActivityEntrance : EjectItem
	{
		[SerializeField]
		private Text countdownTex;

		[SerializeField]
		private MButton button;

		private int _surTime;

		private void Awake()
		{
			base.gameObject.SetActive(false);
			button.onClick.AddListener(_ClickEntrance);
		}

		public override void Init()
		{
			RechargeActivityMgr.I.RefreshActivityData();
			_ShowEntrance();
			_Countdown();
		}

		public override bool Eject()
		{
			bool flag = RechargeActivityMgr.I.JudgeAutoEject() != 0;
			if (flag)
			{
				_EnterActivityDialog();
			}
			return flag;
		}

		private void _ShowEntrance()
		{
			base.gameObject.SetActive(!cf_const.IsForceCloseRechargeActivity && RechargeActivityMgr.I.JudgeUnlocked() && RechargeActivityMgr.I.JudgeInActivity());
		}

		private void _Countdown()
		{
			countdownTex.DOKill(false);
			_surTime = RechargeActivityMgr.I.GetEndTime();
			if (_surTime != 0)
			{
				countdownTex.DOCounter(_surTime, 0, _surTime, delegate(int value)
				{
					countdownTex.text = Util.Helper.FormatTimerD(value);
					_surTime = value;
					DispatcherMgr.Emit("RechargeActivity_EndTimeCountdown", _surTime);
				}).OnComplete(Init);
			}
		}

		private void _ClickEntrance(MButton btn)
		{
			_EnterActivityDialog();
		}

		private void _EnterActivityDialog()
		{
			RechargeActivityDialog rechargeActivityDialog = SceneMgr.AddTopNode<RechargeActivityDialog>("Prefabs/Activity/Recharge/RechargeActivityDialog");
			rechargeActivityDialog.CloseCallback = delegate(int state)
			{
				End((state == 1) ? EjectType.Continue : EjectType.Break);
			};
			rechargeActivityDialog.Show(_surTime);
		}

		private void OnDestroy()
		{
			countdownTex.DOKill(false);
		}
	}
}
