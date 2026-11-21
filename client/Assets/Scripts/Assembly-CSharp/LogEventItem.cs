public static class LogEventItem
{
	public static void Init()
	{
		DispatcherMgr.OnG<PropID, int, int, string>("TrackUpdateItem", TrackUpdateItem);
	}

	private static void TrackUpdateItem(PropID id, int oldCount, int nowCount, string s_place)
	{
		if (nowCount == oldCount)
		{
			return;
		}
		string text = id.ToString();
		LogEvent.I.Track("").Add(text, nowCount).UserSet();
		if (id == PropID.Energy)
		{
			if (nowCount < oldCount)
			{
				return;
			}
			LogEvent.I.Track("Get_Energy").Add("Type", s_place).CommonAttr(false).Send(false);
		}
		else if (id != PropID.Money)
		{
			if (nowCount < oldCount)
			{
				LogEvent.I.Track("Item_Use")
					.Add("Type", text)
					.Add("Itemnum", DB.Bag.GetTotalUseNum(id))
					.Add("Current_Money", oldCount)
					.Add("DifficultyLevel", DB.User.DynamicDifficultyLevel)
					.CommonAttr(false)
					.Send(true);
			}
			else
			{
				LogEvent.I.Track("Item_Get")
					.Add("ItemNum", nowCount - oldCount)
					.Add("ItemType", text)
					.Add("Current_Money", oldCount)
					.Add("Type", s_place)
					.CommonAttr(false)
					.Send(false);
			}
		}
		else
		{
			int value = (nowCount < oldCount) ? (oldCount - nowCount) : (nowCount - oldCount);
			string key = (nowCount < oldCount) ? "Coins_Use" : "Coins_Get";
			LogEvent.I.Track(key)
				.Add("CoinNum", value)
				.Add("Type", s_place)
				.Add("Current_Money", oldCount)
				.CommonAttr(true)
				.Send(false);
		}
	}
}
