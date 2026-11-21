using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IHouse3DTask : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected List<int> curHouse3DTaskIDs;

		public DBList<int> CurHouse3DTaskIDs = new DBList<int>();

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected List<int> completeTaskIDs;

		public DBList<int> CompleteTaskIDs = new DBList<int>();

		public IHouse3DTask()
		{
			CurHouse3DTaskIDs.Init(() => curHouse3DTaskIDs, delegate(List<int> value)
			{
				curHouse3DTaskIDs = value;
			}, delegate
			{
				Asse.Save("curHouse3DTaskIDs", curHouse3DTaskIDs);
			});
			CompleteTaskIDs.Init(() => completeTaskIDs, delegate(List<int> value)
			{
				completeTaskIDs = value;
			}, delegate
			{
				Asse.Save("completeTaskIDs", completeTaskIDs);
			});
			InitData();
		}

		public sealed override void InitData()
		{
			curHouse3DTaskIDs = new List<int> { 1 };
			completeTaskIDs = new List<int>();
		}
	}
}
