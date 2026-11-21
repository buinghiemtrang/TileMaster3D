using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IGiftNoAds : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected string lastRefreshTime;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected int playInterstitialSuccessCount;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected int autoPopCount;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected int isCloseAutoPop;

		public virtual string LastRefreshTime
		{
			get
			{
				return lastRefreshTime;
			}
			set
			{
				if (!string.Equals(lastRefreshTime, value))
				{
					lastRefreshTime = value;
					Asse.Save("lastRefreshTime", value);
				}
			}
		}

		public virtual int PlayInterstitialSuccessCount
		{
			get
			{
				return playInterstitialSuccessCount;
			}
			set
			{
				if (playInterstitialSuccessCount != value)
				{
					playInterstitialSuccessCount = value;
					Asse.Save("playInterstitialSuccessCount", value);
				}
			}
		}

		public virtual int AutoPopCount
		{
			get
			{
				return autoPopCount;
			}
			set
			{
				if (autoPopCount != value)
				{
					autoPopCount = value;
					Asse.Save("autoPopCount", value);
				}
			}
		}

		public virtual int IsCloseAutoPop
		{
			get
			{
				return isCloseAutoPop;
			}
			set
			{
				if (isCloseAutoPop != value)
				{
					isCloseAutoPop = value;
					Asse.Save("isCloseAutoPop", value);
				}
			}
		}

		public IGiftNoAds()
		{
			InitData();
		}

		public sealed override void InitData()
		{
			lastRefreshTime = string.Empty;
			playInterstitialSuccessCount = 0;
			autoPopCount = 0;
			isCloseAutoPop = 0;
		}
	}
}
