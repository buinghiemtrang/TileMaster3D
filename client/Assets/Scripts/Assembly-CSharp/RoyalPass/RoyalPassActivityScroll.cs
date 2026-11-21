using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Com.TheFallenGames.OSA.Core;
using UnityEngine;

namespace RoyalPass
{
	public class RoyalPassActivityScroll : OSA<RoyalPassPWP, RoyalPassIVH>
	{
		public Action firstReadyFinishCallback;

		private bool isInit;

		private bool isFirst;

		private List<RoyalPassActivityRewardInfo> infos;

		private int ordinaryRewardsNum;

		protected override void Awake()
		{
			base.Awake();
			isInit = false;
			isFirst = true;
		}

		protected override void Start()
		{
			base.Start();
			isInit = true;
		}

		public void RefreshItems(List<RoyalPassActivityRewardInfo> infos)
		{
			this.infos = infos;
			ordinaryRewardsNum = infos.Where((RoyalPassActivityRewardInfo w) => w.type == RewardItemType.Ordinary).ToList().Count;
			StopCoroutine("IERefresh");
			StartCoroutine("IERefresh");
		}

		public void MoveTo(float time)
		{
			if (isInit && infos != null)
			{
				int curGrade = DB.RoyalPassActivity.CurGrade;
				int num = 0;
				if (curGrade != 0)
				{
					num = curGrade + 1;
				}
				SmoothScrollTo(Mathf.Clamp(num, 0, ordinaryRewardsNum - 1), time);
			}
		}

		private IEnumerator IERefresh()
		{
			while (!isInit)
			{
				yield return null;
			}
			ResetItems(infos.Count);
			if (isFirst)
			{
				isFirst = false;
				if (firstReadyFinishCallback != null)
				{
					firstReadyFinishCallback();
				}
			}
		}

		protected override RoyalPassIVH CreateViewsHolder(int itemIndex)
		{
			RoyalPassIVH royalPassIVH = new RoyalPassIVH();
			royalPassIVH.Init(_Params.GetItem(infos[itemIndex].type), _Params.Content, itemIndex);
			return royalPassIVH;
		}

		protected override void UpdateViewsHolder(RoyalPassIVH newOrRecycled)
		{
			int itemIndex = newOrRecycled.ItemIndex;
			if (itemIndex < infos.Count)
			{
				newOrRecycled.UpdateViewsHolder(ordinaryRewardsNum, infos[itemIndex]);
				ScheduleComputeVisibilityTwinPass();
			}
		}

		protected override bool IsRecyclable(RoyalPassIVH potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
		{
			return potentiallyRecyclable.CanPresentModelType(infos[indexOfItemThatWillBecomeVisible]);
		}

		protected override float UpdateItemSizeOnTwinPass(RoyalPassIVH viewsHolder)
		{
			viewsHolder.UpdateSize();
			return viewsHolder.root.rect.height;
		}
	}
}
