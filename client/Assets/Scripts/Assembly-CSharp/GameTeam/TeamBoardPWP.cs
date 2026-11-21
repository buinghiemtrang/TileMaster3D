using System;
using Com.TheFallenGames.OSA.Core;
using Festival;
using UnityEngine;

namespace GameTeam
{
	[Serializable]
	public class TeamBoardPWP : BaseParams
	{
		public RectTransform chatItem;

		public RectTransform requestItem;

		public RectTransform leaveOrJoinItem;

		public RectTransform ShortBattlePassItem;

		private RectTransform _royalPassActivityItem;

		public override void InitIfNeeded(IOSA iAdapter)
		{
			base.InitIfNeeded(iAdapter);
			chatItem.gameObject.SetActive(false);
			requestItem.gameObject.SetActive(false);
			leaveOrJoinItem.gameObject.SetActive(false);
			ShortBattlePassItem.gameObject.SetActive(false);
		}

		public RectTransform GetItem(BoardInfoType type)
		{
			switch (type)
			{
			case BoardInfoType.RequestingEnergy:
				return requestItem;
			case BoardInfoType.Chat:
				return chatItem;
			case BoardInfoType.RoyalPassActivity:
				if (!_royalPassActivityItem)
				{
					_royalPassActivityItem = (RectTransform)FestivalMgr.I.LoadAsset<GameObject>("Prefabs/Activity/RoyalPass/{0}_RoyalPassActivityTeamBoardItem").transform;
				}
				return _royalPassActivityItem;
			case BoardInfoType.ShortBattlePass:
				return ShortBattlePassItem;
			default:
				return leaveOrJoinItem;
			}
		}
	}
}
