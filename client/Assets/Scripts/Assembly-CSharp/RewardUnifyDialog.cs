using System;
using System.Collections.Generic;
using DG.Tweening;
using Festival;
using ShortBattlePass;
using Spine;
using Spine.Unity;
using UnityEngine;

public class RewardUnifyDialog : DialogBase
{
	protected struct RewInfo
	{
		public int originalReward;

		public bool bVipReward;

		public int propId;

		public int value;
	}

	protected string EventCountRewardPlayAni = "RewardUnifyDialog_CountPlayAni";

	protected string AnimationID_ItemAni = "RewardUnifyDialog.AnimationID_ItemAni";

	[SerializeField]
	protected RewardItem itemPrefab;

	[SerializeField]
	protected GridLayoutCenter pPropGridLayout;

	[SerializeField]
	protected MButton btnAds;

	[SerializeField]
	protected MButton btnSmallReceive;

	[SerializeField]
	protected MButton btnReceive;

	[SerializeField]
	protected Transform tfChestParent;

	[SerializeField]
	protected CanvasGroup canvasGroupBtn;

	[SerializeField]
	protected CanvasGroup canvasGroupTitle;

	protected Action<bool> _callback;

	protected readonly List<RewInfo> _rewInfos = new List<RewInfo>();

	protected REWARD_PLACE _place;

	protected RewardType _type;

	protected string _sSign = "";

	protected SkeletonGraphic _spineChest;

	protected bool _isPause;

	protected Spine.AnimationState.TrackEntryDelegate _spineTrackEntryDelegate;

	protected readonly List<CardSystemCards> _rewCards = new List<CardSystemCards>();

	protected bool bForcedCloseAdsBtn;

	public bool isShowOnly;

	protected override void Awake()
	{
		base.Awake();
		AnimationID_ItemAni += GetHashCode().ToString();
		btnReceive.onClick.AddListener(ClickBtnReceive);
		btnSmallReceive.onClick.AddListener(ClickBtnReceive);
		btnAds.onClick.AddListener(ClickBtnAdsReceive);
		itemPrefab.gameObject.SetActive(value: false);
		canvasGroupBtn.alpha = 0f;
		canvasGroupBtn.blocksRaycasts = false;
		canvasGroupTitle.alpha = 0f;
		EventCountRewardPlayAni = CounterMgr.Create(delegate
		{
			if ((bool)_spineChest)
			{
				PlayChestAnimation();
			}
			else
			{
				PlayItemMoveAnimation();
			}
		});
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		DOTween.Kill(AnimationID_ItemAni);
	}

	public void SetComplete(Action<bool> callback)
	{
		_callback = callback;
	}

	public void SetRewardType(RewardType type)
	{
		if (type == RewardType.None)
		{
			return;
		}
		if (type == RewardType.ActivityOne)
		{
			type = RewardType.RoyalPassActivity;
		}
		string text;
		if (type == RewardType.ActivityTwo || type == RewardType.RoyalPassActivity)
		{
			text = "{0}_" + type;
		}
		else if (type == RewardType.Chest || type == RewardType.Novice)
		{
			text = "{0}_Chest";
		}
		else
		{
			text = type.ToString();
		}
		_type = type;
		SkeletonGraphic skeletonGraphic = Singleton<FestivalMgr>.I.LoadAsset<SkeletonGraphic>("Chests/" + text);
		if ((bool)skeletonGraphic)
		{
			_spineChest = UnityEngine.Object.Instantiate(skeletonGraphic, tfChestParent, worldPositionStays: false);
		}
	}

	public RewardUnifyDialog SetChestSkin(string skinName)
	{
		if ((bool)_spineChest && !string.IsNullOrEmpty(skinName))
		{
			_spineChest.Skeleton.SetSkin(skinName);
			_spineChest.Skeleton.SetSlotsToSetupPose();
		}
		return this;
	}

	public void SetRewardPlace(REWARD_PLACE place, string sign = "")
	{
		_place = place;
		_sSign = sign;
	}

	public void SetInfo(string rewards, bool bShowAdFreeIcon = false)
	{
		SetInfo(rewards.ToInt(2, ',', '|'), bShowAdFreeIcon);
	}

	public void SetInfo(int[,] rewards, bool bShowAdFreeIcon = false)
	{
		_rewCards.Clear();
		_rewInfos.Clear();
		int length = rewards.GetLength(0);
		for (int i = 0; i < length; i++)
		{
			int num = rewards[i, 0];
			if (num == 20)
			{
				rewards[i, 1] = 0;
				if (!bShowAdFreeIcon && DB.User.AdFree)
				{
					continue;
				}
			}
			int num2 = rewards[i, 1];
			int num3 = Math.Abs(num2);
			rewards[i, 1] = num3;
			int originalReward = 0;
			switch ((PropID)num)
			{
			case PropID.AdFree:
				rewards[i, 1] = 0;
				Global.pStores.AdFree = true;
				break;
			case PropID.EnergyTime:
				DB.Bag.AddEnergyUnlimitedTime(num3 * 3600);
				break;
			case PropID.Medal:
				DB.Bag.ModifyMedal(num3);
				break;
			case PropID.CardPack:
			{
				if (cf_const.cardSystemClose)
				{
					continue;
				}
				int currentCardBoxIds = Singleton<CardSystemManager>.I.GetCurrentCardBoxIds(rewards[i, 1]);
				List<CardSystemCards> collection = Singleton<CardSystemManager>.I.OpenCardBox(currentCardBoxIds);
				_rewCards.AddRange(collection);
				LogEvent.I.Track().Add("CardNum", DB.CardSystem.Cards.Count).UserSet();
				LogEvent.I.Track("Card_Box_Get").Add("CardNum", _rewCards.Count).Add("Type", _sSign)
					.Add("BoxType", rewards[i, 1])
					.Add("OwnedCards", DB.CardSystem.Cards.Count)
					.CommonAttr()
					.Send();
				continue;
			}
			case PropID.BattlePass:
				Singleton<ShortBattlePassMgr>.I.PaidSuccess();
				break;
			default:
				if (!isShowOnly)
				{
					originalReward = DB.Bag.Modify((PropID)num, num3, b_update: false, _sSign);
				}
				break;
			}
			_rewInfos.Add(new RewInfo
			{
				originalReward = originalReward,
				bVipReward = (num2 < 0),
				propId = num,
				value = rewards[i, 1]
			});
		}
		InitItems();
	}

	protected virtual void InitItems()
	{
		if (_rewInfos.Count == 0)
		{
			return;
		}
		foreach (RewInfo rewInfo in _rewInfos)
		{
			RewardItem rewardItem = UnityEngine.Object.Instantiate(itemPrefab);
			rewardItem.SetInfo(rewInfo.propId, rewInfo.value);
			pPropGridLayout.AddItem(rewardItem.transform);
			rewardItem.gameObject.SetActive(value: true);
			rewardItem.transform.SetScale(0f);
		}
		int maxColumn = ((_rewInfos.Count == 9 || _rewInfos.Count == 5) ? 3 : 4);
		pPropGridLayout.SetWidth(maxColumn);
		CounterMgr.Add(EventCountRewardPlayAni);
		pPropGridLayout.SetCompleteCallBack(delegate
		{
			CounterMgr.Del(EventCountRewardPlayAni);
		});
	}

	public virtual void Show()
	{
		btnAds.gameObject.SetActive(!bForcedCloseAdsBtn && _place != REWARD_PLACE.None && _type != RewardType.Novice);
		btnSmallReceive.gameObject.SetActive(btnAds.gameObject.activeSelf);
		btnReceive.gameObject.SetActive(!btnAds.gameObject.activeSelf);
		DispatcherMgr.Emit("GamePause", true, "Reward");
		ShowMask();
	}

	protected void _ShowCanvasGroup()
	{
		TweenMgr.DelayCall(0.3f, delegate
		{
			canvasGroupBtn.DOFade(1f, 0.2f);
			canvasGroupBtn.blocksRaycasts = true;
		});
		canvasGroupTitle.DOFade(1f, 0.3f);
	}

	protected void _PlayPropItemMoveAni()
	{
		if (_rewInfos.Count == 0)
		{
			return;
		}
		foreach (Transform item in pPropGridLayout.items)
		{
			item.DOLocalMove(Vector3.zero, 0.3f).From();
			item.DOScale(1f, 0.3f);
		}
	}

	protected virtual void PlayItemMoveAnimation()
	{
		_PlayPropItemMoveAni();
		_ShowCanvasGroup();
	}

	protected void PlayChestAnimation()
	{
		_spineChest.AnimationState.SetAnimation(0, "close-idle", loop: false);
		_spineTrackEntryDelegate = delegate
		{
			HandleEvent();
		};
		_spineChest.AnimationState.Complete += _spineTrackEntryDelegate;
	}

	protected void HandleEvent()
	{
		string name = _spineChest.AnimationState.Tracks.Items[0].Animation.Name;
		if (name == "close-idle")
		{
			AudioMgr.I.PlaySound("chest_open");
			_spineChest.AnimationState.SetAnimation(0, "open", loop: false);
		}
		else if (name == "open")
		{
			PlayItemMoveAnimation();
			_spineChest.AnimationState.SetAnimation(0, "open-idle", loop: true);
			_spineChest.DOFade(0f, 0.3f);
		}
		else if (name == "open-idle")
		{
			_spineChest.AnimationState.Complete -= _spineTrackEntryDelegate;
		}
	}

	public void Receive(bool isAds = false)
	{
		if (!isShowOnly || isAds)
		{
			for (int i = 0; i < _rewInfos.Count; i++)
			{
				PropID propId = (PropID)_rewInfos[i].propId;
				if (propId != PropID.AdFree && propId != PropID.EnergyTime && propId != PropID.Medal && propId != PropID.CardPack && propId != PropID.BattlePass)
				{
					if (isAds)
					{
						DB.Bag.Modify(propId, _rewInfos[i].value, b_update: false, _sSign);
					}
					DB.Bag.ModifyShow(propId, _rewInfos[i].originalReward);
				}
			}
		}
		_close(isAds);
	}

	private void _close(bool isAds)
	{
		DispatcherMgr.Emit("GamePause", false, "Reward");
		DispatcherMgr.Emit("GotRewardEvent");
		_Close(delegate
		{
			_callback?.Invoke(isAds);
		});
	}

	private void ClickBtnReceive(MButton button)
	{
		Receive();
	}

	private void ClickBtnAdsReceive(MButton button)
	{
		Global.ShowRewardedVideo(delegate(string info)
		{
			if (info.Equals("succ"))
			{
				Receive(isAds: true);
			}
		}, _place);
	}
}
