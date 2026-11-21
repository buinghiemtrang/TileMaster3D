using ShortBattlePass;

public class ShortPassAnimation : MainIconAnimation
{
	public override void SetInfo()
	{
		_newAddKeyCount = ShortBattlePassMgr.I.CacheScore;
		ShortBattlePassMgr.I.CacheScore = 0;
	}
}
