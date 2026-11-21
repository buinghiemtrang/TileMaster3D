using System;

namespace TreasureHunt
{
	public class TreasureHuntActivityInfo
	{
		public class TeamInfo
		{
			public string id;

			public string name;

			public DateTime created;

			public string badge;

			public string description;

			public string type;

			public int requiredLevel;

			public int members;

			public int limit;
		}

		public string issue;

		public string startTime;

		public string endTime;

		public string currentTime;

		public long endTimeSpan;

		public bool available;

		public TeamInfo myTeam;
	}
}
