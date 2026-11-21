using System;
using System.Collections.Generic;

[Serializable]
public class RewardAdViewData
{
	public List<int> ReceivedRewardId;

	public bool HasDailyShowPop;

	public int SeenCount;

	public long LastDate;

	public RewardAdViewData()
	{
		ReceivedRewardId = new List<int>();
	}
}
