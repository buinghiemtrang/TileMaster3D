using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class RewardAdView : IRewardAdView
	{
		public void Save()
		{
			Asse.Save("data", data);
		}
	}
}
