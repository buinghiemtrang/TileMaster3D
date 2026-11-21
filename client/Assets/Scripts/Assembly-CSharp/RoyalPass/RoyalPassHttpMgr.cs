using BestHTTP;
using Newtonsoft.Json;
using UnityEngine;

namespace RoyalPass
{
	public class RoyalPassHttpMgr : HttpMgrBase
	{
		private static RoyalPassHttpMgr _instance;

		private string GETACTIVITY = "RoyalPassHttp_GetActivity";

		private string SENDTEAMREWARD = "RoyalPassHttp_SendTeamReward";

		public static RoyalPassHttpMgr I
		{
			get
			{
				if (_instance == null)
				{
					_instance = new GameObject("RoyalPassActivity").AddComponent<RoyalPassHttpMgr>();
				}
				return _instance;
			}
		}

		public void GetActivity()
		{
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			OwnHttpMgr.I.SendGet("https://service.fuqiangame.com/team/GetPassActivity", headers, delegate(HTTPResponse resp)
			{
				ServerDataRoot<RoyalPassActivityInfo> serverDataRoot = SendInfo<RoyalPassActivityInfo>(GETACTIVITY, resp);
				if (serverDataRoot.code == 1 && serverDataRoot.data != null)
				{
					Singleton<RoyalPassActivityMgr>.I.ActivityData(serverDataRoot.data);
				}
			}, GETACTIVITY);
		}

		public void SendTeamReward()
		{
			if (!Singleton<RoyalPassActivityMgr>.I.JudgeHasState(ActivityState.PaidSuccess) || Singleton<RoyalPassActivityMgr>.I.JudgeHasState(ActivityState.SentRewards))
			{
				return;
			}
			Singleton<RoyalPassActivityMgr>.I.SendReward();
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			headers.Add("Content-Type", "application/json");
			string body = JsonConvert.SerializeObject(new
			{
				UserName = DB.Account.GetName(),
				Medal = DB.User.Medal,
				Type = "RoyalPassActivity",
				Content = "I got the Master Pass!",
				Extension = "{'1':'RoyalPassActivity_Send_Team_Reward'}"
			});
			OwnHttpMgr.I.SendPost("https://service.fuqiangame.com/team/CreateTeamBoard", headers, body, delegate(HTTPResponse resp)
			{
				if (SendInfo(SENDTEAMREWARD, resp).code != 1)
				{
					Singleton<RoyalPassActivityMgr>.I.SentRewardFail();
				}
			}, SENDTEAMREWARD);
		}
	}
}
