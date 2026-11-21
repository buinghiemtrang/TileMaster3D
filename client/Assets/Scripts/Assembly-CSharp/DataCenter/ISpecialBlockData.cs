using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ISpecialBlockData : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int rewardCount;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int availableDailyCount;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int levelCreateCount;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int coexistingCount;

		public ISpecialBlockData()
		{
			InitData();
		}

		public virtual int RewardCount
		{
			get
			{
				return rewardCount;
			}
			set
			{
				if (rewardCount != value)
				{
					rewardCount = value;
					Asse.Save("rewardCount", value);
				}
			}
		}

		public virtual int AvailableDailyCount
		{
			get
			{
				return availableDailyCount;
			}
			set
			{
				if (availableDailyCount != value)
				{
					availableDailyCount = value;
					Asse.Save("availableDailyCount", value);
				}
			}
		}

		public virtual int LevelCreateCount
		{
			get
			{
				return levelCreateCount;
			}
			set
			{
				if (levelCreateCount != value)
				{
					levelCreateCount = value;
					Asse.Save("levelCreateCount", value);
				}
			}
		}

		public virtual int CoexistingCount
		{
			get
			{
				return coexistingCount;
			}
			set
			{
				if (coexistingCount != value)
				{
					coexistingCount = value;
					Asse.Save("coexistingCount", value);
				}
			}
		}

		public sealed override void InitData()
		{
			rewardCount = 0;
			availableDailyCount = 0;
			levelCreateCount = 0;
			coexistingCount = 0;
		}
	}
}
