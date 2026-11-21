using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IVictoriesActivity : AbsDataModel
	{
		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected string issue;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected string startTime;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected string endTime;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected bool isAutoEjectedActivityDialog;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int rewardId;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int curVictoryNum;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int highestVictoryNum;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int maxVictoryNum;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected List<int> receivedChests;

		public DBList<int> ReceivedChests = new DBList<int>();

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected bool isReceivedEnergy;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected bool isAutoEjectedNoticeDialog;

		public IVictoriesActivity()
		{
			ReceivedChests.Init(() => receivedChests, delegate(List<int> value)
			{
				receivedChests = value;
			}, delegate
			{
				Asse.Save("receivedChests", receivedChests);
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

		public virtual bool IsAutoEjectedActivityDialog
		{
			get
			{
				return isAutoEjectedActivityDialog;
			}
			set
			{
				if (isAutoEjectedActivityDialog != value)
				{
					isAutoEjectedActivityDialog = value;
					Asse.Save("isAutoEjectedActivityDialog", value);
				}
			}
		}

		public virtual int RewardId
		{
			get
			{
				return rewardId;
			}
			set
			{
				if (rewardId != value)
				{
					rewardId = value;
					Asse.Save("rewardId", value);
				}
			}
		}

		public virtual int CurVictoryNum
		{
			get
			{
				return curVictoryNum;
			}
			set
			{
				if (curVictoryNum != value)
				{
					curVictoryNum = value;
					Asse.Save("curVictoryNum", value);
				}
			}
		}

		public virtual int HighestVictoryNum
		{
			get
			{
				return highestVictoryNum;
			}
			set
			{
				if (highestVictoryNum != value)
				{
					highestVictoryNum = value;
					Asse.Save("highestVictoryNum", value);
				}
			}
		}

		public virtual int MaxVictoryNum
		{
			get
			{
				return maxVictoryNum;
			}
			set
			{
				if (maxVictoryNum != value)
				{
					maxVictoryNum = value;
					Asse.Save("maxVictoryNum", value);
				}
			}
		}

		public virtual bool IsReceivedEnergy
		{
			get
			{
				return isReceivedEnergy;
			}
			set
			{
				if (isReceivedEnergy != value)
				{
					isReceivedEnergy = value;
					Asse.Save("isReceivedEnergy", value);
				}
			}
		}

		public virtual bool IsAutoEjectedNoticeDialog
		{
			get
			{
				return isAutoEjectedNoticeDialog;
			}
			set
			{
				if (isAutoEjectedNoticeDialog != value)
				{
					isAutoEjectedNoticeDialog = value;
					Asse.Save("isAutoEjectedNoticeDialog", value);
				}
			}
		}

		public sealed override void InitData()
		{
			issue = string.Empty;
			startTime = string.Empty;
			endTime = string.Empty;
			isAutoEjectedActivityDialog = false;
			rewardId = 0;
			curVictoryNum = 0;
			highestVictoryNum = 0;
			maxVictoryNum = 0;
			receivedChests = new List<int>();
			isReceivedEnergy = false;
			isAutoEjectedNoticeDialog = false;
		}
	}
}
