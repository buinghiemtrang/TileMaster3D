using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IRechargeActivity : AbsDataModel
	{
		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected List<int> receivedRewards;

		public DBList<int> ReceivedRewards = new DBList<int>();

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected string issue;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected float cumulativeRecharge;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected string startTime;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected string endTime;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int activityState;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int rewardGroupId;

		public IRechargeActivity()
		{
			ReceivedRewards.Init(() => receivedRewards, delegate(List<int> value)
			{
				receivedRewards = value;
			}, delegate
			{
				Asse.Save("receivedRewards", receivedRewards);
			});
			InitData();
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

		public virtual float CumulativeRecharge
		{
			get
			{
				return cumulativeRecharge;
			}
			set
			{
				if (cumulativeRecharge != value)
				{
					cumulativeRecharge = value;
					Asse.Save("cumulativeRecharge", value);
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

		public virtual int ActivityState
		{
			get
			{
				return activityState;
			}
			set
			{
				if (activityState != value)
				{
					activityState = value;
					Asse.Save("activityState", value);
				}
			}
		}

		public virtual int RewardGroupId
		{
			get
			{
				return rewardGroupId;
			}
			set
			{
				if (rewardGroupId != value)
				{
					rewardGroupId = value;
					Asse.Save("rewardGroupId", value);
				}
			}
		}

		public sealed override void InitData()
		{
			receivedRewards = new List<int>();
			issue = string.Empty;
			cumulativeRecharge = 0f;
			startTime = string.Empty;
			endTime = string.Empty;
			activityState = 0;
			rewardGroupId = 0;
		}
	}
}
