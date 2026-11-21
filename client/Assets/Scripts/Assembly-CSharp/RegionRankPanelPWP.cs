using System;
using Com.TheFallenGames.OSA.CustomParams;

[Serializable]
public class RegionRankPanelPWP : BaseParamsWithPrefab
{
	public float DefaultItemHeight
	{
		set
		{
			DefaultItemSize = value;
		}
	}
}
