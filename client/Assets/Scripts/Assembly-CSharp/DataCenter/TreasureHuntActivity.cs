using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class TreasureHuntActivity : ITreasureHuntActivity
	{
		[JsonProperty]
		protected Dictionary<string, Dictionary<string, int>> allScores;

		public Dictionary<string, Dictionary<string, int>> AllScores
		{
			get
			{
				return allScores;
			}
			set
			{
				allScores = value;
				Asse.Write("allScores", allScores);
			}
		}

		public TreasureHuntActivity()
		{
			allScores = new Dictionary<string, Dictionary<string, int>>();
		}

		public int SaveScore(string teamId, string issue, int score)
		{
			Dictionary<string, int> dictionary;
			if (!allScores.ContainsKey(teamId))
			{
				dictionary = new Dictionary<string, int>();
				allScores.Add(teamId, dictionary);
			}
			else
			{
				dictionary = allScores[teamId];
			}
			if (!dictionary.ContainsKey(issue))
			{
				dictionary.Add(issue, score);
			}
			else
			{
				dictionary[issue] += score;
			}
			Asse.Write("allScores", allScores);
			Asse.Send("allScores." + teamId + "." + issue, allScores[teamId][issue]);
			return dictionary[issue];
		}

		public int GetScore(string teamId, string issue)
		{
			if (allScores.ContainsKey(teamId) && allScores[teamId].ContainsKey(issue))
			{
				return allScores[teamId][issue];
			}
			return 0;
		}

		public override void ReadLocalData()
		{
			allScores = StorageMgr.GetObject<Dictionary<string, Dictionary<string, int>>>(Asse.GetName() + ".allScores", "{}");
		}

		public override void DelLocalData()
		{
			StorageMgr.DelKey(Asse.GetName() + ".allScores");
		}

		public override void WriteLocalData()
		{
			StorageMgr.SetObject(Asse.GetName() + ".allScores", allScores);
		}
	}
}
