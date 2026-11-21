using System;
using System.Collections.Generic;
using BestHTTP;

namespace DataCenter
{
	public abstract class AbsDispatchServlet
	{
		protected readonly Dictionary<string, IAssembleDB> dictCls = new Dictionary<string, IAssembleDB>();

		protected string sUid;

		public Dictionary<string, IAssembleDB> DictCls => dictCls;

		public bool bLogined { get; protected set; }

		public T Create<T>(string name = "") where T : AbsDataModel, new()
		{
			AssembleDB<T> assembleDB = new AssembleDB<T>(this, name);
			assembleDB.Data = new T();
			assembleDB.Data.Asse = assembleDB;
			dictCls.Add(assembleDB.GetName(), assembleDB);
			return assembleDB.Data;
		}

		public abstract HTTPRequest Post(string dbname, DataInfo info, Action<bool> cbFinish);
	}
}
