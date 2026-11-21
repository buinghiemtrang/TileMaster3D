using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;

namespace Recharge
{
	public class RechargeActivityScroll : OSA<RechargePwp, RechargeIvh>
	{
		private List<RewardInfo> _rewardInfos;

		public void RefreshItems()
		{
			base.Start();
			_rewardInfos = RechargeActivityMgr.I.GetRewardInfos();
			ResetItems(_rewardInfos.Count);
			for (int i = 0; i < _rewardInfos.Count; i++)
			{
				if (!_rewardInfos[i].IsReceived)
				{
					if (i >= 2)
					{
						ScrollTo(i - 1);
					}
					break;
				}
			}
		}

		protected override RechargeIvh CreateViewsHolder(int itemIndex)
		{
			RechargeIvh rechargeIvh = new RechargeIvh();
			rechargeIvh.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
			return rechargeIvh;
		}

		protected override void UpdateViewsHolder(RechargeIvh newOrRecycled)
		{
			RewardInfo data = _rewardInfos[newOrRecycled.ItemIndex];
			newOrRecycled.PItem.Init(data);
		}
	}
}
