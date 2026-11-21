using System;
using Com.TheFallenGames.OSA.Core;
using UnityEngine;

namespace RoyalPass
{
	[Serializable]
	public class RoyalPassPWP : BaseParams
	{
		public RectTransform titelItem;

		public RectTransform ordinaryItem;

		public RectTransform specialItem;

		public override void InitIfNeeded(IOSA iAdapter)
		{
			base.InitIfNeeded(iAdapter);
			titelItem.gameObject.SetActive(false);
			ordinaryItem.gameObject.SetActive(false);
			specialItem.gameObject.SetActive(false);
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
