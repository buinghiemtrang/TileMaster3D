using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ShortBattlePass
{
	public class ShortBattlePassDialog : DialogBase
	{
		[SerializeField]
		private Text timeTex;

		[SerializeField]
		private Text progressTex;

		[SerializeField]
		private Text gradeTex;

		[SerializeField]
		private Image barImg;

		[SerializeField]
		private GameObject finishValueObj;

		[SerializeField]
		private MButton closeButton;

		[SerializeField]
		private MButton helpButton;

		[SerializeField]
		private MButton activateButton;

		[SerializeField]
		private MButton keyButton;

		[SerializeField]
		private ShortBattlePassScroll scroll;

		[SerializeField]
		private GameObject activatedObj;

		public Action CloseCallback;

		private Sequence _upgradeSequence;

		private ShortBattlePassActivateDialog _activateDialog;

		protected override void Awake()
		{
			base.Awake();
			closeButton.onClick.AddListener(_ClickClose);
			helpButton.onClick.AddListener(_ClickHelp);
			activateButton.onClick.AddListener(_ClickActivate);
			keyButton.onClick.AddListener(_ClickHelp);
			DispatcherMgr.On<int>("ShortBattlePass_Time_Change", _RefreshTime);
			DispatcherMgr.On<GiftType>("BuySuccessCallBack", _IapBuySuccess);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			progressTex.DOKill();
			scroll.DOKill();
			DispatcherMgr.Off<int>("ShortBattlePass_Time_Change", _RefreshTime);
			DispatcherMgr.Off<GiftType>("BuySuccessCallBack", _IapBuySuccess);
		}

		public void Show(string openType, int remainingTime)
		{
			_RefreshTime(remainingTime);
			_RefreshActivateButton();
			_RefreshScroll();
			_RefreshGrade();
			_Open(_ShowBuySuccessRewardsDialog);
			LogEvent.I.Track("Little_Master_Pass_Open").Add("ActivityTime", DB.RoyalPassActivity.Issue).CommonAttr(true)
				.Send(false);
			LogEvent.I.Track("Home_Frame_Open").Add("Name", "LittleMasterPass").Add("OpenType", openType)
				.CommonAttr(true)
				.Send(false);
		}

		private void _RefreshScroll()
		{
			List<RewardInfo> rewardInfos = Singleton<ShortBattlePassMgr>.I.GetRewardInfos();
			scroll.RefreshItems(rewardInfos);
		}

		private void _ClickClose(MButton button)
		{
			_Close(CloseCallback);
		}

		private void _ClickHelp(MButton button)
		{
			SceneMgr.AddTopNode<DialogShow>("Prefabs/Activity/ShortBattlePass/ShortBattlePassHelpDialog");
		}

		private void _ClickActivate(MButton button)
		{
			_activateDialog = ShortBattlePassActivateDialog.Create();
			_activateDialog.CloseCallback = delegate
			{
				_activateDialog = null;
			};
		}

		private void _RefreshActivateButton()
		{
			bool flag = Singleton<ShortBattlePassMgr>.I.JudgeHasState(ActivityState.CostSuccess);
			activatedObj.SetActive(flag);
			activateButton.gameObject.SetActive(!flag);
		}

		private void _RefreshTime(int value)
		{
			timeTex.text = Util.Helper.FormatTimer(value, "hh:mm:ss");
			if (finishValueObj.activeSelf != (value == 0))
			{
				finishValueObj.SetActive(value == 0);
			}
			if (timeTex.gameObject.activeSelf == (value == 0))
			{
				timeTex.gameObject.SetActive(value != 0);
			}
		}

		private void _IapBuySuccess(GiftType type)
		{
			if (type == GiftType.ShortBattlePass)
			{
				if (_activateDialog != null)
				{
					_activateDialog.ClickClose(null);
				}
				_ShowBuySuccessRewardsDialog();
			}
		}

		private void _ShowBuySuccessRewardsDialog()
		{
			if (Singleton<ShortBattlePassMgr>.I.ShowBuySuccessRewardsDialog(delegate
			{
				_RefreshActivateButton();
				scroll.Unlock();
				DispatcherMgr.Emit("ShortBattlePass_Unlock");
			}))
			{
				LogEvent.I.Track("Little_Master_Pass_Callback").Add("GradeNum", DB.ShortBattlePass.Data.Grade).Add("Type", "Activity")
					.CommonAttr(true)
					.Send(false);
			}
		}

		private void _RefreshGrade()
		{
			int grade = DB.ShortBattlePass.Data.Grade;
			int residualExp = DB.ShortBattlePass.Data.ResidualExp;
			cfgShortBattlePassReward rewardConfigByGrade = Singleton<ShortBattlePassMgr>.I.GetRewardConfigByGrade(grade);
			if (rewardConfigByGrade != null)
			{
				progressTex.text = ((rewardConfigByGrade.Exp > 0) ? string.Format("{0}/{1}", residualExp, rewardConfigByGrade.Exp) : "Full");
				barImg.fillAmount = ((rewardConfigByGrade.Exp == 0) ? 1f : ((float)residualExp / (float)rewardConfigByGrade.Exp));
				gradeTex.text = grade.ToString();
			}
		}

		public static ShortBattlePassDialog Create(string openType, int remainingTime)
		{
			ShortBattlePassDialog shortBattlePassDialog = SceneMgr.AddTopNode<ShortBattlePassDialog>("Prefabs/Activity/ShortBattlePass/ShortBattlePassDialog");
			shortBattlePassDialog.Show(openType, remainingTime);
			return shortBattlePassDialog;
		}
	}
}
