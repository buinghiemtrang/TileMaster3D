using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IChest : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int id;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int num;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected bool isReceived;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected bool isCompleted;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int idLv;

		public IChest()
		{
			InitData();
		}

		public virtual int Id
		{
			get
			{
				return id;
			}
			set
			{
				if (id != value)
				{
					id = value;
					Asse.Save("id", value);
				}
			}
		}

		public virtual int Num
		{
			get
			{
				return num;
			}
			set
			{
				if (num != value)
				{
					num = value;
					Asse.Save("num", value);
				}
			}
		}

		public virtual bool IsReceived
		{
			get
			{
				return isReceived;
			}
			set
			{
				if (isReceived != value)
				{
					isReceived = value;
					Asse.Save("isReceived", value);
				}
			}
		}

		public virtual bool IsCompleted
		{
			get
			{
				return isCompleted;
			}
			set
			{
				if (isCompleted != value)
				{
					isCompleted = value;
					Asse.Save("isCompleted", value);
				}
			}
		}

		public virtual int IdLv
		{
			get
			{
				return idLv;
			}
			set
			{
				if (idLv != value)
				{
					idLv = value;
					Asse.Save("idLv", value);
				}
			}
		}

		public sealed override void InitData()
		{
			id = 0;
			num = 0;
			isReceived = false;
			isCompleted = false;
			idLv = 1;
		}
	}
}
