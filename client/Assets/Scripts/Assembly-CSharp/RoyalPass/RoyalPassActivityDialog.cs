using System;
using DG.Tweening;
using Festival;
using UnityEngine;
using UnityEngine.UI;

namespace RoyalPass
{
	public class RoyalPassActivityDialog : DialogBase
	{
		[SerializeField]
		private Text timeTex;

		[SerializeField]
		private Text progressTex;

		[SerializeField]
		private GameObject lvStarObj;

		[SerializeField]
		private Text lvTex;

		[SerializeField]
		private Image barImg;

		[SerializeField]
		private GameObject finishValueObj;

		[SerializeField]
		private MButton closeBtn;

		[SerializeField]
		private MButton helpBtn;

		[SerializeField]
		private MButton activateBtn;

		[SerializeField]
		private MButton keyBtn;

		[SerializeField]
		private RoyalPassActivityScroll scroll;

		[SerializeField]
		private GameObject activatedObj;

		[SerializeField]
		private GameObject maskObj;

		public Action closeCallback;

		private Tweener delayTw;

		private Tweener risingExpTw;

		private Sequence upgradeDelaySeq;

		protected override void Awake()
		{
			base.Awake();
			maskObj.SetActive(false);
			scroll.firstReadyFinishCallback = _ScrollFirstReadyFinish;
			closeBtn.onClick.AddListener(_clickClose);
			helpBtn.onClick.AddListener(_clickHelp);
			activateBtn.onClick.AddListener(_clickActivate);
			keyBtn.onClick.AddListener(_clickHelp);
			DispatcherMgr.On<int>("RoyalPassActivity_TimeChange", _RefreshTime);
			DispatcherMgr.On<GiftType>("BuySuccessCallBack", _iAPBuySuc);
		}

		public void Show(string openType, int surTime)
		{
			Singleton<RoyalPassActivityMgr>.I.CheckReissue();
			_RefreshTime(surTime);
			_RefreshActivateBtn();
			_Upgrade(false, false);
			_RefreshScroll();
			_Open();
			LogEvent.I.Track("Master_Pass_Open").Add("ActivityTime", DB.RoyalPassActivity.Issue).CommonAttr(true)
				.Send(false);
			LogEvent.I.HomeFrameOpen("MaterPass", openType);
		}

		private void _ScrollFirstReadyFinish()
		{
			scroll.MoveTo(0f);
			_Upgrade(true, false);
		}

		private void _clickClose(MButton btn)
		{
			btn.interactable = false;
			_Close(closeCallback);
		}

		private void _clickHelp(MButton btn)
		{
			FestivalMgr.I.AddTopNode<DialogShow>("Prefabs/Activity/RoyalPass/{0}_RoyalPassActivityHelpDialog");
		}

		private void _clickActivate(MButton btn)
		{
			FestivalMgr.I.AddTopNode<RoyalPassActivityActivateDialog>("Prefabs/Activity/RoyalPass/{0}_RoyalPassActivityActivateDialog").Show();
		}

		private void _RefreshTime(int surTime)
		{
			timeTex.text = Util.Helper.FormatTimer(surTime, "hh:mm:ss");
			if (finishValueObj.activeSelf != (surTime == 0))
			{
				finishValueObj.SetActive(surTime == 0);
			}
			if (timeTex.gameObject.activeSelf == (surTime == 0))
			{
				timeTex.gameObject.SetActive(surTime != 0);
			}
		}

		private void _RefreshActivateBtn()
		{
			bool flag = Singleton<RoyalPassActivityMgr>.I.JudgeHasState(ActivityState.PaidSuccess);
			activatedObj.SetActive(flag);
			activateBtn.gameObject.SetActive(!flag);
		}

		private void _RefreshScroll()
		{
			scroll.RefreshItems(Singleton<RoyalPassActivityMgr>.I.GetActivityRewardInfo());
		}

		private void _Upgrade(bool isUpgrade, bool isMoveScroll)
		{
			int grade;
			int exp = Singleton<RoyalPassActivityMgr>.I.GetGrade(out grade);
			int curGrade = DB.RoyalPassActivity.CurGrade;
			int curExp = DB.RoyalPassActivity.CurExp;
			if (grade - curGrade >= 2)
			{
				curGrade = grade - 1;
				Singleton<RoyalPassActivityMgr>.I.SetGrade(curGrade, 0);
				curExp = 0;
			}
			cfgRoyalPassActivityReward curCfg = Singleton<RoyalPassActivityMgr>.I.GetRewardCfg(curGrade);
			_SetProgress(curExp, curCfg.exp);
			lvStarObj.SetActive(curGrade == 0);
			lvTex.gameObject.SetActive(curGrade != 0);
			lvTex.text = curGrade.ToString();
			if (!isUpgrade)
			{
				return;
			}
			bool isUp = grade > curGrade;
			if (grade < curGrade)
			{
				Singleton<RoyalPassActivityMgr>.I.SetGrade(grade, exp);
				_Upgrade(false, false);
				_RefreshScroll();
				scroll.MoveTo(0f);
				return;
			}
			if (grade > curGrade)
			{
				maskObj.SetActive(true);
			}
			else if (exp < curExp)
			{
				Singleton<RoyalPassActivityMgr>.I.SetGrade(grade, exp);
				_Upgrade(false, false);
				_RefreshScroll();
				scroll.MoveTo(0f);
				return;
			}
			risingExpTw.OwnKill();
			int toExp;
			float delay;
			if (isUp)
			{
				toExp = curCfg.exp;
				delay = 0.8f;
			}
			else
			{
				toExp = exp;
				delay = ((curExp != toExp) ? 0.8f : 0f);
			}
			risingExpTw = TweenMgr.NumberTo(delay, curExp, toExp, delegate(float va)
			{
				_SetProgress(va, curCfg.exp);
			}).OnComplete(delegate
			{
				Singleton<RoyalPassActivityMgr>.I.SetGrade(grade, isUp ? 0 : exp);
				if (isUp)
				{
					_Upgrade(true, true);
				}
				else if (isMoveScroll)
				{
					DispatcherMgr.Emit("RoyalPassActivity_UpgradeFinish", grade - 1);
					upgradeDelaySeq = DOTween.Sequence();
					upgradeDelaySeq.AppendInterval(1f);
					upgradeDelaySeq.AppendCallback(delegate
					{
						DispatcherMgr.Emit("RoyalPassActivity_ScrollStartMove", grade);
						scroll.MoveTo(1f);
					});
					upgradeDelaySeq.AppendInterval(1f);
					upgradeDelaySeq.AppendCallback(delegate
					{
						maskObj.SetActive(false);
						DispatcherMgr.Emit("RoyalPassActivity_ScrollMoveFinish", true, grade);
					});
				}
			});
		}

		private void _SetProgress(float cur, int exp)
		{
			string text = "Full";
			if (exp > 0)
			{
				text = string.Format("{0}/{1}", Mathf.FloorToInt(cur), exp);
			}
			progressTex.text = text;
			barImg.fillAmount = ((exp == 0) ? 1f : (cur / (float)exp));
		}

		private bool _PaidRewardsDialog()
		{
			int[,] rewards = Singleton<RoyalPassActivityMgr>.I.RiceivePaidReward();
			int length = rewards.GetLength(0);
			if (length != 0)
			{
				maskObj.SetActive(true);
				RoyalPassActivityActivateDialog.I?.ClickClose(null);
				_RefreshActivateBtn();
				DispatcherMgr.Emit("RoyalPassActivity_Unlock");
				delayTw = TweenMgr.DelayCall(1f, delegate
				{
					RoyalPassActivityBuySuccessRewardsDialog royalPassActivityBuySuccessRewardsDialog = FestivalMgr.I.AddTopNode<RoyalPassActivityBuySuccessRewardsDialog>("Prefabs/Activity/RoyalPass/{0}_RoyalPassActivityBuySuccessRewardsDialog");
					royalPassActivityBuySuccessRewardsDialog.receiveCallback = delegate
					{
						maskObj.SetActive(false);
						_RefreshScroll();
					};
					royalPassActivityBuySuccessRewardsDialog.Show(rewards);
					LogEvent.I.Track("Master_Pass_Callback").Add("GradeNum", DB.RoyalPassActivity.CurGrade).Add("Type", "Activity")
						.CommonAttr(true)
						.Send(false);
				});
			}
			return length != 0;
		}

		private void _iAPBuySuc(GiftType type)
		{
			if (type == GiftType.BattleToken)
			{
				_PaidRewardsDialog();
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			delayTw.OwnKill();
			risingExpTw.OwnKill();
			if (upgradeDelaySeq != null)
			{
				upgradeDelaySeq.Kill();
				upgradeDelaySeq = null;
			}
			DispatcherMgr.Off<int>("RoyalPassActivity_TimeChange", _RefreshTime);
			DispatcherMgr.Off<GiftType>("BuySuccessCallBack", _iAPBuySuc);
		}
	}
}
