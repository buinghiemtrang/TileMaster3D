using System;
using System.Globalization;

public static class DateTimeExtend
{
	public static readonly DateTime ZeroUTC = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public static readonly DateTime ZeroLocal = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);

	public const string TimeFormatFull = "yyyyMMddHHmmss";

	public const string TimeFormatDate = "yyyyMMdd";

	private const int SecondsPerMinute = 60;

	private const int SecondsPerHour = 3600;

	private const int SecondsPerDay = 86400;

	public static long TotalSeconds(this DateTime dateTime)
	{
		DateTime dateTime2 = ((dateTime.Kind == DateTimeKind.Local) ? ZeroLocal : ZeroUTC);
		return (long)(dateTime - dateTime2).TotalSeconds;
	}

	public static long TotalDays(this DateTime dateTime)
	{
		DateTime dateTime2 = ((dateTime.Kind == DateTimeKind.Local) ? ZeroLocal : ZeroUTC);
		return (long)(dateTime - dateTime2).TotalDays;
	}

	public static DateTime ParseBy(this DateTime self, string timeInfo, string timeType, bool isLocal)
	{
		if (string.IsNullOrEmpty(timeInfo))
		{
			return self;
		}
		DateTimeStyles styles = (isLocal ? DateTimeStyles.AssumeLocal : DateTimeStyles.AssumeUniversal);
		if (!DateTime.TryParseExact(timeInfo, timeType, CultureInfo.InvariantCulture, styles, out var result))
		{
			return self;
		}
		return result;
	}

	public static DateTime ParseFull(this DateTime self, string timeInfo, bool isLocal = true)
	{
		if (string.IsNullOrEmpty(timeInfo))
		{
			return self;
		}
		DateTimeStyles styles = (isLocal ? DateTimeStyles.AssumeLocal : DateTimeStyles.AssumeUniversal);
		if (!DateTime.TryParseExact(timeInfo, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, styles, out var result))
		{
			return self;
		}
		return result;
	}

	public static DateTime ParseDate(this DateTime self, string timeInfo, bool isLocal = true)
	{
		if (string.IsNullOrEmpty(timeInfo))
		{
			return self;
		}
		DateTimeStyles styles = (isLocal ? DateTimeStyles.AssumeLocal : DateTimeStyles.AssumeUniversal);
		if (!DateTime.TryParseExact(timeInfo, "yyyyMMdd", CultureInfo.InvariantCulture, styles, out var result))
		{
			return self;
		}
		return result;
	}

	public static DateTime Parse(this DateTime self, string s, CultureInfo culture, bool isLocal)
	{
		DateTimeStyles styles = (isLocal ? DateTimeStyles.AssumeLocal : DateTimeStyles.AssumeUniversal);
		if (!DateTime.TryParse(s, culture, styles, out var result))
		{
			result = self;
		}
		return result;
	}

	public static string ToStringDate(this DateTime dateTime)
	{
		return dateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
	}

	public static string ToStringFull(this DateTime dateTime)
	{
		return dateTime.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
	}

	public static long GetLocalTimeStamp()
	{
		return Convert.ToInt64((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
	}
}
