using System;
using UnityEngine;

public static class LogEventLogin
{
	public static void Init(Action callbackFirstday)
	{
		LogEventItem.Init();
		DB.Track.OpenAppCount++;
		int dayOfYear = DateTime.UtcNow.DayOfYear;
		if (dayOfYear != DB.Track.LastLoginDay)
		{
			DB.Track.LastLoginDay = dayOfYear;
			DB.Track.PlayDays++;
			DB.Track.GameTimes = 0;
			LogEvent.I.Track("LoginDaily_Firstday").Add("day", DB.Track.PlayDays).Send(true);
			callbackFirstday?.Invoke();
			DB.Other.PopNetworkBoxCount = 0;
		}
		LogEvent.I.UpdateJoinTime();
		DB.Track.LoginTime = DateTime.Now.ToString();
		DB.Track.SetFristLoginTime();
		PlayerPrefs.Save();
		LogEvent.I.Track("Start_Login").Add("LogType", "app").Add("FirstBegin", DB.Track.OpenAppCount == 1).IsNew().Send(true);
		LogEvent.I.Track("User_Base_Information")
			.Add("Level_ID", DB.User.Maxlv + 1)
			.Add("AdFree", DB.User.AdFree)
			.Add("UserId", DB.Account.Userid)
			.Add("DeviceId", SystemInfo.deviceUniqueIdentifier)
			.Add("LoginPlatform", DB.Account.LoginPlatform)
			.Add("GameVersion", Application.version)
			.Add("OperatingSystem", SystemInfo.operatingSystem)
			.Add("DeviceModel", SystemInfo.deviceModel)
			.IsNew()
			.Send(false);
		LogEvent.I.Track()
			.Add("AccountID", DB.Account.Userid)
			.UserSet();
		LogEvent.I.Track()
			.Add("LoginType", "app")
			.Add("OpenNum", DB.Track.OpenAppCount)
			.Add("PlayDays", DB.Track.PlayDays)
			.Add("NowVersion", Application.version)
			.Add("MaxLevel", DB.User.Maxlv + 1)
			.UserSet();
		if (DB.Track.OpenAppCount != 1)
		{
			return;
		}
		LogEvent.I.Track("custom_first_open").SendFireBase();
		LogEvent.I.Track()
			.Add("LoginTime", DB.Track.LoginTime)
			.Add("GameVersion", Application.version)
			.Add("PhoneModel", SystemInfo.deviceModel)
			.Add("OSVersion", SystemInfo.operatingSystem)
			.UserSetOnce();
	}

	public static void LogInError(string logInType, string errorMessage)
	{
		LogEvent.I.Track("LogIn_Error").CommonAttr(true).Add("LogInType", logInType).Add("ErrorMessage", errorMessage).Send(false);
	}
}
