using Com.TheFallenGames.OSA.Core;

public class RegionRankPanelVH : BaseItemViewsHolder
{
	public RegionRankPanelItem Item;

	public override void CollectViews()
	{
		base.CollectViews();
		Item = root.GetComponent<RegionRankPanelItem>();
	}
}
