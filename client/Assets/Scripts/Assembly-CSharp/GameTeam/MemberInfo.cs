namespace GameTeam
{
	public class MemberInfo
	{
		public class Projection
		{
			public User User;

			public Account Account;

			public Team Team;

			public Projection()
			{
				User = new User();
				Account = new Account
				{
					name = ""
				};
				Team = new Team();
			}
		}

		public class User
		{
			public int maxlv;
		}

		public class Account
		{
			public string name;
		}

		public class Team
		{
			public int Helps;

			public bool IsLeader;
		}

		public string userId;

		public Projection projection;
	}
}
