using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IBag : AbsDataModel
	{
		[DBValue(DBStoreType.None, DBPutType.Remote)]
		[JsonProperty]
		protected Dictionary<string, int> prop;

		[DBValue(DBStoreType.None, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<string, int> useNum;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string energyGainTime;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string energyUnlimitedTime;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected List<int> medal;

		public DBList<int> Medal = new DBList<int>();

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected List<int> newGetMedals;

		public DBList<int> NewGetMedals = new DBList<int>();

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected Dictionary<string, int> useTotalNum;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected int isGetNoviceProp;

		public IBag()
		{
			Medal.Init(() => medal, delegate(List<int> value)
			{
				medal = value;
			}, delegate
			{
				Asse.Save("medal", medal);
			});
			NewGetMedals.Init(() => newGetMedals, delegate(List<int> value)
			{
				newGetMedals = value;
			}, delegate
			{
				Asse.Save("newGetMedals", newGetMedals);
			});
			InitData();
		}

		public virtual Dictionary<string, int> Prop
		{
			get
			{
				return prop;
			}
			set
			{
				prop = value;
			}
		}

		public virtual Dictionary<string, int> UseNum
		{
			get
			{
				return useNum;
			}
			set
			{
				useNum = value;
			}
		}

		public virtual string EnergyGainTime
		{
			get
			{
				return energyGainTime;
			}
			set
			{
				if (string.Equals(energyGainTime, value))
				{
					return;
				}
				energyGainTime = value;
				Asse.Save("energyGainTime", value);
			}
		}

		public virtual string EnergyUnlimitedTime
		{
			get
			{
				return energyUnlimitedTime;
			}
			set
			{
				if (string.Equals(energyUnlimitedTime, value))
				{
					return;
				}
				energyUnlimitedTime = value;
				Asse.Save("energyUnlimitedTime", value);
			}
		}

		public virtual Dictionary<string, int> UseTotalNum
		{
			get
			{
				return useTotalNum;
			}
			set
			{
				useTotalNum = value;
				Asse.Save("useTotalNum", value);
			}
		}

		public virtual int IsGetNoviceProp
		{
			get
			{
				return isGetNoviceProp;
			}
			set
			{
				if (isGetNoviceProp != value)
				{
					isGetNoviceProp = value;
					Asse.Save("isGetNoviceProp", value);
				}
			}
		}

		public virtual void SetUseTotalNum(string key, int value)
		{
			useTotalNum[key] = value;
			Asse.Save("useTotalNum." + key, value);
		}

		public virtual void SaveUseTotalNum()
		{
			Asse.Save("useTotalNum", useTotalNum);
		}

		public sealed override void InitData()
		{
			prop = new Dictionary<string, int>
			{
				{ PropID.Money.ToString(), 0 },
				{ PropID.Star.ToString(), 0 },
				{ PropID.Energy.ToString(), 0 },
				{ PropID.Hint.ToString(), 0 },
				{ PropID.Freeze.ToString(), 0 },
				{ PropID.Magnet.ToString(), 0 },
				{ PropID.Back.ToString(), 0 },
				{ PropID.Refresh.ToString(), 0 },
				{ PropID.Resurgence.ToString(), 0 },
				{ PropID.EnergyRecoverUpLimit.ToString(), 0 }
			};
			useNum = new Dictionary<string, int>();
			energyGainTime = string.Empty;
			energyUnlimitedTime = string.Empty;
			medal = new List<int>();
			newGetMedals = new List<int>();
			useTotalNum = new Dictionary<string, int>();
			isGetNoviceProp = 0;
		}
	}
}
