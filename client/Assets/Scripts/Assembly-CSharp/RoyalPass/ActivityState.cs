namespace RoyalPass
{
	public enum ActivityState
	{
		None = 0,
		Started = 1,
		ResetRewards = 2,
		ActivityDialog = 4,
		PaidSuccess = 8,
		WaitingReceivePaidReward = 0x10,
		SentRewards = 0x20,
		WaitingEnd = 0x40,
		EarlyEnded = 0x80,
		Ended = 0x100
	}
}
