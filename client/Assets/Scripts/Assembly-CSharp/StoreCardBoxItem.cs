using System;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class StoreCardBoxItem : MonoBehaviour
{
	[SerializeField]
	private MButton _buttonFree;

	[SerializeField]
	private MButton _buttonGold;

	[SerializeField]
	private MButton _buttonKey;

	[SerializeField]
	private Text _freeCD;

	[SerializeField]
	private Text _goldCount;

	[SerializeField]
	private Text _keyCount;

	[SerializeField]
	private Text _cardBoxName;

	[SerializeField]
	private Text _cardBoxDescribe;

	[SerializeField]
	private SkeletonGraphic _chestSkeletonGraphic;

	private string _chestSkinName;

	private int _cardBoxQualityId;

	private int _goldCost;

	private int _freeCdTime;

	private void Awake()
	{
		_buttonFree.onClick.AddListener(OnClickButtonFree);
		_buttonGold.onClick.AddListener(OnClickButtonGold);
		_buttonKey.onClick.AddListener(OnClickButtonKey);
		DispatcherMgr.On("CardSystemDrawCard", UpdateUI);
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off("CardSystemDrawCard", UpdateUI);
	}

	public void InitData(int cardBoxQualityId, int goldCost, int freeCd)
	{
		_cardBoxQualityId = cardBoxQualityId;
		_goldCost = goldCost;
		_freeCdTime = freeCd;
		UpdateFreeCdTime();
		UpdateUI();
	}

	private void UpdateFreeCdTime()
	{
		_buttonFree.gameObject.SetActive(false);
		_freeCD.transform.parent.gameObject.SetActive(false);
		if (_freeCdTime == -1)
		{
			return;
		}
		int totalSec = (int)(Singleton<CardSystemManager>.I.GetCardBoxCD(_cardBoxQualityId) - DateTime.UtcNow.TotalSeconds());
		if (totalSec > 0)
		{
			_freeCD.transform.parent.gameObject.SetActive(true);
			_freeCD.DOKill();
			_freeCD.DOCounter(totalSec, 0, (float)totalSec, Util.Helper.FormatTimerD).OnComplete(delegate
			{
				_buttonFree.gameObject.SetActive(true);
				_freeCD.transform.parent.gameObject.SetActive(false);
			});
		}
		else
		{
			_buttonFree.gameObject.SetActive(true);
		}
	}

	private void UpdateUI()
	{
		_buttonKey.gameObject.SetActive(false);
		_buttonGold.gameObject.SetActive(false);
		if (_goldCost != -1)
		{
			_buttonGold.gameObject.SetActive(true);
			_goldCount.text = string.Format("{0}", _goldCost);
		}
	}

	private void OnClickButtonFree(MButton button)
	{
		if (_freeCdTime != -1)
		{
			ShowRewardCards("ShopFree");
			Singleton<CardSystemManager>.I.RefreshCardBoxCD(_cardBoxQualityId, _freeCdTime);
			UpdateFreeCdTime();
		}
	}

	private void OnClickButtonGold(MButton button)
	{
		if (_goldCost != -1)
		{
			if (DB.Bag.GetProp(PropID.Money) < _goldCost)
			{
				GiftDataMgr.ins.TryPopGiftImmediately(GiftTriggerType.Gold, null, true);
			}
			else
			{
				DB.Bag.Modify(PropID.Money, -_goldCost, true, "BuyCardBox");
				DispatcherMgr.Emit("CardSystemDrawCard");
				ShowRewardCards("ShopCoins");
			}
		}
	}

	private void OnClickButtonKey(MButton button)
	{
	}

	private void ShowRewardCards(string sign)
	{
		Global.CreateReward(string.Format("{0}|{1}", 30, _cardBoxQualityId), RewardType.None, REWARD_PLACE.None, sign, false);
	}
}
