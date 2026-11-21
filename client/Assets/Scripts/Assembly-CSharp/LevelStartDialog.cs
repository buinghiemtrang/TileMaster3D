using System;
using DG.Tweening;
using GameScene_Level;
using UnityEngine;
using UnityEngine.UI;

public class LevelStartDialog : DialogBase
{
	[SerializeField]
	private Text titleTex;

	[SerializeField]
	private GameObject boosterObj;

	[SerializeField]
	private GameObject boosterCanUseIcon;

	[SerializeField]
	private GameObject boosterNotCanUseIcon;

	[SerializeField]
	private Image barImg;

	[SerializeField]
	private Text barTex;

	[SerializeField]
	private MButton selectBtn;

	[SerializeField]
	private GameObject propUnlockObj;

	[SerializeField]
	private GameObject propLockObj;

	[SerializeField]
	private GameObject propNumObj;

	[SerializeField]
	private GameObject propSelected;

	[SerializeField]
	private Text propNumTex;

	[SerializeField]
	private Text unlockLvTex;

	[SerializeField]
	private Image startButtonImg;

	[SerializeField]
	private ChangeOutlineOrShadowColor startButtonValue;

	[SerializeField]
	private Sprite[] startButtonSprites;

	[SerializeField]
	private Image bgImg;

	[SerializeField]
	private Sprite[] bgSprites;

	[SerializeField]
	private Image iconImg;

	[SerializeField]
	private Sprite[] iconSprites;

	[SerializeField]
	private GameObject normalButtonObject;

	[SerializeField]
	private GameObject regionRankButtonObject;

	[SerializeField]
	private Text crownCountTxt;

	public Action<bool, bool> cback;

	private bool isUseBooster;

	private bool isUseMagnet;

	private Tweener propTw;

	public LevelStartDialog()
	{
		fMaskAlpha = 0.8f;
	}

	protected override void Awake()
	{
		base.Awake();
		DispatcherMgr.On<int, int>("Update" + PropID.Magnet, _RefreshProp);
	}

	public void Show()
	{
		int num = DB.User.Curlv + 1;
		titleTex.SetLKey("Language29", string.Format(":{0}", num));
		int levelHardLevel = LevelMgr.GetLevelHardLevel();
		ChangeOutlineOrShadowColor component = titleTex.GetComponent<ChangeOutlineOrShadowColor>();
		Sprite sprite;
		Sprite sprite2;
		switch (levelHardLevel)
		{
		case 1:
			sprite = bgSprites[1];
			sprite2 = startButtonSprites[1];
			component.ChangeColor(1);
			startButtonValue.ChangeColor(1);
			break;
		case 2:
			sprite = bgSprites[2];
			sprite2 = startButtonSprites[2];
			component.ChangeColor(2);
			startButtonValue.ChangeColor(2);
			break;
		default:
			sprite = bgSprites[0];
			sprite2 = startButtonSprites[0];
			component.ChangeColor(0);
			startButtonValue.ChangeColor(0);
			break;
		}
		bgImg.sprite = sprite;
		startButtonImg.sprite = sprite2;
		LevelMgr.GetLevelStatus(out var isRapidly, out var isRegenerate);
		Sprite sprite3;
		if (isRapidly)
		{
			sprite3 = iconSprites[0];
		}
		else if (isRegenerate)
		{
			sprite3 = iconSprites[1];
		}
		else
		{
			sprite3 = null;
		}
		iconImg.gameObject.SetActive((bool)sprite3);
		if ((bool)sprite3)
		{
			iconImg.sprite = sprite3;
		}
		bool flag = BoosterMgr.I.JudgeUnlock();
		boosterObj.SetActive(flag);
		float num2 = (flag ? 44.5f : 50.5f);
		vecOrign = Vector3.up * num2;
		tfRoot.SetHeight(flag ? 805f : 553f);
		tfRoot.SetLocPosY(vecOrign.y);
		if (flag)
		{
			BoosterMgr.I.GetProgress(out var cur, out var max);
			barTex.text = string.Format("{0}/{1}", cur, max);
			boosterCanUseIcon.SetActive(max <= cur);
			boosterNotCanUseIcon.SetActive(cur < max);
			barImg.fillAmount = (float)cur / (float)max;
			isUseBooster = 0 < cur;
		}
		propUnlockObj.SetActive(cf_const.magnetUnlockLv <= num);
		propLockObj.SetActive(num < cf_const.magnetUnlockLv);
		unlockLvTex.SetLKey("Language29", string.Format("\n{0}", num));
		if (num < cf_const.magnetUnlockLv)
		{
			selectBtn.interactable = false;
		}
		else
		{
			int prop = DB.Bag.GetProp(PropID.Magnet);
			if (DB.User.IsUseMagnet && 0 < prop)
			{
				isUseMagnet = true;
			}
			_InitPropUI(prop, isSetNum: true);
		}
		if (GSingleton<RegionRankActivityManager>.ins.GetChallengeCount() < 1)
		{
			normalButtonObject.SetActive(value: true);
			regionRankButtonObject.SetActive(value: false);
		}
		else
		{
			normalButtonObject.SetActive(value: false);
			regionRankButtonObject.SetActive(value: true);
			crownCountTxt.text = "x" + GSingleton<RegionRankActivityManager>.ins.GetWinningStreak();
		}
		_Open();
	}

	public void ClickHelp(MButton btn)
	{
		// thân inline của BoosterHelpDialog.Show()
		SceneMgr.AddTopNode<BoosterHelpDialog>("Prefabs/BoosterHelpDialog").Show();
	}

	public void ClickSelect(MButton btn)
	{
		if (DB.Bag.GetProp(PropID.Magnet) != 0)
		{
			isUseMagnet = !isUseMagnet;
			propNumObj.SetActive(!isUseMagnet);
			propSelected.SetActive(isUseMagnet);
			return;
		}
		BuyPropDialog buyPropDialog = SceneMgr.AddTopNode<BuyPropDialog>("Prefabs/BuyPropDialog");
		buyPropDialog.func_callback_adv = delegate
		{
			DB.Bag.Modify(PropID.Magnet, 1, b_update: true, "Game");
		};
		buyPropDialog.func_callback_close = delegate(bool isTip)
		{
			bool flag = GiftDataMgr.ins.TryPopGiftImmediately(GiftTriggerType.MagnetProp, null, false);
			if (isTip && !flag)
			{
				Global.CreateMsgBoxDialog("Language53");
			}
		};
		buyPropDialog.Show(PropID.Magnet, REWARD_PLACE.AdMagnet, isPause: false);
	}

	public void ClickStart(MButton btn)
	{
		btn.interactable = false;
		_Close(delegate
		{
			if (cback != null)
			{
				cback(isUseBooster, isUseMagnet);
			}
		});
	}

	private void _InitPropUI(int num = 0, bool isSetNum = false)
	{
		if (isSetNum)
		{
			propNumTex.text = num.ToString();
		}
		propNumObj.SetActive(!isUseMagnet);
		propSelected.SetActive(isUseMagnet);
	}

	private void _RefreshProp(int last, int end)
	{
		if (propTw != null)
		{
			propTw.Kill();
			propTw = null;
		}
		if (DB.User.IsUseMagnet && end > 0)
		{
			isUseMagnet = true;
		}
		_InitPropUI();
		propTw = TweenMgr.NumberTo(0.2f, last, end, delegate(int va)
		{
			propNumTex.text = va.ToString();
		});
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (propTw != null)
		{
			propTw.Kill();
			propTw = null;
		}
		DispatcherMgr.Off<int, int>("Update" + PropID.Magnet, _RefreshProp);
	}
}
