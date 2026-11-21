using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IFacebookAdValue : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected List<string> cacheValue;

		public DBList<string> CacheValue = new DBList<string>();

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected float adCurrentValue;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected float adAndPuechaseCurrentValue;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected float ad2IapValue;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected float ad2IapValueKey;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected float ad2IapValueDelta;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int fbLogVersion;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected float adImpression;

		public IFacebookAdValue()
		{
			CacheValue.Init(() => cacheValue, delegate(List<string> value)
			{
				cacheValue = value;
			}, delegate
			{
				Asse.Save("cacheValue", cacheValue);
			});
			InitData();
		}

		public virtual float AdCurrentValue
		{
			get
			{
				return adCurrentValue;
			}
			set
			{
				if (adCurrentValue != value)
				{
					adCurrentValue = value;
					Asse.Save("adCurrentValue", value);
				}
			}
		}

		public virtual float AdAndPuechaseCurrentValue
		{
			get
			{
				return adAndPuechaseCurrentValue;
			}
			set
			{
				if (adAndPuechaseCurrentValue != value)
				{
					adAndPuechaseCurrentValue = value;
					Asse.Save("adAndPuechaseCurrentValue", value);
				}
			}
		}

		public virtual float Ad2IapValue
		{
			get
			{
				return ad2IapValue;
			}
			set
			{
				if (ad2IapValue != value)
				{
					ad2IapValue = value;
					Asse.Save("ad2IapValue", value);
				}
			}
		}

		public virtual float Ad2IapValueKey
		{
			get
			{
				return ad2IapValueKey;
			}
			set
			{
				if (ad2IapValueKey != value)
				{
					ad2IapValueKey = value;
					Asse.Save("ad2IapValueKey", value);
				}
			}
		}

		public virtual float Ad2IapValueDelta
		{
			get
			{
				return ad2IapValueDelta;
			}
			set
			{
				if (ad2IapValueDelta != value)
				{
					ad2IapValueDelta = value;
					Asse.Save("ad2IapValueDelta", value);
				}
			}
		}

		public virtual int FbLogVersion
		{
			get
			{
				return fbLogVersion;
			}
			set
			{
				if (fbLogVersion != value)
				{
					fbLogVersion = value;
					Asse.Save("fbLogVersion", value);
				}
			}
		}

		public virtual float AdImpression
		{
			get
			{
				return adImpression;
			}
			set
			{
				if (adImpression != value)
				{
					adImpression = value;
					Asse.Save("adImpression", value);
				}
			}
		}

		public sealed override void InitData()
		{
			cacheValue = new List<string>();
			adCurrentValue = 0f;
			adAndPuechaseCurrentValue = 0f;
			ad2IapValue = 0f;
			ad2IapValueKey = 0f;
			adImpression = 0f;
			ad2IapValueDelta = 0f;
			fbLogVersion = 0;
		}
	}
}
