using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IFirebaseAdValue : AbsDataModel
	{
		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<string, float> keyAdTotalValue;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<string, float> keyAdRevenueValue;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<string, float> keyAdPurchaseRevenueValue;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected long keyAdRevenueTiDu001Index;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected float keyAdRevenueTiDu001Value;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected long keyAdRevenueTiDu03Index;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected float keyAdRevenueTiDu03Value;

		public virtual Dictionary<string, float> KeyAdTotalValue
		{
			get
			{
				return keyAdTotalValue;
			}
			set
			{
				keyAdTotalValue = value;
				Asse.Save("keyAdTotalValue", value);
			}
		}

		public virtual Dictionary<string, float> KeyAdRevenueValue
		{
			get
			{
				return keyAdRevenueValue;
			}
			set
			{
				keyAdRevenueValue = value;
				Asse.Save("keyAdRevenueValue", value);
			}
		}

		public virtual Dictionary<string, float> KeyAdPurchaseRevenueValue
		{
			get
			{
				return keyAdPurchaseRevenueValue;
			}
			set
			{
				keyAdPurchaseRevenueValue = value;
				Asse.Save("keyAdPurchaseRevenueValue", value);
			}
		}

		public virtual long KeyAdRevenueTiDu001Index
		{
			get
			{
				return keyAdRevenueTiDu001Index;
			}
			set
			{
				keyAdRevenueTiDu001Index = value;
				Asse.Save("keyAdRevenueTiDu001Index", value);
			}
		}

		public virtual float KeyAdRevenueTiDu001Value
		{
			get
			{
				return keyAdRevenueTiDu001Value;
			}
			set
			{
				if (keyAdRevenueTiDu001Value != value)
				{
					keyAdRevenueTiDu001Value = value;
					Asse.Save("keyAdRevenueTiDu001Value", value);
				}
			}
		}

		public virtual long KeyAdRevenueTiDu03Index
		{
			get
			{
				return keyAdRevenueTiDu03Index;
			}
			set
			{
				keyAdRevenueTiDu03Index = value;
				Asse.Save("keyAdRevenueTiDu03Index", value);
			}
		}

		public virtual float KeyAdRevenueTiDu03Value
		{
			get
			{
				return keyAdRevenueTiDu03Value;
			}
			set
			{
				if (keyAdRevenueTiDu03Value != value)
				{
					keyAdRevenueTiDu03Value = value;
					Asse.Save("keyAdRevenueTiDu03Value", value);
				}
			}
		}

		public IFirebaseAdValue()
		{
			InitData();
		}

		public virtual void SetKeyAdTotalValue(string key, float value)
		{
			keyAdTotalValue[key] = value;
			Asse.Save("keyAdTotalValue." + key, value);
		}

		public virtual void SaveKeyAdTotalValue()
		{
			Asse.Save("keyAdTotalValue", keyAdTotalValue);
		}

		public virtual void SetKeyAdRevenueValue(string key, float value)
		{
			keyAdRevenueValue[key] = value;
			Asse.Save("keyAdRevenueValue." + key, value);
		}

		public virtual void SaveKeyAdRevenueValue()
		{
			Asse.Save("keyAdRevenueValue", keyAdRevenueValue);
		}

		public virtual void SetKeyAdPurchaseRevenueValue(string key, float value)
		{
			keyAdPurchaseRevenueValue[key] = value;
			Asse.Save("keyAdPurchaseRevenueValue." + key, value);
		}

		public virtual void SaveKeyAdPurchaseRevenueValue()
		{
			Asse.Save("keyAdPurchaseRevenueValue", keyAdPurchaseRevenueValue);
		}

		public virtual void SetKeyAdRevenueTiDu001Index(string key, object value)
		{
			Asse.SetValue(keyAdRevenueTiDu001Index, key, value);
			Asse.Save("keyAdRevenueTiDu001Index." + key, value);
		}

		public virtual void SaveKeyAdRevenueTiDu001Index()
		{
			Asse.Save("keyAdRevenueTiDu001Index", keyAdRevenueTiDu001Index);
		}

		public virtual void SetKeyAdRevenueTiDu03Index(string key, object value)
		{
			Asse.SetValue(keyAdRevenueTiDu03Index, key, value);
			Asse.Save("keyAdRevenueTiDu03Index." + key, value);
		}

		public virtual void SaveKeyAdRevenueTiDu03Index()
		{
			Asse.Save("keyAdRevenueTiDu03Index", keyAdRevenueTiDu03Index);
		}

		public sealed override void InitData()
		{
			keyAdTotalValue = new Dictionary<string, float>();
			keyAdRevenueValue = new Dictionary<string, float>();
			keyAdPurchaseRevenueValue = new Dictionary<string, float>();
			keyAdRevenueTiDu001Index = 0L;
			keyAdRevenueTiDu001Value = 0f;
			keyAdRevenueTiDu03Index = 0L;
			keyAdRevenueTiDu03Value = 0f;
		}
	}
}
