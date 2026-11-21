using BestHTTP;
using UnityEngine;

namespace MonthRanking
{
	public class MonthRankingHttpMgr : HttpMgrBase
	{
		private static MonthRankingHttpMgr _instance;

		private const string GETPersonalRankingActivityKey = "MonthRankingHttp_GetPersonalRankingActivity";

		private const string GETPersonalRankingDataKey = "MonthRankingHttp_GetPersonalRankingData";

		public static MonthRankingHttpMgr I
		{
			get
			{
				if (_instance == null)
				{
					_instance = new GameObject("MonthRankingActivity").AddComponent<MonthRankingHttpMgr>();
				}
				return _instance;
			}
		}

		private void _GetPersonalRankingActivityTest()
		{
			headers.Clear();
			headers.Add("Content-Type", "application/json");
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			headers.Add("CommonServiceToken", "9b5ba0c0419840ccab872275f41e5cc7");
			OwnHttpMgr.I.SendGet("https://service.fuqiangame.com/common/GetTestActivity", headers, delegate(HTTPResponse resp)
			{
				ServerDataRoot<MonthRankingActivityInfo> serverDataRoot = SendInfo<MonthRankingActivityInfo>("MonthRankingHttp_GetPersonalRankingActivity", resp);
				if (serverDataRoot.code == 1 && serverDataRoot.data != null)
				{
					MonthRankingActivityMgr.I.RefreshActivityInfo(serverDataRoot.data);
				}
			}, "MonthRankingHttp_GetPersonalRankingActivity");
		}

		public void GetPersonalRankingActivity()
		{
			headers.Clear();
			headers.Add("Content-Type", "application/json");
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			headers.Add("CommonServiceToken", "9b5ba0c0419840ccab872275f41e5cc7");
			OwnHttpMgr.I.SendGet("https://service.fuqiangame.com/common/GetGeneralActivity?type=WholeMonth", headers, delegate(HTTPResponse resp)
			{
				ServerDataRoot<MonthRankingActivityInfo> serverDataRoot = SendInfo<MonthRankingActivityInfo>("MonthRankingHttp_GetPersonalRankingActivity", resp);
				if (serverDataRoot.code == 1 && serverDataRoot.data != null)
				{
					MonthRankingActivityMgr.I.RefreshActivityInfo(serverDataRoot.data);
				}
			}, "MonthRankingHttp_GetPersonalRankingActivity");
		}

		public void GetPersonalRankingData(string issue)
		{
			if (string.IsNullOrEmpty(issue))
			{
				return;
			}
			headers.Clear();
			string text = "User.MonthIssueSumStar." + issue;
			string text2 = "{\"" + text + "\": {$type: \"number\"}}";
			string url = string.Format("{0}/ranking/get?query={4}&projection={{\"Account.name\":1,\"User.sumStar\":1,\"User.medal\":1,\"{1}\":1}}&sort={{\"{1}\":{2}}}&limit={3}", serverUrl, text, -1, cf_const.monthRankingActivityLimit, text2);
			headers.Add("Content-Type", "application/json");
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			OwnHttpMgr.I.SendGet(url, headers, delegate(HTTPResponse resp)
			{
				ServerDataListRoot<PersonalMonthStarRankingData> serverDataListRoot = SendListInfo<PersonalMonthStarRankingData>("MonthRankingHttp_GetPersonalRankingData", resp);
				if (serverDataListRoot.code == 1 && serverDataListRoot.data != null)
				{
					MonthRankingActivityMgr.I.RefreshRankingData(serverDataListRoot.data);
				}
			}, "MonthRankingHttp_GetPersonalRankingData");
		}
	}
}
