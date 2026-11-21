using System;
using System.Collections.Generic;

namespace TreasureHunt
{
	public class TreasureHuntMemberInfo
	{
		public class Projection
		{
			public Id _id;

			public TreasureHuntActivity TreasureHuntActivity;

			public Account Account;
		}

		public class Id
		{
			public int timestamp;

			public int machine;

			public int pid;

			public int increment;

			public DateTime creationTime;
		}

		public class TreasureHuntActivity
		{
			public Dictionary<string, Dictionary<string, int>> allScores;
		}

		public class Account
		{
			public string name;
		}

		public string userId;

		public Projection projection;
	}
}
