using System.Collections;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;

namespace TreasureHunt
{
	public class TreasureHuntActivityScroll : OSA<TreasureHuntPWP, TreasureHuntIVH>
	{
		private bool isInit;

		private List<MemberInfo> infos;

		private List<cfgTreasureHuntActivityRankingReward> cfgs;

		private bool isShowRnakingChest;

		protected override void Awake()
		{
			base.Awake();
			isInit = false;
		}

		protected override void Start()
		{
			base.Start();
			_Params.DefaultItemHeight = _Params.ItemPrefab.sizeDelta.y;
			isInit = true;
		}

		public void RefreshItems(bool isShowRnakingChest, List<cfgTreasureHuntActivityRankingReward> cfgs, List<MemberInfo> infos)
		{
			this.isShowRnakingChest = isShowRnakingChest;
			this.cfgs = cfgs;
			this.infos = infos;
			StopCoroutine("IERefresh");
			StartCoroutine("IERefresh");
		}

		private IEnumerator IERefresh()
		{
			while (!isInit)
			{
				yield return null;
			}
			ResetItems(infos.Count);
		}

		protected override TreasureHuntIVH CreateViewsHolder(int itemIndex)
		{
			TreasureHuntIVH treasureHuntIVH = new TreasureHuntIVH();
			treasureHuntIVH.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
			return treasureHuntIVH;
		}

		protected override void UpdateViewsHolder(TreasureHuntIVH newOrRecycled)
		{
			int itemIndex = newOrRecycled.ItemIndex;
			cfgTreasureHuntActivityRankingReward cfg = null;
			if (isShowRnakingChest && cfgs.Count > itemIndex)
			{
				cfg = cfgs[itemIndex];
			}
			newOrRecycled.pItem.Init(cfg, itemIndex + 1, infos[itemIndex]);
		}
	}
}
