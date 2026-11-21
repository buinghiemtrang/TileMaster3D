using BestHTTP;
using Newtonsoft.Json;
using UnityEngine;

namespace GameTeam
{
	public class TeamHttpMgr : ShareHttpMgr
	{
		public static TeamHttpMgr I;

		public const string GETTEAMSKEY = "TeamHttp_GetTeams";

		public const string SEARCHKEY = "TeamHttp_Search";

		public const string GETTEAMMEMBERSKEY = "TeamHttp_GetTeamMembers";

		public const string CREATETEAMKEY = "TeamHttp_CreateTeam";

		public const string EDITTEAMKEY = "TeamHttp_EditTeam";

		public const string JOINTEAMKEY = "TeamHttp_JoinTeam";

		public const string LEAVETEAMKEY = "TeamHttp_LeaveTeam";

		public const string KICKTEAMKEY = "TeamHttp_KickTeam";

		public const string GETJOINTEAMAPPLIESKEY = "TeamHttp_GetJoinTeamApplies";

		public const string AGREEJOINTEAMAPPLYKEY = "TeamHttp_AgreeJoinTeamApply";

		public const string REJECTJOINTEAMAPPLYKEY = "TeamHttp_RejectJoinTeamApply";

		public const string GETBOARDSKEY = "TeamHttp_GetBoards";

		public const string ASKENERGYKEY = "TeamHttp_AskEnergy";

		public const string ENERGYHELPKEY = "TeamHttp_EnergyHelp";

		public const string GETLASTREQUESTINGENERGYKEY = "TeamHttp_GetLastRequestingEnergy";

		public const string CHAT = "TeamHttp_Chat";

		public const string ADDTEAMBOARDRECEIVER = "AddTeamBoardReceiver";

		protected override void Awake()
		{
			base.Awake();
			I = this;
			OwnHttpMgr.I.Timeout = 5.0;
			OwnHttpMgr.I.ConnectTimeout = 2.0;
		}

		public void GetTeams()
		{
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			OwnHttpMgr.I.SendGet("https://service.fuqiangame.com/team/GetTeams", headers, delegate(HTTPResponse resp)
			{
				SendListInfo<TeamInfo>("TeamHttp_GetTeams", resp);
			}, "TeamHttp_GetTeams");
		}

		public void VagueSearch(string name)
		{
			if (!string.IsNullOrEmpty(name))
			{
				headers.Clear();
				headers.Add("Authorization", "Bearer " + DataManager.I.Token);
				OwnHttpMgr.I.SendGet("https://service.fuqiangame.com/team/GetTeams?name=" + name, headers, delegate(HTTPResponse resp)
				{
					SendListInfo<TeamInfo>("TeamHttp_Search", resp);
				}, "TeamHttp_Search");
			}
			else
			{
				Debug.Log("vague search name cannot empty!");
			}
		}

		public void GetTeamMembers(string teamId, string key = "TeamHttp_GetTeamMembers", int maxlvSort = -1, int limit = 50)
		{
			headers.Clear();
			string url = string.Format("{0}/ranking/get?query={{\"Team.TeamId\":\"{1}\"}}&projection={{\"Account.name\":1,\"Account.photoUrl\":1,\"User.sumStar\":1,\"User.maxlv\":1,\"Team\":1}}&sort={{\"User.maxlv\":{2}}}&limit={3}", "https://service.fuqiangame.com", teamId, maxlvSort, limit);
			headers.Add("Content-Type", "application/json");
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			OwnHttpMgr.I.SendGet(url, headers, delegate(HTTPResponse resp)
			{
				SendListInfo<MemberInfo>(key, resp);
			}, key);
		}

		public void CreateTeam(string name, string badge, string desc, string type, int requiredLevel)
		{
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			headers.Add("Content-Type", "application/json");
			string body = JsonConvert.SerializeObject(new
			{
				name = name,
				badge = badge,
				description = desc,
				type = type,
				requiredLevel = requiredLevel
			});
			OwnHttpMgr.I.SendPost("https://service.fuqiangame.com/team/CreateTeam", headers, body, delegate(HTTPResponse resp)
			{
				SendInfo<TeamInfo>("TeamHttp_CreateTeam", resp);
			}, "TeamHttp_CreateTeam");
		}

		public void EditTeam(string badge, string desc, string type, int requiredLevel)
		{
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			headers.Add("Content-Type", "application/json");
			string body = JsonConvert.SerializeObject(new
			{
				badge = badge,
				description = desc,
				type = type,
				requiredLevel = requiredLevel
			});
			OwnHttpMgr.I.SendPost("https://service.fuqiangame.com/team/UpdateTeam", headers, body, delegate(HTTPResponse resp)
			{
				SendInfo<TeamInfo>("TeamHttp_EditTeam", resp);
			}, "TeamHttp_EditTeam");
		}

		public void JoinTeam(string teamId)
		{
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			headers.Add("Content-Type", "application/json");
			string body = JsonConvert.SerializeObject(new
			{
				TeamId = teamId,
				RequiredLevelField = "User.maxlv",
				UserName = DB.Account.GetName()
			});
			OwnHttpMgr.I.SendPost("https://service.fuqiangame.com/team/JoinTeam", headers, body, delegate(HTTPResponse resp)
			{
				SendInfo("TeamHttp_JoinTeam", resp);
			}, "TeamHttp_JoinTeam");
		}

		public void LeaveTeam(int maxlvSort = -1)
		{
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			headers.Add("Content-Type", "application/json");
			string body = JsonConvert.SerializeObject(new
			{
				AlternateLeaderSort = string.Format("{{'User.maxlv':{0}}}", maxlvSort),
				UserName = DB.Account.GetName()
			});
			OwnHttpMgr.I.SendPost("https://service.fuqiangame.com/team/LeaveTeam", headers, body, delegate(HTTPResponse resp)
			{
				SendInfo("TeamHttp_LeaveTeam", resp);
			}, "TeamHttp_LeaveTeam");
		}

		public void KickTeam(string userId, string userName, int maxlvSort = -1)
		{
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			headers.Add("Content-Type", "application/json");
			string body = JsonConvert.SerializeObject(new
			{
				UserId = userId,
				UserName = userName,
				AlternateLeaderSort = string.Format("{{'User.maxlv':{0}}}", maxlvSort)
			});
			OwnHttpMgr.I.SendPost("https://service.fuqiangame.com/team/KickTeam", headers, body, delegate(HTTPResponse resp)
			{
				SendInfo("TeamHttp_KickTeam", resp);
			}, "TeamHttp_KickTeam");
		}

		public void GetJoinTeamApplies()
		{
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			OwnHttpMgr.I.SendGet("https://service.fuqiangame.com/team/GetJoinTeamApplies", headers, delegate(HTTPResponse resp)
			{
				SendListInfo<ApplyJoinTeamInfo>("TeamHttp_GetJoinTeamApplies", resp);
			}, "TeamHttp_GetJoinTeamApplies");
		}

		public void AgreeJoinTeamApply(string id)
		{
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			headers.Add("Content-Type", "application/json");
			string body = JsonConvert.SerializeObject(new
			{
				GUID = id
			});
			OwnHttpMgr.I.SendPost("https://service.fuqiangame.com/team/AgreeJoinTeamApply", headers, body, delegate(HTTPResponse resp)
			{
				SendInfo("TeamHttp_AgreeJoinTeamApply", resp);
			}, "TeamHttp_AgreeJoinTeamApply");
		}

		public void RejectJoinTeamApply(string id)
		{
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			headers.Add("Content-Type", "application/json");
			string body = JsonConvert.SerializeObject(new
			{
				GUID = id
			});
			OwnHttpMgr.I.SendPost("https://service.fuqiangame.com/team/RejectJoinTeamApply", headers, body, delegate(HTTPResponse resp)
			{
				SendInfo("TeamHttp_RejectJoinTeamApply", resp);
			}, "TeamHttp_RejectJoinTeamApply");
		}

		public void GetBoards(string key = "TeamHttp_GetBoards")
		{
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			OwnHttpMgr.I.SendGet("https://service.fuqiangame.com/team/GetBoards", headers, delegate(HTTPResponse resp)
			{
				SendListInfo<BoardsInfo>(key, resp);
			}, key);
		}

		public void AskEnergy()
		{
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			headers.Add("Content-Type", "application/json");
			string body = JsonConvert.SerializeObject(new
			{
				UserName = DB.Account.GetName(),
				Medal = DB.User.Medal
			});
			OwnHttpMgr.I.SendPost("https://service.fuqiangame.com/team/AskLive", headers, body, delegate(HTTPResponse resp)
			{
				SendInfo("TeamHttp_AskEnergy", resp);
			}, "TeamHttp_AskEnergy");
		}

		public void EnergyHelp(string id)
		{
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			headers.Add("Content-Type", "application/json");
			string body = JsonConvert.SerializeObject(new
			{
				GUID = id,
				UserName = DB.Account.GetName()
			});
			OwnHttpMgr.I.SendPost("https://service.fuqiangame.com/team/Help", headers, body, delegate(HTTPResponse resp)
			{
				SendInfo("TeamHttp_EnergyHelp", resp);
			}, "TeamHttp_EnergyHelp");
		}

		public void GetLastRequestingEnergy()
		{
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			OwnHttpMgr.I.SendGet("https://service.fuqiangame.com/team/GetLastRequestingLift", headers, delegate(HTTPResponse resp)
			{
				SendInfo<ApplyEnergyInfo>("TeamHttp_GetLastRequestingEnergy", resp);
			}, "TeamHttp_GetLastRequestingEnergy");
		}

		public void Chat(string content)
		{
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			headers.Add("Content-Type", "application/json");
			string body = JsonConvert.SerializeObject(new
			{
				UserName = DB.Account.GetName(),
				Content = content,
				Medal = DB.User.Medal
			});
			OwnHttpMgr.I.SendPost("https://service.fuqiangame.com/team/Chat", headers, body, delegate(HTTPResponse resp)
			{
				SendInfo("TeamHttp_Chat", resp);
			}, "TeamHttp_Chat");
		}

		public void AddTeamBoardReceiver(string GUID)
		{
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			headers.Add("Content-Type", "application/json");
			string body = JsonConvert.SerializeObject(new
			{
				GUID = GUID
			});
			OwnHttpMgr.I.SendPost("https://service.fuqiangame.com/team/AddTeamBoardReceiver", headers, body, delegate(HTTPResponse resp)
			{
				SendInfo("AddTeamBoardReceiver", resp);
			}, "AddTeamBoardReceiver");
		}
	}
}
