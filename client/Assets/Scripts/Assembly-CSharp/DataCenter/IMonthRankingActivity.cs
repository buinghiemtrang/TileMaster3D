using System.Collections.Generic;
using MonthRanking;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IMonthRankingActivity : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected bool isAutoEjected;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected bool isReceived;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected bool isCanReceive;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected bool isAvailable;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string issue;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected int state;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected string startTime;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected string endTime;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected List<RankingData> personalMonthStarRankingDatas;

		public DBList<RankingData> PersonalMonthStarRankingDatas = new DBList<RankingData>();

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected int rewardCfgGroupId;

		public IMonthRankingActivity()
		{
			PersonalMonthStarRankingDatas.Init(() => personalMonthStarRankingDatas, delegate(List<RankingData> value)
			{
				personalMonthStarRankingDatas = value;
			}, delegate
			{
				Asse.Save("personalMonthStarRankingDatas", personalMonthStarRankingDatas);
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

		public virtual bool IsAvailable
		{
			get
			{
				return isAvailable;
			}
			set
			{
				if (isAvailable != value)
				{
					isAvailable = value;
					Asse.Save("isAvailable", value);
				}
			}
		}

		public virtual string Issue
		{
			get
			{
				return issue;
			}
			set
			{
				if (string.Equals(issue, value))
				{
					return;
				}
				issue = value;
				Asse.Save("issue", value);
			}
		}

		public virtual int State
		{
			get
			{
				return state;
			}
			set
			{
				if (state != value)
				{
					state = value;
					Asse.Save("state", value);
				}
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

		public virtual int RewardCfgGroupId
		{
			get
			{
				return rewardCfgGroupId;
			}
			set
			{
				if (rewardCfgGroupId != value)
				{
					rewardCfgGroupId = value;
					Asse.Save("rewardCfgGroupId", value);
				}
			}
		}

		public sealed override void InitData()
		{
			isAutoEjected = false;
			isReceived = false;
			isCanReceive = false;
			isAvailable = false;
			issue = string.Empty;
			state = 0;
			startTime = string.Empty;
			endTime = string.Empty;
			personalMonthStarRankingDatas = new List<RankingData>();
			rewardCfgGroupId = 1;
		}
	}
}
