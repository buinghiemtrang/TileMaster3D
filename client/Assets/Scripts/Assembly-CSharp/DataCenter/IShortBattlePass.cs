using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IShortBattlePass : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected ShortBattlePassData data;

		public IShortBattlePass()
		{
			InitData();
		}

		public virtual ShortBattlePassData Data
		{
			get
			{
				return data;
			}
			set
			{
				data = value;
				Asse.Save("data", value);
			}
		}

		public virtual void SetData(string key, object value)
		{
			Asse.SetValue(data, key, value);
			Asse.Save("data." + key, value);
		}

		public virtual void SaveData()
		{
			Asse.Save("data", data);
		}

		public sealed override void InitData()
		{
			data = new ShortBattlePassData();
		}
	}
}
