using Com.TheFallenGames.OSA.Core;
using UnityEngine;
using frame8.Logic.Misc.Other.Extensions;

namespace RoyalPass
{
	public class RoyalPassIVH : BaseItemViewsHolder
	{
		public RoyalPassActivityItem pItem;

		private RoyalPassActivityRewardInfo model;

		public override void CollectViews()
		{
			base.CollectViews();
			pItem = root.GetComponentAtPath<RoyalPassActivityItem>("Root");
		}

		public virtual bool CanPresentModelType(RoyalPassActivityRewardInfo modelnew)
		{
			return modelnew.type == model.type;
		}

		public void UpdateViewsHolder(int ordinaryRewardsNum, RoyalPassActivityRewardInfo model)
		{
			pItem.Init(ordinaryRewardsNum, model);
			this.model = model;
		}

		public void UpdateSize()
		{
			root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pItem.height);
		}
	}
}
