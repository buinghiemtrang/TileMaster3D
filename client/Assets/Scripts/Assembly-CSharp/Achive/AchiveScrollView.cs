using Com.TheFallenGames.OSA.Core;
using DG.Tweening;

namespace Achive
{
	public class AchiveScrollView : OSA<MyParamsWithPrefab, MyItemViewsHolder>
	{
		protected override void Start()
		{
			base.Start();
			_Params.DefaultItemHeight = _Params.ItemPrefab.sizeDelta.y;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			DOTween.Kill("DelayCall_RefreshItems_Achive");
		}

		public void RefreshItems()
		{
			DOTween.Kill("DelayCall_RefreshItems_Achive");
			TweenMgr.DelayCall(0.1f, delegate
			{
				ResetItems(DataCfg.cfg_achives.Count);
			}).SetId("DelayCall_RefreshItems_Achive");
		}

		protected override MyItemViewsHolder CreateViewsHolder(int itemIndex)
		{
			MyItemViewsHolder myItemViewsHolder = new MyItemViewsHolder();
			myItemViewsHolder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
			return myItemViewsHolder;
		}

		protected override void UpdateViewsHolder(MyItemViewsHolder newOrRecycled)
		{
			if (newOrRecycled.ItemIndex < DataCfg.cfg_achives.Count)
			{
				newOrRecycled.pItem.InitData(newOrRecycled.ItemIndex, DataCfg.cfg_achives[newOrRecycled.ItemIndex]);
			}
		}
	}
}
