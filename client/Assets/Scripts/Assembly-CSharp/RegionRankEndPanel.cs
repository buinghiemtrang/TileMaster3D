using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class RegionRankEndPanel : MonoBehaviour
{
	[SerializeField]
	private MButton btnClose;

	[SerializeField]
	private DialogShow dialog;

	[SerializeField]
	private RegionRankEndPanelItem personalItem;

	[SerializeField]
	private RegionRankEndPanelItem nationalItem;

	[SerializeField]
	private GameObject noNetwork;

	[SerializeField]
	private GameObject networkLoading;

	[SerializeField]
	private MButton rewardButton;

	[SerializeField]
	private MButton okButton;

	[SerializeField]
	private GameObject nationalRewardObj;

	[SerializeField]
	private Text nationalTip;

	private RegionRankClientItemData _personalItemData;

	private RegionRankClientItemData _nationalItemData;

	private int _personalRanking;

	private int _nationalRanking;

	private int _loadCount;

	private cfgRegionRankReward _personalRewardConfig;

	private cfgRegionRankReward _nationalRewardConfig;

	private bool _isUnknownRegion;

	private static bool IsGm;

	private Action _endCall;

	private void Awake()
	{
		btnClose.onClick.AddListener(ClickClose);
		rewardButton.onClick.AddListener(ClickClaim);
		okButton.onClick.AddListener(ClickNoReward);
	}

	private void Start()
	{
		networkLoading.SetActive(true);
		noNetwork.SetActive(false);
		personalItem.gameObject.SetActive(false);
		nationalItem.gameObject.SetActive(false);
		btnClose.gameObject.SetActive(false);
		rewardButton.gameObject.SetActive(false);
		okButton.gameObject.SetActive(false);
		_isUnknownRegion = GSingleton<RegionRankActivityManager>.ins.IsUnknownRegion();
		_loadCount = ((!_isUnknownRegion) ? 2 : 1);
		Singleton<RegionRankServerManager>.I.GetPersonalRankData(delegate(bool succ, RegionRankClientData data)
		{
			if (succ)
			{
				RegionRankClientItemData regionRankClientItemData = new RegionRankClientItemData();
				RegionRankExtraData regionRankExtraData = new RegionRankExtraData();
				regionRankExtraData.name = DB.Account.GetName();
				regionRankExtraData.medal = DB.User.Medal;
				regionRankClientItemData.Data = regionRankExtraData;
				regionRankClientItemData.Key = DB.Account.Userid;
				regionRankClientItemData.Score = GSingleton<RegionRankActivityManager>.ins.GetSelfData().Score;
				_personalItemData = regionRankClientItemData;
				_personalRanking = data.SelfRank;
			}
			_loadCount--;
			if (_loadCount == 0)
			{
				SetInfo();
			}
		}, false);
		if (!_isUnknownRegion)
		{
			Singleton<RegionRankServerManager>.I.GetNationalRankData(delegate(bool succ, RegionRankClientData data)
			{
				if (succ)
				{
					_nationalItemData = data.List.Find((RegionRankClientItemData a) => a.Key == GSingleton<RegionRankActivityManager>.ins.GetRegionCode());
					_nationalRanking = data.SelfRank;
					if (_nationalItemData == null)
					{
						_isUnknownRegion = true;
					}
				}
				_loadCount--;
				if (_loadCount == 0)
				{
					SetInfo();
				}
			}, false);
		}
	}

	private void SetInfo()
	{
		networkLoading.SetActive(false);
		if (_personalItemData == null || (!_isUnknownRegion && _nationalItemData == null))
		{
			noNetwork.SetActive(true);
			btnClose.gameObject.SetActive(true);
			return;
		}
		personalItem.gameObject.SetActive(true);
		personalItem.Init(_personalItemData, _personalRanking, RegionRankType.Personanl);
		_personalRewardConfig = personalItem.RewardConfig;
		if (!_isUnknownRegion)
		{
			nationalItem.gameObject.SetActive(true);
			nationalItem.Init(_nationalItemData, _nationalRanking, RegionRankType.National);
			_nationalRewardConfig = nationalItem.RewardConfig;
		}
		bool flag;
		if (_nationalRewardConfig == null)
		{
			flag = false;
			nationalRewardObj.SetActive(false);
		}
		else
		{
			flag = _personalItemData.Score >= cf_regionRank_const.NationalRewardMinScore;
			if (flag)
			{
				nationalRewardObj.SetActive(false);
			}
			else
			{
				nationalRewardObj.SetActive(true);
				_nationalRewardConfig = null;
			}
		}
		MButton mButton;
		if (flag || _personalRewardConfig != null)
		{
			mButton = rewardButton;
		}
		else
		{
			if (!IsGm)
			{
				GSingleton<RegionRankActivityManager>.ins.ActivityEnd(_personalRanking);
			}
			mButton = okButton;
		}
		mButton.gameObject.SetActive(true);
		LogEvent.I.Peak_Match_Event_End(_personalRanking, (!_isUnknownRegion) ? _nationalRanking : (-1));
	}

	private void ClickClose(MButton button)
	{
		_endCall?.Invoke();
		dialog.ClickClose();
	}

	private void ClickNoReward(MButton button)
	{
		if (!IsGm)
		{
			GSingleton<RegionRankActivityManager>.ins.ActivityEnd(_personalRanking);
		}
		_endCall?.Invoke();
		dialog.ClickClose();
	}

	private void ClickClaim(MButton button)
	{
		if (!IsGm)
		{
			GSingleton<RegionRankActivityManager>.ins.ActivityEnd(_personalRanking);
		}
		cfgRegionRankReward cfgRegionRankReward = MergeReward();
		Global.CreateReward(cfgRegionRankReward.Reward, (cfgRegionRankReward.RankType == 1) ? RewardType.RegionRankPersonalChest : RewardType.RegionRankNationalChest, REWARD_PLACE.None, "PeakTreasureChest", false, false).SetChestSkin(cfgRegionRankReward.ChestSkin).SetComplete(delegate
		{
			_endCall?.Invoke();
		});
		dialog.ClickClose();
	}

	private cfgRegionRankReward MergeReward()
	{
		cfgRegionRankReward nationalRewardConfig = _nationalRewardConfig;
		StringBuilder sb = null;
		if (_personalRewardConfig == null)
		{
			if (nationalRewardConfig != null)
			{
				return nationalRewardConfig;
			}
		}
		else if (nationalRewardConfig == null)
		{
			return _personalRewardConfig;
		}
		cfgRegionRankReward cfgRegionRankReward = new cfgRegionRankReward();
		cfgRegionRankReward.RankType = _nationalRewardConfig.RankType;
		cfgRegionRankReward.ChestSkin = ((_nationalRanking <= _personalRanking) ? _nationalRewardConfig : _personalRewardConfig).ChestSkin;
		sb = new StringBuilder();
		Add(_personalRewardConfig);
		sb.Append(",");
		Add(_nationalRewardConfig);
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		string[] array = sb.ToString().Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			int[] array2 = array[i].ToInt('|');
			if (!dictionary.ContainsKey(array2[0]))
			{
				dictionary[array2[0]] = 0;
			}
			dictionary[array2[0]] += array2[1];
		}
		sb.Clear();
		foreach (KeyValuePair<int, int> item in dictionary)
		{
			sb.Append(item.Key);
			sb.Append('|');
			sb.Append(item.Value);
			sb.Append(',');
		}
		string text = sb.ToString();
		cfgRegionRankReward.Reward = text.Substring(0, text.Length - 1).ToInt(2, ',', '|');
		return cfgRegionRankReward;

		void Add(cfgRegionRankReward config)
		{
			if (config != null && config.Reward != null)
			{
				int length = config.Reward.GetLength(0);
				int length2 = config.Reward.GetLength(1);
				for (int j = 0; j < length; j++)
				{
					for (int k = 0; k < length2; k++)
					{
						sb.Append(config.Reward[j, k]);
						if (k != length2 - 1)
						{
							sb.Append("|");
						}
					}
					if (j != length - 1)
					{
						sb.Append(",");
					}
				}
			}
		}
	}

	public static RegionRankEndPanel Create(Action endCall, bool isGM = false)
	{
		IsGm = isGM;
		RegionRankEndPanel regionRankEndPanel = SceneMgr.AddTopNode<RegionRankEndPanel>("Prefabs/RegionRankEndPanel");
		regionRankEndPanel._endCall = endCall;
		return regionRankEndPanel;
	}
}
