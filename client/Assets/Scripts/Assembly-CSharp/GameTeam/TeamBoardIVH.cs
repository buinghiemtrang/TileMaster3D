using Com.TheFallenGames.OSA.Core;
using UnityEngine;

namespace GameTeam
{
	public class TeamBoardIVH : BaseItemViewsHolder
	{
		public ItemInBoard pItem;

		private BoardsInfo.BoardInfo model;

		public override void CollectViews()
		{
			base.CollectViews();
			pItem = root.GetComponent<ItemInBoard>();
		}

		public virtual bool CanPresentModelType(BoardsInfo.BoardInfo modelnew)
		{
			return modelnew.GetInfoType() == model.GetInfoType();
		}

		public void UpdateViewsHolder(BoardsInfo.BoardInfo model)
		{
			pItem.Init(model);
			this.model = model;
		}

		public void UpdateSize()
		{
			root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pItem.height);
		}
	}
}
