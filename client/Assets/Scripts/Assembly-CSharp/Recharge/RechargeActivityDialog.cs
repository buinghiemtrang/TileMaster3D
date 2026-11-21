using System;
using UnityEngine;
using UnityEngine.UI;

namespace Recharge
{
	public class RechargeActivityDialog : DialogBase
	{
		[SerializeField]
		private Text titleValueTex;

		[SerializeField]
		private MButton closeButton;

		[SerializeField]
		private Text countdownTex;

		[SerializeField]
		private RechargeActivityScroll scroll;

		public Action<int> CloseCallback;

		protected override void Awake()
		{
			base.Awake();
			closeButton.onClick.AddListener(_ClickClose);
			DispatcherMgr.On("RechargeActivity_ClickJumpStoreButton", _JumpStore);
			DispatcherMgr.On<int>("RechargeActivity_EndTimeCountdown", _Countdown);
		}

		public void Show(int surTime)
		{
			titleValueTex.SetLBy(string.Format("${0}", DB.RechargeActivity.CumulativeRecharge));
			scroll.RefreshItems();
			_Countdown(surTime);
			_Open();
		}

		private void _Countdown(int surTime)
		{
			countdownTex.text = Util.Helper.FormatTimerD(surTime);
		}

		private void _ClickClose(MButton button)
		{
			_Close(delegate
			{
				CloseCallback?.InvokeSafe(1);
			});
		}

		private void _JumpStore()
		{
			_Close(delegate
			{
				CloseCallback?.InvokeSafe(2);
				SceneMainBottom.instance.ClickBtnShop(null);
			});
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			DispatcherMgr.Off("RechargeActivity_ClickJumpStoreButton", _JumpStore);
			DispatcherMgr.Off<int>("RechargeActivity_EndTimeCountdown", _Countdown);
		}
	}
}
