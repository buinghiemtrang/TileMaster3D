using System;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;

public class StarRankingActivityScroll : OSA<MyParamsWithPrefab, MyItemViewsHolder>
{
	public Action<StarRankingData> showOwnCallback;

	private List<StarRankingData> datas;

	protected override void Start()
	{
		base.Start();
		datas = Singleton<StarRankingActivityMgr>.I.GetPlayerData(out var ownData);
		if (showOwnCallback != null)
		{
			showOwnCallback(ownData);
		}
		datas.Insert(cf_const.activityRankingStarUpDownCondition[0], new StarRankingData
		{
			dataType = StarRankingDataType.UpSign
		});
		datas.Insert(cf_const.activityRankingStarUpDownCondition[1], new StarRankingData
		{
			dataType = StarRankingDataType.DownSign
		});
		ResetItems(datas.Count);
	}

	protected override MyItemViewsHolder CreateViewsHolder(int itemIndex)
	{
		MyItemViewsHolder myItemViewsHolder = new MyItemViewsHolder();
		myItemViewsHolder.data = datas[itemIndex];
		myItemViewsHolder.Init(_Params.GetItem(datas[itemIndex].dataType), _Params.Content, itemIndex);
		return myItemViewsHolder;
	}

	protected override void UpdateViewsHolder(MyItemViewsHolder item)
	{
		if (item.ItemIndex < datas.Count)
		{
			item.pItem.Init(datas[item.ItemIndex], true, true);
			ScheduleComputeVisibilityTwinPass();
		}
	}

	protected override bool IsRecyclable(MyItemViewsHolder item, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
	{
		return item.data.dataType == datas[indexOfItemThatWillBecomeVisible].dataType;
	}

	protected override float UpdateItemSizeOnTwinPass(MyItemViewsHolder viewsHolder)
	{
		viewsHolder.UpdateSize();
		return viewsHolder.root.rect.height;
	}
}
