using Com.TheFallenGames.OSA.Core;

namespace GameTeam
{
	public class MyTeamScrollView : OSA<MyTeamPWP, MyTeamIVH>
	{
		protected override void Start()
		{
			base.Start();
			_Params.DefaultItemHeight = _Params.ItemPrefab.sizeDelta.y;
			if (TeamDataMgr.I.MyTeamInfo != null && TeamDataMgr.I.MyTeamMemberInfos != null)
			{
				ResetItems(TeamDataMgr.I.MyTeamMemberInfos.Count, false, true);
			}
		}

		public void RefreshItems()
		{
			if (TeamDataMgr.I.MyTeamMemberInfos != null)
			{
				ResetItems(TeamDataMgr.I.MyTeamMemberInfos.Count, false, true);
			}
		}

		protected override MyTeamIVH CreateViewsHolder(int itemIndex)
		{
			MyTeamIVH myTeamIVH = new MyTeamIVH();
			myTeamIVH.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
			return myTeamIVH;
		}

		protected override void UpdateViewsHolder(MyTeamIVH newOrRecycled)
		{
			int itemIndex = newOrRecycled.ItemIndex;
			if (TeamDataMgr.I.MyTeamMemberInfos != null && itemIndex < TeamDataMgr.I.MyTeamMemberInfos.Count)
			{
				newOrRecycled.pItem.InitData(itemIndex, TeamDataMgr.I.MyTeamMemberInfos[itemIndex]);
			}
		}
	}
}
