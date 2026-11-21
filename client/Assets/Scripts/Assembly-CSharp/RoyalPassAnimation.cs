using RoyalPass;

public class RoyalPassAnimation : MainIconAnimation
{
	public override void SetInfo()
	{
		_newAddKeyCount = Singleton<RoyalPassActivityMgr>.I.CacheScore;
		Singleton<RoyalPassActivityMgr>.I.CacheScore = 0;
	}
}
