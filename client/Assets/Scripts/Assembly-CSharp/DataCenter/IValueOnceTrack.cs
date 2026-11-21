using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IValueOnceTrack : AbsDataModel
	{
		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<string, int> fbLogNames;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<string, int> firebaseLogNames;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<string, int> afLogNames;

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
		protected int levelFailCount;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int adRewardCount;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int adInterCount;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int adBannerCount;

		public IValueOnceTrack()
		{
			InitData();
		}

		public virtual Dictionary<string, int> FbLogNames
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

		public virtual Dictionary<string, int> FirebaseLogNames
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

		public virtual Dictionary<string, int> AfLogNames
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

		public virtual int LevelFailCount
		{
			get
			{
				return levelFailCount;
			}
			set
			{
				if (levelFailCount != value)
				{
					levelFailCount = value;
					Asse.Save("levelFailCount", value);
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

		public virtual void SetFbLogNames(string key, int value)
		{
			fbLogNames[key] = value;
			Asse.Save("fbLogNames." + key, value);
		}

		public virtual void SaveFbLogNames()
		{
			Asse.Save("fbLogNames", fbLogNames);
		}

		public virtual void SetFirebaseLogNames(string key, int value)
		{
			firebaseLogNames[key] = value;
			Asse.Save("firebaseLogNames." + key, value);
		}

		public virtual void SaveFirebaseLogNames()
		{
			Asse.Save("firebaseLogNames", firebaseLogNames);
		}

		public virtual void SetAfLogNames(string key, int value)
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
			fbLogNames = new Dictionary<string, int>();
			firebaseLogNames = new Dictionary<string, int>();
			afLogNames = new Dictionary<string, int>();
			logVersion = "0.0.0";
			iaaValue = 0f;
			levelFailCount = 0;
			adRewardCount = 0;
			adInterCount = 0;
			adBannerCount = 0;
		}
	}
}
