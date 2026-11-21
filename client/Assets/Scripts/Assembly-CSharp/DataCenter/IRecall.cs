using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IRecall : AbsDataModel
	{
		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected string offlineStartTime;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int satisfiedGear;

		public IRecall()
		{
			InitData();
		}

		public virtual string OfflineStartTime
		{
			get
			{
				return offlineStartTime;
			}
			set
			{
				if (string.Equals(offlineStartTime, value))
				{
					return;
				}
				offlineStartTime = value;
				Asse.Save("offlineStartTime", value);
			}
		}

		public virtual int SatisfiedGear
		{
			get
			{
				return satisfiedGear;
			}
			set
			{
				if (satisfiedGear != value)
				{
					satisfiedGear = value;
					Asse.Save("satisfiedGear", value);
				}
			}
		}

		public sealed override void InitData()
		{
			offlineStartTime = string.Empty;
			satisfiedGear = 0;
		}
	}
}
