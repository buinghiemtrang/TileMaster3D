using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IOther : AbsDataModel
	{
		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int interstitialSucNum;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int pigBankGold;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected bool attrack;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected Dictionary<string, GameParkTickets> parkTickets;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int luckyPrizeRewardIndex;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string luckyPrizeRefreshTime;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected List<int> luckyPrizeKind;

		public DBList<int> LuckyPrizeKind = new DBList<int>();

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string roadToRichesEndTime;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int roadToRichesCount;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected int popNetworkBoxCount;

		public IOther()
		{
			LuckyPrizeKind.Init(() => luckyPrizeKind, delegate(List<int> value)
			{
				luckyPrizeKind = value;
			}, delegate
			{
				Asse.Save("luckyPrizeKind", luckyPrizeKind);
			});
			InitData();
		}

		public virtual int InterstitialSucNum
		{
			get
			{
				return interstitialSucNum;
			}
			set
			{
				if (interstitialSucNum != value)
				{
					interstitialSucNum = value;
					Asse.Save("interstitialSucNum", value);
				}
			}
		}

		public virtual int PigBankGold
		{
			get
			{
				return pigBankGold;
			}
			set
			{
				if (pigBankGold != value)
				{
					pigBankGold = value;
					Asse.Save("pigBankGold", value);
				}
			}
		}

		public virtual bool Attrack
		{
			get
			{
				return attrack;
			}
			set
			{
				if (attrack != value)
				{
					attrack = value;
					Asse.Save("attrack", value);
				}
			}
		}

		public virtual Dictionary<string, GameParkTickets> ParkTickets
		{
			get
			{
				return parkTickets;
			}
			set
			{
				parkTickets = value;
				Asse.Save("parkTickets", value);
			}
		}

		public virtual int LuckyPrizeRewardIndex
		{
			get
			{
				return luckyPrizeRewardIndex;
			}
			set
			{
				if (luckyPrizeRewardIndex != value)
				{
					luckyPrizeRewardIndex = value;
					Asse.Save("luckyPrizeRewardIndex", value);
				}
			}
		}

		public virtual string LuckyPrizeRefreshTime
		{
			get
			{
				return luckyPrizeRefreshTime;
			}
			set
			{
				if (string.Equals(luckyPrizeRefreshTime, value))
				{
					return;
				}
				luckyPrizeRefreshTime = value;
				Asse.Save("luckyPrizeRefreshTime", value);
			}
		}

		public virtual string RoadToRichesEndTime
		{
			get
			{
				return roadToRichesEndTime;
			}
			set
			{
				if (string.Equals(roadToRichesEndTime, value))
				{
					return;
				}
				roadToRichesEndTime = value;
				Asse.Save("roadToRichesEndTime", value);
			}
		}

		public virtual int RoadToRichesCount
		{
			get
			{
				return roadToRichesCount;
			}
			set
			{
				if (roadToRichesCount != value)
				{
					roadToRichesCount = value;
					Asse.Save("roadToRichesCount", value);
				}
			}
		}

		public virtual int PopNetworkBoxCount
		{
			get
			{
				return popNetworkBoxCount;
			}
			set
			{
				if (popNetworkBoxCount != value)
				{
					popNetworkBoxCount = value;
					Asse.Save("popNetworkBoxCount", value);
				}
			}
		}

		public virtual void SetParkTickets(string key, GameParkTickets value)
		{
			parkTickets[key] = value;
			Asse.Save("parkTickets." + key, value);
		}

		public virtual void SaveParkTickets()
		{
			Asse.Save("parkTickets", parkTickets);
		}

		public sealed override void InitData()
		{
			attrack = false;
			interstitialSucNum = 0;
			pigBankGold = 0;
			parkTickets = new Dictionary<string, GameParkTickets>();
			luckyPrizeRewardIndex = 0;
			luckyPrizeRefreshTime = string.Empty;
			luckyPrizeKind = new List<int>();
			roadToRichesEndTime = string.Empty;
			roadToRichesCount = 0;
			popNetworkBoxCount = 0;
		}
	}
}
