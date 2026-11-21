using System.Collections.Generic;

public class RewardVideosData
{
	public string LastRefreshTime;

	public Dictionary<int, int> RewardVideosLookedCount;

	public Dictionary<int, string> RewardVideosCoolingStartTime;

	public Dictionary<int, int> RewardVideosGear;

	public Dictionary<int, int> SingleLevel_DailyCurrentLevelLookAdCount;

	public int SingleLevel_CurLevelFailedCount;

	public Dictionary<int, int> SingleLevel_PerEnterLookAdCount;

	public RewardVideosData()
	{
		LastRefreshTime = string.Empty;
		RewardVideosLookedCount = new Dictionary<int, int>();
		RewardVideosCoolingStartTime = new Dictionary<int, string>();
		RewardVideosGear = new Dictionary<int, int>();
		SingleLevel_DailyCurrentLevelLookAdCount = new Dictionary<int, int>();
		SingleLevel_PerEnterLookAdCount = new Dictionary<int, int>();
	}
}
