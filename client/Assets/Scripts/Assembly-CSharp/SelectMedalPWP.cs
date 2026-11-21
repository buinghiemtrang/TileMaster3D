using System;
using Com.TheFallenGames.OSA.Core;
using UnityEngine;

[Serializable]
public class SelectMedalPWP : BaseParams
{
	public RectTransform groupItem;

	public override void InitIfNeeded(IOSA iAdapter)
	{
		base.InitIfNeeded(iAdapter);
		groupItem.gameObject.SetActive(value: false);
	}

	public RectTransform GetItem()
	{
		return groupItem;
	}
}
