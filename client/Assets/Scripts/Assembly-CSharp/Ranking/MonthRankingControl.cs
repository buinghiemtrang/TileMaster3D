using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Ranking
{
	public class MonthRankingControl : RankingControlBase
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

			public Dictionary<string, int> MonthIssueSumStar;

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
		protected class DataPersonalMonthStarRanking
		{
			public string userId;

			public Projection projection;
		}

		[Serializable]
		protected class DataRoot
		{
			public int code;

			public string msg;

			public List<DataPersonalMonthStarRanking> data;
		}

		private const string AddressTemplate = "https://service.fuqiangame.com/ranking/get?query={3}&projection={{\"Account.name\":1,\"User.sumStar\":1,\"User.medal\":1,\"{0}\":1}}&sort={{\"{0}\":{1}}}&limit={2}";

		protected override string StorageKey_Data => "MonthStarRanking.data";

		protected override string StorageKey_RefreshTime => "MonthStarRanking.refreshTime";

		private string MonthKey => DB.User.MonthIssue;

		protected override string Field => string.Format("User.MonthIssueSumStar.{0}", MonthKey);

		public override string Query => string.Format("{{\"{0}\": {{$type: \"number\"}}}}", Field);

		public override int RequestCount => 1000;

		protected override bool Descending => true;

		public override string Address => string.Format("https://service.fuqiangame.com/ranking/get?query={3}&projection={{\"Account.name\":1,\"User.sumStar\":1,\"User.medal\":1,\"{0}\":1}}&sort={{\"{0}\":{1}}}&limit={2}", Field, Descending ? (-1) : 1, RequestCount, Query);

		protected override RankingData GetUserData()
		{
			RankingData rankingData = base.GetUserData();
			if (rankingData != null)
			{
				return rankingData;
			}
			rankingData = new RankingData();
			rankingData.userID = DB.Account.Userid;
			rankingData.name = DB.Account.GetName();
			rankingData.ranking = RequestCount + 1;
			rankingData.limit = RequestCount;
			rankingData.score = DB.User.GetMonthIssueSumStar(MonthKey);
			rankingData.sumScore = DB.User.SumStar;
			rankingData.medal = DB.User.Medal;
			rankingData.rankingType = RankingType.PersonalMonth;
			rankingData.isOwn = true;
			return rankingData;
		}

		private RankingData getLocalOwnRankingData()
		{
			RankingData rankingData = new RankingData();
			rankingData.userID = DB.Account.Userid;
			rankingData.name = DB.Account.GetName();
			rankingData.ranking = RequestCount;
			rankingData.limit = RequestCount;
			rankingData.score = DB.User.GetMonthIssueSumStar(DB.User.MonthIssue);
			rankingData.sumScore = DB.User.SumStar;
			rankingData.medal = DB.User.Medal;
			rankingData.rankingType = RankingType.PersonalMonth;
			rankingData.isOwn = true;
			return rankingData;
		}

		protected override void DeserializeProcessRespondData(string respondContent)
		{
			_data.Clear();
			DataRoot dataRoot = JsonConvert.DeserializeObject<DataRoot>(respondContent);
			for (int i = 0; i < dataRoot.data.Count; i++)
			{
				DataPersonalMonthStarRanking dataPersonalMonthStarRanking = dataRoot.data[i];
				RankingData rankingData = new RankingData();
				rankingData.userID = dataPersonalMonthStarRanking.userId;
				rankingData.isOwn = dataPersonalMonthStarRanking.userId.Equals(DB.Account.Userid);
				string text;
				if (dataPersonalMonthStarRanking.projection.Account != null && !string.IsNullOrEmpty(dataPersonalMonthStarRanking.projection.Account.name))
				{
					text = dataPersonalMonthStarRanking.projection.Account.name;
				}
				else if (rankingData.isOwn)
				{
					text = DB.Account.GetName();
				}
				else
				{
					text = "Player" + dataPersonalMonthStarRanking.userId.Substring(0, 8);
				}
				rankingData.name = text;
				rankingData.medal = dataPersonalMonthStarRanking.projection.User.medal;
				int num = 0;
				if (dataPersonalMonthStarRanking.projection.User.MonthIssueSumStar != null)
				{
					num = dataPersonalMonthStarRanking.projection.User.MonthIssueSumStar[MonthKey];
				}
				int num2 = dataPersonalMonthStarRanking.projection.User.sumStar;
				if (rankingData.isOwn)
				{
					rankingData.score = Math.Max(num, DB.User.GetMonthIssueSumStar(MonthKey));
					num2 = Math.Max(num2, DB.User.SumStar);
				}
				else
				{
					rankingData.score = num;
				}
				rankingData.ranking = i;
				rankingData.sumScore = num2;
				rankingData.rankingType = RankingType.PersonalMonth;
				rankingData.limit = RequestCount;
				_data.Add(rankingData);
			}
			RankingData rankingData2 = _data.FirstOrDefault((RankingData fd) => fd.isOwn);
			RankingData localOwnRankingData = getLocalOwnRankingData();
			if (rankingData2 == null)
			{
				int index = _data.Count - 1;
				if (_data[index].score < localOwnRankingData.score)
				{
					_data.RemoveAt(index);
					_data.Insert(index, localOwnRankingData);
				}
			}
			else
			{
				rankingData2.score = Math.Max(rankingData2.score, localOwnRankingData.score);
			}
			_data.Sort((RankingData a, RankingData b) => b.score - a.score);
			for (int j = 0; j < _data.Count; j++)
			{
				_data[j].ranking = j;
			}
		}
	}
}
