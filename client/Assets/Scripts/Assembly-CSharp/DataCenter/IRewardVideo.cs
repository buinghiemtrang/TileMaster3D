using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IRewardVideo : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected RewardVideosData rewardVideosData;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string dailyCD;

		public virtual RewardVideosData RewardVideosData
		{
			get
			{
				return rewardVideosData;
			}
			set
			{
				rewardVideosData = value;
				Asse.Save("rewardVideosData", value);
			}
		}

		public virtual string DailyCD
		{
			get
			{
				return dailyCD;
			}
			set
			{
				if (!string.Equals(dailyCD, value))
				{
					dailyCD = value;
					Asse.Save("dailyCD", value);
				}
			}
		}

		public IRewardVideo()
		{
			InitData();
		}

		public virtual void SetRewardVideosData(string key, object value)
		{
			Asse.SetValue(rewardVideosData, key, value);
			Asse.Save("rewardVideosData." + key, value);
		}

		public virtual void SaveRewardVideosData()
		{
			Asse.Save("rewardVideosData", rewardVideosData);
		}

		public sealed override void InitData()
		{
			rewardVideosData = new RewardVideosData();
			dailyCD = "";
		}
	}
}
