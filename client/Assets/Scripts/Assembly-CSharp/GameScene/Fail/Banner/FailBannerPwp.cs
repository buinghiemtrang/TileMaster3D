using System;
using Com.TheFallenGames.OSA.Core;
using UnityEngine;

namespace GameScene.Fail.Banner
{
	[Serializable]
	public class FailBannerPwp : BaseParams
	{
		[SerializeField]
		private RectTransform _reviveGiftFailBannerPrefab;

		[SerializeField]
		private RectTransform _timeLimitGiftOneFailBannerPrefab;

		[SerializeField]
		private RectTransform _timeLimitGiftTwoFailBannerPrefab;

		[SerializeField]
		private RectTransform _storeGiftFailBannerPrefab;

		[SerializeField]
		private RectTransform _shortBattlePassFailBannerPrefab;

		[SerializeField]
		private RectTransform _freeGoldFailBannerPrefab;

		public RectTransform GetPrefab(FailBanner banner)
		{
			switch (banner.BannerType)
			{
			case FailBannerType.Revive:
				return _reviveGiftFailBannerPrefab;
			case FailBannerType.TimeLimitGift:
				if (((TimeLimitGiftFailBanner)banner).Order != 1)
				{
					return _timeLimitGiftTwoFailBannerPrefab;
				}
				return _timeLimitGiftOneFailBannerPrefab;
			case FailBannerType.ShortBattlePass:
				return _shortBattlePassFailBannerPrefab;
			case FailBannerType.FreeGold:
				return _freeGoldFailBannerPrefab;
			default:
				return _storeGiftFailBannerPrefab;
			}
		}
	}
}
