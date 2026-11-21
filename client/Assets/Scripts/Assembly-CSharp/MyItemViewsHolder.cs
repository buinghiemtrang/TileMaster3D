using Com.TheFallenGames.OSA.Core;
using UnityEngine;

public class MyItemViewsHolder : BaseItemViewsHolder
{
	public StarRankingActivityItemBase pItem;

	public StarRankingData data;

	private float _height;

	public override void CollectViews()
	{
		base.CollectViews();
		pItem = root.GetComponent<StarRankingActivityItemBase>();
		_height = pItem.Rect().height;
	}

	public void UpdateSize()
	{
		root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _height);
	}
}
