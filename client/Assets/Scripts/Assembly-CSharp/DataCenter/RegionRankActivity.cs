using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class RegionRankActivity : IRegionRankActivity
	{
		public RegionRankActivityData GetData()
		{
			if (Data.Count == 0)
			{
				Data.Add(new RegionRankActivityData(), true);
			}
			return Data[0];
		}

		public void SaveData()
		{
			Data.Save();
		}
	}
}
