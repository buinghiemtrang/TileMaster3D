using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ISignBoard : AbsDataModel
	{
		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected string lastReceiveTime;

		public ISignBoard()
		{
			InitData();
		}

		public virtual string LastReceiveTime
		{
			get
			{
				return lastReceiveTime;
			}
			set
			{
				if (string.Equals(lastReceiveTime, value))
				{
					return;
				}
				lastReceiveTime = value;
				Asse.Save("lastReceiveTime", value);
			}
		}

		public sealed override void InitData()
		{
			lastReceiveTime = string.Empty;
		}
	}
}
