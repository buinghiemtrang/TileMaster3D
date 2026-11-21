// [rebuild] Fake server: phần TEAM (FAKE_SERVER). Ghi đè route team/* + ranking/get (biến thể thành viên team) qua RegisterTeam.
// Giao thức: docs/net_protocol.md mục 2.4-2.6, 3.1, 3.2, 4, 5.2. Client đọc: GameTeam/*, TeamBoard, RequestEnergyBtn, RedDotHttpMgr, TreasureHuntHttpMgr.
// Trạng thái (FakeServerState):
//   "teams"     {teamId: {id,name,badge,description,type(Open|Close),requiredLevel,limit,created,botJoins,lastBotJoin,botSeq,lastBotAct,
//                         members:[{id,name,bot,leader,helps,maxlv,sumStar,medal}], boards:[BoardInfo...], applies:[{GUID,TeamId,UserId,UserName,Created,bot,spec}]}}
//   "userTeam"  {userId: teamId}      "boardSeen" {userId: ISO lần GetBoards cuối}      "lives" {userId: {guid: {GUID,SendUserId,SendUserName}}}
// Thành viên người chơi (bot=false) lấy maxlv/tên/sumStar từ Doc(s, uid); Team.{TeamId,Helps,IsLeader} được ghi lại vào doc (client không tự ghi).
// Lỗi nghiệp vụ = HTTP 200 + code 0 + msg (client: -1 = lỗi mạng; JoinTeam đọc msg "RequiredLevel"/"Full").
#if FAKE_SERVER
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

public static partial class FakeServerHandlers
{
	private const int TmLimit = 50; // = cf_const.teamMaxMembers

	private const int TmBoardCap = 50;

	private const int TmBotJoinSeconds = 90; // [giả định] team do người chơi làm trưởng: mỗi 90 s có 1 bot xin vào / vào thẳng

	private const int TmBotJoinMax = 8;

	private const int TmBotActSeconds = 150; // [giả định] bot nói chuyện / xin tim mỗi 150 s (khi người chơi đang gọi API team)

	private const int TmBotHelpSeconds = 20; // [giả định] mỗi 20 s một bot cho tim vào yêu cầu của người chơi

	private const int TmApplyApproveSeconds = 15; // [giả định] trưởng team bot duyệt đơn xin vào sau 15 s

	private static readonly string[] TmTeamNames = { "Tile Titans", "Match Masters", "Lucky Cats", "Dragon Rise", "Pixel Pandas", "Zen Sorters", "Night Owls", "Star Hunters" };

	private static readonly string[] TmTeamDescs =
	{
		"Friendly team, everyone is welcome!", "We help each other with lives every day.", "Chill and clear levels together. Request to join.", "Active players only, please.",
		"Join us for big events!", "Casual players, relax and play. Request to join.", "Aiming for the top ranking.", "Newbies welcome, we share tips. Request to join."
	};

	private static readonly string[] TmTeamBadges = { "team-icon-21", "team-icon-1", "team-icon-33", "team-icon-45", "team-icon-8", "team-icon-60", "team-icon-12", "team-icon-27" }; // iconName trong cfg_TeamBadge.json

	private static readonly int[] TmTeamReq = { 0, 0, 0, 0, 50, 0, 100, 0 };

	private static readonly bool[] TmTeamOpen = { true, true, false, true, true, false, true, false };

	private static readonly int[] TmTeamSize = { 18, 12, 9, 15, 20, 6, 14, 5 };

	// Câu bot tránh chữ bị WordsBlock che (vd "ur" có trong danh sách gốc → "our" thành "o**")
	private static readonly string[] TmChatLines = { "Hi everyone!", "Good luck on the next level :)", "Anyone need lives? Ask and I will help.", "That level was so hard!", "GG team", "New event soon, let's go!", "Thanks for the help!", "Who is online?" };

	private sealed class TmMem
	{
		public string Id;

		public string Name;

		public string Photo;

		public int Maxlv;

		public int SumStar;

		public int Helps;

		public int Medal;

		public bool Leader;

		public bool Bot;

		public JObject User;
	}

	static partial void RegisterTeam(Dictionary<string, Func<FakeRequest, FakeServerState, FakeResponse>> r)
	{
		r[Host + "/team/GetTeams"] = TmGetTeams;
		r[Host + "/team/GetMyTeam"] = TmGetMyTeam;
		r[Host + "/team/CreateTeam"] = TmCreateTeam;
		r[Host + "/team/UpdateTeam"] = TmUpdateTeam;
		r[Host + "/team/JoinTeam"] = TmJoinTeam;
		r[Host + "/team/LeaveTeam"] = TmLeaveTeam;
		r[Host + "/team/KickTeam"] = TmKickTeam;
		r[Host + "/team/GetJoinTeamApplies"] = TmGetApplies;
		r[Host + "/team/AgreeJoinTeamApply"] = (q, s) => TmDealApply(q, s, true);
		r[Host + "/team/RejectJoinTeamApply"] = (q, s) => TmDealApply(q, s, false);
		r[Host + "/team/GetBoards"] = TmGetBoards;
		r[Host + "/team/Chat"] = TmChat;
		r[Host + "/team/AskLive"] = TmAskLive;
		r[Host + "/team/Help"] = TmHelp;
		r[Host + "/team/AddTeamBoardReceiver"] = TmAddReceiver;
		r[Host + "/team/CreateTeamBoard"] = TmCreateBoard;
		r[Host + "/team/GetLastRequestingLift"] = TmGetLastRequest;
		r[Host + "/team/GetLives"] = TmGetLives;
		r[Host + "/team/ReceiveLive"] = TmReceiveLive;
		r[Host + "/team/CheckNewTeamBoard"] = TmCheckNewBoard;
		r[Host + "/team/GetTeamRanking"] = TmGetTeamRanking;
		r[Host + "/team/GetActivity"] = TmGetActivity; // Treasure Hunt: thêm myTeam
		r[Host + "/ranking/get"] = TmRankingGet;
	}

	// ---------- tiện ích ----------
	private static FakeResponse TmFail(string msg)
	{
		return Json(new JObject { ["code"] = 0, ["msg"] = msg });
	}

	private static FakeResponse TmUnauth()
	{
		return Json(new JObject { ["code"] = -1, ["msg"] = "unauthorized" });
	}

	private static string TmIso(DateTime d)
	{
		return d.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
	}

	// Newtonsoft biến chuỗi ISO thành token Date khi nạp lại fake_server.json → luôn đọc qua hàm này.
	private static DateTime TmDt(JToken t, DateTime fallback)
	{
		if (t == null || t.Type == JTokenType.Null)
		{
			return fallback;
		}
		if (t.Type == JTokenType.Date)
		{
			return t.Value<DateTime>().ToUniversalTime();
		}
		DateTime d;
		if (DateTime.TryParse((string)t, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out d))
		{
			return d;
		}
		return fallback;
	}

	private static int TmInt(JToken t, int def)
	{
		if (t == null)
		{
			return def;
		}
		if (t.Type == JTokenType.Integer || t.Type == JTokenType.Float)
		{
			return (int)t;
		}
		int v;
		if (t.Type == JTokenType.String && int.TryParse((string)t, out v))
		{
			return v;
		}
		return def;
	}

	private static int? TmNullableInt(JToken t)
	{
		if (t == null || t.Type == JTokenType.Null)
		{
			return null;
		}
		return TmInt(t, 0);
	}

	private static int TmHash(string str)
	{
		uint h = 2166136261u;
		foreach (char c in str)
		{
			h = (h ^ c) * 16777619u;
		}
		return (int)(h & 0x7fffffff);
	}

	private static JObject TmObj(JToken t)
	{
		JObject o = t as JObject;
		if (o != null)
		{
			return o;
		}
		if (t != null && t.Type == JTokenType.String)
		{
			try
			{
				return JObject.Parse((string)t);
			}
			catch (Exception)
			{
			}
		}
		return null;
	}

	private static JObject TmState(FakeServerState s, string key)
	{
		JObject o = s.Get(key) as JObject;
		if (o == null)
		{
			o = new JObject();
			s.Set(key, o);
		}
		return o;
	}

	private static string TmNewId()
	{
		return Guid.NewGuid().ToString("N").Substring(0, 24);
	}

	private static JArray TmMembers(JObject team)
	{
		return (JArray)team["members"];
	}

	private static JArray TmBoards(JObject team)
	{
		return (JArray)team["boards"];
	}

	private static JArray TmApplies(JObject team)
	{
		return (JArray)team["applies"];
	}

	private static JObject TmFindMember(JObject team, string id)
	{
		return TmMembers(team).OfType<JObject>().FirstOrDefault(m => (string)m["id"] == id);
	}

	private static bool TmIsBot(JObject m)
	{
		return (bool?)m["bot"] == true;
	}

	private static bool TmIsLeader(JObject m)
	{
		return (bool?)m["leader"] == true;
	}

	private static JObject TmInfo(JObject t)
	{
		return new JObject
		{
			["id"] = (string)t["id"],
			["name"] = (string)t["name"],
			["created"] = TmIso(TmDt(t["created"], DateTime.UtcNow)),
			["badge"] = (string)t["badge"],
			["description"] = (string)t["description"],
			["type"] = (string)t["type"],
			["requiredLevel"] = TmInt(t["requiredLevel"], 0),
			["members"] = TmMembers(t).Count,
			["limit"] = TmInt(t["limit"], TmLimit)
		};
	}

	// ---------- seed ----------
	private static string TmBotId(int teamIdx, int k)
	{
		return ("b07" + (teamIdx & 0xff).ToString("x2") + k.ToString("x4")).PadRight(24, '0');
	}

	private static JObject TmMakeBot(int teamIdx, int k, int minLv)
	{
		Random rnd = new Random(teamIdx * 1009 + k * 37 + 11);
		int maxlv = minLv + 5 + rnd.Next(0, 60 + (teamIdx % 8) * 10);
		string name = BotNames[(teamIdx * 7 + k * 7) % BotNames.Length] /* k*7: 30 tên khác nhau (k*5 chỉ ra 6 tên) */ + ((k % 2 == 1) ? ((teamIdx * 3 + k) % 90 + 10).ToString() : "");
		return new JObject
		{
			["id"] = TmBotId(teamIdx, k),
			["name"] = name,
			["bot"] = true,
			["leader"] = false,
			["helps"] = rnd.Next(0, 80),
			["maxlv"] = maxlv,
			["sumStar"] = maxlv * rnd.Next(900, 1700), // khớp ràng buộc truy vấn xếp hạng: sumStar < maxlv*2000
			["medal"] = rnd.Next(0, 20)
		};
	}

	private static JObject TmTeams(FakeServerState s)
	{
		JObject teams = s.Get("teams") as JObject;
		if (teams == null)
		{
			teams = new JObject();
			for (int i = 0; i < TmTeamNames.Length; i++)
			{
				JObject t = TmSeedTeam(i);
				teams[(string)t["id"]] = t;
			}
			s.Set("teams", teams);
		}
		return teams;
	}

	private static JObject TmSeedTeam(int i)
	{
		DateTime now = DateTime.UtcNow;
		JObject t = new JObject
		{
			["id"] = ("7ea" + i.ToString("x2")).PadRight(24, '0'),
			["name"] = TmTeamNames[i],
			["badge"] = TmTeamBadges[i],
			["description"] = TmTeamDescs[i],
			["type"] = TmTeamOpen[i] ? "Open" : "Close",
			["requiredLevel"] = TmTeamReq[i],
			["limit"] = TmLimit,
			["created"] = TmIso(now.AddDays(-(10 + i * 9))),
			["botJoins"] = 0,
			["lastBotJoin"] = TmIso(now),
			["botSeq"] = 0,
			["lastBotAct"] = TmIso(now),
			["members"] = new JArray(),
			["boards"] = new JArray(),
			["applies"] = new JArray()
		};
		JArray members = TmMembers(t);
		JObject best = null;
		for (int k = 0; k < TmTeamSize[i]; k++)
		{
			JObject m = TmMakeBot(i, k, TmTeamReq[i]);
			members.Add(m);
			if (best == null || TmInt(m["maxlv"], 0) > TmInt(best["maxlv"], 0))
			{
				best = m;
			}
		}
		best["leader"] = true;
		List<JObject> bots = members.OfType<JObject>().ToList();
		for (int j = 0; j < 4 && j < bots.Count; j++)
		{
			TmAddBoard(t, "JoinTeam", (string)bots[j]["id"], (string)bots[j]["name"], null, now.AddMinutes(-(240 - j * 25)), null);
		}
		for (int j = 0; j < 3; j++)
		{
			JObject b = bots[(i + j * 2) % bots.Count];
			TmAddBoard(t, "Chat", (string)b["id"], (string)b["name"], TmChatLines[(i + j) % TmChatLines.Length], now.AddMinutes(-(90 - j * 20)), TmInt(b["medal"], 0));
		}
		TmAddRequest(t, bots[(i + 1) % bots.Count], 2, now.AddMinutes(-40));
		return t;
	}

	// ---------- bảng tin ----------
	private static JObject TmAddBoard(JObject team, string type, string userId, string userName, string content, DateTime created, int? medal, string extension = null)
	{
		JObject b = new JObject
		{
			["_t"] = type,
			["GUID"] = Guid.NewGuid().ToString("N"),
			["Type"] = type,
			["UserId"] = userId,
			["UserName"] = userName,
			["Created"] = TmIso(created),
			["Medal"] = medal.HasValue ? (JToken)medal.Value : JValue.CreateNull(),
			["Content"] = content,
			["LifeReceived"] = 0,
			["LifeMax"] = 0,
			["LifeSenders"] = new JArray(),
			["EndTime"] = TmIso(created), // DateTime không nullable phía client
			["Extension"] = extension,
			["Receivers"] = new JArray()
		};
		JArray boards = TmBoards(team);
		boards.Add(b);
		while (boards.Count > TmBoardCap)
		{
			boards.RemoveAt(0);
		}
		return b;
	}

	private static JObject TmAddRequest(JObject team, JObject requester, int received, DateTime created)
	{
		JObject b = TmAddBoard(team, "RequestingLife", (string)requester["id"], (string)requester["name"], null, created, TmIsBot(requester) ? TmInt(requester["medal"], 0) : (int?)null);
		b["LifeMax"] = 5;
		b["LifeReceived"] = received;
		b["EndTime"] = TmIso(created.AddHours(4));
		JArray senders = (JArray)b["LifeSenders"];
		foreach (JObject m in TmMembers(team).OfType<JObject>())
		{
			if (senders.Count >= received)
			{
				break;
			}
			if (TmIsBot(m) && (string)m["id"] != (string)requester["id"])
			{
				senders.Add((string)m["id"]);
			}
		}
		b["LifeReceived"] = senders.Count;
		return b;
	}

	private static JObject TmBoardOut(JObject b)
	{
		JObject o = (JObject)b.DeepClone();
		o["Created"] = TmIso(TmDt(b["Created"], DateTime.UtcNow));
		o["EndTime"] = TmIso(TmDt(b["EndTime"], DateTime.UtcNow));
		return o;
	}

	private static bool TmRequestActive(JObject b, DateTime now)
	{
		return (string)b["Type"] == "RequestingLife" && TmDt(b["EndTime"], now) > now && TmInt(b["LifeReceived"], 0) < TmInt(b["LifeMax"], 0);
	}

	private static void TmAddLive(FakeServerState s, string uid, string senderId, string senderName)
	{
		JObject lives = TmState(s, "lives");
		JObject mine = lives[uid] as JObject;
		if (mine == null)
		{
			mine = new JObject();
			lives[uid] = mine;
		}
		string guid = Guid.NewGuid().ToString("N");
		mine[guid] = new JObject { ["GUID"] = guid, ["SendUserId"] = senderId, ["SendUserName"] = senderName };
		s.MarkDirty();
	}

	// Mô phỏng "người chơi khác": bot vào team, bot nói chuyện / xin tim, bot cho tim vào yêu cầu của người chơi. Gọi mỗi khi người chơi đụng API team.
	private static void TmTick(FakeServerState s, JObject team)
	{
		DateTime now = DateTime.UtcNow;
		JArray members = TmMembers(team);
		JObject leader = members.OfType<JObject>().FirstOrDefault(TmIsLeader);
		int limit = TmInt(team["limit"], TmLimit);
		// 1) bot xin vào team do người chơi làm trưởng
		if (leader != null && !TmIsBot(leader))
		{
			int joins = TmInt(team["botJoins"], 0);
			JArray applies = TmApplies(team);
			if (joins < TmBotJoinMax && applies.Count < 3 && members.Count + applies.Count < limit && (now - TmDt(team["lastBotJoin"], now)).TotalSeconds >= TmBotJoinSeconds)
			{
				JObject bot = TmMakeBot(TmHash((string)team["id"]) % 200 + 20, 100 + joins, TmInt(team["requiredLevel"], 0));
				team["botJoins"] = joins + 1;
				team["lastBotJoin"] = TmIso(now);
				if ((string)team["type"] == "Open")
				{
					members.Add(bot);
					TmAddBoard(team, "JoinTeam", (string)bot["id"], (string)bot["name"], null, now, null);
				}
				else
				{
					applies.Add(new JObject
					{
						["GUID"] = Guid.NewGuid().ToString("N"),
						["TeamId"] = (string)team["id"],
						["UserId"] = (string)bot["id"],
						["UserName"] = (string)bot["name"],
						["Created"] = TmIso(now),
						["bot"] = true,
						["spec"] = bot
					});
				}
				s.MarkDirty();
			}
		}
		// 2) bot cho tim vào yêu cầu của người chơi (tạo "Live" cho người xin)
		foreach (JObject b in TmBoards(team).OfType<JObject>())
		{
			string requester = (string)b["UserId"];
			JObject rm = TmFindMember(team, requester);
			if (rm == null || TmIsBot(rm) || !TmRequestActive(b, now))
			{
				continue;
			}
			int received = TmInt(b["LifeReceived"], 0);
			int max = TmInt(b["LifeMax"], 0);
			int want = Math.Min(max, (int)((now - TmDt(b["Created"], now)).TotalSeconds / TmBotHelpSeconds));
			JArray senders = (JArray)b["LifeSenders"];
			while (received < want)
			{
				JObject helper = members.OfType<JObject>().FirstOrDefault(m => TmIsBot(m) && (string)m["id"] != requester && !senders.Any(x => (string)x == (string)m["id"]));
				if (helper == null)
				{
					break;
				}
				received++;
				senders.Add((string)helper["id"]);
				helper["helps"] = TmInt(helper["helps"], 0) + 1;
				TmAddLive(s, requester, (string)helper["id"], (string)helper["name"]);
			}
			if (received != TmInt(b["LifeReceived"], 0))
			{
				b["LifeReceived"] = received;
				if (received >= max)
				{
					b["EndTime"] = TmIso(now); // [giả định] đủ tim thì yêu cầu kết thúc → nút xin tim mở lại
				}
				s.MarkDirty();
			}
		}
		// 3) bot hoạt động trên bảng tin
		List<JObject> bots = members.OfType<JObject>().Where(TmIsBot).ToList();
		if (bots.Count > 0 && (now - TmDt(team["lastBotAct"], now)).TotalSeconds >= TmBotActSeconds)
		{
			int seq = TmInt(team["botSeq"], 0) + 1;
			team["botSeq"] = seq;
			team["lastBotAct"] = TmIso(now);
			JObject bot = bots[(seq * 3) % bots.Count];
			bool hasBotRequest = TmBoards(team).OfType<JObject>().Any(x => TmRequestActive(x, now) && TmFindMember(team, (string)x["UserId"]) != null && TmIsBot(TmFindMember(team, (string)x["UserId"])));
			if (seq % 7 == 6)
			{
				TmAddBoard(team, "RoyalPassActivity", (string)bot["id"], (string)bot["name"], null, now, null);
			}
			else if (seq % 4 == 2 && !hasBotRequest)
			{
				TmAddRequest(team, bot, 1, now);
			}
			else
			{
				TmAddBoard(team, "Chat", (string)bot["id"], (string)bot["name"], TmChatLines[(seq + TmHash((string)bot["id"])) % TmChatLines.Length], now, TmInt(bot["medal"], 0));
			}
			s.MarkDirty();
		}
	}

	// ---------- thành viên ----------
	private static TmMem TmView(FakeServerState s, JObject m)
	{
		TmMem v = new TmMem { Id = (string)m["id"], Bot = TmIsBot(m), Leader = TmIsLeader(m), Helps = TmInt(m["helps"], 0) };
		if (v.Bot)
		{
			v.Name = (string)m["name"];
			v.Maxlv = TmInt(m["maxlv"], 1);
			v.SumStar = TmInt(m["sumStar"], 0);
			v.Medal = TmInt(m["medal"], 0);
			v.User = new JObject { ["maxlv"] = v.Maxlv, ["sumStar"] = v.SumStar, ["medal"] = v.Medal };
			return v;
		}
		JObject doc = Doc(s, v.Id);
		JObject acc = TmObj(doc["Account"]);
		JObject usr = TmObj(doc["User"]) ?? new JObject();
		string nm = (acc == null) ? null : (string)acc["name"];
		if (string.IsNullOrEmpty(nm))
		{
			nm = (string)m["name"];
		}
		if (string.IsNullOrEmpty(nm))
		{
			nm = "Player" + v.Id.Substring(0, Math.Min(8, v.Id.Length));
		}
		v.Name = nm;
		v.Photo = (acc == null) ? null : (string)acc["photoUrl"];
		v.Maxlv = TmInt(usr["maxlv"], 0);
		v.SumStar = TmInt(usr["sumStar"], 0);
		v.Medal = TmInt(usr["medal"], 0);
		v.User = usr;
		return v;
	}

	private static int TmMonthScore(TmMem v, string issue)
	{
		if (v.Bot)
		{
			return TmHash(v.Id + issue) % 120 * Math.Max(1, DateTime.Now.Day);
		}
		JObject ms = TmObj(v.User["MonthIssueSumStar"]);
		if (ms == null || ms[issue] == null)
		{
			return -1;
		}
		return TmInt(ms[issue], -1);
	}

	private static void TmSyncDoc(FakeServerState s, string uid, JObject team)
	{
		JObject doc = Doc(s, uid);
		JObject tm = doc["Team"] as JObject;
		bool changed = false;
		if (team == null)
		{
			if (tm != null)
			{
				foreach (string k in new[] { "TeamId", "Helps", "IsLeader" })
				{
					if (tm[k] != null)
					{
						tm.Remove(k);
						changed = true;
					}
				}
			}
		}
		else
		{
			JObject m = TmFindMember(team, uid);
			if (tm == null)
			{
				tm = new JObject();
				doc["Team"] = tm;
				changed = true;
			}
			JToken[] vals = { (string)team["id"], TmInt(m["helps"], 0), TmIsLeader(m) };
			string[] keys = { "TeamId", "Helps", "IsLeader" };
			for (int i = 0; i < keys.Length; i++)
			{
				if (tm[keys[i]] == null || !JToken.DeepEquals(tm[keys[i]], vals[i]))
				{
					tm[keys[i]] = vals[i];
					changed = true;
				}
			}
		}
		if (changed)
		{
			s.MarkDirty();
		}
	}

	private static JObject TmMyTeam(FakeServerState s, string uid)
	{
		JObject ut = TmState(s, "userTeam");
		string tid = (string)ut[uid];
		if (string.IsNullOrEmpty(tid))
		{
			return null;
		}
		JObject team = TmTeams(s)[tid] as JObject;
		if (team == null || TmFindMember(team, uid) == null)
		{
			ut.Remove(uid);
			TmSyncDoc(s, uid, null);
			s.MarkDirty();
			return null;
		}
		TmSyncDoc(s, uid, team);
		return team;
	}

	private static void TmJoinMember(FakeServerState s, JObject team, string uid, string nameHint)
	{
		DateTime now = DateTime.UtcNow;
		JObject m = new JObject { ["id"] = uid, ["name"] = nameHint, ["bot"] = false, ["leader"] = false, ["helps"] = 0 };
		TmMembers(team).Add(m);
		TmState(s, "userTeam")[uid] = (string)team["id"];
		TmAddBoard(team, "JoinTeam", uid, TmView(s, m).Name, null, now, null);
		// Làm bảng tin có sinh khí ngay khi vào: một lời chào + một yêu cầu tim của bot nếu chưa có
		List<JObject> bots = TmMembers(team).OfType<JObject>().Where(TmIsBot).ToList();
		if (bots.Count > 0)
		{
			int h = TmHash((string)team["id"] + uid);
			JObject bot = bots[h % bots.Count];
			TmAddBoard(team, "Chat", (string)bot["id"], (string)bot["name"], TmChatLines[h % TmChatLines.Length], now.AddSeconds(1), TmInt(bot["medal"], 0));
			if (!TmBoards(team).OfType<JObject>().Any(x => TmRequestActive(x, now)))
			{
				TmAddRequest(team, bots[(h / 7) % bots.Count], 1, now.AddSeconds(2));
			}
		}
		team["lastBotAct"] = TmIso(now);
		// huỷ các đơn xin vào của người này ở mọi team
		foreach (JProperty p in TmTeams(s).Properties())
		{
			JArray ap = TmApplies((JObject)p.Value);
			foreach (JObject a in ap.OfType<JObject>().Where(a => (string)a["UserId"] == uid).ToList())
			{
				ap.Remove(a);
			}
		}
		TmSyncDoc(s, uid, team);
		s.MarkDirty();
	}

	private static void TmRemoveMember(FakeServerState s, JObject team, string id, string boardType)
	{
		JObject m = TmFindMember(team, id);
		if (m == null)
		{
			return;
		}
		string name = TmView(s, m).Name;
		bool wasLeader = TmIsLeader(m);
		bool human = !TmIsBot(m);
		TmMembers(team).Remove(m);
		if (human)
		{
			TmState(s, "userTeam").Remove(id);
			TmSyncDoc(s, id, null);
		}
		JArray members = TmMembers(team);
		if (members.Count == 0)
		{
			TmTeams(s).Remove((string)team["id"]);
			s.MarkDirty();
			return;
		}
		TmAddBoard(team, boardType, id, name, null, DateTime.UtcNow, null);
		if (wasLeader)
		{
			// AlternateLeaderSort {'User.maxlv':-1}: người maxlv cao nhất kế nhiệm
			JObject next = members.OfType<JObject>().OrderByDescending(x => TmView(s, x).Maxlv).First();
			next["leader"] = true;
			if (!TmIsBot(next))
			{
				TmSyncDoc(s, (string)next["id"], team);
			}
			else
			{
				TmApplies(team).Clear(); // trưởng bot không còn duyệt đơn
			}
		}
		s.MarkDirty();
	}

	// Đơn xin vào team Close của người chơi được trưởng bot duyệt sau TmApplyApproveSeconds (xét khi GetMyTeam).
	private static void TmResolveApplies(FakeServerState s, string uid)
	{
		if (TmMyTeam(s, uid) != null)
		{
			return;
		}
		DateTime now = DateTime.UtcNow;
		int myLv = TmInt(TmObj(Doc(s, uid)["User"])?["maxlv"], 0);
		foreach (JProperty p in TmTeams(s).Properties().ToList())
		{
			JObject t = (JObject)p.Value;
			JArray ap = TmApplies(t);
			foreach (JObject a in ap.OfType<JObject>().Where(x => (string)x["UserId"] == uid).ToList())
			{
				if ((now - TmDt(a["Created"], now)).TotalSeconds < TmApplyApproveSeconds)
				{
					continue;
				}
				ap.Remove(a);
				s.MarkDirty();
				if (TmMembers(t).Count >= TmInt(t["limit"], TmLimit) || myLv < TmInt(t["requiredLevel"], 0))
				{
					continue;
				}
				TmJoinMember(s, t, uid, (string)a["UserName"]);
				return;
			}
		}
	}

	// ---------- endpoints: danh sách / team của tôi ----------
	private static FakeResponse TmGetTeams(FakeRequest q, FakeServerState s)
	{
		string name;
		q.Query.TryGetValue("name", out name);
		List<JObject> list = new List<JObject>();
		foreach (JProperty p in TmTeams(s).Properties())
		{
			JObject t = (JObject)p.Value;
			if (!string.IsNullOrEmpty(name) && ((string)t["name"]).IndexOf(name, StringComparison.OrdinalIgnoreCase) < 0)
			{
				continue;
			}
			list.Add(t);
		}
		JArray arr = new JArray();
		foreach (JObject t in list.OrderByDescending(x => TmMembers(x).Count).ThenBy(x => (string)x["name"]).Take(100))
		{
			arr.Add(TmInfo(t));
		}
		return Ok(arr);
	}

	private static FakeResponse TmGetMyTeam(FakeRequest q, FakeServerState s)
	{
		string uid = UserIdFromAuth(q);
		if (uid == null)
		{
			return TmUnauth();
		}
		TmResolveApplies(s, uid);
		JObject team = TmMyTeam(s, uid);
		if (team == null)
		{
			return Ok(JValue.CreateNull());
		}
		TmTick(s, team);
		return Ok(TmInfo(team));
	}

	private static FakeResponse TmCreateTeam(FakeRequest q, FakeServerState s)
	{
		string uid = UserIdFromAuth(q);
		JObject b = Body(q);
		if (uid == null || b == null)
		{
			return TmUnauth();
		}
		string name = ((string)b["name"] ?? "").Trim();
		if (name.Length == 0)
		{
			return TmFail("Name");
		}
		if (TmMyTeam(s, uid) != null)
		{
			return TmFail("AlreadyInTeam");
		}
		JObject teams = TmTeams(s);
		if (teams.Properties().Any(p => string.Equals((string)p.Value["name"], name, StringComparison.OrdinalIgnoreCase)))
		{
			return TmFail("NameExists");
		}
		string type = (string)b["type"];
		string id = TmNewId();
		JObject t = new JObject
		{
			["id"] = id,
			["name"] = name,
			["badge"] = string.IsNullOrEmpty((string)b["badge"]) ? "team-icon-1" : (string)b["badge"],
			["description"] = (string)b["description"] ?? "",
			["type"] = (type == "Close") ? "Close" : "Open",
			["requiredLevel"] = TmInt(b["requiredLevel"], 0),
			["limit"] = TmLimit,
			["created"] = TmIso(DateTime.UtcNow),
			["botJoins"] = 0,
			["lastBotJoin"] = TmIso(DateTime.UtcNow),
			["botSeq"] = 0,
			["lastBotAct"] = TmIso(DateTime.UtcNow),
			["members"] = new JArray(),
			["boards"] = new JArray(),
			["applies"] = new JArray()
		};
		teams[id] = t;
		TmJoinMember(s, t, uid, null);
		TmFindMember(t, uid)["leader"] = true;
		TmSyncDoc(s, uid, t);
		s.MarkDirty();
		return Ok(TmInfo(t));
	}

	private static FakeResponse TmUpdateTeam(FakeRequest q, FakeServerState s)
	{
		string uid = UserIdFromAuth(q);
		JObject b = Body(q);
		if (uid == null || b == null)
		{
			return TmUnauth();
		}
		JObject team = TmMyTeam(s, uid);
		if (team == null)
		{
			return TmFail("NoTeam");
		}
		if (!TmIsLeader(TmFindMember(team, uid)))
		{
			return TmFail("NotLeader");
		}
		if (!string.IsNullOrEmpty((string)b["badge"]))
		{
			team["badge"] = (string)b["badge"];
		}
		if (b["description"] != null)
		{
			team["description"] = (string)b["description"] ?? "";
		}
		if ((string)b["type"] == "Open" || (string)b["type"] == "Close")
		{
			team["type"] = (string)b["type"];
		}
		if (b["requiredLevel"] != null)
		{
			team["requiredLevel"] = TmInt(b["requiredLevel"], 0);
		}
		s.MarkDirty();
		return Ok(TmInfo(team));
	}

	// JoinTeam: Open = vào ngay (client gọi onJoinSuccess). Close = tạo đơn xin vào, client chỉ hiện "đã gửi"; [giả định] trưởng bot tự duyệt sau 15 s (xét ở GetMyTeam).
	private static FakeResponse TmJoinTeam(FakeRequest q, FakeServerState s)
	{
		string uid = UserIdFromAuth(q);
		JObject b = Body(q);
		if (uid == null || b == null)
		{
			return TmUnauth();
		}
		if (TmMyTeam(s, uid) != null)
		{
			return TmFail("AlreadyInTeam");
		}
		JObject team = TmTeams(s)[(string)b["TeamId"] ?? ""] as JObject;
		if (team == null)
		{
			return TmFail("NotFound");
		}
		int myLv = TmInt(TmObj(Doc(s, uid)["User"])?["maxlv"], 0);
		if (myLv < TmInt(team["requiredLevel"], 0))
		{
			return TmFail("RequiredLevel");
		}
		if (TmMembers(team).Count >= TmInt(team["limit"], TmLimit))
		{
			return TmFail("Full");
		}
		string uname = (string)b["UserName"];
		if ((string)team["type"] == "Open")
		{
			TmJoinMember(s, team, uid, uname);
			return Ok();
		}
		JArray ap = TmApplies(team);
		if (!ap.OfType<JObject>().Any(a => (string)a["UserId"] == uid))
		{
			ap.Add(new JObject
			{
				["GUID"] = Guid.NewGuid().ToString("N"),
				["TeamId"] = (string)team["id"],
				["UserId"] = uid,
				["UserName"] = uname ?? TmView(s, new JObject { ["id"] = uid, ["bot"] = false }).Name,
				["Created"] = TmIso(DateTime.UtcNow),
				["bot"] = false
			});
			s.MarkDirty();
		}
		return Ok();
	}

	private static FakeResponse TmLeaveTeam(FakeRequest q, FakeServerState s)
	{
		string uid = UserIdFromAuth(q);
		if (uid == null)
		{
			return TmUnauth();
		}
		JObject team = TmMyTeam(s, uid);
		if (team != null)
		{
			TmRemoveMember(s, team, uid, "LeaveTeam");
		}
		return Ok();
	}

	private static FakeResponse TmKickTeam(FakeRequest q, FakeServerState s)
	{
		string uid = UserIdFromAuth(q);
		JObject b = Body(q);
		if (uid == null || b == null)
		{
			return TmUnauth();
		}
		JObject team = TmMyTeam(s, uid);
		if (team == null)
		{
			return TmFail("NoTeam");
		}
		if (!TmIsLeader(TmFindMember(team, uid)))
		{
			return TmFail("NotLeader");
		}
		string target = (string)b["UserId"];
		if (target == uid || TmFindMember(team, target) == null)
		{
			return TmFail("NotMember");
		}
		TmRemoveMember(s, team, target, "LeaveTeam");
		return Ok();
	}

	private static FakeResponse TmGetApplies(FakeRequest q, FakeServerState s)
	{
		string uid = UserIdFromAuth(q);
		if (uid == null)
		{
			return TmUnauth();
		}
		JArray arr = new JArray();
		JObject team = TmMyTeam(s, uid);
		if (team != null && TmIsLeader(TmFindMember(team, uid)))
		{
			TmTick(s, team);
			foreach (JObject a in TmApplies(team).OfType<JObject>())
			{
				arr.Add(new JObject
				{
					["JoinTeamApply"] = new JObject
					{
						["GUID"] = (string)a["GUID"],
						["TeamId"] = (string)a["TeamId"],
						["UserId"] = (string)a["UserId"],
						["UserName"] = (string)a["UserName"],
						["Created"] = TmIso(TmDt(a["Created"], DateTime.UtcNow))
					}
				});
			}
		}
		return Ok(arr);
	}

	private static FakeResponse TmDealApply(FakeRequest q, FakeServerState s, bool agree)
	{
		string uid = UserIdFromAuth(q);
		JObject b = Body(q);
		if (uid == null || b == null)
		{
			return TmUnauth();
		}
		JObject team = TmMyTeam(s, uid);
		if (team == null || !TmIsLeader(TmFindMember(team, uid)))
		{
			return TmFail("NotLeader");
		}
		string guid = (string)b["GUID"];
		JArray ap = TmApplies(team);
		JObject a = ap.OfType<JObject>().FirstOrDefault(x => (string)x["GUID"] == guid);
		if (a == null)
		{
			return TmFail("NotFound");
		}
		ap.Remove(a);
		s.MarkDirty();
		if (!agree)
		{
			return Ok();
		}
		if (TmMembers(team).Count >= TmInt(team["limit"], TmLimit))
		{
			return TmFail("Full");
		}
		string aid = (string)a["UserId"];
		if (TmFindMember(team, aid) != null)
		{
			return Ok();
		}
		JObject spec = a["spec"] as JObject;
		if (spec != null)
		{
			TmMembers(team).Add((JObject)spec.DeepClone());
			TmAddBoard(team, "JoinTeam", aid, (string)a["UserName"], null, DateTime.UtcNow, null);
		}
		else if (TmMyTeam(s, aid) == null)
		{
			TmJoinMember(s, team, aid, (string)a["UserName"]);
		}
		s.MarkDirty();
		return Ok();
	}

	// ---------- bảng tin / tim ----------
	private static FakeResponse TmGetBoards(FakeRequest q, FakeServerState s)
	{
		string uid = UserIdFromAuth(q);
		if (uid == null)
		{
			return TmUnauth();
		}
		JObject team = TmMyTeam(s, uid);
		if (team == null)
		{
			return Ok(new JArray());
		}
		TmTick(s, team);
		JObject dict = new JObject();
		foreach (JObject b in TmBoards(team).OfType<JObject>().OrderBy(x => TmDt(x["Created"], DateTime.UtcNow)))
		{
			dict[(string)b["GUID"]] = TmBoardOut(b); // client đọc Boards theo thứ tự chèn: cũ → mới, mới nhất ở cuối
		}
		TmState(s, "boardSeen")[uid] = TmIso(DateTime.UtcNow);
		s.MarkDirty();
		return Ok(new JArray { new JObject { ["Boards"] = dict } });
	}

	private static FakeResponse TmCheckNewBoard(FakeRequest q, FakeServerState s)
	{
		string uid = UserIdFromAuth(q);
		if (uid == null)
		{
			return TmUnauth();
		}
		JObject team = TmMyTeam(s, uid);
		if (team == null)
		{
			return Ok(false);
		}
		TmTick(s, team);
		DateTime seen = TmDt(TmState(s, "boardSeen")[uid], DateTime.MinValue);
		bool any = TmBoards(team).OfType<JObject>().Any(b => (string)b["UserId"] != uid && TmDt(b["Created"], DateTime.MinValue) > seen);
		return Ok(any);
	}

	private static FakeResponse TmChat(FakeRequest q, FakeServerState s)
	{
		string uid = UserIdFromAuth(q);
		JObject b = Body(q);
		if (uid == null || b == null)
		{
			return TmUnauth();
		}
		JObject team = TmMyTeam(s, uid);
		string content = ((string)b["Content"] ?? "").Trim();
		if (team == null || content.Length == 0)
		{
			return TmFail("NoTeam");
		}
		TmRememberName(team, uid, (string)b["UserName"]);
		TmAddBoard(team, "Chat", uid, TmView(s, TmFindMember(team, uid)).Name, content, DateTime.UtcNow, TmNullableInt(b["Medal"]));
		s.MarkDirty();
		return Ok();
	}

	private static void TmRememberName(JObject team, string uid, string name)
	{
		JObject m = TmFindMember(team, uid);
		if (m != null && !string.IsNullOrEmpty(name))
		{
			m["name"] = name;
		}
	}

	private static FakeResponse TmAskLive(FakeRequest q, FakeServerState s)
	{
		string uid = UserIdFromAuth(q);
		JObject b = Body(q);
		if (uid == null || b == null)
		{
			return TmUnauth();
		}
		JObject team = TmMyTeam(s, uid);
		if (team == null)
		{
			return TmFail("NoTeam");
		}
		DateTime now = DateTime.UtcNow;
		if (TmBoards(team).OfType<JObject>().Any(x => (string)x["UserId"] == uid && TmRequestActive(x, now)))
		{
			return TmFail("Requesting");
		}
		TmRememberName(team, uid, (string)b["UserName"]);
		JObject m = TmFindMember(team, uid);
		JObject req = TmAddRequest(team, new JObject { ["id"] = uid, ["name"] = TmView(s, m).Name, ["bot"] = false }, 0, now);
		req["Medal"] = TmNullableInt(b["Medal"]).HasValue ? (JToken)TmNullableInt(b["Medal"]).Value : JValue.CreateNull();
		s.MarkDirty();
		return Ok();
	}

	private static FakeResponse TmHelp(FakeRequest q, FakeServerState s)
	{
		string uid = UserIdFromAuth(q);
		JObject b = Body(q);
		if (uid == null || b == null)
		{
			return TmUnauth();
		}
		JObject team = TmMyTeam(s, uid);
		if (team == null)
		{
			return TmFail("NoTeam");
		}
		string guid = (string)b["GUID"];
		JObject board = TmBoards(team).OfType<JObject>().FirstOrDefault(x => (string)x["GUID"] == guid && (string)x["Type"] == "RequestingLife");
		DateTime now = DateTime.UtcNow;
		if (board == null)
		{
			return TmFail("NotFound");
		}
		JArray senders = (JArray)board["LifeSenders"];
		if ((string)board["UserId"] == uid || senders.Any(x => (string)x == uid) || !TmRequestActive(board, now))
		{
			return TmFail("Full");
		}
		TmRememberName(team, uid, (string)b["UserName"]);
		senders.Add(uid);
		board["LifeReceived"] = senders.Count;
		if (senders.Count >= TmInt(board["LifeMax"], 0))
		{
			board["EndTime"] = TmIso(now);
		}
		JObject me = TmFindMember(team, uid);
		me["helps"] = TmInt(me["helps"], 0) + 1;
		JObject requester = TmFindMember(team, (string)board["UserId"]);
		if (requester != null && !TmIsBot(requester))
		{
			TmAddLive(s, (string)requester["id"], uid, TmView(s, me).Name);
		}
		TmSyncDoc(s, uid, team);
		s.MarkDirty();
		return Ok();
	}

	private static FakeResponse TmAddReceiver(FakeRequest q, FakeServerState s)
	{
		string uid = UserIdFromAuth(q);
		JObject b = Body(q);
		if (uid == null || b == null)
		{
			return TmUnauth();
		}
		JObject team = TmMyTeam(s, uid);
		if (team == null)
		{
			return TmFail("NoTeam");
		}
		string guid = (string)b["GUID"];
		JObject board = TmBoards(team).OfType<JObject>().FirstOrDefault(x => (string)x["GUID"] == guid);
		if (board == null)
		{
			return TmFail("NotFound");
		}
		JArray rec = (JArray)board["Receivers"];
		if ((string)board["UserId"] == uid || rec.Any(x => (string)x == uid))
		{
			return TmFail("Received");
		}
		rec.Add(uid);
		s.MarkDirty();
		return Ok();
	}

	private static FakeResponse TmCreateBoard(FakeRequest q, FakeServerState s)
	{
		string uid = UserIdFromAuth(q);
		JObject b = Body(q);
		if (uid == null || b == null)
		{
			return TmUnauth();
		}
		JObject team = TmMyTeam(s, uid);
		if (team == null)
		{
			return Ok(); // không có team: coi như xong để client khỏi thử lại mãi
		}
		JToken ext = b["Extension"];
		string extension = (ext == null || ext.Type == JTokenType.Null) ? null : (ext.Type == JTokenType.String ? (string)ext : ext.ToString(Newtonsoft.Json.Formatting.None));
		TmRememberName(team, uid, (string)b["UserName"]);
		TmAddBoard(team, string.IsNullOrEmpty((string)b["Type"]) ? "Chat" : (string)b["Type"], uid, TmView(s, TmFindMember(team, uid)).Name, (string)b["Content"], DateTime.UtcNow, TmNullableInt(b["Medal"]), extension);
		s.MarkDirty();
		return Ok();
	}

	private static FakeResponse TmGetLastRequest(FakeRequest q, FakeServerState s)
	{
		string uid = UserIdFromAuth(q);
		if (uid == null)
		{
			return TmUnauth();
		}
		JObject team = TmMyTeam(s, uid);
		if (team == null)
		{
			return Ok(JValue.CreateNull());
		}
		TmTick(s, team);
		DateTime now = DateTime.UtcNow;
		JObject b = TmBoards(team).OfType<JObject>().LastOrDefault(x => (string)x["UserId"] == uid && (string)x["Type"] == "RequestingLife");
		if (b == null || TmDt(b["EndTime"], now) <= now)
		{
			return Ok(JValue.CreateNull()); // không có yêu cầu đang chạy → client gọi tiếp GetLives
		}
		DateTime end = TmDt(b["EndTime"], now);
		return Ok(new JObject
		{
			["guid"] = (string)b["GUID"],
			["type"] = "RequestingLife",
			["userId"] = uid,
			["userName"] = (string)b["UserName"],
			["lifeReceived"] = TmInt(b["LifeReceived"], 0),
			["lifeMax"] = TmInt(b["LifeMax"], 0),
			["lifeSenders"] = b["LifeSenders"].DeepClone(),
			["created"] = TmIso(TmDt(b["Created"], now)),
			["endTime"] = TmIso(end),
			["endTimeSpan"] = (end - now).Ticks, // client: new TimeSpan(endTimeSpan)
			["canHelp"] = false
		});
	}

	private static FakeResponse TmGetLives(FakeRequest q, FakeServerState s)
	{
		string uid = UserIdFromAuth(q);
		if (uid == null)
		{
			return TmUnauth();
		}
		JObject team = TmMyTeam(s, uid);
		if (team != null)
		{
			TmTick(s, team);
		}
		JObject mine = TmState(s, "lives")[uid] as JObject;
		return Ok(new JArray { new JObject { ["Lives"] = (mine == null) ? new JObject() : (JObject)mine.DeepClone() } });
	}

	private static FakeResponse TmReceiveLive(FakeRequest q, FakeServerState s)
	{
		string uid = UserIdFromAuth(q);
		JObject b = Body(q);
		if (uid == null)
		{
			return TmUnauth();
		}
		JObject mine = TmState(s, "lives")[uid] as JObject;
		if (mine != null && b != null)
		{
			List<string> ids = new List<string>();
			if (b["GUID"] != null)
			{
				ids.Add((string)b["GUID"]);
			}
			JArray many = b["GUIDS"] as JArray;
			if (many != null)
			{
				ids.AddRange(many.Select(x => (string)x));
			}
			foreach (string id in ids)
			{
				mine.Remove(id ?? "");
			}
			s.MarkDirty();
		}
		return Ok(); // idempotent; client cộng năng lượng phía client
	}

	// ---------- xếp hạng ----------
	private static JObject TmRankTeam(JObject t)
	{
		return TmInfo(t); // cùng các trường: id,name,badge,description,type,requiredLevel,members,limit,created
	}

	private static FakeResponse TmGetTeamRanking(FakeRequest q, FakeServerState s)
	{
		string uid = UserIdFromAuth(q);
		string match = q.Query.TryGetValue("match", out string ms) ? ms : "";
		int limit = q.Query.TryGetValue("limit", out string ls) && int.TryParse(ls, out int li) ? li : 100;
		string issue = null;
		const string marker = "MonthIssueSumStar.";
		int a = match.IndexOf(marker, StringComparison.Ordinal);
		if (a >= 0)
		{
			a += marker.Length;
			int e = match.IndexOf('"', a);
			issue = (e > a) ? match.Substring(a, e - a) : "";
		}
		List<KeyValuePair<long, JObject>> list = new List<KeyValuePair<long, JObject>>();
		foreach (JProperty p in TmTeams(s).Properties())
		{
			JObject t = (JObject)p.Value;
			long sum = 0;
			bool any = false;
			foreach (JObject m in TmMembers(t).OfType<JObject>())
			{
				TmMem v = TmView(s, m);
				if (issue == null)
				{
					sum += v.SumStar;
					any = true;
				}
				else
				{
					int sc = TmMonthScore(v, issue);
					if (sc >= 0)
					{
						sum += sc;
						any = true;
					}
				}
			}
			if (any)
			{
				list.Add(new KeyValuePair<long, JObject>(sum, t));
			}
		}
		list = list.OrderByDescending(x => x.Key).ThenBy(x => (string)x.Value["name"]).ToList();
		JArray ranking = new JArray();
		JObject my = null;
		JObject myTeam = (uid == null) ? null : TmMyTeam(s, uid);
		foreach (KeyValuePair<long, JObject> kv in list)
		{
			if (myTeam != null && (string)kv.Value["id"] == (string)myTeam["id"])
			{
				my = new JObject { ["sum"] = (int)Math.Min(int.MaxValue, kv.Key), ["team"] = TmRankTeam(kv.Value) };
			}
			if (ranking.Count < limit)
			{
				ranking.Add(new JObject { ["sum"] = (int)Math.Min(int.MaxValue, kv.Key), ["team"] = TmRankTeam(kv.Value) });
			}
		}
		return Ok(new JObject { ["ranking"] = ranking, ["my"] = my });
	}

	private static int TmThScore(string id, string issue)
	{
		int days = 1;
		DateTime start;
		if (DateTime.TryParseExact(issue, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out start))
		{
			days = Math.Max(1, Math.Min(7, (DateTime.Now.Date - start).Days + 1));
		}
		return (20 + TmHash(id + issue) % 100) * days; // [giả định] thang điểm treasure hunt không rõ
	}

	private static FakeResponse TmRankingGet(FakeRequest q, FakeServerState s)
	{
		string query = q.Query.TryGetValue("query", out string qs) ? qs : "";
		if (!query.Contains("Team.TeamId"))
		{
			return RankingGet(q, s);
		}
		Match tm = Regex.Match(query, "\"Team\\.TeamId\"\\s*:\\s*\"([^\"]+)\"");
		JArray rows = new JArray();
		JObject team = tm.Success ? TmTeams(s)[tm.Groups[1].Value] as JObject : null;
		if (team == null)
		{
			return Ok(rows);
		}
		int limit = q.Query.TryGetValue("limit", out string ls) && int.TryParse(ls, out int li) ? li : 50;
		bool desc = !q.Query.TryGetValue("sort", out string so) || so.Contains("-1");
		Match th = Regex.Match(query, "TreasureHuntActivity\\.allScores\\.([^\".]+)\\.([^\"]+)\"");
		List<KeyValuePair<int, JObject>> list = new List<KeyValuePair<int, JObject>>();
		foreach (JObject m in TmMembers(team).OfType<JObject>())
		{
			TmMem v = TmView(s, m);
			if (!th.Success)
			{
				JObject user = new JObject { ["maxlv"] = v.Maxlv, ["sumStar"] = v.SumStar, ["medal"] = v.Medal };
				JObject row = new JObject
				{
					["userId"] = v.Id,
					["projection"] = new JObject
					{
						["Account"] = new JObject { ["name"] = v.Name, ["photoUrl"] = v.Photo },
						["User"] = user,
						["Team"] = new JObject { ["TeamId"] = (string)team["id"], ["Helps"] = v.Helps, ["IsLeader"] = v.Leader }
					}
				};
				list.Add(new KeyValuePair<int, JObject>(v.Maxlv, row));
				continue;
			}
			string teamKey = th.Groups[1].Value;
			string issue = th.Groups[2].Value;
			int score;
			if (v.Bot)
			{
				score = TmThScore(v.Id, issue);
			}
			else
			{
				JObject all = TmObj(TmObj(Doc(s, v.Id)["TreasureHuntActivity"])?["allScores"]);
				JObject byTeam = (all == null) ? null : TmObj(all[teamKey]);
				JToken sc = (byTeam == null) ? null : byTeam[issue];
				if (sc == null || (sc.Type != JTokenType.Integer && sc.Type != JTokenType.Float))
				{
					continue; // truy vấn $type:"number": chưa có điểm thì không có dòng
				}
				score = (int)sc;
			}
			list.Add(new KeyValuePair<int, JObject>(score, new JObject
			{
				["userId"] = v.Id,
				["projection"] = new JObject
				{
					["Account"] = new JObject { ["name"] = v.Name },
					["TreasureHuntActivity"] = new JObject { ["allScores"] = new JObject { [teamKey] = new JObject { [issue] = score } } }
				}
			}));
		}
		foreach (KeyValuePair<int, JObject> kv in (desc ? list.OrderByDescending(x => x.Key) : list.OrderBy(x => x.Key)).Take(limit))
		{
			rows.Add(kv.Value);
		}
		return Ok(rows);
	}

	// 5.2 Treasure Hunt: như lịch sẵn có nhưng gắn myTeam của người chơi.
	private static FakeResponse TmGetActivity(FakeRequest q, FakeServerState s)
	{
		FakeResponse resp = Activity(WeekWindow(), "yyyyMMdd", true);
		string uid = UserIdFromAuth(q);
		JObject team = (uid == null) ? null : TmMyTeam(s, uid);
		if (team == null)
		{
			return resp;
		}
		JObject o;
		using (System.IO.StringReader sr = new System.IO.StringReader(resp.Body))
		using (Newtonsoft.Json.JsonTextReader jr = new Newtonsoft.Json.JsonTextReader(sr) { DateParseHandling = Newtonsoft.Json.DateParseHandling.None })
		{
			o = JObject.Load(jr);
		}
		JObject data = o["data"] as JObject;
		if (data != null)
		{
			data["myTeam"] = TmInfo(team);
		}
		return Json(o);
	}
}
#endif
