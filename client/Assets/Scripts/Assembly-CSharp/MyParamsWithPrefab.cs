using System;
using Com.TheFallenGames.OSA.Core;
using UnityEngine;

[Serializable]
public class MyParamsWithPrefab : BaseParams
{
	public RectTransform rankItem;

	public RectTransform boundaryUpItem;

	public RectTransform boundaryDownItem;

	public override void InitIfNeeded(IOSA iAdapter)
	{
		rankItem.gameObject.SetActive(value: false);
		boundaryUpItem.gameObject.SetActive(value: false);
		boundaryDownItem.gameObject.SetActive(value: false);
		base.InitIfNeeded(iAdapter);
	}

	public RectTransform GetItem(StarRankingDataType dataType)
	{
		switch (dataType)
		{
		case StarRankingDataType.UpSign:
			return boundaryUpItem;
		case StarRankingDataType.DownSign:
			return boundaryDownItem;
		default:
			return rankItem;
		}
	}
}
