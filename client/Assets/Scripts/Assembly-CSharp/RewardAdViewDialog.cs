using System;
using System.Collections.Generic;
using DG.Tweening;
using RewardAdView;
using RoyalPass;
using UnityEngine;
using UnityEngine.UI;

public class RewardAdViewDialog : MonoBehaviour
{
	public Image ProcessBar;

	public Text _timeText;

	public GameObject TimePart;

	public MButton WatchBtn;

	public GameObject GreyBtn;

	public List<RewardAdViewItem> toggles;

	private int _leftTime;

	private int _skeletonIndex;

	private DialogShow _dialog;

	private RoyalPassActivityRewardTip rewardBubble;

	public DialogShow Dialog => _dialog;

	private void OnShow()
	{
		_dialog = GetComponent<DialogShow>();
		WatchBtn.onClick.AddListener(WatchRewardAd);
		StartCD();
		RefreshUI();
	}

	private void StartCD()
	{
		_timeText.DOKill(false);
		_leftTime = RewardAdViewManager.I.GetLeftTime();
		if (_leftTime > -1)
		{
			_timeText.DOCounter(_leftTime, 0, _leftTime, Util.Helper.FormatTimer).OnComplete(delegate
			{
				RewardAdViewManager.I.RefreshData();
				RefreshUI();
				StartCD();
			});
		}
		else
		{
			TimePart.SetActive(false);
		}
	}

	private void RefreshUI()
	{
		int maxCount = RewardAdViewManager.I.GetMaxCount();
		int currentCount = RewardAdViewManager.I.GetCurrentCount();
		int leftTime = RewardAdViewManager.I.GetLeftTime();
		WatchBtn.gameObject.SetActive(currentCount != maxCount && leftTime >= 1);
		GreyBtn.SetActive(currentCount == maxCount || leftTime < 1);
		ProcessBar.fillAmount = (float)currentCount / (float)maxCount;
		Vector2 sizeDelta = ProcessBar.rectTransform.sizeDelta;
		for (int i = 0; i < toggles.Count; i++)
		{
			RewardAdViewItem rewardAdViewItem = toggles[i];
			bool flag = RewardAdViewManager.I.HasReward(i);
			rewardAdViewItem.IndexBg.isOn = flag;
			rewardAdViewItem.RightIcon.SetActive(flag);
			cfgAccumlatedAdReward reward = RewardAdViewManager.I.GetReward(i);
			rewardAdViewItem.RewardTxt.gameObject.SetActive(!flag && reward.showChest == 0);
			rewardAdViewItem.RewardTxt.text = string.Format("x{0}", reward.rewards[0, 1]);
			if (reward.count < 0)
			{
				rewardAdViewItem.gameObject.SetActive(false);
			}
			else
			{
				rewardAdViewItem.gameObject.SetActive(true);
				rewardAdViewItem.CountTxt.text = reward.count.ToString();
				rewardAdViewItem.RT.anchoredPosition = new Vector2(sizeDelta.x * ((float)reward.count / (float)maxCount) - sizeDelta.x * 0.5f, 0f);
			}
		}
	}

	private void WatchRewardAd(MButton btn)
	{
		RewardAdViewManager.I.SeeRewardAd(OnComplete);
	}

	private void OnComplete(bool isSuccess)
	{
		if (isSuccess)
		{
			RefreshUI();
		}
	}

	private void OnDestroy()
	{
		if (_timeText != null)
		{
			_timeText.DOKill(false);
		}
		if (ProcessBar != null)
		{
			ProcessBar.DOKill(false);
		}
	}

	public static void Create(Action callback = null)
	{
		RewardAdViewDialog rewardAdViewDialog = SceneMgr.AddTopNode<RewardAdViewDialog>("UI/RewardAdViewDialog");
		rewardAdViewDialog.OnShow();
		rewardAdViewDialog._dialog.func_callback_close = callback;
	}

	public void ClickShowChest(MButton button)
	{
		int index = int.Parse(button.sMsg);
		cfgAccumlatedAdReward reward = RewardAdViewManager.I.GetReward(index);
		if (reward.showChest == 1)
		{
			rewardBubble = SceneMgr.AddTopNode<RoyalPassActivityRewardTip>("Prefabs/Activity/RoyalPass/RoyalPassActivityRewardTip");
			rewardBubble.yOffset = -50f;
			rewardBubble.xOffset = 0f;
			rewardBubble.ShowReward(button.transform, reward.rewards);
		}
	}
}
