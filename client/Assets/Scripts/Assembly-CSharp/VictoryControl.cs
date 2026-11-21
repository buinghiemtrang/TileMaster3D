using System;
using System.Collections.Generic;
using _Game._Functions.PropIceBreakingGift;
using _Game._Functions.RoadToRiches.Scripts;
using DG.Tweening;
using Festival;
using GameScene;
using GameScene_Element;
using GameScene_Level;
using RoyalPass;
using ShortBattlePass;
using TreasureHunt;
using UnityEngine;
using UnityEngine.UI;

public class VictoryControl : VictoryBase
{
	public static VictoryControl instance;

	public GameObject adBtn;

	public GameObject hasBgNextBtn;

	public GameObject noBgNextBtn;

	public GameObject homeBtn;

	public GameObject smallClaimBtn;

	public GameObject smallAdClaimBtn;

	public Text claimStarText;

	public Text adClaimStarText;

	public Text smallClaimStarText;

	public Text smallAdClaimStarText;

	public GameObject LeftRightMoveAdsObj;

	public NoAdsBanner pNoAds;

	public VictoriesActivityBanner pVictoriesActivity;

	public GameParkBanner gameParkBanner;

	public DialogShow pDialog;

	public Transform tfWinEff;

	public Text starTex;

	public Sprite spStar;

	public Text adMultipleTex;

	public Transform giftBannerParent;

	public GameObject btnResurgenceObj;

	public LeftRightMoveAdsCtl adsCrl;

	public Transform activityRoot;

	public Transform activityNode2;

	public Transform activityNode3;

	public RoadToRichesDialog pRoadToRichesDialog;

	private int _starNum;

	private int _adMultiple;

	public bool isPlayRewardAd;

	private List<BezierInfo> _flyStars;

	private Tween _moveTw;

	private LinearTriggerOne pLinearTriggerOne;

	private bool _isForceGoHome;

	private bool _isUnlockAdMulti;

	private int nMoveAnimCount;

	private void Awake()
	{
		DispatcherMgr.Emit("GamePause", true, "Victory");
		instance = this;
		adsCrl.cback = delegate(int va)
		{
			_adMultiple = va;
			if (adMultipleTex != null)
			{
				adMultipleTex.text = string.Format("{0}", _starNum * va);
			}
		};
		pLinearTriggerOne = new LinearTriggerOne();
	}

	private void OnDestroy()
	{
		instance = null;
		pLinearTriggerOne.Clear();
		if (_moveTw != null)
		{
			_moveTw.Kill();
		}
		DOTween.Kill("DoTween_Victory_Delay_NextFunction");
		if (_flyStars == null)
		{
			return;
		}
		for (int num = _flyStars.Count - 1; num >= 0; num--)
		{
			BezierInfo bezierInfo = _flyStars[num];
			if (bezierInfo != null)
			{
				if (bezierInfo.Sequence != null)
				{
					bezierInfo.Sequence.Kill();
				}
				if ((bool)bezierInfo.Target)
				{
					UnityEngine.Object.Destroy(bezierInfo.Target);
				}
				_flyStars.RemoveAt(num);
			}
		}
	}

	private void Start()
	{
		isPlayRewardAd = false;
		int totalNumber = ElementsMgr.I.Data.TotalNumber;
		int remainingNumber = ElementsMgr.I.Data.RemainingNumber;
		LogEvent.I.Level_Victory(LevelMgr.Data.Type, totalNumber, remainingNumber, LevelMgr.Data.IsRegionRankLevel);
		LogEvent.I.AF_LevelWin(DB.User.Curlv + 1);
		DB.Track.ConsecutiveVictory = DB.Track.ConsecutiveVictory + 1;
		AudioMgr.I.PauseMusic();
		AudioMgr.I.PlaySound("victory");
		if (DB.User.Curlv == DB.User.Maxlv)
		{
			DB.User.Maxlv = DB.User.Maxlv + 1;
			Singleton<PropIceBreakingGiftManager>.I.AddPassLevelCount(1);
			_starNum = DB.User.SumStarLevel;
			ModifySumStar(_starNum);
			BoosterMgr.I.SetGear(1, isDirect: false);
			DB.User.AddMonthIssueLevel(1);
			VictoriesActivityMgr.I.SetData(isWin: true);
			Singleton<RoyalPassActivityMgr>.I.SetScore(1);
			Singleton<ShortBattlePassMgr>.I.SetScore(1);
			Singleton<TreasureHuntActivityMgr>.I.SetScore(totalNumber);
			ModelChestControl.NeedShowAnimation = true;
			starTex.text = _starNum.ToString();
			if (DB.User.Maxlv == cf_const.RoadToRichesUnlockLv)
			{
				Singleton<RoadToRichesMgr>.I.Initialized();
			}
		}
		_isForceGoHome = cf_const.IsVictoryBackHome > 0 && cf_const.VictoryBackHomeLevel <= DB.User.Maxlv;
		_isUnlockAdMulti = cf_const.ShowAdMultiStarLevel <= DB.User.Maxlv;
		DB.User.Curlv = DB.User.Maxlv;
		Singleton<LogEventValueOnceTrack>.I.Trigger(ValueOnceTrackTriggerType.LevelPass, 1f);
		Singleton<LogEventValueRepeatTrack>.I.Trigger(ValueTrackTriggerType.LevelPass, 0f);
		if (LevelMgr.Data.EnableDynamicDifficulty)
		{
			LevelMgr.Data.Difficulty.Victory();
		}
		else
		{
			Debug.LogError("EnableDynamicDifficulty: false");
		}
		DB.User.BombModelProbability = DB.User.BombModelProbability + 5;
		if (DB.User.Curlv == 1)
		{
			btnResurgenceObj.SetActive(value: false);
		}
		Singleton<NetworkInspectionManager>.I.CheckNetwork(null);
	}

	public void SetBgHeight(bool isShowAd)
	{
		if (pDialog != null)
		{
			bool flag = _isUnlockAdMulti && isShowAd;
			pDialog.tfRoot.SetHeight(flag ? 540f : 452f);
			pDialog.Open(flag ? 330f : 320f, delegate
			{
			});
			tfWinEff.gameObject.SetActive(value: true);
		}
	}

	private void ShowButtonState(bool isShowAd = true)
	{
		bool isUnlockAdMulti = _isUnlockAdMulti;
		bool flag = isUnlockAdMulti && isShowAd;
		LeftRightMoveAdsObj.SetActive(flag);
		ResetBtnState();
		smallClaimStarText.text = starTex.text;
		claimStarText.text = starTex.text;
		if (!_isForceGoHome)
		{
			homeBtn.SetActive(value: true);
			hasBgNextBtn.SetActive(!isUnlockAdMulti || !isShowAd);
			adBtn.SetActive(flag);
			noBgNextBtn.SetActive(flag);
			adMultipleTex = adClaimStarText;
		}
		else if (isUnlockAdMulti && isShowAd)
		{
			smallClaimBtn.gameObject.SetActive(value: true);
			smallAdClaimBtn.gameObject.SetActive(value: true);
			adMultipleTex = smallAdClaimStarText;
		}
		else
		{
			homeBtn.SetActive(value: true);
			hasBgNextBtn.gameObject.SetActive(value: true);
		}
		if (gameParkBanner.Show())
		{
			gameParkBanner.ONGo = delegate
			{
				_home(isLookAd: false);
			};
		}
		else if (pVictoriesActivity.Show())
		{
			pVictoriesActivity.onGo = _home;
		}
		else
		{
			bool flag2 = pNoAds.JudgeShow();
			cfbt_Stores cfbt_Stores = _JudgeShowGiftBanner();
			if (flag2)
			{
				if (cfbt_Stores == null || UnityEngine.Random.Range(1, 11) >= cf_const.noAdBannerAndGiftBannerWeight)
				{
					pNoAds.Show();
					return;
				}
			}
			else if (cfbt_Stores == null)
			{
				return;
			}
			_ShowGiftBanner(cfbt_Stores);
		}
	}

	private void ResetBtnState()
	{
		homeBtn.SetActive(value: false);
		hasBgNextBtn.gameObject.SetActive(value: false);
		adBtn.gameObject.SetActive(value: false);
		noBgNextBtn.SetActive(value: false);
		smallClaimBtn.gameObject.SetActive(value: false);
		smallAdClaimBtn.gameObject.SetActive(value: false);
	}

	private void _ShowGiftBanner(cfbt_Stores cfg)
	{
		GiftBanner giftBanner = Singleton<FestivalMgr>.I.LoadAndInstantiate<GiftBanner>("Festival/Result/{0}/GiftBanner");
		giftBanner.transform.SetParent(giftBannerParent, worldPositionStays: false);
		giftBanner.Show(cfg);
	}

	private cfbt_Stores _JudgeShowGiftBanner()
	{
		if (DB.User.Maxlv + 1 <= cf_const.giftBannerUnlockLv)
		{
			return null;
		}
		if (!IAPManager.instance.isInitialized)
		{
			return null;
		}
		cfbt_Gift showGiftInMainScene = GiftDataMgr.ins.GetShowGiftInMainScene();
		if (showGiftInMainScene == null)
		{
			return null;
		}
		int[] array = showGiftInMainScene.storeIndex.ToInt(',');
		if (array.Length != 1)
		{
			return null;
		}
		if (DB.GiftStalls.GetGiftDataById(showGiftInMainScene.id) == null)
		{
			return null;
		}
		return GiftDataMgr.ins.GetStoreCfgByIndex(array[0]);
	}

	public void ClickHome(MButton button)
	{
		button.interactable = false;
		_home(isLookAd: true);
	}

	public void ClickBtnNext(MButton button)
	{
		if (_isForceGoHome)
		{
			_home(isLookAd: true);
			return;
		}
		bool flag = Singleton<RoyalPassActivityMgr>.I.JudgeOpen() && Singleton<RoyalPassActivityMgr>.I.JudgeHasState(RoyalPass.ActivityState.ActivityDialog);
		int num = VictoriesActivityMgr.I.JudgeAutoEject();
		if (flag)
		{
			SceneMgr.SetData("Open" + FunctionType.RoyalPass.ToString());
			_home(isLookAd: false);
			return;
		}
		// UNVERIFIED: asm so sánh (num - 4) >= 0xFFFFFFFE tức num == 2 || num == 3; giá trị enum đưa vào chuỗi là hằng 2 (FunctionType.Victories)
		if (num == 2 || num == 3)
		{
			SceneMgr.SetData("Open" + FunctionType.Victories.ToString());
			_home(isLookAd: true);
			return;
		}
		if (!cf_const.watchRewardedVideoNotOutNextInterstitial)
		{
			if (isPlayRewardAd)
			{
				_next();
				return;
			}
		}
		else
		{
			isPlayRewardAd = false;
		}
		if (DB.User.Curlv == cf_const.unlockRateUsLv + 1 || cf_const.IsOpenVictoryReward)
		{
			_next();
			return;
		}
		Global.ShowNextInterstitial(delegate
		{
			_next();
		}, DB.User.Curlv, INTER_PLACE.AdInterstitial);
	}

	public void ClickAdBtn(MButton btn)
	{
		adsCrl.SetStop();
		Global.ShowRewardedVideo(delegate(string info)
		{
			if (!info.Equals("succ") || starTex == null)
			{
				return;
			}
			int star = _starNum;
			int num = (_adMultiple - 1) * _starNum;
			isPlayRewardAd = true;
			ModifySumStar(num);
			Sprite sprite = Singleton<FestivalMgr>.I.LoadAsset<Sprite>("Festival/Common/{0}/VictoryStar");
			if ((bool)sprite)
			{
				spStar = sprite;
			}
			Vector3 p = SceneMgr.tfTopRoot.InverseTransformPoint(adBtn.transform.position);
			Vector3 p2 = SceneMgr.tfTopRoot.InverseTransformPoint(starTex.transform.parent.position);
			Vector3 p3 = Utils.Vec3(p.x + (p2.x - p.x) * 0.5f, p2.y, 0f);
			int count = num / 20;
			if (count > 49)
			{
				count = 50;
			}
			if (count < 2)
			{
				count = 1;
			}
			int one = num / count;
			int remaining = count;
			_flyStars = TweenMgr.BezierFlyerH(spStar, 0.6f, p, p3, p2, count, Utils.Vec2(-100f, 100f), 1f, 0.01f, delegate
			{
				starTex.text = (star + one).ToString();
				remaining--;
				if (remaining == 0)
				{
					starTex.text = (_adMultiple * _starNum).ToString();
					if (_isForceGoHome)
					{
						_home(isLookAd: false);
					}
					else
					{
						_next();
					}
				}
			});
		}, REWARD_PLACE.AdStarDouble);
	}

	private void _next()
	{
		pDialog._Close(delegate
		{
			if ((bool)AudioMgr.I && (bool)GameMgr.I)
			{
				BlackCutDialog.Show(delegate
				{
					AudioMgr.I.ResumeMusic();
					DispatcherMgr.Emit("GamePause", false, "Victory");
					GameMgr.I.NewLevel(LevelType.Ordinary, -1, -1);
				});
			}
		});
	}

	private void _home(bool isLookAd)
	{
		Action functionAction = delegate
		{
			pDialog._Close();
			BlackCutDialog.Show(delegate
			{
				AudioMgr.I.ResumeMusic();
				DispatcherMgr.Emit("GamePause", false, "Victory");
				GiftDataMgr.ins.ResetShowPopStatus(true);
				SceneMgr.LoadScene("MainScene", "GameScene", delegate
				{
					BlackCutDialog.Hide();
				});
			});
		};
		if (isLookAd && !cf_const.IsOpenVictoryReward)
		{
			Global.ShowLvInterstitial(delegate
			{
				functionAction();
			}, INTER_PLACE.AdInterstitial);
		}
		else
		{
			functionAction();
		}
	}

	public void CreateFunctionQueue(bool isShowAd)
	{
		if (GameScene.ActivityInResultMgr.I != null)
		{
			GameScene.ActivityInResultMgr.I.Initialized();
		}
		if (GameScene.ActivityInResultMgr.I != null && GameScene.ActivityInResultMgr.I.bIsShow)
		{
			pLinearTriggerOne.Push(delegate
			{
				ShowActivityMoveAnim();
				TweenMgr.DelayCall(1f, NextFunction).SetId("DoTween_Victory_Delay_NextFunction");
			});
		}
		else
		{
			activityNode3.gameObject.SetActive(value: false);
		}
		if (Singleton<RoadToRichesMgr>.I.EndTime != default(DateTime))
		{
			pLinearTriggerOne.Push(delegate
			{
				ShowActivityMoveAnim();
				pRoadToRichesDialog.LevelUp();
			});
		}
		else
		{
			activityNode2.gameObject.SetActive(value: false);
		}
		pLinearTriggerOne.Push(delegate
		{
			ShowButtonState(isShowAd);
		});
	}

	public void NextFunction()
	{
		pLinearTriggerOne.Next();
	}

	private void ShowActivityMoveAnim()
	{
		nMoveAnimCount++;
		if (_moveTw != null)
		{
			_moveTw.Kill();
		}
		_moveTw = ((RectTransform)activityRoot).DOAnchorPosX(nMoveAnimCount * -550, 0.4f);
	}
}
