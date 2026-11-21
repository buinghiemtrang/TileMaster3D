using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using UnityEngine;

namespace GameTeam
{
	public class TeamBoardScrollView : OSA<TeamBoardPWP, TeamBoardIVH>
	{
		private List<BoardsInfo.BoardInfo> boardInfos = new List<BoardsInfo.BoardInfo>();

		private int itemIndex;

		private const int MaxSystemMsgShowCount = 6;

		protected override void Start()
		{
			base.Start();
		}

		public void RefreshItems()
		{
			boardInfos.Clear();
			int count = 0;
			if (TeamDataMgr.I.MyBoardsInfos != null && TeamDataMgr.I.MyBoardsInfos.Count >= 1 && TeamDataMgr.I.MyBoardsInfos[0] != null && TeamDataMgr.I.MyBoardsInfos[0].Boards != null)
			{
				int num = 0;
				foreach (KeyValuePair<string, BoardsInfo.BoardInfo> board in TeamDataMgr.I.MyBoardsInfos[0].Boards)
				{
					BoardsInfo.BoardInfo value = board.Value;
					BoardInfoType infoType = value.GetInfoType();
					if (infoType == BoardInfoType.Join || infoType == BoardInfoType.Leave)
					{
						if (num < MaxSystemMsgShowCount)
						{
							boardInfos.Add(value);
							num++;
						}
					}
					else
					{
						boardInfos.Add(value);
					}
				}
				count = boardInfos.Count;
			}
			ResetItems(count, false, true);
			if (count != 0)
			{
				ScrollTo(count - 1);
			}
		}

		protected override TeamBoardIVH CreateViewsHolder(int itemIndex)
		{
			TeamBoardIVH teamBoardIVH = new TeamBoardIVH();
			RectTransform item = _Params.GetItem(boardInfos[itemIndex].GetInfoType());
			teamBoardIVH.Init(item, _Params.Content, itemIndex);
			return teamBoardIVH;
		}

		protected override void UpdateViewsHolder(TeamBoardIVH newOrRecycled)
		{
			itemIndex = newOrRecycled.ItemIndex;
			if (itemIndex < boardInfos.Count)
			{
				newOrRecycled.UpdateViewsHolder(boardInfos[itemIndex]);
				ScheduleComputeVisibilityTwinPass();
			}
		}

		protected override bool IsRecyclable(TeamBoardIVH potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
		{
			return potentiallyRecyclable.CanPresentModelType(boardInfos[indexOfItemThatWillBecomeVisible]);
		}

		protected override float UpdateItemSizeOnTwinPass(TeamBoardIVH viewsHolder)
		{
			viewsHolder.UpdateSize();
			return viewsHolder.root.rect.height;
		}
	}
}
