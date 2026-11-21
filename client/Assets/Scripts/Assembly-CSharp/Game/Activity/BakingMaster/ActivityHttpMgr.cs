using System;
using System.Collections.Generic;
using BestHTTP;
using Newtonsoft.Json;
using OwnHttp;
using SimpleJSON;
using UnityEngine;

namespace Game.Activity.BakingMaster
{
	public class ActivityHttpMgr
	{
		private const string GetMatchDataKey = "BakingMasterHttp_GetMatchDataKey";

		private const string GetIssueDataKey = "BakingMasterHttp_GetIssueDataKey";

		private const string PostMatchDataKey = "BakingMasterHttp_PostMatchDataKey";

		private const string DeleteCageDataKey = "BakingMasterHttp_DeleteCageDataKey";

		public static ActivityHttpMgr I { get; } = new ActivityHttpMgr();

		public void PostActivityGrade(int level, int grade)
		{
			RequestMgr.Headers.Clear();
			CheckRepeatRequestsMgr.I.Creat("BakingMasterHttp_PostMatchDataKey", HTTPMethods.Post, "https://service.fuqiangame.com/team/SetMatch", delegate(ResultInfo info)
			{
				OwnHttp.ServerDataRoot<RequestMatchData> serverDataRoot = ParsingServerData.SendInfo<RequestMatchData>(info);
				if (serverDataRoot != null && serverDataRoot.code == 1 && serverDataRoot.Data != null)
				{
					DB.BakingMasterActivity.UpdateOwnGrade(serverDataRoot.Data);
					DispatcherMgr.Emit("BakingMaster_UpdateOwnGrade");
				}
			})
				?.AddHeader("Authorization", "Bearer " + DataManager.I.Token)
				?.AddHeader("Content-Type", "application/json")
				?.AddParameter(JsonConvert.SerializeObject(new PostMatchGrade
				{
					Issue = DB.BakingMasterActivity.Info.Issue,
					LevelId = level,
					Grade = ((level != 0) ? grade : 0),
					Extension = new JSONObject
					{
						["userName"] = new JSONString(DB.Account.GetName())
					}.ToString()
				}))
				?.AddCustomTimeout(5f)
				?.Send();
		}

		public void RequestActivityData()
		{
			RequestMgr.Headers.Clear();
			CheckRepeatRequestsMgr.I.Creat("BakingMasterHttp_GetMatchDataKey", HTTPMethods.Get, "https://service.fuqiangame.com/team/GetMatchs?issue=" + DB.BakingMasterActivity.Info.Issue, delegate(ResultInfo resp)
			{
				OwnHttp.ServerDataListRoot<RequestMatchData> serverDataListRoot = ParsingServerData.SendListInfo<RequestMatchData>(resp);
				if (serverDataListRoot != null && serverDataListRoot.code == 1 && serverDataListRoot.Data != null)
				{
					DB.BakingMasterActivity.SetMatchData(serverDataListRoot.Data);
					DispatcherMgr.Emit("BakingMaster_RefreshActivityData");
				}
			})
				?.AddHeader("Authorization", "Bearer " + DataManager.I.Token)
				?.AddCustomTimeout(5f)
				?.Send();
		}

		private void _requestIssue(int[,] cfg, int id)
		{
			string url = string.Format("{0}/team/GetGeneralActivity?type={1}&days={2}&offsetForDays={3}&minutes={4}", "https://service.fuqiangame.com", ((DateType)cfg[id, 0]).ToString(), cfg[id, 1], cfg[id, 2], cfg[id, 3]);
			RequestMgr.Headers.Clear();
			CheckRepeatRequestsMgr.I.Creat("BakingMasterHttp_GetIssueDataKey", HTTPMethods.Get, url, delegate(ResultInfo resp)
			{
				OwnHttp.ServerDataRoot<RequestIssueData> serverDataRoot = ParsingServerData.SendInfo<RequestIssueData>(resp);
				if (serverDataRoot != null && serverDataRoot.code == 1 && serverDataRoot.Data != null)
				{
					RequestIssueData data = serverDataRoot.Data;
					if (DB.BakingMasterActivity.Info != null && DB.BakingMasterActivity.Info.issue != data.issue)
					{
						DB.BakingMasterActivity.State = 0;
						DB.BakingMasterActivity.Data.InitData();
						DB.BakingMasterActivity.SaveData();
						if (!string.IsNullOrEmpty(DB.BakingMasterActivity.Info.issue))
						{
							LogEvent.I.Track("Cake_Baking_End")
								.Add("ActivityTime", DB.BakingMasterActivity.Info.issue)
								.Add("Team_ID", DB.BakingMasterActivity.Info.myTeam?.id)
								.Add("CakeGrade", ActivityMgr.I.GetUnlockLayerCount())
								.Add("MyFirst", ActivityMgr.I.GetMyFirstCount())
								.CommonAttr()
								.Send();
						}
					}
					DB.BakingMasterActivity.CfgId = id;
					DB.BakingMasterActivity.Info = data;
					DB.BakingMasterActivity.Info.RefreshTime();
					DispatcherMgr.Emit("BakingMaster_RefreshActivityInfo");
				}
			})
				?.AddHeader("Authorization", "Bearer " + DataManager.I.Token)
				?.AddHeader("CommonServiceToken", "9b5ba0c0419840ccab872275f41e5cc7")
				?.AddCustomTimeout(5f)
				?.Send();
		}

		public void RequestIssueById()
		{
			int[,] array = cf_const.activityBakingMasterDate.ToInt(4, '|', ',');
			int id = Mathf.Clamp(DB.BakingMasterActivity.CfgId, 0, array.GetLength(0) - 1);
			_requestIssue(array, id);
		}

		public void RequestIssue()
		{
			int[,] array = cf_const.activityBakingMasterDate.ToInt(4, '|', ',');
			int length = array.GetLength(0);
			if (length == 1)
			{
				_requestIssue(array, 0);
				return;
			}
			DateTime utcNow = DateTime.UtcNow;
			for (int num = length - 1; num >= 0; num--)
			{
				GetIssueTime((DateType)array[num, 0], array[num, 1], array[num, 2], array[num, 3], out var startTime, out var endTime);
				if (utcNow >= startTime && utcNow <= endTime)
				{
					_requestIssue(array, num);
					break;
				}
			}
		}

		private void GetIssueTime(DateType type, int days, int offset, int minutes, out DateTime startTime, out DateTime endTime)
		{
			DateTime dateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Today);
			switch (type)
			{
			case DateType.Year:
			{
				DateTime dateTime2 = new DateTime(dateTime.Year, 0, 0);
				if (days < 0)
				{
					dateTime2 = dateTime2.AddYears(1);
				}
				dateTime = dateTime2.AddDays(days);
				break;
			}
			case DateType.Month:
			{
				DateTime dateTime3 = new DateTime(dateTime.Year, dateTime.Month, 0);
				if (days < 0)
				{
					dateTime3 = dateTime3.AddMonths(1);
				}
				dateTime = dateTime3.AddDays(days);
				break;
			}
			case DateType.Week:
			{
				int num = ((dateTime.DayOfWeek != 0) ? ((int)dateTime.DayOfWeek) : 7);
				int num2 = ((days != 0) ? days : 7);
				dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).AddDays(num2 - num);
				break;
			}
			case DateType.Day:
				break;
			default:
				throw new ArgumentOutOfRangeException("type", type, null);
			}
			startTime = dateTime.AddMinutes(offset);
			endTime = startTime.AddMinutes(minutes);
		}

		public void RequestClearLoserAll()
		{
			RequestMgr.Headers.Clear();
			CheckRepeatRequestsMgr.I.Creat("BakingMasterHttp_GetMatchDataKey", HTTPMethods.Delete, "https://service.fuqiangame.com/team/DeleteZeroGradeMatch?issue=" + DB.BakingMasterActivity.Info.Issue, delegate(ResultInfo resp)
			{
				OwnHttp.ServerDataListRoot<RequestMatchData> serverDataListRoot = ParsingServerData.SendListInfo<RequestMatchData>(resp);
				if (serverDataListRoot != null && serverDataListRoot.code == 1)
				{
					ActivityMgr.I.ClearLoserData();
				}
			})
				?.AddHeader("Authorization", "Bearer " + DataManager.I.Token)
				?.AddCustomTimeout(5f)
				?.Send();
		}
	}
}
