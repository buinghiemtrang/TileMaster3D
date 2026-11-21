using System;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;

namespace GameTeam
{
	public class TeamJoinRequestScrollView : OSA<TeamJoinRequestPWP, TeamJoinRequestIVH>
	{
		private List<ApplyJoinTeamInfo> infos = new List<ApplyJoinTeamInfo>();

		public Action<bool, string, string> cback;

		protected override void Start()
		{
			base.Start();
			_Params.DefaultItemHeight = _Params.ItemPrefab.sizeDelta.y;
		}

		public void SetInfos(List<ApplyJoinTeamInfo> infos)
		{
			this.infos.Clear();
			if (infos != null)
			{
				this.infos = infos;
			}
			ResetItems(this.infos.Count, false, false);
		}

		private void RefreshItems()
		{
			ResetItems(infos.Count, false, false);
		}

		protected override TeamJoinRequestIVH CreateViewsHolder(int itemIndex)
		{
			TeamJoinRequestIVH teamJoinRequestIVH = new TeamJoinRequestIVH();
			teamJoinRequestIVH.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
			return teamJoinRequestIVH;
		}

		protected override void UpdateViewsHolder(TeamJoinRequestIVH newOrRecycled)
		{
			if (newOrRecycled.ItemIndex >= infos.Count)
			{
				return;
			}
			newOrRecycled.pItem.InitData(infos[newOrRecycled.ItemIndex]);
			newOrRecycled.pItem.cback = cback;
		}
	}
}
