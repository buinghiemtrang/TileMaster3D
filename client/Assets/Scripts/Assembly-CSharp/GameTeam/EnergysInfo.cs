using System.Collections.Generic;

namespace GameTeam
{
	public class EnergysInfo
	{
		public class EnergyInfo
		{
			public string GUID;

			public string SendUserId;

			public string SendUserName;
		}

		public Dictionary<string, EnergyInfo> Lives;
	}
}
