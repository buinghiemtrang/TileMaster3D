using GameScene;
using GameScene_Level;

public class RegionRankGameScore : ActivityInResult
{
	public override bool IsOpen()
	{
		if (GSingleton<RegionRankActivityManager>.ins.GetActivityState() == ActivityStateBase.During)
		{
			return LevelMgr.Data.RegionRankScore > 0;
		}
		return false;
	}

	public override int GetScore()
	{
		return LevelMgr.Data.RegionRankScore;
	}
}
