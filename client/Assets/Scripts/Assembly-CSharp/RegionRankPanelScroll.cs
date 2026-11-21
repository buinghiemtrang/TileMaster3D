using System.Collections;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using UnityEngine;

public class RegionRankPanelScroll : OSA<RegionRankPanelPWP, RegionRankPanelVH>
{
	private bool _isInit;

	private List<RegionRankClientItemData> _datas;

	private Coroutine _refreshListIE;

	private RegionRankType _rankType;

	protected override void Awake()
	{
		base.Awake();
		_isInit = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_refreshListIE != null)
		{
			StopCoroutine(_refreshListIE);
		}
	}

	protected override void Start()
	{
		base.Start();
		_Params.DefaultItemHeight = _Params.ItemPrefab.sizeDelta.y;
		_isInit = true;
	}

	public void RefreshItems(List<RegionRankClientItemData> infos, RegionRankType rankType)
	{
		_datas = infos;
		_rankType = rankType;
		if (_refreshListIE != null)
		{
			StopCoroutine(_refreshListIE);
		}
		_refreshListIE = StartCoroutine("IERefresh");
	}

	private IEnumerator IERefresh()
	{
		while (!_isInit)
		{
			yield return null;
		}
		ResetItems((_datas != null) ? _datas.Count : 0);
		ScrollTo(0);
	}

	protected override RegionRankPanelVH CreateViewsHolder(int itemIndex)
	{
		RegionRankPanelVH regionRankPanelVH = new RegionRankPanelVH();
		regionRankPanelVH.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		return regionRankPanelVH;
	}

	protected override void UpdateViewsHolder(RegionRankPanelVH newOrRecycled)
	{
		int itemIndex = newOrRecycled.ItemIndex;
		newOrRecycled.Item.Init(_datas[itemIndex], itemIndex + 1, _rankType);
	}
}
