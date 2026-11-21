using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UseLevelRewardDialog : DialogBase
{
	private class RewardInfo
	{
		public int Id;

		public int Value;

		public UserLevelRewardItem Item;
	}

	[SerializeField]
	private Transform tfPropContent;

	[SerializeField]
	private MButton _btnContinue;

	[SerializeField]
	private MButton _btnAd;

	[SerializeField]
	private UserLevelRewardItem _rewardItem;

	[SerializeField]
	protected CanvasGroup canvasGroupBtn;

	[SerializeField]
	protected CanvasGroup canvasGroupTitle;

	[SerializeField]
	private Image Icon;

	private bool _showRewardAd;

	private List<RewardInfo> _rewardItems = new List<RewardInfo>();

	private Dictionary<int, int> _rewarDictionary;

	private Action _closeCallback;

	public UseLevelRewardDialog()
	{
		fMaskAlpha = 0.8f;
	}

	public UseLevelRewardDialog AddCloseCallback(Action action)
	{
		_closeCallback = action;
		return this;
	}

	protected override void Awake()
	{
		base.Awake();
		_btnAd.onClick.AddListener(ClickBtnReceive);
		_btnContinue.onClick.AddListener(ClickBtnContinue);
		canvasGroupBtn.alpha = 0f;
		canvasGroupBtn.blocksRaycasts = false;
		canvasGroupTitle.alpha = 0f;
		DispatcherMgr.Emit("GamePause", true, "UserLevelReward");
	}

	public void Show()
	{
		ShowMask();
		_PlayPropItemMoveAni(0f);
		_ShowCanvasGroup(0f);
		DB.Bag.Modify((PropID)_rewardItems[0].Id, _rewardItems[0].Value, b_update: false, "UserLevelReward");
	}

	private void _PlayPropItemMoveAni(float delay)
	{
		if (_rewardItems.Count == 0)
		{
			return;
		}
		foreach (RewardInfo rewardItem in _rewardItems)
		{
			rewardItem.Item.transform.DOLocalMove(Vector3.zero, 0.3f).SetDelay(delay).From();
			rewardItem.Item.transform.DOScale(1f, 0.3f).SetDelay(delay);
		}
	}

	private void PlayItemMoveAnimation(float delay)
	{
		_PlayPropItemMoveAni(delay);
		_ShowCanvasGroup(delay);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		DispatcherMgr.Emit("GamePause", false, "UserLevelReward");
		DispatcherMgr.Emit("GetRewardByUserLevel", _rewardItems[0].Id, _btnAd.interactable ? (_rewardItems[0].Value << 1) : _rewardItems[0].Value, _closeCallback);
	}

	private void _ShowCanvasGroup(float delay)
	{
		DOTweenModuleExtend.DelayCall(delay + 0.3f, delegate
		{
			canvasGroupBtn.DOFade(1f, 0.2f);
			canvasGroupBtn.blocksRaycasts = true;
		});
		canvasGroupTitle.DOFade(1f, delay + 0.3f);
	}

	private void ClickBtnReceive(MButton button)
	{
		_btnAd.interactable = false;
		Global.ShowRewardedVideo(delegate(string info)
		{
			if (info != null)
			{
				if (info.Equals("succ"))
				{
					_showRewardAd = true;
					DB.Bag.Modify((PropID)_rewardItems[0].Id, _rewardItems[0].Value, b_update: false, "UserLevelReward");
					Receive();
				}
				else
				{
					_btnAd.interactable = true;
				}
			}
		}, REWARD_PLACE.AdUserLevel);
		StopAllCoroutines();
		StartCoroutine(IEDelayResumeButton());
	}

	private IEnumerator IEDelayResumeButton()
	{
		yield return new WaitForSeconds(0.5f);
		_btnAd.interactable = true;
	}

	private void ClickBtnContinue(MButton button)
	{
		_btnContinue.interactable = false;
		if (!_showRewardAd && DB.User.Curlv >= cfgUserLevel_const.UserInterstitialUnlockLevel)
		{
			Global.ShowUserLevelInterstitial(delegate(string info)
			{
				if (info.Equals("succ"))
				{
					DispatcherMgr.Emit("GameScene_User_Level_Reward_Play_Interstitial_Success");
				}
				Receive();
			}, INTER_PLACE.UserLevelInterstitial);
		}
		else
		{
			Receive();
		}
	}

	private void Receive()
	{
		Singleton<SpecialBlockRewardManager>.I.ResetCd();
		Singleton<UserLevelRewardManager>.I.UpdateStage();
		_Close();
	}

	public UseLevelRewardDialog SetInfo(string rewards)
	{
		return SetInfo(rewards.ToInt(2, ',', '|'));
	}

	private UseLevelRewardDialog SetInfo(int[,] rewards)
	{
		for (int i = 0; i < rewards.GetLength(0); i++)
		{
			int num = rewards[i, 0];
			if (num != 20 || !DB.User.AdFree)
			{
				RewardInfo rewardInfo = new RewardInfo();
				rewards[i, 1] = Math.Abs(rewards[i, 1]);
				rewardInfo.Id = num;
				rewardInfo.Value = rewards[i, 1];
				_rewardItems.Add(rewardInfo);
			}
		}
		InitItems();
		return this;
	}

	private void InitItems()
	{
		if (_rewardItems.Count != 0)
		{
			Icon.sprite = Resources.Load<Sprite>(string.Format("Props/prop{0}", _rewardItems[0].Id));
			_rewardItems[0].Item = _rewardItem;
			_rewardItem.Init(_rewardItems[0].Id, _rewardItems[0].Value);
		}
	}
}
