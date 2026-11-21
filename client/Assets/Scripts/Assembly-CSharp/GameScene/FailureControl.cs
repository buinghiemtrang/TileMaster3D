using System;
using _Game._Functions.RoadToRiches.Scripts;
using DG.Tweening;
using Fishtail.PlayTheBall.Vibration;
using Function_Reward_Video;
using GameScene.Fail.Banner;
using GameScene_Element;
using GameScene_Level;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace GameScene
{
	public class FailureControl : MonoBehaviour
	{
		public DialogShow pDialog;

		public Transform tfBackground;

		public Transform tipsRoot;

		public MButton adBtn;

		public Transform trfAdBtn;

		public Transform trfBuyBtn;

		public Transform trfPropBuyBtn;

		public Text goldTex2;

		public Text timeTex;

		public Text countdownTex;

		public Text adNumTex;

		public Text propNumTex;

		public Text timeFailContentText;

		public FailBannerControl failBannerControl;

		public GameObject TObj;

		public GameObject CObj;

		public GameObject activityInfoObj;

		public GameObject activityRoadToRichesObj;

		public ActivityInResultMgr _ActivityInResultMgr;

		private Sequence closeBtnSeq;

		private Tween countdownTw;

		private Tween tweenDelayCallback;

		public Action func_restart;

		public bool isTimeOut;

		private bool isShowAdBtn;

		private LinearTriggerOne pLinearTriggerOne;

		private Transform _beforeShowRoot;

		private bool isNewRewardCtrl;

		private void OnDestroy()
		{
			if (closeBtnSeq != null)
			{
				closeBtnSeq.Kill();
				closeBtnSeq = null;
			}
			if (countdownTw != null)
			{
				countdownTw.Kill();
				countdownTw = null;
			}
			if (tweenDelayCallback != null)
			{
				tweenDelayCallback.Kill();
			}
			if (pLinearTriggerOne != null)
			{
				pLinearTriggerOne.Clear();
			}
			foreach (Transform item in tipsRoot.GetComponentInChildren<Transform>())
			{
				item.DOKill();
			}
		}

		private void Awake()
		{
			DispatcherMgr.Emit("GamePause", true, "Failure");
			pDialog.func_callback_close = delegate
			{
				DispatcherMgr.Emit("GamePause", false, "Failure");
				GameMgr.I.ModifyGameStatus(GameStatus.Playing);
			};
			if (activityInfoObj != null)
			{
				activityInfoObj.SetActive(value: false);
			}
			if (activityRoadToRichesObj != null)
			{
				activityRoadToRichesObj.SetActive(value: false);
			}
		}

		private void Start()
		{
			DB.Track.ConsecutiveVictory = 0;
			_CreateFunctionQueue();
			TObj.SetActive(isTimeOut);
			CObj.SetActive(!isTimeOut);
			_beforeShowRoot = (isTimeOut ? TObj : CObj).transform;
			timeTex.text = string.Format("+{0}", cf_const.nReviveTime);
			timeFailContentText.SetLBy(cf_const.nReviveTime);
			bool flag;
			isNewRewardCtrl = Singleton<RewardVideoManager>.I.IsOpen(PointType.Revive);
			if (isNewRewardCtrl)
			{
				RewardVideoData adData = Singleton<RewardVideoManager>.I.GetAdData(PointType.Revive);
				flag = adData.State == RewardVideoState.Open;
				isShowAdBtn = flag;
				int remainingCount = adData.RemainingCount;
				if (!flag)
				{
					RefreshAdCountdown(adData.CoolingEndRemainingTime);
				}
				adBtn.gameObject.SetActive(isShowAdBtn);
				adNumTex.transform.parent.gameObject.SetActive(remainingCount > 0);
				if (remainingCount > 0)
				{
					adNumTex.text = remainingCount.ToString();
				}
			}
			else
			{
				flag = _adShow();
			}
			tfBackground.SetHeight(isShowAdBtn ? 563f : 460f);
			DB.Bag.GetProp(PropID.Resurgence);
			trfPropBuyBtn.gameObject.SetActive(value: false);
			trfBuyBtn.gameObject.SetActive(value: true);
			float num = (flag ? (-150f) : (-106f));
			trfBuyBtn.SetScale((flag && isShowAdBtn) ? 0.7f : 1f);
			trfBuyBtn.SetAnchorPosY(isShowAdBtn ? num : 0f);
			goldTex2.text = DB.User.GetGoldReviveNeedGold(cf_const.reviveNeedGold).ToString();
			int prop = DB.Bag.GetProp(PropID.Money);
			int goldReviveNeedGold = DB.User.GetGoldReviveNeedGold(cf_const.reviveNeedGold);
			// vtable 0x258 của DB.User = IUser.get_Maxlv (vtables.tsv)
			failBannerControl.Init(DB.User.Maxlv + 1, isTimeOut, isTimeOut ? cf_const.nReviveTime : 0, goldReviveNeedGold <= prop, this);
			VibrationController.I.ImpactH();
			AudioMgr.I.PauseMusic();
			int totalNumber = ElementsMgr.I.Data.TotalNumber;
			int remainingNumber = ElementsMgr.I.Data.RemainingNumber;
			if (LevelMgr.Data.EnableDynamicDifficulty)
			{
				LevelMgr.Data.Difficulty.Fail(isTimeOut, remainingNumber, totalNumber);
			}
			else
			{
				Debug.LogError("EnableDynamicDifficulty: false");
			}
			LogEvent.I.Level_Fail(LevelMgr.Data.Type, remainingNumber, totalNumber, remainingNumber, isTimeOut, LevelMgr.Data.IsRegionRankLevel);
			Singleton<LogEventValueOnceTrack>.I.Trigger(ValueOnceTrackTriggerType.LevelFail, 1f);
			func_restart = delegate
			{
				GameMgr.I.Init(isReset: true);
			};
			Singleton<NetworkInspectionManager>.I.CheckNetwork(null);
		}

		private void _CreateFunctionQueue()
		{
			pLinearTriggerOne = new LinearTriggerOne();
			if (_ActivityInResultMgr != null)
			{
				_ActivityInResultMgr.Initialized();
				if (_ActivityInResultMgr.bIsShow)
				{
					pLinearTriggerOne.Push(ShowActivityInfo);
				}
			}
			if (Singleton<RoadToRichesMgr>.I.EndTime != default(DateTime))
			{
				pLinearTriggerOne.Push(ShowActivityRoadToRiches);
			}
			if (!pLinearTriggerOne.IsComplete)
			{
				pLinearTriggerOne.Prepend(delegate
				{
					tweenDelayCallback.Kill();
				});
			}
			pLinearTriggerOne.Next();
		}

		public void ClickAdRevive(MButton btn)
		{
			// ShowRewardedVideo(callback, REWARD_PLACE.AdRevive) là thân inline của Global.ShowRewardedVideo (place = 1)
			Global.ShowRewardedVideo(delegate(string b_result)
			{
				if (b_result.Equals("succ"))
				{
					if (cf_const.reviveAdNum > 0)
					{
						DB.User.SurReviveAdNum = DB.User.SurReviveAdNum - 1;
					}
					_adShow();
					Revive();
					if (isNewRewardCtrl)
					{
						// thân inline của RewardVideoManager.AddLookCount(PointType.Revive, 1)
						Singleton<RewardVideoManager>.I.AddLookCount(PointType.Revive);
					}
				}
			}, REWARD_PLACE.AdRevive);
		}

		public void ClickGoldRevive(MButton btn)
		{
			int prop = DB.Bag.GetProp(PropID.Money);
			int goldReviveNeedGold = DB.User.GetGoldReviveNeedGold(cf_const.reviveNeedGold);
			if (prop < goldReviveNeedGold)
			{
				// thân inline của StoresMgr.PopStoreDialog(GiftTriggerType.Gold, false) (DisplayClass21_0: isPause=false, type=1)
				Global.pStores.PopStoreDialog(GiftTriggerType.Gold, isPause: false);
			}
			else
			{
				DB.User.UseGoldReviveNumberTimes = DB.User.UseGoldReviveNumberTimes + 1;
				DB.Bag.Modify(PropID.Money, -goldReviveNeedGold, b_update: true, "Revive");
				Revive();
			}
		}

		public void Revive()
		{
			GameMgr.I.Revive(isTimeOut ? cf_const.nReviveTime : 0);
			PlatformMgr.I.OutPlacePoint();
			pDialog.ClickClose();
			LogEvent.I.Level_Revive();
			LogEvent.I.AF_Revival();
		}

		public void ClickGoHome(MButton button)
		{
			VictoriesActivityMgr.I.SetData(isWin: false);
			DB.User.BombModelProbability = DB.User.BombModelProbability + 20;
			DB.Bag.ModifyEnergy(-1, b_update: true, "");
			BoosterMgr.I.SetGear(0, isDirect: true);
			Global.ShowLvInterstitial(delegate
			{
				pDialog._Close();
				BlackCutDialog.Show(delegate
				{
					AudioMgr.I.ResumeMusic();
					// ghi isShowPopGift = true là thân inline của GiftDataMgr.ResetShowPopStatus(bool)
					GiftDataMgr.ins.ResetShowPopStatus(true);
					SceneMgr.LoadScene("MainScene", "GameScene", delegate
					{
						BlackCutDialog.Hide();
					});
				});
			}, INTER_PLACE.AdInterstitial);
		}

		public void CloseClick(MButton button)
		{
			if (pLinearTriggerOne.IsComplete)
			{
				if (Singleton<RoadToRichesMgr>.I.EndTime != default(DateTime))
				{
					LogEvent.I.Track("Road_To_Wealth_Fail").Add("Road_ID", DB.Other.RoadToRichesCount).CommonAttr(b_max: true)
						.Send();
					DB.Other.RoadToRichesCount = 0;
				}
				pDialog._Close();
				// thân inline của FailureQuitDialog.Create(isTimeOut)
				FailureQuitDialog.Create(isTimeOut);
			}
			else
			{
				pLinearTriggerOne.Next();
			}
		}

		public void ClickPropBuy(MButton button)
		{
			DB.Bag.Modify(PropID.Resurgence, -1, b_update: true, "Revive");
			Revive();
		}

		private bool _adShow()
		{
			if ((bool)adBtn && (bool)trfAdBtn && (bool)adNumTex && (bool)countdownTex)
			{
				isShowAdBtn = cf_const.reviveAdNum != 0;
				trfAdBtn.gameObject.SetActive(isShowAdBtn);
				if (!isShowAdBtn)
				{
					return false;
				}
				int surReviveAdNum = DB.User.SurReviveAdNum;
				bool flag = cf_const.reviveAdNum == -1;
				if (flag)
				{
					adNumTex.transform.parent.gameObject.SetActive(value: false);
				}
				else
				{
					adNumTex.text = DB.User.SurReviveAdNum.ToString();
				}
				adBtn.interactable = flag || surReviveAdNum > 0;
				trfAdBtn.transform.SetScale((flag || surReviveAdNum > 0) ? 1f : 0.7f);
				if (flag || surReviveAdNum > 0)
				{
					return true;
				}
				RefreshAdCountdown(Convert.ToInt32((MDateTime.Parse(DB.User.AdNumResetTime) - DateTime.Now).TotalSeconds));
				return false;
			}
			isShowAdBtn = false;
			return false;
		}

		private void RefreshAdCountdown(int leftTime)
		{
			if (countdownTw != null)
			{
				countdownTw.Kill();
				countdownTw = null;
			}
			int num = Mathf.Max(leftTime, 0);
			countdownTw = TweenMgr.NumberTo(num, num, 0, delegate(int va)
			{
				countdownTex.text = Helper.FormatTimer(va, "hh:mm:ss");
			}).SetEase(Ease.Linear).SetId("FailureControl_Countdown").OnComplete(delegate
			{
				Global.ResetAdNum();
				_adShow();
			});
		}

		private void ShowActivityInfo()
		{
			if (_beforeShowRoot != null)
			{
				_beforeShowRoot.DOKill();
				_beforeShowRoot.DOLocalMoveX(-720f, 0.5f);
			}
			if (activityInfoObj != null)
			{
				activityInfoObj.SetActive(value: true);
				activityInfoObj.transform.SetLocPosX(720f);
				activityInfoObj.transform.DOLocalMoveX(0f, 0.5f);
				_beforeShowRoot = activityInfoObj.transform;
			}
		}

		private void ShowActivityRoadToRiches()
		{
			if (_beforeShowRoot != null)
			{
				_beforeShowRoot.DOKill();
				_beforeShowRoot.DOLocalMoveX(-720f, 0.5f);
			}
			if (activityRoadToRichesObj != null)
			{
				activityRoadToRichesObj.SetActive(value: true);
				activityRoadToRichesObj.transform.SetLocPosX(720f);
				activityRoadToRichesObj.transform.DOLocalMoveX(0f, 0.5f);
				// bản gốc gán activityInfoObj (không phải activityRoadToRichesObj) cho _beforeShowRoot ở cuối hàm (đọc từ assembly 0x10c75a0)
				_beforeShowRoot = activityInfoObj.transform;
			}
		}
	}
}
