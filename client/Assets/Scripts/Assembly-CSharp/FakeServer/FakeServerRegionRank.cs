// [rebuild] FAKE_SERVER: Region Rank (rank/api/v1/app/upCustomRank + findCustomRankList).
// Bản gốc là bảng xếp hạng chung trên server (người chơi thật). Ở đây: lưu điểm upload vào state "regionRank",
// trộn với bot cố định theo rankType [giả định]:
//  - region_rank_personal_<vùng>_<issue>: 49 bot (nhóm 50 người), điểm tăng dần theo tiến độ ngày của đợt.
//  - region_rank_national_<issue>: 31 vùng trong cfgRegionRankRegions, điểm cộng dồn (isAddUp) của mình cộng vào vùng mình.
#if FAKE_SERVER
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public static partial class FakeServerHandlers
{
	private const string RegionRankKey = "regionRank";

	private const int RegionRankDuration = 86400; // cf_regionRank_const.ContinueTime

	private static readonly string[] RegionCodes = { "cn", "us", "gb", "ru", "ca", "fr", "jp", "kr", "in", "pk", "eg", "ph", "bd", "mx", "br", "ng", "id", "co", "vn", "cu", "za", "th", "ir", "ar", "de", "ke", "sa", "au", "tw", "tr", "it" };

	private static FakeResponse RegionRankUpload(FakeRequest q, FakeServerState s)
	{
		JArray list = Body(q)?["list"] as JArray;
		if (list == null)
		{
			return Ok();
		}
		JObject root = _RegionRankRoot(s);
		foreach (JToken t in list)
		{
			string type = (string)t["rankType"];
			string member = (string)t["member"];
			if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(member))
			{
				continue;
			}
			JObject table = root[type] as JObject;
			if (table == null)
			{
				table = new JObject();
				root[type] = table;
			}
			JObject row = table[member] as JObject;
			if (row == null)
			{
				row = new JObject { ["score"] = 0 };
				table[member] = row;
			}
			int score = (int?)t["score"] ?? 0;
			row["score"] = ((bool?)t["isAddUp"] ?? false) ? ((int)row["score"] + score) : score;
			row["expireAt"] = (long?)t["expireAt"] ?? 0;
			if (t["extraData"] != null && t["extraData"].Type == JTokenType.String)
			{
				row["data"] = (string)t["extraData"];
			}
		}
		s.MarkDirty();
		return Ok();
	}

	private static FakeResponse RegionRankFind(FakeRequest q, FakeServerState s)
	{
		JObject body = Body(q) ?? new JObject();
		string type = (string)body["rankType"] ?? "";
		string member = (string)body["member"];
		long expireAt = (long?)body["expireAt"] ?? 0;
		int size = Math.Max(1, (int?)body["size"] ?? 100);
		long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		double progress = (expireAt > 0) ? Clamp01(1.0 - (double)(expireAt - now) / RegionRankDuration) : 0.5;
		JObject stored = _RegionRankRoot(s)[type] as JObject ?? new JObject();
		Dictionary<string, KeyValuePair<int, string>> rows = new Dictionary<string, KeyValuePair<int, string>>();
		Random rnd = new Random(_StableHash(type));
		if (type.StartsWith("region_rank_national_"))
		{
			foreach (string code in RegionCodes)
			{
				int full = 200 + rnd.Next(0, 6000);
				rows[code] = new KeyValuePair<int, string>((int)(full * progress), null);
			}
			foreach (JProperty p in stored.Properties())
			{
				int baseScore = rows.TryGetValue(p.Name, out KeyValuePair<int, string> b) ? b.Key : 0;
				rows[p.Name] = new KeyValuePair<int, string>(baseScore + ((int?)p.Value["score"] ?? 0), null);
			}
		}
		else
		{
			for (int k = 0; k < 49; k++)
			{
				int full = 3 + rnd.Next(0, 70);
				string data = new JObject { ["name"] = BotNames[k % BotNames.Length] + ((k >= BotNames.Length) ? (k / BotNames.Length + 1).ToString() : ""), ["medal"] = rnd.Next(0, 20) }.ToString(Newtonsoft.Json.Formatting.None);
				rows["b0tR" + (k + 1).ToString("D2") + "00000000000000000000"] = new KeyValuePair<int, string>((int)(full * progress), data);
			}
			foreach (JProperty p in stored.Properties())
			{
				rows[p.Name] = new KeyValuePair<int, string>((int?)p.Value["score"] ?? 0, (string)p.Value["data"]);
			}
		}
		List<KeyValuePair<string, KeyValuePair<int, string>>> sorted = new List<KeyValuePair<string, KeyValuePair<int, string>>>(rows);
		sorted.Sort((x, y) => y.Value.Key.CompareTo(x.Value.Key));
		JArray list = new JArray();
		int selfRank = 0;
		for (int k = 0; k < sorted.Count; k++)
		{
			if (sorted[k].Key == member)
			{
				selfRank = k + 1;
			}
			if (k < size)
			{
				list.Add(new JObject { ["key"] = sorted[k].Key, ["score"] = sorted[k].Value.Key, ["data"] = sorted[k].Value.Value });
			}
		}
		return Ok(new JObject { ["list"] = list, ["nextRefreshAt"] = now + 60, ["selfRank"] = selfRank, ["total"] = sorted.Count });
	}

	private static JObject _RegionRankRoot(FakeServerState s)
	{
		JObject root = s.Get(RegionRankKey) as JObject;
		if (root == null)
		{
			root = new JObject();
			s.Set(RegionRankKey, root);
			root = (JObject)s.Get(RegionRankKey);
		}
		// bỏ bảng đã hết hạn quá 7 ngày
		long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		List<string> dead = new List<string>();
		foreach (JProperty p in root.Properties())
		{
			long exp = 0;
			foreach (JProperty r in ((JObject)p.Value).Properties())
			{
				exp = Math.Max(exp, (long?)r.Value["expireAt"] ?? 0);
			}
			if (exp > 0 && exp + 7 * 86400 < now)
			{
				dead.Add(p.Name);
			}
		}
		foreach (string d in dead)
		{
			root.Remove(d);
			s.MarkDirty();
		}
		return root;
	}

	private static double Clamp01(double v)
	{
		return (v < 0) ? 0 : ((v > 1) ? 1 : v);
	}

	private static int _StableHash(string str)
	{
		unchecked
		{
			int h = 23;
			foreach (char c in str)
			{
				h = h * 31 + c;
			}
			return h & 0x7fffffff;
		}
	}
}
#endif
