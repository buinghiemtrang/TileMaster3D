// [rebuild] Handler fake server (FAKE_SERVER). Mỗi route: "host/path" (hoặc "host/*") -> (FakeRequest, FakeServerState) -> FakeResponse.
// Trả null = không trả lời. Giao thức chi tiết: docs/net_protocol.md (số mục ghi trong comment).
// Lỗi nghiệp vụ trả HTTP 200 + code; HTTP khác 2xx = client coi là lỗi mạng.
// Trạng thái: "guests" {external_token: userId}, "docs" {userId: tài liệu /data (JObject theo tên lớp DB)}.
#if FAKE_SERVER
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public static partial class FakeServerHandlers
{
	public const string Host = "service.fuqiangame.com";

	public const string OssHost = "match3d.oss-accelerate.aliyuncs.com";

	public const string Region = "VN";

	public static void Register(Dictionary<string, Func<FakeRequest, FakeServerState, FakeResponse>> r)
	{
		// Boot (mục 1)
		r[OssHost + "/*"] = (q, s) => FakeResponse.Error(404); // 1.1 version_manifest + 2.7 video: như không có file → dùng config preinstall
		r[Host + "/utils/api/v1/getUserNation"] = (q, s) => Ok(new JObject { ["ip"] = "127.0.0.1", ["nation"] = Region });
		r[Host + "/common/time"] = (q, s) => Ok(DateTime.Now.ToString("yyyyMMddHHmmss")); // giờ máy (xem 0.6)
		r[Host + "/connect/token"] = ConnectToken;
		r[Host + "/data"] = Data;
		r[Host + "/notice/api/v1/app/appGetNotice"] = (q, s) => Json(new JObject { ["code"] = 0, ["msg"] = "", ["data"] = new JArray() }); // code 0 = thành công (riêng endpoint này)
		r[Host + "/utils/api/v1/getFBDeletion"] = (q, s) => Ok(new JObject { ["isDelete"] = false, ["deletion"] = null });
		r[Host + "/common/GetMonthIssue"] = (q, s) => Ok(DateTime.Now.ToString("yyyyMM"));
		// Sảnh (mục 2)
		r[Host + "/mail/api/v1/app/appFindEmail"] = (q, s) => Ok(new JObject { ["list"] = new JArray(), ["total"] = 0 });
		r[Host + "/mail/api/v1/app/appGetEmail"] = (q, s) => Ok();
		r[Host + "/mail/api/v1/app/appClaimEmail"] = (q, s) => Ok();
		r[Host + "/team/CheckNewTeamBoard"] = (q, s) => Ok(false);
		r[Host + "/team/GetLives"] = (q, s) => Ok(new JArray { new JObject { ["Lives"] = new JObject() } }); // data[0] bắt buộc có
		r[Host + "/team/ReceiveLive"] = (q, s) => Ok();
		// Xếp hạng (mục 3)
		r[Host + "/ranking/get"] = RankingGet;
		r[Host + "/team/GetTeamRanking"] = (q, s) => Ok(new JObject { ["ranking"] = new JArray(), ["my"] = null });
		r[Host + "/common/GetGeneralActivity"] = (q, s) => Activity(MonthWindow(), "yyyy'Y'MM'M'"); // 3.3: xếp hạng tháng (issue dạng 2026Y11M)
		r[Host + "/common/GetTestActivity"] = (q, s) => FakeResponse.Error(404);
		// Team (mục 4): chưa có team
		r[Host + "/team/GetMyTeam"] = (q, s) => Ok(JValue.CreateNull());
		r[Host + "/team/GetTeams"] = (q, s) => Ok(new JArray());
		r[Host + "/team/GetJoinTeamApplies"] = (q, s) => Ok(new JArray());
		r[Host + "/team/GetBoards"] = (q, s) => Ok(new JArray());
		r[Host + "/team/GetLastRequestingLift"] = (q, s) => Ok(JValue.CreateNull());
		foreach (string p in new[] { "JoinTeam", "LeaveTeam", "KickTeam", "AgreeJoinTeamApply", "RejectJoinTeamApply", "AskLive", "Help", "Chat", "AddTeamBoardReceiver", "CreateTeamBoard" })
		{
			r[Host + "/team/" + p] = (q, s) => Ok();
		}
		// Activity (mục 5): lịch mẫu theo chu kỳ (lịch thật do server hãng quyết, không có dữ liệu) — [giả định]
		r[Host + "/team/GetPassActivity"] = (q, s) => Activity(MonthWindow(), "yyyyMM"); // Royal/Master Pass: theo tháng
		r[Host + "/team/GetActivity"] = (q, s) => Activity(WeekWindow(), "yyyyMMdd", true); // Treasure Hunt: T2–T4
		r[Host + "/team/GetGeneralActivity"] = (q, s) => Activity(GeneralWindow(q), "yyyyMMddHHmm"); // Baking Master: theo tham số cf_const.activityBakingMasterDate
		r[Host + "/team/GetMatchs"] = (q, s) => Ok(new JArray());
		r[Host + "/team/DeleteZeroGradeMatch"] = (q, s) => Ok(new JArray());
		r[Host + "/rank/api/v1/app/upCustomRank"] = RegionRankUpload; // FakeServerRegionRank.cs
		r[Host + "/rank/api/v1/app/findCustomRankList"] = RegionRankFind;
		// IAP (mục 6) — FAKE_SDK đã bỏ qua xác minh; giữ cho trường hợp tắt FAKE_SDK
		r[Host + "/utils/api/v1/validateGoogleOrderV2"] = (q, s) => Ok(new JObject { ["dillId"] = Body(q)?["dillId"], ["status"] = 0, ["isSandbox"] = true });
		r[Host + "/utils/api/v1/deliverIosOrder"] = (q, s) => Ok();
		RegisterTeam(r); // FakeServerTeam.cs (nếu có) ghi đè route team/* và ranking team
	}

	static partial void RegisterTeam(Dictionary<string, Func<FakeRequest, FakeServerState, FakeResponse>> r);

	// ---------- helpers ----------
	private static FakeResponse Json(JObject o)
	{
		return FakeResponse.Json(o.ToString(Newtonsoft.Json.Formatting.None));
	}

	private static FakeResponse Ok(JToken data = null)
	{
		JObject o = new JObject { ["code"] = 1, ["msg"] = "" };
		if (data != null)
		{
			o["data"] = data;
		}
		return Json(o);
	}

	private static JObject Body(FakeRequest q)
	{
		try
		{
			return string.IsNullOrEmpty(q.Body) ? null : JObject.Parse(q.Body);
		}
		catch (Exception)
		{
			return null;
		}
	}

	private static Dictionary<string, string> Form(FakeRequest q)
	{
		Dictionary<string, string> d = new Dictionary<string, string>();
		foreach (string part in (q.Body ?? "").Split('&'))
		{
			int eq = part.IndexOf('=');
			if (eq > 0)
			{
				d[Uri.UnescapeDataString(part.Substring(0, eq))] = Uri.UnescapeDataString(part.Substring(eq + 1).Replace('+', ' '));
			}
		}
		return d;
	}

	private static string UserIdFromAuth(FakeRequest q)
	{
		string a = q.Header("Authorization");
		if (string.IsNullOrEmpty(a))
		{
			return null;
		}
		if (a.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
		{
			a = a.Substring(7);
		}
		return a.StartsWith("fake.") ? a.Substring(5) : null;
	}

	private static JObject Doc(FakeServerState s, string userId)
	{
		JObject docs = s.Get("docs") as JObject;
		if (docs == null)
		{
			docs = new JObject();
			s.Set("docs", docs);
		}
		JObject doc = docs[userId] as JObject;
		if (doc == null)
		{
			doc = new JObject();
			docs[userId] = doc;
			s.MarkDirty();
		}
		return doc;
	}

	// ---------- mục 5: lịch activity ----------
	private struct Window
	{
		public DateTime Start;

		public DateTime End;
	}

	private static Window MonthWindow()
	{
		DateTime n = DateTime.Now;
		DateTime st = new DateTime(n.Year, n.Month, 1);
		return new Window { Start = st, End = st.AddMonths(1).AddSeconds(-1) };
	}

	private static Window WeekWindow()
	{
		DateTime d = DateTime.Now.Date;
		int fromMonday = ((int)d.DayOfWeek + 6) % 7;
		DateTime st = d.AddDays(-fromMonday);
		// Treasure Hunt: "Open from Monday to Wednesday" (cfgActivityEntranceInformation id 4) → T2 00:00 + 72h; T5–CN available=false.
		return new Window { Start = st, End = st.AddDays(3).AddSeconds(-1) };
	}

	// Baking Master (Master League): tính y như client ActivityHttpMgr.GetIssueTime (đã dịch):
	// type=Year|Month|Week|Day; Week: days 1..7 = T2..CN (0 = CN); offsetForDays là PHÚT; minutes = độ dài.
	// cf_const.activityBakingMasterDate "2,6,0,2880" = Thứ 7 00:00, 48h (khớp mô tả "Open on weekends").
	// Client lấy ngày theo UTC của DateTime.Today (bỏ giờ) → ở đây dùng cùng cách để hai bên trùng cửa sổ.
	// Cờ test (QA/Editor): true = cửa sổ mở từ đầu hôm nay, bất kể thứ mấy. Đặt PlayerPrefs "FakeServer.ForceGeneralActivity"=1 trước khi Play.
	public static bool ForceGeneralActivity;

	private static Window GeneralWindow(FakeRequest q)
	{
		string type = q.Query.TryGetValue("type", out string t) ? t : "Week";
		int days = q.Query.TryGetValue("days", out string ds) && int.TryParse(ds, out int di) ? di : 0;
		int off = q.Query.TryGetValue("offsetForDays", out string os) && int.TryParse(os, out int oi) ? oi : 0;
		int minutes = q.Query.TryGetValue("minutes", out string ms) && int.TryParse(ms, out int mi) ? mi : 1440;
		DateTime u = TimeZoneInfo.ConvertTimeToUtc(DateTime.Today);
		DateTime d = new DateTime(u.Year, u.Month, u.Day);
		switch (type)
		{
		case "Year":
			d = new DateTime(u.Year, 1, 1).AddDays(-1);
			d = (days < 0) ? d.AddYears(1).AddDays(days) : d.AddDays(days);
			break;
		case "Month":
			d = new DateTime(u.Year, u.Month, 1).AddDays(-1);
			d = (days < 0) ? d.AddMonths(1).AddDays(days) : d.AddDays(days);
			break;
		case "Day":
			break;
		default:
		{
			int dow = (u.DayOfWeek == DayOfWeek.Sunday) ? 7 : (int)u.DayOfWeek;
			d = d.AddDays(((days != 0) ? days : 7) - dow);
			break;
		}
		}
		if (ForceGeneralActivity)
		{
			d = new DateTime(u.Year, u.Month, u.Day);
		}
		// Window dùng giờ máy (Activity() so với DateTime.Now): đổi mốc UTC sang giờ địa phương.
		DateTime st = DateTime.SpecifyKind(d.AddMinutes(off), DateTimeKind.Utc).ToLocalTime();
		return new Window { Start = st, End = st.AddMinutes(minutes) };
	}

	private static FakeResponse Activity(Window w, string issueFormat, bool withTeam = false)
	{
		DateTime n = DateTime.Now;
		long left = (w.End > n) ? (w.End - n).Ticks : 0L; // endTimeSpan = ticks (client: new TimeSpan(endTimeSpan))
		JObject d = new JObject
		{
			["issue"] = w.Start.ToString(issueFormat),
			["startTime"] = w.Start.ToString("yyyy-MM-ddTHH:mm:ss"),
			["endTime"] = w.End.ToString("yyyy-MM-ddTHH:mm:ss"),
			["currentTime"] = n.ToString("yyyy-MM-ddTHH:mm:ss"),
			["endTimeSpan"] = left,
			["available"] = left > 0
		};
		if (withTeam)
		{
			d["myTeam"] = null;
		}
		return Ok(d);
	}

	// ---------- 1.4 connect/token ----------
	private static FakeResponse ConnectToken(FakeRequest q, FakeServerState s)
	{
		Dictionary<string, string> f = Form(q);
		string ext = f.TryGetValue("external_token", out string e) ? e : "";
		string provider = f.TryGetValue("provider", out string p) ? p : "guest";
		JObject guests = s.Get("guests") as JObject;
		if (guests == null)
		{
			guests = new JObject();
			s.Set("guests", guests);
		}
		string key = provider + ":" + ext;
		string userId = (string)guests[key];
		if (string.IsNullOrEmpty(userId))
		{
			userId = Guid.NewGuid().ToString("N").Substring(0, 24); // >= 8 ký tự (ranking cắt Substring(0,8))
			guests[key] = userId;
			s.MarkDirty();
		}
		string token = "fake." + userId;
		JObject resp = new JObject
		{
			["access_token"] = token,
			["expires_in"] = 315360000,
			["token_type"] = "Bearer",
			["scope"] = "all.full_access",
			["code"] = 1,
			["msg"] = "",
			["data"] = new JObject
			{
				["token"] = token,
				["user"] = new JObject
				{
					["UserId"] = userId,
					["DisplayName"] = "",
					["Email"] = "",
					["PhotoUrl"] = "",
					["ProviderId"] = provider,
					["ExternalId"] = ext
				}
			}
		};
		return Json(resp);
	}

	// ---------- 0.4 / 1.5 / 1.6 /data ----------
	private static FakeResponse Data(FakeRequest q, FakeServerState s)
	{
		string uid = UserIdFromAuth(q);
		if (uid == null)
		{
			return Json(new JObject { ["code"] = -1, ["msg"] = "unauthorized" });
		}
		if (q.Method == "DELETE")
		{
			JObject docs = s.Get("docs") as JObject;
			if (docs != null)
			{
				docs.Remove(uid);
				s.MarkDirty();
			}
			return Ok();
		}
		JObject doc = Doc(s, uid);
		if (q.Method == "GET")
		{
			return Json(new JObject { ["code"] = 1, ["msg"] = "", ["data"] = doc.DeepClone() });
		}
		JObject body = Body(q);
		if (body == null)
		{
			return Json(new JObject { ["code"] = -1, ["msg"] = "bad body" });
		}
		foreach (JProperty kv in body.Properties())
		{
			SetPath(doc, kv.Name, kv.Value.DeepClone());
		}
		s.MarkDirty();
		return Ok();
	}

	private static void SetPath(JObject root, string path, JToken value)
	{
		string[] parts = path.Split('.');
		JObject cur = root;
		for (int i = 0; i < parts.Length - 1; i++)
		{
			JObject next = cur[parts[i]] as JObject;
			if (next == null)
			{
				// giá trị cũ có thể là chuỗi JSON (client chấp nhận cả hai dạng) → thử parse
				JToken old = cur[parts[i]];
				if (old != null && old.Type == JTokenType.String)
				{
					try
					{
						next = JObject.Parse((string)old);
					}
					catch (Exception)
					{
						next = null;
					}
				}
				if (next == null)
				{
					next = new JObject();
				}
				cur[parts[i]] = next;
			}
			cur = next;
		}
		cur[parts[parts.Length - 1]] = value;
	}

	// ---------- 3.1 ranking/get ----------
	// Trả: dòng của chính mình (nếu có điểm) + bot cố định theo ngày (bản gốc là người chơi thật).
	// Client lấy _data[Count-1] khi không thấy mình → danh sách không bao giờ rỗng.
	private static readonly string[] BotNames = { "Anna", "Minh", "Lucas", "Sofia", "Hana", "Liam", "Mia", "Noah", "Emma", "Kenji", "Linh", "Oliver", "Ava", "Mateo", "Yuki", "Chloe", "Huy", "Ethan", "Lily", "Leo", "Zoe", "Daniel", "Mai", "Jack", "Nora", "Ryan", "Thao", "Max", "Ella", "Sam" };

	private static FakeResponse RankingGet(FakeRequest q, FakeServerState s)
	{
		string query = q.Query.TryGetValue("query", out string qs) ? qs : "";
		int limit = q.Query.TryGetValue("limit", out string ls) && int.TryParse(ls, out int li) ? li : 100;
		string uid = UserIdFromAuth(q);
		JArray rows = new JArray();
		if (query.Contains("Team.TeamId"))
		{
			return Ok(rows); // chưa có team → danh sách thành viên rỗng (hợp lệ)
		}
		string issue = null;
		if (query.Contains("MonthIssueSumStar."))
		{
			int a = query.IndexOf("MonthIssueSumStar.") + "MonthIssueSumStar.".Length;
			int b = query.IndexOf('"', a);
			issue = (b > a) ? query.Substring(a, b - a) : "";
		}
		List<KeyValuePair<int, JObject>> list = new List<KeyValuePair<int, JObject>>();
		DateTime now = DateTime.Now;
		Random rnd = new Random(now.Year * 1000 + now.DayOfYear);
		int dayFactor = Math.Max(1, now.Day);
		for (int k = 0; k < BotNames.Length; k++)
		{
			int sumStar = 300 + rnd.Next(0, 40000);
			int maxlv = sumStar / 1500 + 2 + rnd.Next(0, 30); // query sao: sumStar < maxlv*2000
			int month = rnd.Next(0, 120) * dayFactor;
			JObject user = new JObject { ["sumStar"] = sumStar, ["maxlv"] = maxlv, ["medal"] = rnd.Next(0, 20) };
			if (issue != null)
			{
				user["MonthIssueSumStar"] = new JObject { [issue] = month };
			}
			string botId = "b0t" + (k + 1).ToString("D2") + "0000000000000000000"; // 24 ký tự
			list.Add(new KeyValuePair<int, JObject>((issue != null) ? month : sumStar, Row(botId, BotNames[k], null, user)));
		}
		if (uid != null)
		{
			JObject doc = Doc(s, uid);
			JObject user = (JObject)((doc["User"] as JObject) ?? new JObject()).DeepClone();
			JObject account = (doc["Account"] as JObject) ?? new JObject();
			int score = -1;
			if (issue != null)
			{
				JObject m = user["MonthIssueSumStar"] as JObject;
				if (m != null && m[issue] != null)
				{
					score = (int)m[issue];
				}
			}
			else
			{
				score = (user["sumStar"] != null) ? (int)user["sumStar"] : 0;
			}
			if (score >= 0)
			{
				list.Add(new KeyValuePair<int, JObject>(score, Row(uid, (string)account["name"], (string)account["photoUrl"], user)));
			}
		}
		list.Sort((x, y) => y.Key.CompareTo(x.Key));
		for (int k = 0; k < list.Count && k < limit; k++)
		{
			rows.Add(list[k].Value);
		}
		return Ok(rows);
	}

	private static JObject Row(string userId, string name, string photo, JObject user)
	{
		return new JObject
		{
			["userId"] = userId,
			["projection"] = new JObject
			{
				["_id"] = new JObject { ["$oid"] = userId },
				["Account"] = new JObject { ["name"] = name, ["photoUrl"] = photo },
				["User"] = user
			}
		};
	}
}
#endif
