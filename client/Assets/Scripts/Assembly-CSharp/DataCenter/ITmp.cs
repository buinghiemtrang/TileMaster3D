using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ITmp : AbsDataModel
	{
		[DBValue(DBStoreType.None, DBPutType.Local)]
		[JsonProperty]
		protected bool firstDay;

		public ITmp()
		{
			InitData();
		}

		public virtual bool FirstDay
		{
			get
			{
				return firstDay;
			}
			set
			{
				firstDay = value;
			}
		}

		public sealed override void InitData()
		{
			firstDay = false;
		}
	}
}
