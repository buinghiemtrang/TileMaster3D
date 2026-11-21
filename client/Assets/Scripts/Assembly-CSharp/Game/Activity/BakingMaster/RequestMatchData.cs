using SimpleJSON;

namespace Game.Activity.BakingMaster
{
	public class RequestMatchData
	{
		public string id;

		public string issue;

		public string teamId;

		public string userId;

		public int levelId;

		public int grade;

		public string extension;

		private string _userName;

		public string GetUserName()
		{
			if (string.IsNullOrEmpty(_userName))
			{
				ParseExtension();
			}
			return _userName;
		}

		public void ParseExtension()
		{
			JSONNode jSONNode = JSONNode.Parse(extension);
			if (jSONNode.HasKey("userName"))
			{
				_userName = jSONNode["userName"];
			}
		}
	}
}
