namespace TreasureHunt
{
	public enum ActivityState
	{
		None = 0,
		ResetRewards = 1,
		Started = 2,
		PosterDialog = 4,
		JoinedTeam = 8,
		WaitingEnd = 0x10,
		Ended = 0x20
	}
}
