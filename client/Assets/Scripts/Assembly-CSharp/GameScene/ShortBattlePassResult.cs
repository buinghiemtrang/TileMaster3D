using ShortBattlePass;

namespace GameScene
{
	public class ShortBattlePassResult : ActivityInResult
	{
		public override bool IsOpen()
		{
			if (Singleton<ShortBattlePassMgr>.I != null)
			{
				if (Singleton<ShortBattlePassMgr>.I.JudgeOpen())
				{
					return !Singleton<ShortBattlePassMgr>.I.JudgeHasState(ActivityState.WaitingReceiveReward);
				}
				return false;
			}
			return false;
		}

		public override int GetScore()
		{
			if (ActivityInResultMgr.I != null && ActivityInResultMgr.I.bIsVictory)
			{
				return cf_const.ShortBattlePassUnitScore;
			}
			return 0;
		}
	}
}
