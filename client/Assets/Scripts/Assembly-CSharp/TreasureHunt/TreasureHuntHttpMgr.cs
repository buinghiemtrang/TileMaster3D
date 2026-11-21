using BestHTTP;
using UnityEngine;

namespace TreasureHunt
{
	public class TreasureHuntHttpMgr : HttpMgrBase
	{
		private static TreasureHuntHttpMgr _instance;

		private const string GetTeamMembersKey = "TreasureHuntHttp_GetTeamMembers";

		private const string GetActivityKey = "TreasureHuntHttp_GetActivity";

		private HTTPRequest _getActivityRequest;

		public static TreasureHuntHttpMgr I
		{
			get
			{
				if (_instance == null)
				{
					_instance = new GameObject("TreasureHuntActivity").AddComponent<TreasureHuntHttpMgr>();
				}
				return _instance;
			}
		}

		public void GetActivity()
		{
			headers.Clear();
			string url = "https://service.fuqiangame.com/team/GetActivity";
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			_getActivityRequest = OwnHttpMgr.I.SendGet(url, headers, delegate(HTTPResponse resp)
			{
				_getActivityRequest = null;
				ServerDataRoot<TreasureHuntActivityInfo> serverDataRoot = SendInfo<TreasureHuntActivityInfo>("TreasureHuntHttp_GetActivity", resp);
				if (serverDataRoot.code == 1 && serverDataRoot.data != null)
				{
					Singleton<TreasureHuntActivityMgr>.I.ActivityData(serverDataRoot.data);
				}
			}, "TreasureHuntHttp_GetActivity");
		}

		public void GetTeamMembers()
		{
			headers.Clear();
			string issue = DB.TreasureHuntActivity.Issue;
			string teamId = DB.TreasureHuntActivity.TeamId;
			if (string.IsNullOrEmpty(issue) || string.IsNullOrEmpty(teamId))
			{
				Debug.unityLogger.Log(LogType.Error, "treasure hunt activity get team members fail. ISSUE:" + issue + "\u3001TEAMID:" + teamId);
				return;
			}
			string text = "TreasureHuntActivity.allScores." + teamId + "." + issue;
			string url = string.Format("{0}/ranking/get?query={{$and:[{{\"Team.TeamId\":\"{1}\"}},{{\"{2}\": {{$type: \"number\"}}}}]}}&projection={{\"{2}\":1,\"Account.name\":1}}&sort={{\"{2}\":-1}}&limit={3}", serverUrl, teamId, text, 50);
			headers.Add("Content-Type", "application/json");
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			OwnHttpMgr.I.SendGet(url, headers, delegate(HTTPResponse resp)
			{
				ServerDataListRoot<TreasureHuntMemberInfo> serverDataListRoot = SendListInfo<TreasureHuntMemberInfo>("TreasureHuntHttp_GetTeamMembers", resp);
				if (serverDataListRoot.code == 1 && serverDataListRoot.data != null)
				{
					Singleton<TreasureHuntActivityMgr>.I.MembersData(teamId, issue, serverDataListRoot.data);
				}
			}, "TreasureHuntHttp_GetTeamMembers");
		}

		public bool AbortGetActivityRequest()
		{
			HTTPRequest getActivityRequest = _getActivityRequest;
			if (getActivityRequest != null)
			{
				getActivityRequest.Abort();
			}
			return getActivityRequest != null;
		}
	}
}
