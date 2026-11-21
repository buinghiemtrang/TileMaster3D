using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IValueRepeatTrack : AbsDataModel
	{
		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<string, float> fbLogNames;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<string, float> firebaseLogNames;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<string, float> afLogNames;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected string logVersion;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected string firstLoginTime;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected float iaaValue;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int adRewardCount;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int adInterCount;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int adBannerCount;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int sevenPlayDays;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int beforePlayDay;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int levelPassCount;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected float cumRevenueValue;

		public IValueRepeatTrack()
		{
			InitData();
		}

		public virtual Dictionary<string, float> FbLogNames
		{
			get
			{
				return fbLogNames;
			}
			set
			{
				fbLogNames = value;
				Asse.Save("fbLogNames", value);
			}
		}

		public virtual Dictionary<string, float> FirebaseLogNames
		{
			get
			{
				return firebaseLogNames;
			}
			set
			{
				firebaseLogNames = value;
				Asse.Save("firebaseLogNames", value);
			}
		}

		public virtual Dictionary<string, float> AfLogNames
		{
			get
			{
				return afLogNames;
			}
			set
			{
				afLogNames = value;
				Asse.Save("afLogNames", value);
			}
		}

		public virtual string LogVersion
		{
			get
			{
				return logVersion;
			}
			set
			{
				if (string.Equals(logVersion, value))
				{
					return;
				}
				logVersion = value;
				Asse.Save("logVersion", value);
			}
		}

		public virtual string FirstLoginTime
		{
			get
			{
				return firstLoginTime;
			}
			set
			{
				if (string.Equals(firstLoginTime, value))
				{
					return;
				}
				firstLoginTime = value;
				Asse.Save("firstLoginTime", value);
			}
		}

		public virtual float IaaValue
		{
			get
			{
				return iaaValue;
			}
			set
			{
				if (iaaValue != value)
				{
					iaaValue = value;
					Asse.Save("iaaValue", value);
				}
			}
		}

		public virtual int AdRewardCount
		{
			get
			{
				return adRewardCount;
			}
			set
			{
				if (adRewardCount != value)
				{
					adRewardCount = value;
					Asse.Save("adRewardCount", value);
				}
			}
		}

		public virtual int AdInterCount
		{
			get
			{
				return adInterCount;
			}
			set
			{
				if (adInterCount != value)
				{
					adInterCount = value;
					Asse.Save("adInterCount", value);
				}
			}
		}

		public virtual int AdBannerCount
		{
			get
			{
				return adBannerCount;
			}
			set
			{
				if (adBannerCount != value)
				{
					adBannerCount = value;
					Asse.Save("adBannerCount", value);
				}
			}
		}

		public virtual int SevenPlayDays
		{
			get
			{
				return sevenPlayDays;
			}
			set
			{
				if (sevenPlayDays != value)
				{
					sevenPlayDays = value;
					Asse.Save("sevenPlayDays", value);
				}
			}
		}

		public virtual int BeforePlayDay
		{
			get
			{
				return beforePlayDay;
			}
			set
			{
				if (beforePlayDay != value)
				{
					beforePlayDay = value;
					Asse.Save("beforePlayDay", value);
				}
			}
		}

		public virtual int LevelPassCount
		{
			get
			{
				return levelPassCount;
			}
			set
			{
				if (levelPassCount != value)
				{
					levelPassCount = value;
					Asse.Save("levelPassCount", value);
				}
			}
		}

		public virtual float CumRevenueValue
		{
			get
			{
				return cumRevenueValue;
			}
			set
			{
				if (cumRevenueValue != value)
				{
					cumRevenueValue = value;
					Asse.Save("cumRevenueValue", value);
				}
			}
		}

		public virtual void SetFbLogNames(string key, float value)
		{
			fbLogNames[key] = value;
			Asse.Save("fbLogNames." + key, value);
		}

		public virtual void SaveFbLogNames()
		{
			Asse.Save("fbLogNames", fbLogNames);
		}

		public virtual void SetFirebaseLogNames(string key, float value)
		{
			firebaseLogNames[key] = value;
			Asse.Save("firebaseLogNames." + key, value);
		}

		public virtual void SaveFirebaseLogNames()
		{
			Asse.Save("firebaseLogNames", firebaseLogNames);
		}

		public virtual void SetAfLogNames(string key, float value)
		{
			afLogNames[key] = value;
			Asse.Save("afLogNames." + key, value);
		}

		public virtual void SaveAfLogNames()
		{
			Asse.Save("afLogNames", afLogNames);
		}

		public sealed override void InitData()
		{
			fbLogNames = new Dictionary<string, float>();
			firebaseLogNames = new Dictionary<string, float>();
			afLogNames = new Dictionary<string, float>();
			logVersion = "0.0.0";
			sevenPlayDays = 0;
			cumRevenueValue = 0f;
			beforePlayDay = -1;
			levelPassCount = 0;
			adInterCount = 0;
			adBannerCount = 0;
			iaaValue = 0f;
			adRewardCount = 0;
		}
	}
}
