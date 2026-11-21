using Com.TheFallenGames.OSA.Core;
using UnityEngine;
using frame8.Logic.Misc.Other.Extensions;

namespace ShortBattlePass
{
	public class ShortBattlePassIvh : BaseItemViewsHolder
	{
		public ShortBattlePassItem Item;

		private RewardInfo _model;

		public override void CollectViews()
		{
			base.CollectViews();
			Item = root.GetComponentAtPath<ShortBattlePassItem>("Root");
		}

		public virtual bool CanPresentModelType(RewardInfo model)
		{
			return model.Type == _model.Type;
		}

		public void UpdateViewsHolder(int ordinaryRewardsNum, RewardInfo model)
		{
			Item.Init(ordinaryRewardsNum, model);
			_model = model;
		}

		public void UpdateSize()
		{
			root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Item.height);
		}
	}
}
