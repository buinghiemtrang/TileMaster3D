using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IStarRankingActivity : AbsDataModel
	{
		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected bool isAutoEjected;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected bool isReceived;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected bool isCanReceive;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected string lastRefreshTime;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected string startTime;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected string endTime;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected List<int> rewardIds;

		public DBList<int> RewardIds = new DBList<int>();

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected List<int> aIStars;

		public DBList<int> AIStars = new DBList<int>();

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected List<string> aINames;

		public DBList<string> AINames = new DBList<string>();

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int sumStar;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int level;

		public IStarRankingActivity()
		{
			RewardIds.Init(() => rewardIds, delegate(List<int> value)
			{
				rewardIds = value;
			}, delegate
			{
				Asse.Save("rewardIds", rewardIds);
			});
			AIStars.Init(() => aIStars, delegate(List<int> value)
			{
				aIStars = value;
			}, delegate
			{
				Asse.Save("aIStars", aIStars);
			});
			AINames.Init(() => aINames, delegate(List<string> value)
			{
				aINames = value;
			}, delegate
			{
				Asse.Save("aINames", aINames);
			});
			InitData();
		}

		public virtual bool IsAutoEjected
		{
			get
			{
				return isAutoEjected;
			}
			set
			{
				if (isAutoEjected != value)
				{
					isAutoEjected = value;
					Asse.Save("isAutoEjected", value);
				}
			}
		}

		public virtual bool IsReceived
		{
			get
			{
				return isReceived;
			}
			set
			{
				if (isReceived != value)
				{
					isReceived = value;
					Asse.Save("isReceived", value);
				}
			}
		}

		public virtual bool IsCanReceive
		{
			get
			{
				return isCanReceive;
			}
			set
			{
				if (isCanReceive != value)
				{
					isCanReceive = value;
					Asse.Save("isCanReceive", value);
				}
			}
		}

		public virtual string LastRefreshTime
		{
			get
			{
				return lastRefreshTime;
			}
			set
			{
				if (string.Equals(lastRefreshTime, value))
				{
					return;
				}
				lastRefreshTime = value;
				Asse.Save("lastRefreshTime", value);
			}
		}

		public virtual string StartTime
		{
			get
			{
				return startTime;
			}
			set
			{
				if (string.Equals(startTime, value))
				{
					return;
				}
				startTime = value;
				Asse.Save("startTime", value);
			}
		}

		public virtual string EndTime
		{
			get
			{
				return endTime;
			}
			set
			{
				if (string.Equals(endTime, value))
				{
					return;
				}
				endTime = value;
				Asse.Save("endTime", value);
			}
		}

		public virtual int SumStar
		{
			get
			{
				return sumStar;
			}
			set
			{
				if (sumStar != value)
				{
					sumStar = value;
					Asse.Save("sumStar", value);
				}
			}
		}

		public virtual int Level
		{
			get
			{
				return level;
			}
			set
			{
				if (level != value)
				{
					level = value;
					Asse.Save("level", value);
				}
			}
		}

		public sealed override void InitData()
		{
			isAutoEjected = false;
			isReceived = true;
			isCanReceive = true;
			lastRefreshTime = string.Empty;
			startTime = string.Empty;
			endTime = string.Empty;
			rewardIds = new List<int>();
			aIStars = new List<int>();
			aINames = new List<string>();
			sumStar = 0;
			level = 0;
		}
	}
}
