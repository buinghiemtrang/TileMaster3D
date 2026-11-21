using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;

public class SelectMedalScrollView : OSA<SelectMedalPWP, SelectMedalIVH>
{
	private List<MedalGroupData> medalGroupDatas = new List<MedalGroupData>();

	private List<MedalGroupData> updateTipsDatas = new List<MedalGroupData>();

	private List<cfg_medal> medalCfgs = new List<cfg_medal>();

	private MedalType medalType = MedalType.Normall;

	private int iGroupCount;

	private int lastGroupIndexOfShowTips = -1;

	private int lastCellIndexOfShowTips = -1;

	protected override void Start()
	{
		base.Start();
		DispatcherMgr.On<int, int>("Show_MedalDesc", OnUpdateShowTips);
		RefreshItems();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		DispatcherMgr.Off<int, int>("Show_MedalDesc", OnUpdateShowTips);
	}

	public void SetMedalType(MedalType type)
	{
		medalType = type;
		RefreshItems();
	}

	public void RefreshItems()
	{
		if (MedalMgr.I.CurSelectId != DB.User.Medal)
		{
			MedalMgr.I.CurSelectId = DB.User.Medal;
		}
		lastGroupIndexOfShowTips = -1;
		lastCellIndexOfShowTips = -1;
		medalCfgs = MedalMgr.I.GetCfgsByType(medalType);
		if (medalType == MedalType.Special)
		{
			MedalMgr.I.SortSpecialMedals(medalCfgs);
		}
		iGroupCount = medalCfgs.Count / 3;
		if (medalCfgs.Count != iGroupCount * 3)
		{
			iGroupCount++;
		}
		medalGroupDatas.Clear();
		for (int i = 0; i < iGroupCount; i++)
		{
			MedalGroupData medalGroupData = new MedalGroupData
			{
				groupIndex = i,
				cellIndexOfShowTips = -1,
				medalDatas = new List<MedalData>()
			};
			for (int j = 0; j < 3; j++)
			{
				int num = i * 3 + j;
				if (num >= medalCfgs.Count)
				{
					break;
				}
				medalGroupData.medalDatas.Add(new MedalData
				{
					index = num,
					cellIndex = j,
					bUnlock = false,
					cfg = medalCfgs[num]
				});
			}
			medalGroupDatas.Add(medalGroupData);
		}
		ResetItems(iGroupCount);
	}

	protected override SelectMedalIVH CreateViewsHolder(int itemIndex)
	{
		SelectMedalIVH selectMedalIVH = new SelectMedalIVH();
		selectMedalIVH.Init(_Params.GetItem(), _Params.Content, itemIndex);
		return selectMedalIVH;
	}

	protected override void UpdateViewsHolder(SelectMedalIVH newOrRecycled)
	{
		if (newOrRecycled.ItemIndex < iGroupCount)
		{
			newOrRecycled.UpdateViewsHolder(medalGroupDatas[newOrRecycled.ItemIndex]);
			ScheduleComputeVisibilityTwinPass();
		}
	}

	protected override bool IsRecyclable(SelectMedalIVH potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
	{
		return true;
	}

	protected override float UpdateItemSizeOnTwinPass(SelectMedalIVH viewsHolder)
	{
		viewsHolder.UpdateSize();
		return viewsHolder.root.rect.height;
	}

	private void OnUpdateShowTips(int groupIndex, int cellIndex)
	{
		updateTipsDatas.Clear();
		if (lastGroupIndexOfShowTips == -1)
		{
			medalGroupDatas[groupIndex].cellIndexOfShowTips = cellIndex;
			updateTipsDatas.Add(medalGroupDatas[groupIndex]);
			lastGroupIndexOfShowTips = groupIndex;
			lastCellIndexOfShowTips = cellIndex;
		}
		else if (lastGroupIndexOfShowTips == groupIndex)
		{
			if (lastCellIndexOfShowTips == cellIndex)
			{
				medalGroupDatas[groupIndex].cellIndexOfShowTips = -1;
				updateTipsDatas.Add(medalGroupDatas[lastGroupIndexOfShowTips]);
				lastGroupIndexOfShowTips = -1;
				lastCellIndexOfShowTips = -1;
			}
			else
			{
				medalGroupDatas[groupIndex].cellIndexOfShowTips = cellIndex;
				updateTipsDatas.Add(medalGroupDatas[groupIndex]);
				lastGroupIndexOfShowTips = groupIndex;
				lastCellIndexOfShowTips = cellIndex;
			}
		}
		else
		{
			medalGroupDatas[lastGroupIndexOfShowTips].cellIndexOfShowTips = -1;
			updateTipsDatas.Add(medalGroupDatas[lastGroupIndexOfShowTips]);
			medalGroupDatas[groupIndex].cellIndexOfShowTips = cellIndex;
			updateTipsDatas.Add(medalGroupDatas[groupIndex]);
			lastGroupIndexOfShowTips = groupIndex;
			lastCellIndexOfShowTips = cellIndex;
		}
		ResetItems(iGroupCount);
		SmoothScrollTo(groupIndex, 0.2f);
	}
}
