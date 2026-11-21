namespace GameScene
{
	public class BoosterActivtyInResult : ActivityInResult
	{
		public override bool IsOpen()
		{
			return BoosterMgr.I.JudgeUnlock();
		}

		public override int GetScore()
		{
			if (ActivityInResultMgr.I != null && ActivityInResultMgr.I.bIsVictory)
			{
				return 1;
			}
			return 0;
		}
	}
}
