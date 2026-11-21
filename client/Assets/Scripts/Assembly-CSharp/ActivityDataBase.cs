public class ActivityDataBase
{
	public long AiLastRefreshTime;

	public long StartTime;

	public long EndTime;

	public int Issue;

	public int Score;

	public bool StartPop;

	public ActivityStateBase State;

	public ActivityDataBase()
	{
		Issue = -1;
		State = ActivityStateBase.NoStart;
	}
}
