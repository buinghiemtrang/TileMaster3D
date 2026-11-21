using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IAchieve : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected Dictionary<string, int> reachDatas;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected Dictionary<string, int> achieveDatas;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected Dictionary<string, bool> tipsDatas;

		public IAchieve()
		{
			InitData();
		}

		public virtual Dictionary<string, int> ReachDatas
		{
			get
			{
				return reachDatas;
			}
			set
			{
				reachDatas = value;
				Asse.Save("reachDatas", value);
			}
		}

		public virtual Dictionary<string, int> AchieveDatas
		{
			get
			{
				return achieveDatas;
			}
			set
			{
				achieveDatas = value;
				Asse.Save("achieveDatas", value);
			}
		}

		public virtual Dictionary<string, bool> TipsDatas
		{
			get
			{
				return tipsDatas;
			}
			set
			{
				tipsDatas = value;
				Asse.Save("tipsDatas", value);
			}
		}

		public virtual void SetReachDatas(string key, int value)
		{
			reachDatas[key] = value;
			Asse.Save("reachDatas." + key, value);
		}

		public virtual void SaveReachDatas()
		{
			Asse.Save("reachDatas", reachDatas);
		}

		public virtual void SetAchieveDatas(string key, int value)
		{
			achieveDatas[key] = value;
			Asse.Save("achieveDatas." + key, value);
		}

		public virtual void SaveAchieveDatas()
		{
			Asse.Save("achieveDatas", achieveDatas);
		}

		public virtual void SetTipsDatas(string key, bool value)
		{
			tipsDatas[key] = value;
			Asse.Save("tipsDatas." + key, value);
		}

		public virtual void SaveTipsDatas()
		{
			Asse.Save("tipsDatas", tipsDatas);
		}

		public sealed override void InitData()
		{
			reachDatas = new Dictionary<string, int>();
			achieveDatas = new Dictionary<string, int>();
			tipsDatas = new Dictionary<string, bool>();
		}
	}
}
