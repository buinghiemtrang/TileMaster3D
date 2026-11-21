using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ICardSystem : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected Dictionary<int, int> cards;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected Dictionary<int, int> cardKeys;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected Dictionary<int, long> cardBoxCD;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected List<int> getRewardGroup;

		public DBList<int> GetRewardGroup = new DBList<int>();

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected bool isGuid;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int openCardBoxCount;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<int, int> cardPoolCardTimes;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<int, Dictionary<int, int>> cardPoolQualityTimes;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int cardBoxRandomGroupType;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<int, int> cardBoxGroupIndex;

		public ICardSystem()
		{
			GetRewardGroup.Init(() => getRewardGroup, delegate(List<int> value)
			{
				getRewardGroup = value;
			}, delegate
			{
				Asse.Save("getRewardGroup", getRewardGroup);
			});
			InitData();
		}

		public virtual Dictionary<int, int> Cards
		{
			get
			{
				return cards;
			}
			set
			{
				cards = value;
				Asse.Save("cards", value);
			}
		}

		public virtual Dictionary<int, int> CardKeys
		{
			get
			{
				return cardKeys;
			}
			set
			{
				cardKeys = value;
				Asse.Save("cardKeys", value);
			}
		}

		public virtual Dictionary<int, long> CardBoxCD
		{
			get
			{
				return cardBoxCD;
			}
			set
			{
				cardBoxCD = value;
				Asse.Save("cardBoxCD", value);
			}
		}

		public virtual bool IsGuid
		{
			get
			{
				return isGuid;
			}
			set
			{
				if (isGuid != value)
				{
					isGuid = value;
					Asse.Save("isGuid", value);
				}
			}
		}

		public virtual int OpenCardBoxCount
		{
			get
			{
				return openCardBoxCount;
			}
			set
			{
				if (openCardBoxCount != value)
				{
					openCardBoxCount = value;
					Asse.Save("openCardBoxCount", value);
				}
			}
		}

		public virtual Dictionary<int, int> CardPoolCardTimes
		{
			get
			{
				return cardPoolCardTimes;
			}
			set
			{
				cardPoolCardTimes = value;
				Asse.Save("cardPoolCardTimes", value);
			}
		}

		public virtual Dictionary<int, Dictionary<int, int>> CardPoolQualityTimes
		{
			get
			{
				return cardPoolQualityTimes;
			}
			set
			{
				cardPoolQualityTimes = value;
				Asse.Save("cardPoolQualityTimes", value);
			}
		}

		public virtual int CardBoxRandomGroupType
		{
			get
			{
				return cardBoxRandomGroupType;
			}
			set
			{
				if (cardBoxRandomGroupType != value)
				{
					cardBoxRandomGroupType = value;
					Asse.Save("cardBoxRandomGroupType", value);
				}
			}
		}

		public virtual Dictionary<int, int> CardBoxGroupIndex
		{
			get
			{
				return cardBoxGroupIndex;
			}
			set
			{
				cardBoxGroupIndex = value;
				Asse.Save("cardBoxGroupIndex", value);
			}
		}

		public virtual void SetCards(int key, int value)
		{
			cards[key] = value;
			Asse.Save(string.Format("cards.{0}", key), value);
		}

		public virtual void SaveCards()
		{
			Asse.Save("cards", cards);
		}

		public virtual void SetCardKeys(int key, int value)
		{
			cardKeys[key] = value;
			Asse.Save(string.Format("cardKeys.{0}", key), value);
		}

		public virtual void SaveCardKeys()
		{
			Asse.Save("cardKeys", cardKeys);
		}

		public virtual void SetCardBoxCD(int key, long value)
		{
			cardBoxCD[key] = value;
			Asse.Save(string.Format("cardBoxCD.{0}", key), value);
		}

		public virtual void SaveCardBoxCD()
		{
			Asse.Save("cardBoxCD", cardBoxCD);
		}

		public virtual void SetCardPoolCardTimes(int key, int value)
		{
			cardPoolCardTimes[key] = value;
			Asse.Save(string.Format("cardPoolCardTimes.{0}", key), value);
		}

		public virtual void SaveCardPoolCardTimes()
		{
			Asse.Save("cardPoolCardTimes", cardPoolCardTimes);
		}

		public virtual void SetCardPoolQualityTimes(int key, Dictionary<int, int> value)
		{
			cardPoolQualityTimes[key] = value;
			Asse.Save(string.Format("cardPoolQualityTimes.{0}", key), value);
		}

		public virtual void SaveCardPoolQualityTimes()
		{
			Asse.Save("cardPoolQualityTimes", cardPoolQualityTimes);
		}

		public virtual void SetCardBoxGroupIndex(int key, int value)
		{
			cardBoxGroupIndex[key] = value;
			Asse.Save(string.Format("cardBoxGroupIndex.{0}", key), value);
		}

		public virtual void SaveCardBoxGroupIndex()
		{
			Asse.Save("cardBoxGroupIndex", cardBoxGroupIndex);
		}

		public sealed override void InitData()
		{
			cards = new Dictionary<int, int>();
			cardKeys = new Dictionary<int, int>();
			cardBoxCD = new Dictionary<int, long>();
			getRewardGroup = new List<int>();
			isGuid = false;
			openCardBoxCount = 0;
			cardPoolCardTimes = new Dictionary<int, int>();
			cardPoolQualityTimes = new Dictionary<int, Dictionary<int, int>>();
			cardBoxRandomGroupType = -1;
			cardBoxGroupIndex = new Dictionary<int, int>();
		}
	}
}
