using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IPropIceBreakingGift : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string lastShowTime;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int afterShowPassLevelCount;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int paidGiftType;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int paidGiftBuyCount;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int coinGiftBuyCount;

		public IPropIceBreakingGift()
		{
			InitData();
		}

		public virtual string LastShowTime
		{
			get
			{
				return lastShowTime;
			}
			set
			{
				if (string.Equals(lastShowTime, value))
				{
					return;
				}
				lastShowTime = value;
				Asse.Save("lastShowTime", value);
			}
		}

		public virtual int AfterShowPassLevelCount
		{
			get
			{
				return afterShowPassLevelCount;
			}
			set
			{
				if (afterShowPassLevelCount != value)
				{
					afterShowPassLevelCount = value;
					Asse.Save("afterShowPassLevelCount", value);
				}
			}
		}

		public virtual int PaidGiftType
		{
			get
			{
				return paidGiftType;
			}
			set
			{
				if (paidGiftType != value)
				{
					paidGiftType = value;
					Asse.Save("paidGiftType", value);
				}
			}
		}

		public virtual int PaidGiftBuyCount
		{
			get
			{
				return paidGiftBuyCount;
			}
			set
			{
				if (paidGiftBuyCount != value)
				{
					paidGiftBuyCount = value;
					Asse.Save("paidGiftBuyCount", value);
				}
			}
		}

		public virtual int CoinGiftBuyCount
		{
			get
			{
				return coinGiftBuyCount;
			}
			set
			{
				if (coinGiftBuyCount != value)
				{
					coinGiftBuyCount = value;
					Asse.Save("coinGiftBuyCount", value);
				}
			}
		}

		public sealed override void InitData()
		{
			lastShowTime = string.Empty;
			afterShowPassLevelCount = -1;
			paidGiftType = 1;
			paidGiftBuyCount = 0;
			coinGiftBuyCount = 0;
		}
	}
}
