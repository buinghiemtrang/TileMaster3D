using System;
using System.Globalization;

public static class MDateTime
{
	public static readonly DateTime ZeroUTC = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public static readonly DateTime ZeroLocal = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);

	public const string FormatFull = "yyyyMMddHHmmss";

	public const string FormatDate = "yyyyMMdd";

	public static DateTime Parse(string s, DateTime defaultTime)
	{
		DateTime result = default(DateTime);
		if (!string.IsNullOrEmpty(s) && DateTime.TryParse(s, out result))
		{
			return result;
		}
		return defaultTime;
	}

	public static DateTime Parse(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return DateTime.Now;
		}
		DateTime result;
		if (!DateTime.TryParse(s, out result))
		{
			result = DateTime.Now;
		}
		return result;
	}

	public static DateTime GetDateTimeBySeconds(long seconds)
	{
		if (DateTime.Now.Kind == DateTimeKind.Local)
		{
			return ZeroLocal.AddSeconds(seconds);
		}
		return ZeroUTC.AddSeconds(seconds);
	}

	public static DateTime GetByString(string timeInfo, string timeType, bool isLocal)
	{
		DateTime result;
		if (!DateTime.TryParseExact(timeInfo, timeType, CultureInfo.InvariantCulture, isLocal ? DateTimeStyles.AssumeLocal : DateTimeStyles.AssumeUniversal, out result))
		{
			result = DateTime.Now;
		}
		return result;
	}
}
