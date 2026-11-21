using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IRewardAdView : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected RewardAdViewData data;

		public virtual RewardAdViewData Data
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

		public IRewardAdView()
		{
			InitData();
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
			data = new RewardAdViewData();
		}
	}
}
