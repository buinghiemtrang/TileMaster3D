using Com.TheFallenGames.OSA.Core;
using DG.Tweening;
using UnityEngine;

namespace Ranking
{
	public class RankingScrollView : OSA<MyParamsWithPrefab, MyItemViewsHolder>
	{
		private Tweener refreshTw;

		private bool bHasOwnItem = true;

		private bool bShowOwnItem = true;

		private const int MAX_VISIBLE_COUNT = 7;

		private int ownRanking;

		internal RankingControlBase Controller { get; set; }

		protected override void Start()
		{
			base.Start();
			_Params.DefaultItemHeight = _Params.ItemPrefab.sizeDelta.y;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if (refreshTw != null)
			{
				refreshTw.Kill();
				refreshTw = null;
			}
		}

		public void RefreshItems(int index, bool bHasOwnItem)
		{
			Start();
			Debug.Log(Controller.Address);
			Debug.Log(string.Format("Controller.DataCount:{0}", Controller.DataCount));
			if (refreshTw != null)
			{
				refreshTw.Kill();
				refreshTw = null;
			}
			refreshTw = TweenMgr.DelayCall(0.1f, delegate
			{
				ResetItems(Controller.DataCount, false, true);
				if (Controller.DataCount > 0)
				{
					SmoothScrollTo(0, 0.2f);
				}
			}).SetId("DelayCall_RefreshItems_Ranking");
		}

		protected override MyItemViewsHolder CreateViewsHolder(int itemIndex)
		{
			MyItemViewsHolder myItemViewsHolder = new MyItemViewsHolder();
			myItemViewsHolder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
			return myItemViewsHolder;
		}

		protected override void UpdateViewsHolder(MyItemViewsHolder newOrRecycled)
		{
			if (newOrRecycled.ItemIndex < Controller.DataCount)
			{
				RankingData data = Controller.GetData(newOrRecycled.ItemIndex);
				newOrRecycled.pItem.InitData(data);
			}
		}

		private void ShowOwnItem(bool bShow, bool bForce = false)
		{
			if (bForce || bShowOwnItem != bShow)
			{
				bShowOwnItem = bShow;
				Controller.SetShowOwnItem(bShow);
			}
		}
	}
}
