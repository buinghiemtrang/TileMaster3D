using System;
using System.Collections.Generic;
using DataCenter;

namespace Game.Activity.BakingMaster
{
	public class ActivityMgr : Singleton<ActivityMgr>
	{
		public const string EventRefreshActivityData = "BakingMaster_RefreshActivityData";

		public const string EventRefreshActivityInfo = "BakingMaster_RefreshActivityInfo";

		public const string EventUpdateOwnGrade = "BakingMaster_UpdateOwnGrade";

		public const string EventRefreshLoserData = "BakingMaster_RefreshLoserData";

		public string[] CfgGiftReward;

		public int LevelId { get; set; }

		public void Initialized()
		{
			if (!string.IsNullOrEmpty(cf_const.activityBakingMasterReward))
			{
				CfgGiftReward = cf_const.activityBakingMasterReward.Split('-');
			}
		}

		public ErrorCode JudgeEnterCondition()
		{
			if (DB.User.Maxlv >= cf_const.activityBakingMasterUnlockLv && DataManager.I.bLogined && DB.BakingMasterActivity.Info.EndTime != default(DateTime) && DB.BakingMasterActivity.Info.StartTime != default(DateTime) && !(DateTime.Now < DB.BakingMasterActivity.Info.StartTime) && !(DateTime.Now > DB.BakingMasterActivity.Info.EndTime))
			{
				return (!DB.BakingMasterActivity.IsTeamUser()) ? ErrorCode.NotTeam : ErrorCode.None;
			}
			return ErrorCode.Ended;
		}

		public void SetMatchData(List<RequestMatchData> datas)
		{
			DB.BakingMasterActivity.SetMatchData(datas);
			DispatcherMgr.Emit("BakingMaster_RefreshActivityData");
		}

		public void SetOwnGrade(RequestMatchData data)
		{
			DB.BakingMasterActivity.UpdateOwnGrade(data);
			DispatcherMgr.Emit("BakingMaster_UpdateOwnGrade");
		}

		public void SetIssueData(RequestIssueData data, int cfgId)
		{
			if (DB.BakingMasterActivity.Info.issue != data.issue)
			{
				DB.BakingMasterActivity.State = 0;
				DB.BakingMasterActivity.Data.InitData();
				DB.BakingMasterActivity.SaveData();
				if (!string.IsNullOrEmpty(DB.BakingMasterActivity.Info.issue))
				{
					LogEvent.I.Track("Cake_Baking_End").Add("ActivityTime", DB.BakingMasterActivity.Info.issue).Add("Team_ID", DB.BakingMasterActivity.Info.myTeam?.id)
						.Add("CakeGrade", GetUnlockLayerCount())
						.Add("MyFirst", GetMyFirstCount())
						.CommonAttr()
						.Send();
				}
			}
			DB.BakingMasterActivity.CfgId = cfgId;
			DB.BakingMasterActivity.Info = data;
			DB.BakingMasterActivity.Info.RefreshTime();
			DispatcherMgr.Emit("BakingMaster_RefreshActivityInfo");
		}

		public void ClearLoserData()
		{
			LogEvent.I.Track("Cake_Baking_Fail").Add("ActivityTime", DB.BakingMasterActivity.Info.issue).Add("Team_ID", DB.BakingMasterActivity.Info.myTeam.id)
				.Add("IsFailed", DB.BakingMasterActivity.Loser.Get().Exists((RequestMatchData a) => a.userId == DB.Account.Uid))
				.Add("FailedNumber", DB.BakingMasterActivity.Loser.Count)
				.Add("RescuesNumber", DB.BakingMasterActivity.Data.reviveCount)
				.Add("CakeGrade", GetUnlockLayerCount())
				.Add("MyFirst", GetMyFirstCount())
				.CommonAttr()
				.Send();
			DB.BakingMasterActivity.Loser.Clear();
			DispatcherMgr.Emit("BakingMaster_RefreshLoserData");
		}

		public void SignOutTeamClearLocalData()
		{
			DB.BakingMasterActivity.Loser.Clear();
			for (int num = DB.BakingMasterActivity.UnlockLayerStatus.Length - 1; num >= 0; num--)
			{
				DB.BakingMasterActivity.UnlockLayerStatus[num] = false;
			}
			DB.BakingMasterActivity.SaveUnlockLayerStatus();
			for (int num2 = DB.BakingMasterActivity.Levels.Length - 1; num2 >= 0; num2--)
			{
				DB.BakingMasterActivity.Levels[num2].data.Clear();
			}
			DB.BakingMasterActivity.SaveLevels();
		}

		public int GetUnlockLayerCount()
		{
			return DB.BakingMasterActivity.UnlockLayerStatus.Counts((bool d) => d);
		}

		public int GetMyFirstCount()
		{
			int count = 0;
			DB.BakingMasterActivity.Levels.ForEach(delegate(LevelData cls)
			{
				if (cls.data.Count != 0 && !(cls.data[0].userId != DB.Account.Uid))
				{
					count++;
				}
			});
			return count;
		}
	}
}
