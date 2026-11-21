using System;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace MonthRanking
{
	public class MonthRankingActivityPosterDialog : DialogBase
	{
		[SerializeField]
		private Text textTime;

		[SerializeField]
		private MButton btnPlay;

		[SerializeField]
		private MButton btnHelp;

		[SerializeField]
		private MButton btnClose;

		private DialogShow helpDialog;

		public Action<bool> closeCallback;

		public MonthRankingActivityPosterDialog()
		{
			fMaskAlpha = 0.8f;
		}

		protected override void Awake()
		{
			base.Awake();
			if (btnPlay != null)
			{
				btnPlay.onClick.AddListener(_ClickBtnPlay);
			}
			if (btnHelp != null)
			{
				btnHelp.onClick.AddListener(_ClickBtnHelp);
			}
			DispatcherMgr.On<int>("MonthRankingActivity_GetServerTime", _RefreshTime);
			DispatcherMgr.On<int>("MonthRankingActivity_TimeChange", _RefreshTime);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			DispatcherMgr.Off<int>("MonthRankingActivity_GetServerTime", _RefreshTime);
			DispatcherMgr.Off<int>("MonthRankingActivity_TimeChange", _RefreshTime);
		}

		public void Show(string openType, int surTime)
		{
			_RefreshTime(surTime);
			_Open();
			DB.MonthRankingActivity.IsAutoEjected = true;
			LogEvent.I.HomeFrameOpen("MonthRankingIntroduce", openType);
		}

		private void _ClickBtnPlay(MButton button)
		{
			_CloseSelf(bIsEnterGame: true);
			Action action = delegate
			{
				BlackCutDialog.EnterGame();
			};
			if (DB.Bag.JudgeHasEnergy())
			{
				action();
				return;
			}
			Action goldInsufficientCall = delegate
			{
				SceneMainBottom.instance.btnShop.ClickSelf();
			};
			EnergyDialog energyDialog = SceneMgr.AddTopNode<EnergyDialog>("Prefabs/EnergyDialog");
			energyDialog.cback = action;
			energyDialog.goldInsufficientCall = goldInsufficientCall;
		}

		private void _ClickBtnHelp(MButton btn)
		{
			helpDialog = SceneMgr.AddTopNode<DialogShow>("Prefabs/Activity/MonthRanking/MonthRankingActivityHelpDialog");
			helpDialog.func_callback_close = delegate
			{
				helpDialog = null;
			};
		}

		public void ClickBtnClose(MButton btn)
		{
			_CloseSelf(bIsEnterGame: false);
		}

		private void _RefreshTime(int leftTime)
		{
			if (!(textTime == null))
			{
				textTime.text = Helper.FormatTimer(leftTime, "hh:mm:ss");
			}
		}

		private void _CloseSelf(bool bIsEnterGame)
		{
			helpDialog?.ClickClose();
			_Close(delegate
			{
				closeCallback?.Invoke(bIsEnterGame);
			});
		}
	}
}
