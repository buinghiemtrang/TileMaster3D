using RoyalPass;

namespace GameScene
{
	public class RoyalPassActivityResult : ActivityInResult
	{
		public override bool IsOpen()
		{
			if (Singleton<RoyalPassActivityMgr>.I != null)
			{
				if (Singleton<RoyalPassActivityMgr>.I.JudgeOpen())
				{
					return !Singleton<RoyalPassActivityMgr>.I.JudgeHasState(ActivityState.WaitingEnd);
				}
				return false;
			}
			return false;
		}

		public override int GetScore()
		{
			if (ActivityInResultMgr.I != null && ActivityInResultMgr.I.bIsVictory)
			{
				return cf_const.royalPassActivityUnitScore;
			}
			return 0;
		}
	}
}
