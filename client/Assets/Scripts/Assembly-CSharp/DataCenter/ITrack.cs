using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ITrack : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int openAppCount;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int lastLoginDay;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int playDays;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string loginTime;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int buyNum;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string buyInterval;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected float payTotalMoney;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int lastTaskID;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int afAdRewardAD;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int afInsertionADs;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected float adsRevenue;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected float adsRevenueMax;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected float lifetimeRevenue;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int consecutiveVictory;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string teamPosition;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int teamLifeReceive;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected float firebaseTotalAdsRevenue;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int afPropHintUse;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int afRevival;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int adsNumBanner;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected long fristloginTimeTicks;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected float afInterEcpmALL;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected float afRewardEcpmALL;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int gameTimes;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected Dictionary<string, int> logKeys;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<string, int> adUnitDisplayCount;

		public ITrack()
		{
			InitData();
		}

		public virtual int OpenAppCount
		{
			get
			{
				return openAppCount;
			}
			set
			{
				if (openAppCount != value)
				{
					openAppCount = value;
					Asse.Save("openAppCount", value);
				}
			}
		}

		public virtual int LastLoginDay
		{
			get
			{
				return lastLoginDay;
			}
			set
			{
				if (lastLoginDay != value)
				{
					lastLoginDay = value;
					Asse.Save("lastLoginDay", value);
				}
			}
		}

		public virtual int PlayDays
		{
			get
			{
				return playDays;
			}
			set
			{
				if (playDays != value)
				{
					playDays = value;
					Asse.Save("playDays", value);
				}
			}
		}

		public virtual string LoginTime
		{
			get
			{
				return loginTime;
			}
			set
			{
				if (string.Equals(loginTime, value))
				{
					return;
				}
				loginTime = value;
				Asse.Save("loginTime", value);
			}
		}

		public virtual int BuyNum
		{
			get
			{
				return buyNum;
			}
			set
			{
				if (buyNum != value)
				{
					buyNum = value;
					Asse.Save("buyNum", value);
				}
			}
		}

		public virtual string BuyInterval
		{
			get
			{
				return buyInterval;
			}
			set
			{
				if (string.Equals(buyInterval, value))
				{
					return;
				}
				buyInterval = value;
				Asse.Save("buyInterval", value);
			}
		}

		public virtual float PayTotalMoney
		{
			get
			{
				return payTotalMoney;
			}
			set
			{
				if (payTotalMoney != value)
				{
					payTotalMoney = value;
					Asse.Save("payTotalMoney", value);
				}
			}
		}

		public virtual int LastTaskID
		{
			get
			{
				return lastTaskID;
			}
			set
			{
				if (lastTaskID != value)
				{
					lastTaskID = value;
					Asse.Save("lastTaskID", value);
				}
			}
		}

		public virtual int AfAdRewardAD
		{
			get
			{
				return afAdRewardAD;
			}
			set
			{
				if (afAdRewardAD != value)
				{
					afAdRewardAD = value;
					Asse.Save("afAdRewardAD", value);
				}
			}
		}

		public virtual int AfInsertionADs
		{
			get
			{
				return afInsertionADs;
			}
			set
			{
				if (afInsertionADs != value)
				{
					afInsertionADs = value;
					Asse.Save("afInsertionADs", value);
				}
			}
		}

		public virtual float AdsRevenue
		{
			get
			{
				return adsRevenue;
			}
			set
			{
				if (adsRevenue != value)
				{
					adsRevenue = value;
					Asse.Save("adsRevenue", value);
				}
			}
		}

		public virtual float AdsRevenueMax
		{
			get
			{
				return adsRevenueMax;
			}
			set
			{
				if (adsRevenueMax != value)
				{
					adsRevenueMax = value;
					Asse.Save("adsRevenueMax", value);
				}
			}
		}

		public virtual float LifetimeRevenue
		{
			get
			{
				return lifetimeRevenue;
			}
			set
			{
				if (lifetimeRevenue != value)
				{
					lifetimeRevenue = value;
					Asse.Save("lifetimeRevenue", value);
				}
			}
		}

		public virtual int ConsecutiveVictory
		{
			get
			{
				return consecutiveVictory;
			}
			set
			{
				if (consecutiveVictory != value)
				{
					consecutiveVictory = value;
					Asse.Save("consecutiveVictory", value);
				}
			}
		}

		public virtual string TeamPosition
		{
			get
			{
				return teamPosition;
			}
			set
			{
				if (string.Equals(teamPosition, value))
				{
					return;
				}
				teamPosition = value;
				Asse.Save("teamPosition", value);
			}
		}

		public virtual int TeamLifeReceive
		{
			get
			{
				return teamLifeReceive;
			}
			set
			{
				if (teamLifeReceive != value)
				{
					teamLifeReceive = value;
					Asse.Save("teamLifeReceive", value);
				}
			}
		}

		public virtual float FirebaseTotalAdsRevenue
		{
			get
			{
				return firebaseTotalAdsRevenue;
			}
			set
			{
				if (firebaseTotalAdsRevenue != value)
				{
					firebaseTotalAdsRevenue = value;
					Asse.Save("firebaseTotalAdsRevenue", value);
				}
			}
		}

		public virtual int AfPropHintUse
		{
			get
			{
				return afPropHintUse;
			}
			set
			{
				if (afPropHintUse != value)
				{
					afPropHintUse = value;
					Asse.Save("afPropHintUse", value);
				}
			}
		}

		public virtual int AfRevival
		{
			get
			{
				return afRevival;
			}
			set
			{
				if (afRevival != value)
				{
					afRevival = value;
					Asse.Save("afRevival", value);
				}
			}
		}

		public virtual int AdsNumBanner
		{
			get
			{
				return adsNumBanner;
			}
			set
			{
				if (adsNumBanner != value)
				{
					adsNumBanner = value;
					Asse.Save("adsNumBanner", value);
				}
			}
		}

		public virtual long FristloginTimeTicks
		{
			get
			{
				return fristloginTimeTicks;
			}
			set
			{
				fristloginTimeTicks = value;
				Asse.Save("fristloginTimeTicks", value);
			}
		}

		public virtual float AfInterEcpmALL
		{
			get
			{
				return afInterEcpmALL;
			}
			set
			{
				if (afInterEcpmALL != value)
				{
					afInterEcpmALL = value;
					Asse.Save("afInterEcpmALL", value);
				}
			}
		}

		public virtual float AfRewardEcpmALL
		{
			get
			{
				return afRewardEcpmALL;
			}
			set
			{
				if (afRewardEcpmALL != value)
				{
					afRewardEcpmALL = value;
					Asse.Save("afRewardEcpmALL", value);
				}
			}
		}

		public virtual int GameTimes
		{
			get
			{
				return gameTimes;
			}
			set
			{
				if (gameTimes != value)
				{
					gameTimes = value;
					Asse.Save("gameTimes", value);
				}
			}
		}

		public virtual Dictionary<string, int> LogKeys
		{
			get
			{
				return logKeys;
			}
			set
			{
				logKeys = value;
				Asse.Save("logKeys", value);
			}
		}

		public virtual Dictionary<string, int> AdUnitDisplayCount
		{
			get
			{
				return adUnitDisplayCount;
			}
			set
			{
				adUnitDisplayCount = value;
				Asse.Save("adUnitDisplayCount", value);
			}
		}

		public virtual void SetFristloginTimeTicks(string key, object value)
		{
			Asse.SetValue<long>(fristloginTimeTicks, key, value);
			Asse.Save("fristloginTimeTicks." + key, value);
		}

		public virtual void SaveFristloginTimeTicks()
		{
			Asse.Save("fristloginTimeTicks", fristloginTimeTicks);
		}

		public virtual void SetLogKeys(string key, int value)
		{
			logKeys[key] = value;
			Asse.Save("logKeys." + key, value);
		}

		public virtual void SaveLogKeys()
		{
			Asse.Save("logKeys", logKeys);
		}

		public virtual void SetAdUnitDisplayCount(string key, int value)
		{
			adUnitDisplayCount[key] = value;
			Asse.Save("adUnitDisplayCount." + key, value);
		}

		public virtual void SaveAdUnitDisplayCount()
		{
			Asse.Save("adUnitDisplayCount", adUnitDisplayCount);
		}

		public sealed override void InitData()
		{
			openAppCount = 0;
			lastLoginDay = 0;
			playDays = 0;
			loginTime = string.Empty;
			buyNum = 0;
			buyInterval = string.Empty;
			payTotalMoney = 0f;
			lastTaskID = -1;
			afAdRewardAD = 0;
			afInsertionADs = 0;
			adsRevenue = 0f;
			adsRevenueMax = 0f;
			lifetimeRevenue = 0f;
			consecutiveVictory = 0;
			teamPosition = "Player";
			teamLifeReceive = 0;
			firebaseTotalAdsRevenue = 0f;
			afPropHintUse = 0;
			afRevival = 0;
			adsNumBanner = 0;
			fristloginTimeTicks = 0;
			afInterEcpmALL = 0f;
			afRewardEcpmALL = 0f;
			gameTimes = 0;
			logKeys = new Dictionary<string, int>();
			adUnitDisplayCount = new Dictionary<string, int>();
		}
	}
}
