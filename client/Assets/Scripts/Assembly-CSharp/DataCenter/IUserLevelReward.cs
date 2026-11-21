using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IUserLevelReward : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int userLevel;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected int userScore;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int nextLevelId;

		public IUserLevelReward()
		{
			InitData();
		}

		public virtual int UserLevel
		{
			get
			{
				return userLevel;
			}
			set
			{
				if (userLevel != value)
				{
					userLevel = value;
					Asse.Save("userLevel", value);
				}
			}
		}

		public virtual int UserScore
		{
			get
			{
				return userScore;
			}
			set
			{
				if (userScore != value)
				{
					userScore = value;
					Asse.Save("userScore", value);
				}
			}
		}

		public virtual int NextLevelId
		{
			get
			{
				return nextLevelId;
			}
			set
			{
				if (nextLevelId != value)
				{
					nextLevelId = value;
					Asse.Save("nextLevelId", value);
				}
			}
		}

		public sealed override void InitData()
		{
			userLevel = 0;
			userScore = 0;
			nextLevelId = -1;
		}
	}
}
