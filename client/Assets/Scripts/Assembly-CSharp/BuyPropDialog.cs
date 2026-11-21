using System;
using System.Collections.Generic;
using DG.Tweening;
using Function_Reward_Video;
using UnityEngine;
using UnityEngine.UI;

public class BuyPropDialog : DialogBase
{
	public MButton closeBtn;

	public Image imgIco;

	public Image imgBuyIcon;

	public Image imgAdIcon;

	public Image cdIcon;

	public Text goldTex;

	public Text goldPropNumTex;

	public Text adPropNumTex;

	public MButton btnAdv;

	public MButton buyBtn;

	public Action func_callback_adv;

	public Action<bool> func_callback_close;

	public CanvasGroup topUI;

	public FreeGold free;

	[Header("Cd")]
	public Text CdTxt;

	public MButton CdBtn;

	private REWARD_PLACE kPlace;

	private PropID kPropId;

	private int nCount;

	private int nPrice;

	private Tween topTw;

	private const int maxItemNum = 3;

	private bool isPauseGame;

	[SerializeField]
	private BuyPropItem[] items;

	[SerializeField]
	private Transform tfItemsRoot;

	private Tween countdownTw;

	private PointType pointType;

	private bool isNewRewardAdCtrl;

	public BuyPropDialog()
	{
		fMaskAlpha = 0.8f;
	}

	private void Start()
	{
		DispatcherMgr.On<GiftType>("BuySuccessCallBack", OnBuySuccess);
		topTw = TweenMgr.NumberTo(0.6f, 0f, 1f, delegate(float v)
		{
			topUI.alpha = v;
		});
	}

	private void Init(PropID kpropid, REWARD_PLACE kPlace)
	{
		this.kPlace = kPlace;
		kPropId = kpropid;
		bool flag;
		switch (kpropid)
		{
		case PropID.Hint:
			nCount = cf_const.hintBuyOnceNum;
			nPrice = cf_const.hintNeedGold;
			flag = cf_const.hintAdNum == -1 || DB.User.SurHintAdNum > 0;
			break;
		case PropID.Magnet:
			nCount = cf_const.magnetBuyOnceNum;
			nPrice = cf_const.magnetNeedGold;
			flag = cf_const.magnetAdNum == -1 || (cf_const.magnetAdNum >= 1 && DB.User.SurMagnetAdNum > 0);
			break;
		case PropID.Back:
			nCount = cf_const.backBuyOnceNum;
			nPrice = cf_const.backNeedGold;
			flag = cf_const.backAdNum == -1 || (cf_const.backAdNum >= 1 && DB.User.SurBackAdNum > 0);
			break;
		case PropID.Refresh:
			nCount = cf_const.refreshBuyOnceNum;
			nPrice = cf_const.refreshNeedGold;
			flag = cf_const.refreshAdNum == -1 || (cf_const.refreshAdNum >= 1 && DB.User.SurRefreshAdNum > 0);
			break;
		default:
			nCount = cf_const.freezeBuyOnceNum;
			nPrice = cf_const.freezeNeedGold;
			flag = cf_const.freezeAdNum == -1 || (cf_const.freezeAdNum >= 1 && DB.User.SurFreezeAdNum > 0);
			break;
		}
		Sprite sprite = LoadMgr.LoadRes<Sprite>(string.Format("Props/prop{0}", (int)kpropid));
		imgBuyIcon.sprite = sprite;
		imgAdIcon.sprite = sprite;
		cdIcon.sprite = sprite;
		imgIco.sprite = sprite;
		imgIco.SetNativeSize();
		adPropNumTex.text = "x1";
		goldTex.text = nPrice.ToString();
		goldPropNumTex.text = string.Format("x{0}", nCount);
		flag = RefreshAd(kpropid, flag);
		float height;
		if (cf_const.lvBuyProp <= DB.User.Curlv + 1 && IAPManager.instance.isInitialized)
		{
			tfItemsRoot.gameObject.SetActive(true);
			RefreshPropItems();
			if (!isNewRewardAdCtrl)
			{
				btnAdv.gameObject.SetActive(flag);
			}
			height = (flag ? 900f : 789f);
		}
		else
		{
			tfItemsRoot.gameObject.SetActive(false);
			if (!isNewRewardAdCtrl)
			{
				btnAdv.gameObject.SetActive(flag);
			}
			height = (flag ? 663f : 535f);
		}
		tfRoot.SetHeight(height);
	}

	private bool RefreshAd(PropID id, bool isHasAd)
	{
		pointType = Singleton<RewardVideoManager>.I.GetTypeByPropID(id);
		isNewRewardAdCtrl = Singleton<RewardVideoManager>.I.IsOpen(pointType);
		if (!isNewRewardAdCtrl)
		{
			return isHasAd;
		}
		RewardVideoData adData = Singleton<RewardVideoManager>.I.GetAdData(pointType);
		switch (adData.State)
		{
		case RewardVideoState.Open:
			CdBtn.gameObject.SetActive(false);
			btnAdv.gameObject.SetActive(true);
			return true;
		case RewardVideoState.Closed:
			CdBtn.gameObject.SetActive(false);
			btnAdv.gameObject.SetActive(false);
			return false;
		case RewardVideoState.Cooling:
			CdBtn.gameObject.SetActive(true);
			btnAdv.gameObject.SetActive(false);
			RefreshAdCountdown(adData.CoolingEndRemainingTime);
			return true;
		default:
			return isHasAd;
		}
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
			CdTxt.text = Util.Helper.FormatTimer(va, "hh:mm:ss");
		}).SetEase(Ease.Linear).SetId("FailureControl_Countdown")
			.OnComplete(delegate
			{
				pointType = Singleton<RewardVideoManager>.I.GetTypeByPropID(kPropId);
				isNewRewardAdCtrl = Singleton<RewardVideoManager>.I.IsOpen(pointType);
				if (!isNewRewardAdCtrl)
				{
					return;
				}
				RewardVideoData adData = Singleton<RewardVideoManager>.I.GetAdData(pointType);
				switch (adData.State)
				{
				case RewardVideoState.Open:
					CdBtn.gameObject.SetActive(false);
					btnAdv.gameObject.SetActive(true);
					break;
				case RewardVideoState.Closed:
					CdBtn.gameObject.SetActive(false);
					btnAdv.gameObject.SetActive(false);
					break;
				case RewardVideoState.Cooling:
					CdBtn.gameObject.SetActive(true);
					btnAdv.gameObject.SetActive(false);
					RefreshAdCountdown(adData.CoolingEndRemainingTime);
					break;
				}
			});
	}

	public void Show(PropID kpropid, REWARD_PLACE kPlace, bool isPause = true)
	{
		isPauseGame = isPause;
		if (isPause)
		{
			DispatcherMgr.Emit("GamePause", true, "BuyProp");
		}
		Init(kpropid, kPlace);
		_Open(delegate
		{
			if (cf_const.lvBuyProp <= DB.User.Curlv + 1 && IAPManager.instance.isInitialized)
			{
				return;
			}
			free.lockCloseBtn = delegate(bool islock)
			{
				closeBtn.SetInteractable(!islock);
			};
			free.Init();
		});
	}

	public void ClickBuy(MButton button)
	{
		button.interactable = false;
		closeBtn.interactable = false;
		if (isPauseGame)
		{
			DispatcherMgr.Emit("GamePause", false, "BuyProp");
		}
		_Close();
		if (DB.Bag.GetProp(PropID.Money) < nPrice)
		{
			Global.pStores.PopStoreDialog(GiftTriggerType.Gold);
			return;
		}
		DB.Bag.Modify(PropID.Money, -nPrice, true, "BuyProp");
		DB.Bag.Modify(kPropId, nCount, true, "BuyProp");
	}

	public void ClickAdv(MButton button)
	{
		Global.ShowRewardedVideo(delegate(string info)
		{
			if (!(info == "succ"))
			{
				return;
			}
			switch (kPropId)
			{
			case PropID.Hint:
				DB.User.SurHintAdNum = DB.User.SurHintAdNum - 1;
				break;
			case PropID.Magnet:
				DB.User.SurMagnetAdNum = DB.User.SurMagnetAdNum - 1;
				break;
			case PropID.Back:
				DB.User.SurBackAdNum = DB.User.SurBackAdNum - 1;
				break;
			case PropID.Refresh:
				DB.User.SurRefreshAdNum = DB.User.SurRefreshAdNum - 1;
				break;
			default:
				DB.User.SurFreezeAdNum = DB.User.SurFreezeAdNum - 1;
				break;
			}
			if (func_callback_adv != null)
			{
				func_callback_adv();
			}
			if (isPauseGame)
			{
				DispatcherMgr.Emit("GamePause", false, "BuyProp");
			}
			_Close(_close);
			if (isNewRewardAdCtrl)
			{
				Singleton<RewardVideoManager>.I.AddLookCount(pointType);
			}
		}, kPlace);
	}

	public void ClickClose(MButton button)
	{
		button.interactable = false;
		if (isPauseGame)
		{
			DispatcherMgr.Emit("GamePause", false, "BuyProp");
		}
		_Close(delegate
		{
			if (func_callback_close != null)
			{
				func_callback_close(false);
			}
		});
	}

	private void _close()
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (topTw != null)
		{
			topTw.Kill();
			topTw = null;
		}
		DispatcherMgr.Off<GiftType>("BuySuccessCallBack", OnBuySuccess);
		if (countdownTw != null)
		{
			countdownTw.Kill();
			countdownTw = null;
		}
	}

	private void RefreshPropItems()
	{
		List<cfbt_Stores> cfgsByTypeAndPropID = Global.pStores.GetCfgsByTypeAndPropID(BuySpotType.BuyProp, kPropId);
		if (cfgsByTypeAndPropID == null)
		{
			return;
		}
		int count = cfgsByTypeAndPropID.Count;
		for (int i = 0; i < 3; i++)
		{
			items[i].gameObject.SetActive(i < count);
			if (i < count)
			{
				items[i].Init(cfgsByTypeAndPropID[i]);
			}
		}
	}

	private void OnBuySuccess(GiftType type)
	{
		if (kPropId == (PropID)type)
		{
			if (isPauseGame)
			{
				DispatcherMgr.Emit("GamePause", false, "BuyProp");
			}
			_Close(_close);
		}
	}
}
