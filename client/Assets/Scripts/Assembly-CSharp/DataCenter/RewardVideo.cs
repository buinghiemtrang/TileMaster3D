using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class RewardVideo : IRewardVideo
	{
		public virtual void SaveData()
		{
			Asse.Save("rewardVideosData", rewardVideosData);
		}
	}
}
