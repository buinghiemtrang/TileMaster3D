using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Team : ITeam
	{
		public Team()
		{
			InitData();
		}
	}
}
