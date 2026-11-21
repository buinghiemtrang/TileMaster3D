using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Ranking
{
	public class StarRankingControl : RankingControlBase
	{
		[Serializable]
		protected class Id
		{
			public int timestamp;

			public int machine;

			public int pid;

			public int increment;

			public DateTime creationTime;
		}

		[Serializable]
		protected class Account
		{
			public string name;

			public string photoUrl;
		}

		[Serializable]
		protected class User
		{
			public int sumStar;

			public int medal;
		}

		[Serializable]
		protected class Projection
		{
			public Id _id;

			public Account Account;

			public User User;
		}

		[Serializable]
		protected class DataPersonalSumStarRanking
		{
			public string userId;

			public Projection projection;
		}

		[Serializable]
		protected class DataRoot
		{
			public int code;

			public string msg;

			public List<DataPersonalSumStarRanking> data;
		}

		private const string AddressTemplate = "https://service.fuqiangame.com/ranking/get?query={3}&projection={{\"Account.name\":1,\"User.medal\":1,\"{0}\":1}}&sort={{\"{0}\":{1}}}&limit={2}";

		protected override string StorageKey_Data => "SumStarRanking.data";

		protected override string StorageKey_RefreshTime => "SumStarRanking.refreshTime";

		protected override string Field => "User.sumStar";

		public override string Query => "{\"$expr\":{\"$lt\":[\"$User.sumStar\",{\"$multiply\":[{$toInt:'$User.maxlv'},2000]}]}}";

		public override int RequestCount => 1000;

		protected override bool Descending => true;

		public override string Address => string.Format(AddressTemplate, Field, Descending ? (-1) : 1, RequestCount, Query);

		protected override RankingData GetUserData()
		{
			RankingData rankingData = base.GetUserData();
			if (rankingData != null)
			{
				return rankingData;
			}
			return new RankingData
			{
				userID = DB.Account.Userid,
				name = DB.Account.GetName(),
				ranking = RequestCount + 1,
				limit = RequestCount,
				score = DB.User.SumStar,
				sumScore = DB.User.SumStar,
				rankingType = RankingType.PersonalSum,
				medal = DB.User.Medal,
				isOwn = true
			};
		}

		private RankingData getLocalOwnRankingData()
		{
			return new RankingData
			{
				userID = DB.Account.Userid,
				name = DB.Account.GetName(),
				ranking = RequestCount,
				limit = RequestCount,
				score = DB.User.SumStar,
				sumScore = DB.User.SumStar,
				rankingType = RankingType.PersonalSum,
				medal = DB.User.Medal,
				isOwn = true
			};
		}

		protected override void DeserializeProcessRespondData(string respondContent)
		{
			_data.Clear();
			DataRoot dataRoot = JsonConvert.DeserializeObject<DataRoot>(respondContent);
			for (int i = 0; i < dataRoot.data.Count; i++)
			{
				DataPersonalSumStarRanking dataPersonalSumStarRanking = dataRoot.data[i];
				RankingData rankingData = new RankingData();
				rankingData.userID = dataPersonalSumStarRanking.userId;
				rankingData.isOwn = dataPersonalSumStarRanking.userId.Equals(DB.Account.Userid);
				if (dataPersonalSumStarRanking.projection.Account == null || string.IsNullOrEmpty(dataPersonalSumStarRanking.projection.Account.name))
				{
					if (rankingData.isOwn)
					{
						rankingData.name = DB.Account.GetName();
					}
					else
					{
						rankingData.name = "Player" + dataPersonalSumStarRanking.userId.Substring(0, 8);
					}
				}
				else
				{
					rankingData.name = dataPersonalSumStarRanking.projection.Account.name;
				}
				rankingData.medal = dataPersonalSumStarRanking.projection.User.medal;
				if (!rankingData.isOwn)
				{
					rankingData.score = dataPersonalSumStarRanking.projection.User.sumStar;
					rankingData.sumScore = dataPersonalSumStarRanking.projection.User.sumStar;
				}
				else
				{
					rankingData.score = DB.User.SumStar;
					rankingData.sumScore = DB.User.SumStar;
				}
				rankingData.ranking = i;
				rankingData.rankingType = RankingType.PersonalSum;
				rankingData.limit = RequestCount;
				_data.Add(rankingData);
			}
			RankingData rankingData2 = _data.FirstOrDefault((RankingData fd) => fd.isOwn);
			RankingData rankingData3 = getLocalOwnRankingData();
			if (rankingData2 == null)
			{
				int index = _data.Count - 1;
				if (_data[index].score < rankingData3.score)
				{
					_data.RemoveAt(index);
					_data.Insert(index, rankingData3);
				}
			}
			else
			{
				rankingData2.score = Math.Max(rankingData2.score, rankingData3.score);
			}
			_data.Sort((RankingData a, RankingData b) => b.score - a.score);
			for (int j = 0; j < _data.Count; j++)
			{
				_data[j].ranking = j;
			}
		}
	}
}
