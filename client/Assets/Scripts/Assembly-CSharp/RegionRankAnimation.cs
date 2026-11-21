public class RegionRankAnimation : MainIconAnimation
{
	public override void SetInfo()
	{
		_newAddKeyCount = GSingleton<RegionRankActivityManager>.ins.CacheScore;
		GSingleton<RegionRankActivityManager>.ins.CacheScore = 0;
	}
}
