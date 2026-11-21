using System;
using System.Collections.Generic;
using DataCenter;
using GameMail;
using Newtonsoft.Json;
using UnityEngine;

public class LoginFinishListen : MonoBehaviour
{
	private Dictionary<string, object> _dict_cls = new Dictionary<string, object>();

	private void Start()
	{
		DispatcherMgr.OnG<Dictionary<string, object>>("LoginFinishReadData", LoginFinishReadData);
	}

	private void LoginFinishReadData(Dictionary<string, object> data)
	{
		if (data.Count == 0)
		{
			_UploadDoc();
		}
		else
		{
			_DownloadDoc(data);
		}
		DispatcherMgr.Emit("Guest_Login", GuestLoginState.Success);
	}

	private void _UploadDoc()
	{
		DB.Account.RefreshAccount();
		DispatcherMgr.Emit("Update_UserData");
		DataManager.I.UploadDataAllToRemote();
		SceneMgr.CloseWaiting();
	}

	private void _DownloadDoc(Dictionary<string, object> data)
	{
		Action func_update_data = delegate
		{
			DB.Account.RefreshAccount();
			DataCenterLoginMgr.I.UpdateDataAll();
		};
		Action func_online_to_local = delegate
		{
			foreach (KeyValuePair<string, IAssembleDB> item in DataManager.I.DictCls)
			{
				if (_dict_cls.TryGetValue(item.Key, out var value))
				{
					item.Value.SetData(value);
					item.Value.WriteLocalData();
				}
			}
		};
		Action func_merge_data = delegate
		{
			User user = (User)_dict_cls["User"];
			Bag bag = (Bag)_dict_cls["Bag"];
			_ = _dict_cls["Synthetic"];
			Chest chest = (Chest)_dict_cls["Chest"];
			Account account = (Account)_dict_cls["Account"];
			TreasureHuntActivity treasureHuntActivity = (TreasureHuntActivity)_dict_cls["TreasureHuntActivity"];
			RoyalPassActivity royalPassActivity = (RoyalPassActivity)_dict_cls["RoyalPassActivity"];
			Mail mail = (Mail)_dict_cls["Mail"];
			Other other = (Other)_dict_cls["Other"];
			BakingMasterActivity bakingMasterActivity = (BakingMasterActivity)_dict_cls["BakingMasterActivity"];
			DataCenter.ShortBattlePass shortBattlePass = (DataCenter.ShortBattlePass)_dict_cls["ShortBattlePass"];
			SpecialBlockData specialBlockData = (SpecialBlockData)_dict_cls["SpecialBlockData"];
			UserLevelReward userLevelReward = (UserLevelReward)_dict_cls["UserLevelReward"];
			PropIceBreakingGift propIceBreakingGift = (PropIceBreakingGift)_dict_cls["PropIceBreakingGift"];
			DB.SpecialBlockData.RewardCount = specialBlockData.RewardCount;
			DB.SpecialBlockData.AvailableDailyCount = specialBlockData.AvailableDailyCount;
			DB.UserLevelReward.UserLevel = userLevelReward.UserLevel;
			DB.UserLevelReward.UserScore = userLevelReward.UserScore;
			DB.UserLevelReward.NextLevelId = userLevelReward.NextLevelId;
			foreach (KeyValuePair<string, int> item in bag.Prop)
			{
				if (!DB.Bag.Prop.ContainsKey(item.Key) || DB.Bag.Prop[item.Key] < item.Value)
				{
					DB.Bag.SetProp(item.Key, item.Value);
				}
			}
			DB.Bag.IsGetNoviceProp = bag.IsGetNoviceProp;
			DB.Account.Name = (string.IsNullOrEmpty(account.Name) ? DB.Account.Name : account.Name);
			DB.Chest.Id = Math.Max(DB.Chest.Id, chest.Id);
			DB.Chest.Num = Math.Max(DB.Chest.Num, chest.Num);
			DB.Chest.IdLv = Math.Max(DB.Chest.IdLv, chest.IdLv);
			DB.User.Curlv = Math.Max(DB.User.Curlv, user.Curlv);
			DB.User.Maxlv = Math.Max(DB.User.Maxlv, user.Maxlv);
			DB.User.Newlv = Math.Max(DB.User.Newlv, user.Newlv);
			DB.User.Lastlv = Math.Max(DB.User.Lastlv, user.Lastlv);
			DB.User.AdFree = DB.User.AdFree || user.AdFree;
			DB.User.IsPayingUsers = DB.User.IsPayingUsers || user.IsPayingUsers;
			DB.User.FirstConsumeLv = Math.Max(DB.User.FirstConsumeLv, user.FirstConsumeLv);
			DB.User.SumStar = Math.Max(DB.User.SumStar, user.SumStar);
			DB.User.TotalConsume = Math.Max(DB.User.TotalConsume, user.TotalConsume);
			DB.User.LvMedal = Math.Max(DB.User.LvMedal, user.LvMedal);
			DB.User.DynamicDifficultyLevel = Math.Max(DB.User.DynamicDifficultyLevel, user.DynamicDifficultyLevel);
			DB.User.DynamicDifficultyPoints = Math.Max(DB.User.DynamicDifficultyPoints, user.DynamicDifficultyPoints);
			if (user.MonthIssueSumStar_ != null && user.MonthIssueSumStar_.Count > 0)
			{
				foreach (KeyValuePair<string, int> item2 in user.MonthIssueSumStar_)
				{
					if (!DB.User.MonthIssueSumStar_.ContainsKey(item2.Key))
					{
						DB.User.SetMonthIssueSumStar_(item2.Key, item2.Value);
					}
					else if (DB.User.MonthIssueSumStar_[item2.Key] < item2.Value)
					{
						DB.User.SetMonthIssueSumStar_(item2.Key, item2.Value);
					}
				}
			}
			DB.TreasureHuntActivity.Issue = treasureHuntActivity.Issue;
			DB.TreasureHuntActivity.StartTime = treasureHuntActivity.StartTime;
			DB.TreasureHuntActivity.EndTime = treasureHuntActivity.EndTime;
			DB.TreasureHuntActivity.ActivityRewardGroupId = treasureHuntActivity.ActivityRewardGroupId;
			DB.TreasureHuntActivity.ActivityRankingRewardGroupId = treasureHuntActivity.ActivityRankingRewardGroupId;
			DB.TreasureHuntActivity.IsReceivedActivityReward.Set(treasureHuntActivity.IsReceivedActivityReward.Get());
			DB.TreasureHuntActivity.ActivityState = treasureHuntActivity.ActivityState;
			DB.TreasureHuntActivity.TeamId = treasureHuntActivity.TeamId;
			DB.TreasureHuntActivity.AllScores = treasureHuntActivity.AllScores;
			DB.RoyalPassActivity.Issue = royalPassActivity.Issue;
			DB.RoyalPassActivity.StartTime = royalPassActivity.StartTime;
			DB.RoyalPassActivity.EndTime = royalPassActivity.EndTime;
			DB.RoyalPassActivity.ActivityState = royalPassActivity.ActivityState;
			DB.RoyalPassActivity.RewardsState.Set(royalPassActivity.RewardsState.Get());
			DB.RoyalPassActivity.Scores = royalPassActivity.Scores;
			DB.ShortBattlePass.Data = shortBattlePass.Data;
			DB.BakingMasterActivity.Data = bakingMasterActivity.Data;
			DB.Other.LuckyPrizeKind.Set(other.LuckyPrizeKind.Get());
			DB.Other.LuckyPrizeRefreshTime = other.LuckyPrizeRefreshTime;
			DB.Other.LuckyPrizeRewardIndex = other.LuckyPrizeRewardIndex;
			DB.Other.RoadToRichesCount = other.RoadToRichesCount;
			DB.Other.RoadToRichesEndTime = other.RoadToRichesEndTime;
			DB.Mail.MailIds.Set(mail.MailIds.Get());
			foreach (KeyValuePair<string, MailData> item3 in mail.Mails)
			{
				if (!DB.Mail.Mails.ContainsKey(item3.Key))
				{
					DB.Mail.Save(item3.Key, item3.Value);
				}
			}
			DB.PropIceBreakingGift.LastShowTime = propIceBreakingGift.LastShowTime;
			DB.PropIceBreakingGift.AfterShowPassLevelCount = propIceBreakingGift.AfterShowPassLevelCount;
			DB.PropIceBreakingGift.PaidGiftType = propIceBreakingGift.PaidGiftType;
			DB.PropIceBreakingGift.PaidGiftBuyCount = propIceBreakingGift.PaidGiftBuyCount;
			DB.PropIceBreakingGift.CoinGiftBuyCount = propIceBreakingGift.CoinGiftBuyCount;
			DB.Other.ParkTickets = other.ParkTickets;
			DB.Other.PigBankGold = other.PigBankGold;
		};
		_dict_cls.Clear();
		foreach (KeyValuePair<string, IAssembleDB> item in DataManager.I.DictCls)
		{
			try
			{
				if (!data.ContainsKey(item.Key))
				{
					_dict_cls.Add(item.Key, item.Value.Create());
				}
				else
				{
					string text = item.Value.GetRemoteDataToString(data[item.Key].ToString());
					object value = JsonConvert.DeserializeObject(text, item.Value.GetClsType());
					_dict_cls.Add(item.Key, value);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(item.Key + " data set error: " + ex.Message);
			}
		}
		if (DB.User.Maxlv == 0)
		{
			func_online_to_local();
		}
		else
		{
			if (DataCenterLoginMgr.I.GetPlatformID() != -1)
			{
				InitUserInfoComparisonSet initUserInfoComparisonSet = SceneMgr.AddTopNode<InitUserInfoComparisonSet>("Prefabs/Login/LoginComparisonDialog");
				initUserInfoComparisonSet.Init((Account)_dict_cls["Account"], (User)_dict_cls["User"], (Bag)_dict_cls["Bag"], DB.Account, DB.User, DB.Bag);
				initUserInfoComparisonSet.callback = delegate(bool b_online)
				{
					if (b_online)
					{
						func_online_to_local();
						func_update_data();
					}
					else
					{
						_UploadDoc();
					}
				};
				initUserInfoComparisonSet.pDialog.func_callback_close = delegate
				{
					if (DataCenterLoginMgr.I.GetPlatformID() == -1)
					{
						DispatcherMgr.Emit("Guest_Login_Fail");
					}
					DataCenterLoginMgr.I.Logout();
					DataCenterLoginMgr.I.SignOut(clear: false);
				};
				SceneMgr.CloseWaiting();
				return;
			}
			func_merge_data();
		}
		func_update_data();
		SceneMgr.CloseWaiting();
	}
}
