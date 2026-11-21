using System;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;

namespace GameTeam
{
	public class TeamListScrollView : OSA<TeamListPWP, TeamListIVH>
	{
		public Action onJoinSuccess;

		private List<TeamInfo> datas;

		private bool isJoined;

		protected override void Start()
		{
			base.Start();
			_Params.DefaultItemHeight = _Params.ItemPrefab.sizeDelta.y;
		}

		public void RefreshItems(bool isJoined, List<TeamInfo> _datas)
		{
			this.isJoined = isJoined;
			datas = _datas;
			ResetItems(datas.Count, false, false);
		}

		protected override TeamListIVH CreateViewsHolder(int itemIndex)
		{
			TeamListIVH teamListIVH = new TeamListIVH();
			teamListIVH.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
			return teamListIVH;
		}

		protected override void UpdateViewsHolder(TeamListIVH newOrRecycled)
		{
			newOrRecycled.pItem.onJoinSuccess = () =>
			{
				if (onJoinSuccess != null)
				{
					onJoinSuccess();
				}
			};
			newOrRecycled.pItem.Init(isJoined, datas[newOrRecycled.ItemIndex]);
		}
	}
}
