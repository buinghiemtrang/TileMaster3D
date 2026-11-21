using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TurntableDialog : DialogBase
{
	[SerializeField]
	private MButton closeButton;

	[SerializeField]
	private MButton adButton;

	[SerializeField]
	private TurntableView view;

	[SerializeField]
	private TurntableRotateControl rotateControl;

	[SerializeField]
	private TurntableRewardItem turntableRewardItem;

	[SerializeField]
	private Transform shakeRootTf;

	[SerializeField]
	private TimeBox timeBox;

	[SerializeField]
	private Text adCountText;

	private List<cfgVideoWheelReward> _configs;

	private readonly List<TurntableItemInfo> _turntableItemInfos = new List<TurntableItemInfo>();

	private readonly List<Transform> _props = new List<Transform>();

	public Action<bool> OnCloseCallback;

	[SerializeField]
	private VerticalLayoutGroup vertical;

	private Vector3 flyItemEndPos;

	public bool isTestMode;

	private bool _isInLevel;

	private bool _isWatchAd;

	private int _freeTimes;

	public int BoxLevel { get; set; }

	private void Start()
	{
		turntableRewardItem.Hide();
		view.Init(rotateControl.isCenter, rotateControl.areaNumber);
		SetTurntableRewardsUI();
		_Open();
		timeBox.StartCD((int)(Singleton<TurntableManager>.I.EndTime - DateTime.Now).TotalSeconds, CloseDialog);
		closeButton.onClick.AddListener(ClickClose);
		adButton.onClick.AddListener(OnAdButtonClick);
		flyItemEndPos = turntableRewardItem.iconImg.transform.position;
		vertical.enabled = true;
		RefreshAdText();
	}

	private void SetTurntableRewardsUI()
	{
		_configs = DataCfg.cfgVideoWheelRewards;
		_turntableItemInfos.Clear();
		for (int i = 0; i < _configs.Count; i++)
		{
			cfgVideoWheelReward cfgVideoWheelReward = _configs[i];
			TurntableItemInfo turntableItemInfo = new TurntableItemInfo();
			turntableItemInfo.areaId = i + 1;
			turntableItemInfo.isSelect = false;
			turntableItemInfo.reward = cfgVideoWheelReward.rewards;
			turntableItemInfo.weight = cfgVideoWheelReward.weight;
			turntableItemInfo.limit = cfgVideoWheelReward.limit;
			if (DB.Turntable.CurrentGetRewardIds.Contains(cfgVideoWheelReward.id))
			{
				turntableItemInfo.isSelect = true;
			}
			_turntableItemInfos.Add(turntableItemInfo);
		}
		view.Refresh(_turntableItemInfos);
	}

	private void RefreshAdText()
	{
		adCountText.text = string.Format("({0}/{1})", Singleton<TurntableManager>.I.LeftCount, cf_ads_const.SpinsPerVideoLimit);
	}

	private void CloseDialog()
	{
		ClickClose(null);
	}

	private void ClickClose(MButton button)
	{
		closeButton.interactable = false;
		adButton.interactable = false;
		_Close(delegate
		{
			OnCloseCallback?.Invoke(_isWatchAd);
		});
	}

	private void ReopenClick()
	{
		closeButton.interactable = true;
		adButton.interactable = Singleton<TurntableManager>.I.LeftCount > 0 || isTestMode;
	}

	private void BlockClick()
	{
		closeButton.interactable = false;
		adButton.interactable = false;
	}

	private void OnAdButtonClick(MButton button)
	{
		turntableRewardItem.Hide();
		Global.ShowRewardedVideo(delegate(string state)
		{
			if (state == "succ")
			{
				BlockClick();
				_isWatchAd = true;
				int rewardCount = Singleton<TurntableManager>.I.RewardCount;
				Singleton<TurntableManager>.I.AddWatchCount();
				List<TurntableItemInfo> itemInfos = _turntableItemInfos.FindAll((TurntableItemInfo item) => item.limit <= rewardCount);
				DoTurntableRotate(true, itemInfos);
			}
			else
			{
				Global.ShowMsgTip("Language135");
				ReopenClick();
				CheckAllComplete();
			}
		}, REWARD_PLACE.AdWheelReward);
	}

	private void DoTurntableRotate(bool isAd, List<TurntableItemInfo> itemInfos)
	{
		if (itemInfos.Count <= 0)
		{
			ReopenClick();
			Debug.LogError("满足条件对象不足");
			return;
		}
		AudioMgr.I.PlaySound("turntable");
		int num = RandomAreaByRule(false, itemInfos);
		int index = Mathf.Clamp(num - 1, 0, _configs.Count - 1);
		cfgVideoWheelReward cfgVideoWheelReward = _configs[index];
		DB.Bag.Modify((PropID)cfgVideoWheelReward.rewards[0, 0], cfgVideoWheelReward.rewards[0, 1], true, "LevelTurntable");
		LogEvent.I.AdLuckySpin_Reward(cfgVideoWheelReward.rewards);
		view.isRotating = true;
		rotateControl.AddCompletedCallback(isAd, RotateCompleted);
		rotateControl.Rotate(num);
	}

	private void RotateCompleted(bool isAd, int areaId)
	{
		view.SetSelect(areaId);
		view.isRotating = false;
		int index = Mathf.Clamp(areaId - 1, 0, _configs.Count - 1);
		cfgVideoWheelReward cfgVideoWheelReward = _configs[index];
		Global.CreateReward(cfgVideoWheelReward.rewards, RewardType.None, REWARD_PLACE.AdWheelReward, "", false, true);
		ReopenClick();
		RefreshAdText();
		SetRewardItemSelect(index);
	}

	private void SetRewardItemSelect(int index)
	{
		if (index >= 0 && _turntableItemInfos != null && _turntableItemInfos.Count > index)
		{
			DB.Turntable.CurrentGetRewardIds.Add(_turntableItemInfos[index].reward[0, 0]);
			_turntableItemInfos[index].isSelect = true;
		}
	}

	private void FlyCompleted(Transform prop, int[] values)
	{
		closeButton.interactable = true;
		adButton.interactable = true;
		turntableRewardItem.Show(values[0], values[2]);
		DB.Bag.ModifyShow((PropID)values[0], values[2]);
		_props.Remove(prop);
		UnityEngine.Object.Destroy(prop.gameObject);
		CheckAllComplete();
	}

	private void CheckAllComplete()
	{
		if (!_turntableItemInfos.Any((TurntableItemInfo a) => !a.isSelect))
		{
			adButton.gameObject.SetActive(false);
			closeButton.gameObject.SetActive(true);
		}
	}

	private int RandomAreaByRule(bool isAd, List<TurntableItemInfo> itemInfos)
	{
		int num = itemInfos.Weight((TurntableItemInfo a) => a.weight);
		DebugWeights(itemInfos);
		Debug.Log(string.Format("--------------------------index: {0}", num));
		return itemInfos[num].areaId;
	}

	private void DebugWeights(List<TurntableItemInfo> itemInfos)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < itemInfos.Count; i++)
		{
			stringBuilder.Append(string.Format("{0}|{1},limit: {2};", itemInfos[i].areaId, itemInfos[i].weight, itemInfos[i].limit));
		}
	}

	private new void OnDestroy()
	{
		shakeRootTf.DOKill();
		for (int i = _props.Count - 1; i >= 0; i--)
		{
			Transform transform = _props[i];
			if (transform != null)
			{
				transform.DOKill();
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
		_props.Clear();
	}
}
