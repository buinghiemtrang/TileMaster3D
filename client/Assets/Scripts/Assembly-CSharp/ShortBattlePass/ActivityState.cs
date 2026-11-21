namespace ShortBattlePass
{
	public enum ActivityState
	{
		None = 0,
		Started = 1,
		ActivityDialog = 2,
		CostSuccess = 4,
		WaitingReceivePaidReward = 8,
		SentRewards = 0x10,
		WaitingReceiveReward = 0x20,
		Ended = 0x40,
		Cooling = 0x80
	}
}
