using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpecialBlockRewardDialog : DialogBase
{
	[SerializeField]
	private LeftRightMoveAdsCtl _leftRightMoveAdsCtl;

	[SerializeField]
	private Text _adMultipleCountTexT;

	[SerializeField]
	private MButton _getBtn;

	[SerializeField]
	private MButton _adMultipleBtn;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private RewardItem _rewardItem;

	private Action _callBack;

	private float _adMultiple;

	private int _rewardID;

	private int _rewardCount;

	private int _adAddValue;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		DispatcherMgr.Emit("GamePause", new object[] { false, "SpecialBlockReward" });
		DispatcherMgr.Emit("GetRewardByUserLevel", new object[]
		{
			_rewardID,
			_adAddValue + _rewardCount,
			_callBack
		});
	}

	private void Show(string rewards, Action callBack)
	{
		_Open();
		DispatcherMgr.Emit("GamePause", new object[] { true, "SpecialBlockReward" });
		_callBack = callBack;
		int[,] array = rewards.ToInt(',', '|');
		if (array.Length < 1)
		{
			return;
		}
		_rewardID = array[0, 0];
		_rewardCount = array[0, 1];
		_rewardItem.SetInfo(_rewardID, _rewardCount, false);
		_icon.sprite = Resources.Load<Sprite>(string.Format("Props/prop{0}", _rewardID));
		_leftRightMoveAdsCtl.cback = delegate(int va)
		{
			_adMultiple = va;
			_adMultipleCountTexT.text = string.Format("+{0}", _rewardCount * va);
		};
		_getBtn.onClick.AddListener(OnGetButtonClick);
		_adMultipleBtn.onClick.AddListener(OnMultipleButtonClick);
		DB.Bag.Modify((PropID)_rewardID, _rewardCount, false, "LevelRewardItem");
	}

	private void OnMultipleButtonClick(MButton button)
	{
		if (Singleton<AdManager>.I.IsRewardVideoOk(false))
		{
			_leftRightMoveAdsCtl.SetStop();
			_getBtn.interactable = false;
			_adMultipleBtn.interactable = false;
			StopAllCoroutines();
			StartCoroutine(IEDelayResumeButton());
		}
		Global.ShowRewardedVideo(delegate(string state)
		{
			if (state == "succ")
			{
				_adAddValue = ((int)_adMultiple - 1) * _rewardCount;
				if (_adAddValue > 0)
				{
					DB.Bag.Modify((PropID)_rewardID, _adAddValue, false, "LevelRewardItem");
				}
				_Close();
			}
			else
			{
				_getBtn.interactable = true;
				_adMultipleBtn.interactable = true;
			}
		}, REWARD_PLACE.AdRewardItem);
	}

	private IEnumerator IEDelayResumeButton()
	{
		yield return new WaitForSeconds(0.5f);
		_getBtn.interactable = true;
		_adMultipleBtn.interactable = true;
	}

	private void OnGetButtonClick(MButton button)
	{
		_getBtn.interactable = false;
		_adMultipleBtn.interactable = false;
		_Close();
	}

	public static void Create(string rewards, Action callBack)
	{
		SceneMgr.AddTopNode<SpecialBlockRewardDialog>("SpecialBlockRewardDialog").Show(rewards, callBack);
	}
}
