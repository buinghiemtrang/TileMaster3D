using System.Collections.Generic;
using Game.Activity.BakingMaster;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IBakingMasterActivity : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected ActivityData data;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected RequestIssueData info;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected LevelData[] levels;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected List<RequestMatchData> loser;

		public DBList<RequestMatchData> Loser = new DBList<RequestMatchData>();

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected int state;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected int cfgId;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected PostMatchGrade gameFailure;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected bool[] unlockLayerStatus;

		public IBakingMasterActivity()
		{
			Loser.Init(() => loser, delegate(List<RequestMatchData> value)
			{
				loser = value;
			}, delegate
			{
				Asse.Save("loser", loser);
			});
			InitData();
		}

		public virtual ActivityData Data
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

		public virtual RequestIssueData Info
		{
			get
			{
				return info;
			}
			set
			{
				info = value;
				Asse.Save("info", value);
			}
		}

		public virtual LevelData[] Levels
		{
			get
			{
				return levels;
			}
			set
			{
				levels = value;
				Asse.Save("levels", value);
			}
		}

		public virtual int State
		{
			get
			{
				return state;
			}
			set
			{
				if (state != value)
				{
					state = value;
					Asse.Save("state", value);
				}
			}
		}

		public virtual int CfgId
		{
			get
			{
				return cfgId;
			}
			set
			{
				if (cfgId != value)
				{
					cfgId = value;
					Asse.Save("cfgId", value);
				}
			}
		}

		public virtual PostMatchGrade GameFailure
		{
			get
			{
				return gameFailure;
			}
			set
			{
				gameFailure = value;
				Asse.Save("gameFailure", value);
			}
		}

		public virtual bool[] UnlockLayerStatus
		{
			get
			{
				return unlockLayerStatus;
			}
			set
			{
				if (unlockLayerStatus != value)
				{
					unlockLayerStatus = value;
					Asse.Save("unlockLayerStatus", value);
				}
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

		public virtual void SetInfo(string key, object value)
		{
			Asse.SetValue(info, key, value);
			Asse.Save("info." + key, value);
		}

		public virtual void SaveInfo()
		{
			Asse.Save("info", info);
		}

		public virtual void SetLevels(string key, object value)
		{
			Asse.SetValue(levels, key, value);
			Asse.Save("levels." + key, value);
		}

		public virtual void SaveLevels()
		{
			Asse.Save("levels", levels);
		}

		public virtual void SetGameFailure(string key, object value)
		{
			Asse.SetValue(gameFailure, key, value);
			Asse.Save("gameFailure." + key, value);
		}

		public virtual void SaveGameFailure()
		{
			Asse.Save("gameFailure", gameFailure);
		}

		public sealed override void InitData()
		{
			data = new ActivityData();
			info = new RequestIssueData();
			levels = new LevelData[15];
			for (int num = levels.Length - 1; num >= 0; num--)
			{
				levels[num] = new LevelData();
			}
			loser = new List<RequestMatchData>();
			state = 0;
			cfgId = 0;
			gameFailure = new PostMatchGrade();
			unlockLayerStatus = new bool[5];
		}
	}
}
