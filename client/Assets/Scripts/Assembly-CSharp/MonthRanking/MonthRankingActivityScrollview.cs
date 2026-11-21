using System.Collections;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using UnityEngine;

namespace MonthRanking
{
	public class MonthRankingActivityScrollview : OSA<MonthRankingActivityPWP, MonthRankingActivityIVH>
	{
		private bool _bIsInit;

		private List<RankingData> _infos;

		private Coroutine refreshListIE;

		protected override void Awake()
		{
			base.Awake();
			_bIsInit = false;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (refreshListIE != null)
			{
				StopCoroutine(refreshListIE);
				refreshListIE = null;
			}
		}

		protected override void Start()
		{
			base.Start();
			_Params.DefaultItemHeight = _Params.ItemPrefab.sizeDelta.y;
			_bIsInit = true;
		}

		public void RefreshItems(List<RankingData> infos)
		{
			_infos = infos;
			if (refreshListIE != null)
			{
				StopCoroutine(refreshListIE);
			}
			refreshListIE = StartCoroutine("IERefresh");
		}

		private IEnumerator IERefresh()
		{
			while (!_bIsInit)
			{
				yield return null;
			}
			ResetItems((_infos != null) ? _infos.Count : 0);
		}

		protected override MonthRankingActivityIVH CreateViewsHolder(int itemIndex)
		{
			MonthRankingActivityIVH monthRankingActivityIVH = new MonthRankingActivityIVH();
			monthRankingActivityIVH.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
			return monthRankingActivityIVH;
		}

		protected override void UpdateViewsHolder(MonthRankingActivityIVH newOrRecycled)
		{
			newOrRecycled.pItem.Init(_infos[newOrRecycled.ItemIndex]);
		}
	}
}
