using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RewardCardBoxDialog : RewardUnifyDialog
{
	[SerializeField]
	private CardItem cardPrefab;

	[SerializeField]
	private GridLayoutCenter pCardGridLayout;

	[SerializeField]
	private GameObject objPropTile;

	[SerializeField]
	private Text ttCardTileHint;

	private bool bFirstOpenCardBox;

	protected override void Awake()
	{
		base.Awake();
		cardPrefab.gameObject.SetActive(false);
		ttCardTileHint.text = "";
		bFirstOpenCardBox = (DB.CardSystem.Cards.Count == 0);
	}

	protected override void InitItems()
	{
		base.InitItems();
		if (objPropTile != null)
		{
			objPropTile.SetActive(_rewInfos.Count != 0);
		}
		if (_rewCards.Count == 0)
		{
			return;
		}
		foreach (CardSystemCards cardInfo in _rewCards)
		{
			CardItem cardItem = UnityEngine.Object.Instantiate(cardPrefab);
			pCardGridLayout.AddItem(cardItem.transform);
			cardItem.gameObject.SetActive(true);
			cardItem.transform.SetScale(0f);
			cardItem.ShowCardInfo(cardInfo);
		}
		pCardGridLayout.SetWidth(4);
		CounterMgr.Add(EventCountRewardPlayAni);
		pCardGridLayout.SetCompleteCallBack(delegate
		{
			CounterMgr.Del(EventCountRewardPlayAni);
		});
		if (_rewInfos.Count == 0)
		{
			bForcedCloseAdsBtn = true;
			pCardGridLayout.transform.SetLocPos(Utils.Vec2(0f, 80f));
		}
		ttCardTileHint.SetLKey(bFirstOpenCardBox ? "Card_FirstBonus" : "Card_NoBonus");
	}

	protected override void PlayItemMoveAnimation()
	{
		_PlayPropItemMoveAni();
		if (_rewCards.Count > 0)
		{
			int num = 0;
			foreach (Transform item in pCardGridLayout.items)
			{
				DOTween.Sequence()
					.SetTarget(item)
					.SetId(AnimationID_ItemAni)
					.Append(item.DOLocalMove(Vector3.zero, 0.3f, false).From())
					.Join(item.DOScale(1f, 0.3f))
					.AppendInterval((float)num * 0.1f)
					.AppendCallback(delegate
					{
						item.GetComponent<Animation>().Play("open_card_ani");
					});
				num++;
			}
		}
		_ShowCanvasGroup();
	}
}
