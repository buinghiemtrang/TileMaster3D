using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ISynthetic : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected List<int> unLock;

		public DBList<int> UnLock = new DBList<int>();

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected List<string> unLine;

		public DBList<string> UnLine = new DBList<string>();

		public ISynthetic()
		{
			UnLock.Init(() => unLock, delegate(List<int> value)
			{
				unLock = value;
			}, delegate
			{
				Asse.Save("unLock", unLock);
			});
			UnLine.Init(() => unLine, delegate(List<string> value)
			{
				unLine = value;
			}, delegate
			{
				Asse.Save("unLine", unLine);
			});
			InitData();
		}

		public sealed override void InitData()
		{
			unLock = new List<int>();
			unLine = new List<string>();
		}
	}
}
