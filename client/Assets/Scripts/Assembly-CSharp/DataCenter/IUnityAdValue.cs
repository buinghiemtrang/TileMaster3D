using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IUnityAdValue : AbsDataModel
	{
		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected float ad2IapValue;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected float ad2IapValueKey;

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

		public IUnityAdValue()
		{
			InitData();
		}

		public sealed override void InitData()
		{
			ad2IapValue = 0f;
			ad2IapValueKey = 0f;
		}
	}
}
