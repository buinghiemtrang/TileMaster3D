using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IGiftStalls : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected int stalls;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected DateTime refreshTime;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected int buyCount;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<string, int> showGiftButtonData;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<string, int> showGiftButtonTypeData;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected List<int> recordStoreIndexData;

		public DBList<int> RecordStoreIndexData = new DBList<int>();

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<string, GiftData> recordGiftData;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<string, int> giftTypeIndexData;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<string, int> giftUnlockData;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<string, string> giftBuyData;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int offlineDays;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected DateTime lastLoseFocusTime;

		public IGiftStalls()
		{
			RecordStoreIndexData.Init(() => recordStoreIndexData, delegate(List<int> value)
			{
				recordStoreIndexData = value;
			}, delegate
			{
				Asse.Save("recordStoreIndexData", recordStoreIndexData);
			});
			InitData();
		}

		public virtual int Stalls
		{
			get
			{
				return stalls;
			}
			set
			{
				if (stalls != value)
				{
					stalls = value;
					Asse.Save("stalls", value);
				}
			}
		}

		public virtual DateTime RefreshTime
		{
			get
			{
				return refreshTime;
			}
			set
			{
				refreshTime = value;
				Asse.Save("refreshTime", value);
			}
		}

		public virtual int BuyCount
		{
			get
			{
				return buyCount;
			}
			set
			{
				if (buyCount != value)
				{
					buyCount = value;
					Asse.Save("buyCount", value);
				}
			}
		}

		public virtual Dictionary<string, int> ShowGiftButtonData
		{
			get
			{
				return showGiftButtonData;
			}
			set
			{
				showGiftButtonData = value;
				Asse.Save("showGiftButtonData", value);
			}
		}

		public virtual Dictionary<string, int> ShowGiftButtonTypeData
		{
			get
			{
				return showGiftButtonTypeData;
			}
			set
			{
				showGiftButtonTypeData = value;
				Asse.Save("showGiftButtonTypeData", value);
			}
		}

		public virtual Dictionary<string, GiftData> RecordGiftData
		{
			get
			{
				return recordGiftData;
			}
			set
			{
				recordGiftData = value;
				Asse.Save("recordGiftData", value);
			}
		}

		public virtual Dictionary<string, int> GiftTypeIndexData
		{
			get
			{
				return giftTypeIndexData;
			}
			set
			{
				giftTypeIndexData = value;
				Asse.Save("giftTypeIndexData", value);
			}
		}

		public virtual Dictionary<string, int> GiftUnlockData
		{
			get
			{
				return giftUnlockData;
			}
			set
			{
				giftUnlockData = value;
				Asse.Save("giftUnlockData", value);
			}
		}

		public virtual Dictionary<string, string> GiftBuyData
		{
			get
			{
				return giftBuyData;
			}
			set
			{
				giftBuyData = value;
				Asse.Save("giftBuyData", value);
			}
		}

		public virtual int OfflineDays
		{
			get
			{
				return offlineDays;
			}
			set
			{
				if (offlineDays != value)
				{
					offlineDays = value;
					Asse.Save("offlineDays", value);
				}
			}
		}

		public virtual DateTime LastLoseFocusTime
		{
			get
			{
				return lastLoseFocusTime;
			}
			set
			{
				lastLoseFocusTime = value;
				Asse.Save("lastLoseFocusTime", value);
			}
		}

		public virtual void SetRefreshTime(string key, object value)
		{
			Asse.SetValue(refreshTime, key, value);
			Asse.Save("refreshTime." + key, value);
		}

		public virtual void SaveRefreshTime()
		{
			Asse.Save("refreshTime", refreshTime);
		}

		public virtual void SetShowGiftButtonData(string key, int value)
		{
			showGiftButtonData[key] = value;
			Asse.Save("showGiftButtonData." + key, value);
		}

		public virtual void SaveShowGiftButtonData()
		{
			Asse.Save("showGiftButtonData", showGiftButtonData);
		}

		public virtual void SetShowGiftButtonTypeData(string key, int value)
		{
			showGiftButtonTypeData[key] = value;
			Asse.Save("showGiftButtonTypeData." + key, value);
		}

		public virtual void SaveShowGiftButtonTypeData()
		{
			Asse.Save("showGiftButtonTypeData", showGiftButtonTypeData);
		}

		public virtual void SetRecordGiftData(string key, GiftData value)
		{
			recordGiftData[key] = value;
			Asse.Save("recordGiftData." + key, value);
		}

		public virtual void SaveRecordGiftData()
		{
			Asse.Save("recordGiftData", recordGiftData);
		}

		public virtual void SetGiftTypeIndexData(string key, int value)
		{
			giftTypeIndexData[key] = value;
			Asse.Save("giftTypeIndexData." + key, value);
		}

		public virtual void SaveGiftTypeIndexData()
		{
			Asse.Save("giftTypeIndexData", giftTypeIndexData);
		}

		public virtual void SetGiftUnlockData(string key, int value)
		{
			giftUnlockData[key] = value;
			Asse.Save("giftUnlockData." + key, value);
		}

		public virtual void SaveGiftUnlockData()
		{
			Asse.Save("giftUnlockData", giftUnlockData);
		}

		public virtual void SetGiftBuyData(string key, string value)
		{
			giftBuyData[key] = value;
			Asse.Save("giftBuyData." + key, value);
		}

		public virtual void SaveGiftBuyData()
		{
			Asse.Save("giftBuyData", giftBuyData);
		}

		public virtual void SetLastLoseFocusTime(string key, object value)
		{
			Asse.SetValue(lastLoseFocusTime, key, value);
			Asse.Save("lastLoseFocusTime." + key, value);
		}

		public virtual void SaveLastLoseFocusTime()
		{
			Asse.Save("lastLoseFocusTime", lastLoseFocusTime);
		}

		public sealed override void InitData()
		{
			stalls = 0;
			refreshTime = DateTime.Now;
			buyCount = 0;
			showGiftButtonData = new Dictionary<string, int>();
			showGiftButtonTypeData = new Dictionary<string, int>();
			recordStoreIndexData = new List<int>();
			recordGiftData = new Dictionary<string, GiftData>();
			giftTypeIndexData = new Dictionary<string, int>();
			giftUnlockData = new Dictionary<string, int>();
			giftBuyData = new Dictionary<string, string>();
			offlineDays = 0;
			lastLoseFocusTime = DateTime.Today;
		}
	}
}
