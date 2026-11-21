using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IRegionRankActivity : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected List<RegionRankActivityData> data;

		public DBList<RegionRankActivityData> Data = new DBList<RegionRankActivityData>();

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int lastRanking;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int lastGroup;

		public IRegionRankActivity()
		{
			Data.Init(() => data, delegate(List<RegionRankActivityData> value)
			{
				data = value;
			}, delegate
			{
				Asse.Save("data", data);
			});
			InitData();
		}

		public virtual int LastRanking
		{
			get
			{
				return lastRanking;
			}
			set
			{
				if (lastRanking != value)
				{
					lastRanking = value;
					Asse.Save("lastRanking", value);
				}
			}
		}

		public virtual int LastGroup
		{
			get
			{
				return lastGroup;
			}
			set
			{
				if (lastGroup != value)
				{
					lastGroup = value;
					Asse.Save("lastGroup", value);
				}
			}
		}

		public sealed override void InitData()
		{
			data = new List<RegionRankActivityData>();
			lastRanking = 999;
			lastGroup = 1;
		}
	}
}
