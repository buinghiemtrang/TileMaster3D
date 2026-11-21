using System;
using Com.TheFallenGames.OSA.Core;
using UnityEngine;

namespace ShortBattlePass
{
	[Serializable]
	public class ShortBattlePassPwp : BaseParams
	{
		public RectTransform titelItem;

		public RectTransform ordinaryItem;

		public RectTransform specialItem;

		public override void InitIfNeeded(IOSA iAdapter)
		{
			base.InitIfNeeded(iAdapter);
			titelItem.gameObject.SetActive(value: false);
			ordinaryItem.gameObject.SetActive(value: false);
			specialItem.gameObject.SetActive(value: false);
		}

		public RectTransform GetItem(RewardItemType type)
		{
			return type switch
			{
				RewardItemType.Title => titelItem, 
				RewardItemType.Special => specialItem, 
				_ => ordinaryItem, 
			};
		}
	}
}
