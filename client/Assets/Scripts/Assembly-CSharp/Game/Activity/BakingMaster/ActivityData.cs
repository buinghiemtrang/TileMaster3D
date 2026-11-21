using Newtonsoft.Json;

namespace Game.Activity.BakingMaster
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ActivityData
	{
		[JsonProperty]
		public int reviveCount;

		[JsonProperty]
		public bool[] receive = new bool[5];

		public void InitData()
		{
			reviveCount = 0;
			for (int i = 0; i < receive.Length; i++)
			{
				receive[i] = false;
			}
		}
	}
}
