using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IStoreMultipleActivity : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string lastActivityTime;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected Dictionary<int, int> storeItemBuyCount;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected bool isActivityPushed;

		public virtual string LastActivityTime
		{
			get
			{
				return lastActivityTime;
			}
			set
			{
				if (!string.Equals(lastActivityTime, value))
				{
					lastActivityTime = value;
					Asse.Save("lastActivityTime", value);
				}
			}
		}

		public virtual Dictionary<int, int> StoreItemBuyCount
		{
			get
			{
				return storeItemBuyCount;
			}
			set
			{
				storeItemBuyCount = value;
				Asse.Save("storeItemBuyCount", value);
			}
		}

		public virtual bool IsActivityPushed
		{
			get
			{
				return isActivityPushed;
			}
			set
			{
				if (isActivityPushed != value)
				{
					isActivityPushed = value;
					Asse.Save("isActivityPushed", value);
				}
			}
		}

		public IStoreMultipleActivity()
		{
			InitData();
		}

		public virtual void SetStoreItemBuyCount(int key, int value)
		{
			storeItemBuyCount[key] = value;
			Asse.Save(string.Format("storeItemBuyCount.{0}", key), value);
		}

		public virtual void SaveStoreItemBuyCount()
		{
			Asse.Save("storeItemBuyCount", storeItemBuyCount);
		}

		public sealed override void InitData()
		{
			lastActivityTime = "";
			storeItemBuyCount = new Dictionary<int, int>();
			isActivityPushed = false;
		}
	}
}
