using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ranking
{
	public class TeamSumRankingControl : RankingControlBase
	{
		[Serializable]
		protected class Team
		{
			public string name;

			public DateTime created;

			public string badge;

			public string description;

			public string type;

			public int requiredLevel;

			public int members;

			public int limit;

			public string id;
		}

		[Serializable]
		protected class DataRanking
		{
			public int sum;

			public Team team;
		}

		[Serializable]
		protected class DataTeamSumStarRanking
		{
			public List<DataRanking> ranking;

			public DataRanking my;
		}

		[Serializable]
		protected class DataRoot
		{
			public int code;

			public string msg;

			public DataTeamSumStarRanking data;
		}

		private const string url = "https://service.fuqiangame.com/team/GetTeamRanking?match={0}&group={1}&sort={2}&lookup={3}&project={4}";

		private string lookup = "{$lookup:{from:'Team',localField:'_id',foreignField:'_id',as:'team'}}";

		private DataRanking myRankingData;

		private int _OwnRanking = 101;

		protected override string StorageKey_Data => "TeamSumStarRanking.data";

		protected override string StorageKey_RefreshTime => "TeamSumStarRanking.refreshTime";

		protected override string Field => "User.sumStar";

		public override int RequestCount => 100;

		protected override bool Descending => true;

		private string match => string.Format("{{$match:{{$and:[{{'Team':{{$exists:true}}}},{{\"{0}\":{{$type:\"number\"}}}}]}}}}", Field);

		private string group => string.Format("{{$group:{{_id:{{'$toObjectId':'$Team.TeamId'}},sum:{{$sum:'${0}'}}}}}}", Field);

		private string sort => string.Format("{{$sort:{{sum:{0}}}}}", Descending ? (-1) : 1);

		private string project => string.Format("{{$project:{{'_id':0}}}}&limit={0}", RequestCount);

		public override string Address => string.Format("https://service.fuqiangame.com/team/GetTeamRanking?match={0}&group={1}&sort={2}&lookup={3}&project={4}", match, group, sort, lookup, project);

		protected override RankingData GetUserData()
		{
			if (myRankingData == null)
			{
				return null;
			}
			RankingData rankingData = new RankingData();
			rankingData.name = ((myRankingData.team == null) ? string.Empty : myRankingData.team.name);
			if (myRankingData.team != null)
			{
				rankingData.teamBadge = myRankingData.team.badge;
			}
			rankingData.score = myRankingData.sum;
			rankingData.ranking = _OwnRanking;
			rankingData.rankingType = RankingType.TeamSum;
			rankingData.limit = RequestCount;
			rankingData.isOwn = true;
			return rankingData;
		}

		protected override void DeserializeProcessRespondData(string respondContent)
		{
			_data.Clear();
			DataRoot dataRoot = JsonConvert.DeserializeObject<DataRoot>(respondContent);
			myRankingData = null;
			_OwnRanking = 101;
			myRankingData = dataRoot.data.my;
			for (int i = 0; i < dataRoot.data.ranking.Count; i++)
			{
				DataRanking dataRanking = dataRoot.data.ranking[i];
				RankingData rankingData = new RankingData();
				rankingData.userID = dataRanking.team.id;
				rankingData.name = dataRanking.team.name;
				rankingData.teamBadge = dataRanking.team.badge;
				rankingData.ranking = i;
				rankingData.rankingType = RankingType.TeamSum;
				rankingData.score = dataRanking.sum;
				rankingData.limit = RequestCount;
				rankingData.isOwn = myRankingData != null && dataRanking.team.id.Equals(myRankingData.team.id);
				_data.Add(rankingData);
				if (dataRoot.data.my != null && dataRoot.data.my.team.id.Equals(rankingData.userID))
				{
					_OwnRanking = rankingData.ranking;
				}
			}
		}
	}
}
