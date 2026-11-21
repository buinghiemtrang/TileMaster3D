using System;

namespace DataCenter
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class DBValueAttribute : Attribute
	{
		public readonly DBValueType type = new DBValueType();

		public DBValueAttribute(DBStoreType kStore, DBPutType kPut)
		{
			type.kStore = kStore;
			type.kPut = kPut;
		}
	}
}
