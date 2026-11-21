using Com.TheFallenGames.OSA.Core;
using UnityEngine;

public class SelectMedalIVH : BaseItemViewsHolder
{
	public SelectMedalGroupItem pItem;

	private MedalGroupData model;

	public override void CollectViews()
	{
		base.CollectViews();
		pItem = root.GetComponent<SelectMedalGroupItem>();
	}

	public void UpdateViewsHolder(MedalGroupData medalGroupData)
	{
		model = medalGroupData;
		pItem.Init(medalGroupData);
	}

	public void UpdateSize()
	{
		root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (model.cellIndexOfShowTips == -1) ? 165f : 274f);
	}
}
