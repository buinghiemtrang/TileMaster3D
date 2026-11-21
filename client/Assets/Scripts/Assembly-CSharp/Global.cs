using System;
using DG.Tweening;
using FQDev.UserLabel;
using UnityEngine;

public static class Global
{
	public static StoresMgr pStores = new StoresMgr();

	public static bool CanUseChinese = false;

	public static bool IsShowAdOrPurchase;

	public static bool NeedReloadConfig = false;

	public static DateTime interstitialcd = DateTime.Now;

	public static void ShowBanner()
	{
		Singleton<AdManager>.I.ShowBanner();
	}

	public static void HideBanner()
	{
		Singleton<AdManager>.I.HideBanner();
	}

	private static void _log(string msg = "")
	{
	}

	public static void ShowRewardedVideo(Action<string> callback, REWARD_PLACE place)
	{
		LogEvent.I.Ads_Click(place);
		bool flag = Singleton<NetworkInspectionManager>.I.CheckNetwork(null);
		if (Singleton<AdManager>.I.IsRewardVideoOk(isCheckLoadState: false) && flag)
		{
			LogEvent.I.Ads_Suc(place);
			AudioMgr.I.PauseMusic();
			AudioMgr.I.StopSoundAll();
			SceneMgr.OpenWaiting(2f);
			IsShowAdOrPurchase = true;
			Singleton<AdManager>.I.ShowRewardedVideo(delegate(string sResult)
			{
				IsShowAdOrPurchase = false;
				SceneMgr.CloseWaiting();
				if (sResult == "succ")
				{
					LogEvent.I.Ads_Return(place, "AF");
				}
				InterstitialcdCooling();
				AudioMgr.I.ResumeMusic();
				callback.InvokeSafe(sResult);
			}, place);
			return;
		}
		callback.InvokeSafe("fail");
		CreateMsgBoxDialog("Language135");
	}

	public static void InterstitialcdCooling()
	{
		DispatcherMgr.Emit("ResetInterstitialCoolingTime");
		interstitialcd = DateTime.Now;
		AdvertisingMgr.I.UpdateAdvertisingCd();
	}

	public static void ShowInterstitial(Action<string> callback, INTER_PLACE pLACE)
	{
		AdBreakBox box = null;
		Action showAd = null;
		LogEvent.I.Ads_Interstitial_Click(pLACE);
		AudioMgr.I.PauseMusic();
		SceneMgr.OpenWaiting(2f);
		box = null;
		showAd = delegate
		{
			IsShowAdOrPurchase = true;
			Singleton<AdManager>.I.ShowInterstitial(delegate(string info)
			{
				IsShowAdOrPurchase = false;
				SceneMgr.CloseWaiting();
				AudioMgr.I.ResumeMusic();
				callback.InvokeSafe(info);
				if (info == "succ")
				{
					Pop_Gift.GiftManager.Instance.AddInterstitialPlayCount(1);
				}
				if (box != null)
				{
					box._Close();
				}
			}, pLACE);
			InterstitialcdCooling();
			LogEvent.I.Ads_Interstitial(pLACE);
		};
		if (cf_const.AdBreakTime <= 0f)
		{
			showAd();
			return;
		}
		box = SceneMgr.AddTopNode<AdBreakBox>("Prefabs/AdBreakBox");
		box.Show(delegate
		{
			showAd();
		});
	}

	public static void ShowLvInterstitial(Action<string> callback, INTER_PLACE pLACE)
	{
		if ((DateTime.Now - interstitialcd).TotalSeconds >= (double)cf_const.interstitialIntervalTime && DB.User.Curlv >= cf_const.interstitialUnlockLv && !DB.User.AdFree && Singleton<AdManager>.I.IsInterstitialAdOk(isCheckLoadState: false))
		{
			ShowInterstitial(callback, pLACE);
			return;
		}
		callback.InvokeSafe("fail");
	}

	public static void ShowUserLevelInterstitial(Action<string> callback, INTER_PLACE pLACE)
	{
		if ((DateTime.Now - interstitialcd).TotalSeconds >= (double)cf_const.interstitialIntervalTime && DB.User.Curlv >= cf_const.interstitialUnlockLv && !DB.User.AdFree && Singleton<AdManager>.I.IsInterstitialAdOk(isCheckLoadState: false))
		{
			ShowInterstitial(callback, pLACE);
			return;
		}
		callback?.Invoke("fail");
	}

	public static void ShowNextInterstitial(Action<string> callback, int lv, INTER_PLACE pLACE)
	{
		if ((DateTime.Now - interstitialcd).TotalSeconds >= (double)cf_const.nextInterstitialIntervalTime && cf_const.interstitialUnlockLv <= lv && !DB.User.AdFree && Singleton<AdManager>.I.IsInterstitialAdOk(isCheckLoadState: false))
		{
			ShowInterstitial(callback, pLACE);
			return;
		}
		callback.InvokeSafe("fail");
	}

	public static void ResetAdNum()
	{
		DateTime today = DateTime.Today;
		DateTime dateTime = MDateTime.Parse(DB.User.AdNumResetTime, today);
		if (!(today < dateTime))
		{
			DB.User.AdNumResetTime = today.AddDays(1.0).ToString();
			DB.User.SurReviveAdNum = cf_const.reviveAdNum;
			DB.User.SurHintAdNum = cf_const.hintAdNum;
			DB.User.SurBackAdNum = cf_const.backAdNum;
			DB.User.SurRefreshAdNum = cf_const.refreshAdNum;
			DB.User.SurFreezeAdNum = cf_const.freezeAdNum;
			DB.User.SurFreeGoldNum = cf_const.freeGoldNum;
			DB.User.SurMagnetAdNum = cf_const.magnetAdNum;
			DB.SpecialBlockData.AvailableDailyCount = 0;
		}
	}

	public static void UpdatePlayerDate()
	{
		DB.User.SurReviveAdNum = ((DB.User.SurReviveAdNum < 0) ? cf_const.reviveAdNum : Mathf.Min(DB.User.SurReviveAdNum, cf_const.reviveAdNum));
		DB.User.SurHintAdNum = Mathf.Min(DB.User.SurHintAdNum, cf_const.hintAdNum);
		DB.User.SurBackAdNum = Mathf.Min(DB.User.SurBackAdNum, cf_const.backAdNum);
		DB.User.SurRefreshAdNum = Mathf.Min(DB.User.SurRefreshAdNum, cf_const.refreshAdNum);
		DB.User.SurFreezeAdNum = Mathf.Min(DB.User.SurFreezeAdNum, cf_const.freezeAdNum);
		DB.User.SurFreeGoldNum = Mathf.Min(DB.User.SurFreeGoldNum, cf_const.freeGoldNum);
		DB.User.SurMagnetAdNum = Mathf.Min(DB.User.SurMagnetAdNum, cf_const.magnetAdNum);
	}

	public static MsgBoxDialog CreateMsgBoxDialog(string msg = "", Action<bool> cBack = null, bool isTwoBtn = false, string title = "")
	{
		MsgBoxDialog msgBoxDialog = SceneMgr.AddTopNode<MsgBoxDialog>("Prefabs/MsgBoxDialog");
		if (msg != "")
		{
			Language.LanguageData languageDataByKey = Language.LanguageManager.Instance.GetLanguageDataByKey(msg);
			if (languageDataByKey != null)
			{
				msg = languageDataByKey.value;
			}
		}
		if (title != "")
		{
			Language.LanguageData languageDataByKey = Language.LanguageManager.Instance.GetLanguageDataByKey(title);
			if (languageDataByKey != null)
			{
				title = languageDataByKey.value;
			}
		}
		msgBoxDialog.Show(msg, cBack, isTwoBtn, title);
		return msgBoxDialog;
	}

	public static void ShowMsgTip(string msg = "")
	{
		MsgTip msgTip = SceneMgr.AddTopNode<MsgTip>("Prefabs/MsgTip");
		if (!string.IsNullOrEmpty(msg))
		{
			Language.LanguageData languageDataByKey = Language.LanguageManager.Instance.GetLanguageDataByKey(msg);
			if (languageDataByKey != null)
			{
				msg = languageDataByKey.value;
			}
		}
		msgTip.Show(msg);
	}

	public static RetryDialog ShowRetryDialog(string msg = "", Action<bool> cBack = null)
	{
		RetryDialog retryDialog = SceneMgr.AddTopNode<RetryDialog>("Prefabs/RetryDialog");
		if (!string.IsNullOrEmpty(msg))
		{
			Language.LanguageData languageDataByKey = Language.LanguageManager.Instance.GetLanguageDataByKey(msg);
			if (languageDataByKey != null)
			{
				msg = languageDataByKey.value;
			}
		}
		retryDialog.cback = cBack;
		retryDialog.Show(msg);
		return retryDialog;
	}

	public static RewardUnifyDialog CreateReward(int[,] rewards, RewardType type = RewardType.None, REWARD_PLACE place = REWARD_PLACE.None, string sign = "", bool isShowExeAdIcon = false, bool isShowOnly = false)
	{
		if (string.IsNullOrEmpty(sign))
		{
			sign = type.ToString();
		}
		string s_file = "Prefabs/RewardUnifyDialog";
		if (!cf_const.cardSystemClose)
		{
			for (int num = rewards.GetLength(0) - 1; num >= 0; num--)
			{
				if (rewards[num, 0] == 30)
				{
					s_file = "Prefabs/RewardCardBoxDialog";
					break;
				}
			}
		}
		RewardUnifyDialog rewardUnifyDialog = SceneMgr.AddTopNode<RewardUnifyDialog>(s_file);
		rewardUnifyDialog.SetRewardPlace(place, sign);
		rewardUnifyDialog.SetRewardType(type);
		rewardUnifyDialog.isShowOnly = isShowOnly;
		rewardUnifyDialog.SetInfo(rewards, isShowExeAdIcon);
		rewardUnifyDialog.Show();
		return rewardUnifyDialog;
	}

	public static RewardUnifyDialog CreateReward(string rewards, RewardType rType = RewardType.None, REWARD_PLACE place = REWARD_PLACE.None, string sign = "", bool isShowExeAdIcon = false)
	{
		if (string.IsNullOrEmpty(rewards))
		{
			return null;
		}
		return CreateReward(rewards.ToInt(2, ',', '|'), rType, place, sign, isShowExeAdIcon);
	}

	public static void ShowBubbleTips(string tips = "", Transform tfTarget = null)
	{
		BubbleTips bubbleTips = SceneMgr.AddTopNode<BubbleTips>("Prefabs/BubbleTips");
		if (!string.IsNullOrEmpty(tips))
		{
			Language.LanguageData languageDataByKey = Language.LanguageManager.Instance.GetLanguageDataByKey(tips);
			if (languageDataByKey != null)
			{
				tips = languageDataByKey.value;
			}
		}
		bubbleTips.Show(tips, tfTarget);
	}

	public static void ReceiveReward(int[,] reward, string place)
	{
		if (reward == null)
		{
			return;
		}
		for (int i = 0; i < reward.GetLength(0); i++)
		{
			PropID propID = (PropID)reward[i, 0];
			if (propID != PropID.CardPack && propID != PropID.EnergyRecoverUpLimit)
			{
				if (propID == PropID.EnergyTime)
				{
					DB.Bag.AddEnergyUnlimitedTime(reward[i, 1] * 60);
				}
				else
				{
					DB.Bag.Modify(propID, reward[i, 1], b_update: true, place);
				}
			}
		}
	}

	public static void GetContents(PropID propId, int va, out Sprite sprite, out string text)
	{
		if (propId == PropID.Medal)
		{
			sprite = Resources.Load<Sprite>(string.Format("Props/medal{0}", va));
			text = "x1";
			return;
		}
		sprite = Resources.Load<Sprite>(string.Format("Props/prop{0}", (int)propId));
		switch (propId)
		{
		case PropID.EnergyTime:
			text = string.Format("{0}h", va);
			break;
		case PropID.CardPack:
			text = "x1";
			break;
		default:
			text = string.Format("x{0}", va);
			break;
		}
	}

	public static void GetContents(PropID propId, int value, string character, out Sprite sprite, out string text)
	{
		if (propId == PropID.Medal)
		{
			sprite = Resources.Load<Sprite>(string.Format("Props/medal{0}", value));
			text = character + "1";
			return;
		}
		sprite = Resources.Load<Sprite>(string.Format("Props/prop{0}", (int)propId));
		switch (propId)
		{
		case PropID.EnergyTime:
			text = string.Format("{0}h", value);
			break;
		case PropID.CardPack:
			text = character + "1";
			break;
		default:
			text = string.Format("{0}{1}", character, value);
			break;
		}
	}

	public static void AddNoviceProps()
	{
		if (DB.User.Maxlv > 0 && DB.Bag.IsGetNoviceProp == 0)
		{
			DB.Bag.IsGetNoviceProp = 1;
		}
		if (DB.Bag.IsGetNoviceProp <= 0)
		{
			if (cf_const.NoviceProps != null)
			{
				ReceiveReward(cf_const.NoviceProps, "NoviceHandsel");
			}
			DB.Bag.IsGetNoviceProp = 1;
		}
	}

	public static string GetChannelName()
	{
		if (string.IsNullOrWhiteSpace(UserLabelGetters.GetMediaSource))
		{
			return "None";
		}
		if (string.IsNullOrWhiteSpace(UserLabelGetters.Campaign))
		{
			return "None";
		}
		// UNVERIFIED: separator/punctuation of the three log literals (SL__GetChannelName_MediaSource__, SL____Campaign__, SL__GetChannelName____) not resolved (RVA 0x1173e80)
		UnityEngine.Debug.Log("GetChannelName MediaSource:" + UserLabelGetters.GetMediaSource + ", Campaign:" + UserLabelGetters.Campaign);
		foreach (ConfigHyperBidChannelUpload configHyperBidChannelUpload in DataCfg.ConfigHyperBidChannelUploads)
		{
			if (UserLabelGetters.GetMediaSource.Contains(configHyperBidChannelUpload.afMediasourceMatch, StringComparison.OrdinalIgnoreCase) && UserLabelGetters.Campaign.Contains(configHyperBidChannelUpload.campaignMatch, StringComparison.OrdinalIgnoreCase))
			{
				UnityEngine.Debug.LogError("GetChannelName: " + configHyperBidChannelUpload.setChannelName);
				return configHyperBidChannelUpload.setChannelName;
			}
		}
		UnityEngine.Debug.LogError("GetChannelName: " + UserLabelGetters.GetMediaSource);
		return UserLabelGetters.GetMediaSource;
	}
}
