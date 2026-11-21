using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Other : IOther
	{
		public GameParkTickets GetParkTickets(GameParkType kType)
		{
			string key = kType.ToString();
			if (!parkTickets.ContainsKey(key))
			{
				return new GameParkTickets();
			}
			return parkTickets[key];
		}
	}
}
