using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class RoyalPassActivity : IRoyalPassActivity
	{
		public int SaveScore(string issue, int score)
		{
			if (Scores.ContainsKey(issue))
			{
				score = Scores[issue] + score;
			}
			SetScores(issue, score);
			return score;
		}

		public int GetScore(string issue)
		{
			if (!Scores.ContainsKey(issue))
			{
				return 0;
			}
			return Scores[issue];
		}
	}
}
