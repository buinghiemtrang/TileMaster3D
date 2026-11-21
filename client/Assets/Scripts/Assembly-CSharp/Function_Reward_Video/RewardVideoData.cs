namespace Function_Reward_Video
{
	public struct RewardVideoData
	{
		public RewardVideoState State;

		public int CoolingEndRemainingTime;

		public int RemainingCount;

		public RewardVideoData(RewardVideoState state, int remainingTime, int remainingCount)
		{
			State = state;
			CoolingEndRemainingTime = remainingTime;
			RemainingCount = remainingCount;
		}
	}
}
