using System;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ITeam : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected DateTime askEnergyTime;

		public ITeam()
		{
			InitData();
		}

		public virtual DateTime AskEnergyTime
		{
			get
			{
				return askEnergyTime;
			}
			set
			{
				askEnergyTime = value;
				Asse.Save("askEnergyTime", value);
			}
		}

		public virtual void SetAskEnergyTime(string key, object value)
		{
			Asse.SetValue<DateTime>(askEnergyTime, key, value);
			Asse.Save("askEnergyTime." + key, value);
		}

		public virtual void SaveAskEnergyTime()
		{
			Asse.Save("askEnergyTime", askEnergyTime);
		}

		public sealed override void InitData()
		{
			askEnergyTime = DateTime.Now;
		}
	}
}
