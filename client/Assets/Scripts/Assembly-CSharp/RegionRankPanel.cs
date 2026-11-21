using System;
using DG.Tweening;
using FunctionEject;
using UnityEngine;
using UnityEngine.UI;

public class RegionRankPanel : MonoBehaviour
{
	[SerializeField]
	private MButton helpBtn;

	[SerializeField]
	private Text ttTime;

	[SerializeField]
	private DialogShow dialog;

	[SerializeField]
	private RegionRankPanelScroll scroll;

	[SerializeField]
	private GameObject noNational;

	[SerializeField]
	private GameObject noNetwork;

	[SerializeField]
	private GameObject networkLoading;

	[SerializeField]
	private GameObject noData;

	[SerializeField]
	private Toggle personalToggle;

	[SerializeField]
	private Toggle nationalToggle;

	[SerializeField]
	private RegionRankPanelItem selfItem;

	[SerializeField]
	private GameObject personalBg;

	[SerializeField]
	private GameObject nationalBg;

	private bool _isFirstEnter;

	private Action _endCall;

	private bool _personalLog;

	private bool _nationalLog;

	private RegionRankType _currentRegionRankType = RegionRankType.Personanl;

	private void Start()
	{
		helpBtn.onClick.AddListener(delegate
		{
			SceneMgr.AddTopNode<DialogShow>("Prefabs/RegionRankHelpPanel");
		});
		int activityEndSeconds = GSingleton<RegionRankActivityManager>.ins.GetActivityEndSeconds();
		if (activityEndSeconds < 1)
		{
			ttTime.text = "00:00:00";
		}
		else
		{
			ttTime.DOCounter(activityEndSeconds, 0, activityEndSeconds, Util.Helper.FormatTimerD).OnComplete(delegate
			{
				dialog.ClickClose();
			});
		}
		personalToggle.onValueChanged.AddListener(PersonalShow);
		nationalToggle.onValueChanged.AddListener(NationalShow);
		PersonalShow(true);
	}

	private void PersonalShow(bool isOn)
	{
		if (isOn)
		{
			_currentRegionRankType = RegionRankType.Personanl;
			personalBg.SetActive(true);
			nationalBg.SetActive(false);
			networkLoading.SetActive(true);
			noNational.gameObject.SetActive(false);
			scroll.gameObject.SetActive(false);
			noNetwork.SetActive(false);
			selfItem.SetActive(false);
			noData.SetActive(false);
			Singleton<RegionRankServerManager>.I.ForceAbort();
			Singleton<RegionRankServerManager>.I.GetPersonalRankData(delegate(bool succ, RegionRankClientData data)
			{
				if (this == null || !base.gameObject || _currentRegionRankType != RegionRankType.Personanl)
				{
					return;
				}
				networkLoading.SetActive(false);
				noNetwork.SetActive(!succ);
				if (succ)
				{
					scroll.gameObject.SetActive(true);
					SetSelfItem(data, RegionRankType.Personanl);
					if (data.List.Count < 1)
					{
						scroll.gameObject.SetActive(false);
						noData.SetActive(true);
					}
					else
					{
						scroll.RefreshItems(data.List, RegionRankType.Personanl);
					}
				}
			});
		}
	}

	private void NationalShow(bool isOn)
	{
		if (!isOn)
		{
			return;
		}
		_currentRegionRankType = RegionRankType.National;
		personalBg.SetActive(false);
		nationalBg.SetActive(true);
		noData.SetActive(false);
		Singleton<RegionRankServerManager>.I.ForceAbort();
		if (GSingleton<RegionRankActivityManager>.ins.IsUnknownRegion())
		{
			noNational.gameObject.SetActive(true);
			networkLoading.SetActive(false);
			scroll.gameObject.SetActive(false);
			noNetwork.SetActive(false);
			selfItem.SetActive(false);
			if (!_nationalLog)
			{
				_nationalLog = true;
				LogEvent.I.Peak_Match_Leaderboard_Display(RegionRankType.National, -1);
			}
			return;
		}
		networkLoading.SetActive(true);
		noNational.gameObject.SetActive(false);
		scroll.gameObject.SetActive(false);
		noNetwork.SetActive(false);
		selfItem.SetActive(false);
		Singleton<RegionRankServerManager>.I.GetNationalRankData(delegate(bool succ, RegionRankClientData data)
		{
			if (this == null || !base.gameObject || _currentRegionRankType != RegionRankType.National)
			{
				return;
			}
			networkLoading.SetActive(false);
			noNetwork.SetActive(!succ);
			if (succ)
			{
				scroll.gameObject.SetActive(true);
				SetSelfItem(data, RegionRankType.National);
				if (data.List.Count < 1)
				{
					scroll.gameObject.SetActive(false);
					noData.SetActive(true);
				}
				else
				{
					scroll.RefreshItems(data.List, RegionRankType.National);
				}
			}
		});
	}

	private void SetSelfItem(RegionRankClientData data, RegionRankType rankType)
	{
		selfItem.gameObject.SetActive(true);
		if (rankType == RegionRankType.Personanl)
		{
			RegionRankClientItemData regionRankClientItemData = new RegionRankClientItemData();
			RegionRankExtraData regionRankExtraData = new RegionRankExtraData();
			regionRankExtraData.name = DB.Account.GetName();
			regionRankExtraData.medal = DB.User.Medal;
			regionRankClientItemData.Data = regionRankExtraData;
			regionRankClientItemData.Key = DB.Account.Userid;
			regionRankClientItemData.Score = GSingleton<RegionRankActivityManager>.ins.GetSelfData().Score;
			selfItem.Init(regionRankClientItemData, data.SelfRank, RegionRankType.Personanl);
			if (!_personalLog)
			{
				_personalLog = true;
				LogEvent.I.Peak_Match_Leaderboard_Display(RegionRankType.Personanl, data.SelfRank);
			}
		}
		else
		{
			RegionRankClientItemData regionRankClientItemData2 = data.List.Find((RegionRankClientItemData a) => a.Key == GSingleton<RegionRankActivityManager>.ins.GetRegionCode());
			if (regionRankClientItemData2 == null)
			{
				selfItem.gameObject.SetActive(false);
			}
			else
			{
				selfItem.Init(regionRankClientItemData2, data.SelfRank, rankType);
			}
			if (!_nationalLog)
			{
				_nationalLog = true;
				LogEvent.I.Peak_Match_Leaderboard_Display(rankType, data.SelfRank);
			}
		}
	}

	private void OnDestroy()
	{
		ttTime.DOKill();
		if (_isFirstEnter)
		{
			if (EjectMgr.I != null)
			{
				EjectMgr.I.Next(EjectType.Break);
			}
			LevelStartButton.PlayRegionRankAnim();
		}
		_endCall?.Invoke();
		Singleton<RegionRankServerManager>.I.ForceAbort();
	}

	public static RegionRankPanel Create(Action endcall, bool isFirstEnter = false)
	{
		RegionRankPanel regionRankPanel = SceneMgr.AddTopNode<RegionRankPanel>("Prefabs/RegionRankPanel");
		regionRankPanel._endCall = (Action)Delegate.Combine(regionRankPanel._endCall, endcall);
		regionRankPanel._isFirstEnter = isFirstEnter;
		return regionRankPanel;
	}
}
