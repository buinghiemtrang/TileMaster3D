using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IBooster : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int curGear;

		public IBooster()
		{
			InitData();
		}

		public virtual int CurGear
		{
			get
			{
				return curGear;
			}
			set
			{
				if (curGear != value)
				{
					curGear = value;
					Asse.Save("curGear", value);
				}
			}
		}

		public sealed override void InitData()
		{
			curGear = 0;
		}
	}
}
