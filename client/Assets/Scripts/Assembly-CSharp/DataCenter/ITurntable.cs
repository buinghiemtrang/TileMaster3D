using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ITurntable : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int showTimes;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected List<int> currentGetRewardIds;

		public DBList<int> CurrentGetRewardIds = new DBList<int>();

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int watchAdTimesOneDay;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected long dailyInitTime;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int lastOpenTurntableLevel;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected bool hasDailyShowPop;

		public ITurntable()
		{
			CurrentGetRewardIds.Init(() => currentGetRewardIds, delegate(List<int> value)
			{
				currentGetRewardIds = value;
			}, delegate
			{
				Asse.Save("currentGetRewardIds", currentGetRewardIds);
			});
			InitData();
		}

		public virtual int ShowTimes
		{
			get
			{
				return showTimes;
			}
			set
			{
				if (showTimes != value)
				{
					showTimes = value;
					Asse.Save("showTimes", value);
				}
			}
		}

		public virtual int WatchAdTimesOneDay
		{
			get
			{
				return watchAdTimesOneDay;
			}
			set
			{
				if (watchAdTimesOneDay != value)
				{
					watchAdTimesOneDay = value;
					Asse.Save("watchAdTimesOneDay", value);
				}
			}
		}

		public virtual long DailyInitTime
		{
			get
			{
				return dailyInitTime;
			}
			set
			{
				dailyInitTime = value;
				Asse.Save("dailyInitTime", value);
			}
		}

		public virtual int LastOpenTurntableLevel
		{
			get
			{
				return lastOpenTurntableLevel;
			}
			set
			{
				if (lastOpenTurntableLevel != value)
				{
					lastOpenTurntableLevel = value;
					Asse.Save("lastOpenTurntableLevel", value);
				}
			}
		}

		public virtual bool HasDailyShowPop
		{
			get
			{
				return hasDailyShowPop;
			}
			set
			{
				if (hasDailyShowPop != value)
				{
					hasDailyShowPop = value;
					Asse.Save("hasDailyShowPop", value);
				}
			}
		}

		public virtual void SetDailyInitTime(string key, object value)
		{
			Asse.SetValue(dailyInitTime, key, value);
			Asse.Save("dailyInitTime." + key, value);
		}

		public virtual void SaveDailyInitTime()
		{
			Asse.Save("dailyInitTime", dailyInitTime);
		}

		public sealed override void InitData()
		{
			showTimes = 0;
			currentGetRewardIds = new List<int>();
			watchAdTimesOneDay = 0;
			dailyInitTime = 0L;
			lastOpenTurntableLevel = 0;
			hasDailyShowPop = false;
		}
	}
}
