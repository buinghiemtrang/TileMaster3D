using System.Collections.Generic;
using Newtonsoft.Json;
using TreasureHunt;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ITreasureHuntActivity : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected List<MemberInfo> membersInfo;

		public DBList<MemberInfo> MembersInfo = new DBList<MemberInfo>();

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string issue;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string startTime;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string endTime;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int activityRewardGroupId;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int activityRankingRewardGroupId;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected List<bool> isReceivedActivityReward;

		public DBList<bool> IsReceivedActivityReward = new DBList<bool>();

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int activityState;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string teamId;

		public ITreasureHuntActivity()
		{
			MembersInfo.Init(() => membersInfo, delegate(List<MemberInfo> value)
			{
				membersInfo = value;
			}, delegate
			{
				Asse.Save("membersInfo", membersInfo);
			});
			IsReceivedActivityReward.Init(() => isReceivedActivityReward, delegate(List<bool> value)
			{
				isReceivedActivityReward = value;
			}, delegate
			{
				Asse.Save("isReceivedActivityReward", isReceivedActivityReward);
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

		public virtual int ActivityRewardGroupId
		{
			get
			{
				return activityRewardGroupId;
			}
			set
			{
				if (activityRewardGroupId != value)
				{
					activityRewardGroupId = value;
					Asse.Save("activityRewardGroupId", value);
				}
			}
		}

		public virtual int ActivityRankingRewardGroupId
		{
			get
			{
				return activityRankingRewardGroupId;
			}
			set
			{
				if (activityRankingRewardGroupId != value)
				{
					activityRankingRewardGroupId = value;
					Asse.Save("activityRankingRewardGroupId", value);
				}
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

		public virtual string TeamId
		{
			get
			{
				return teamId;
			}
			set
			{
				if (string.Equals(teamId, value))
				{
					return;
				}
				teamId = value;
				Asse.Save("teamId", value);
			}
		}

		public sealed override void InitData()
		{
			membersInfo = new List<MemberInfo>();
			issue = string.Empty;
			startTime = string.Empty;
			endTime = string.Empty;
			activityRewardGroupId = 0;
			activityRankingRewardGroupId = 0;
			isReceivedActivityReward = new List<bool>();
			activityState = 0;
			teamId = string.Empty;
		}
	}
}
