using System.Collections.Generic;
using System.Linq;
using Com.TheFallenGames.OSA.Core;
using UnityEngine;

namespace ShortBattlePass
{
	public class ShortBattlePassScroll : OSA<ShortBattlePassPwp, ShortBattlePassIvh>
	{
		private List<RewardInfo> _infos;

		private int _ordinaryRewardsNumber;

		public void RefreshItems(List<RewardInfo> infos)
		{
			Start();
			_infos = infos;
			_ordinaryRewardsNumber = infos.Where((RewardInfo w) => w.Type == RewardItemType.Ordinary).ToList().Count;
			ResetItems(infos.Count);
			_MoveTo(1f);
		}

		public void Unlock()
		{
			if (_infos != null)
			{
				for (int i = 0; i < _infos.Count; i++)
				{
					_infos[i].IsPaid = true;
				}
			}
		}

		private void _MoveTo(float time)
		{
			if (_infos == null)
			{
				return;
			}
			int grade = DB.ShortBattlePass.Data.Grade;
			int count = _infos.Count;
			for (int i = 0; i < count; i++)
			{
				RewardInfo rewardInfo = _infos[i];
				if (rewardInfo.Config != null)
				{
					if (grade < rewardInfo.Config.Grade)
					{
						break;
					}
					if (rewardInfo.State != RewardState.All && (rewardInfo.IsPaid || (rewardInfo.State != RewardState.Free && (rewardInfo.State != RewardState.None || rewardInfo.Config.Type != 2))))
					{
						SmoothScrollTo(i, time);
						return;
					}
				}
			}
			if (count != 0)
			{
				SmoothScrollTo(Mathf.Clamp(grade + 1, 0, count), time);
			}
		}

		protected override ShortBattlePassIvh CreateViewsHolder(int itemIndex)
		{
			ShortBattlePassIvh shortBattlePassIvh = new ShortBattlePassIvh();
			shortBattlePassIvh.Init(_Params.GetItem(_infos[itemIndex].Type), _Params.Content, itemIndex);
			return shortBattlePassIvh;
		}

		protected override void UpdateViewsHolder(ShortBattlePassIvh newOrRecycled)
		{
			int itemIndex = newOrRecycled.ItemIndex;
			if (itemIndex < _infos.Count)
			{
				newOrRecycled.UpdateViewsHolder(_ordinaryRewardsNumber, _infos[itemIndex]);
				ScheduleComputeVisibilityTwinPass();
			}
		}

		protected override bool IsRecyclable(ShortBattlePassIvh potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
		{
			return potentiallyRecyclable.CanPresentModelType(_infos[indexOfItemThatWillBecomeVisible]);
		}

		protected override float UpdateItemSizeOnTwinPass(ShortBattlePassIvh viewsHolder)
		{
			viewsHolder.UpdateSize();
			return viewsHolder.root.rect.height;
		}
	}
}
