using GameScene_Element;
using TreasureHunt;

namespace GameScene
{
	public class TreasureHuntActivityInResult : ActivityInResult
	{
		public override bool IsOpen()
		{
			// asm 0x10c53e8: JudgeOpen, rồi (nếu mở) JudgeHasState(WaitingEnd) với kết quả bị bỏ, kết quả trả về là JudgeOpen() lần hai
			if (Singleton<TreasureHuntActivityMgr>.I.JudgeOpen())
			{
				Singleton<TreasureHuntActivityMgr>.I.JudgeHasState(ActivityState.WaitingEnd);
			}
			return Singleton<TreasureHuntActivityMgr>.I.JudgeOpen();
		}

		public override int GetScore()
		{
			return cf_const.treasureHuntActivityUnitScore * ElementsMgr.I.Data.TotalNumber;
		}
	}
}
