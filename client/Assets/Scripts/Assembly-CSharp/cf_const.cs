using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class cf_const
{
	public static float comboMaxTime;

	public static float comboMinTime;

	public static float comboReduceTime;

	public static int nReviveTime;

	public static int nFreezeTime;

	public static int teatherLv;

	public static int maxEnergyRecover;

	public static int maxEnergyUpperLimit;

	public static int addEnergyTime;

	public static int reviveAdNum;

	public static int[] reviveNeedGold;

	public static int hintAdNum;

	public static int hintNeedGold;

	public static int hintBuyOnceNum;

	public static int freezeAdNum;

	public static int freezeNeedGold;

	public static int freezeBuyOnceNum;

	public static int refreshCoolTime;

	public static int backAdNum;

	public static int backNeedGold;

	public static int backBuyOnceNum;

	public static int refreshAdNum;

	public static int refreshNeedGold;

	public static int refreshBuyOnceNum;

	public static int unlockHintLv;

	public static int unlockBackLv;

	public static int unlockRefreshLv;

	public static int unlockFreezeLv;

	public static float highlightTipInterval;

	public static int highlightTipMinLv;

	public static int highlightTipMaxLv;

	public static float hintTipInterval;

	public static int freeGoldNum;

	public static int freeGoldReward;

	public static int watchOnecAdGetEnergyNum;

	public static int buyOnecGetEnergyNum;

	public static int buyEnergyNeedGold;

	public static int randomTaskMinKey;

	public static int randomTaskMaxKey;

	public static int unlockModelRandomMinKey;

	public static int unlockModelRandomMaxKey;

	public static int randomModelMinKey;

	public static int randomModelMaxKey;

	public static int lvStartPageUnlockLv;

	public static int magnetUnlockLv;

	public static int boosterUnlockLv;

	public static string boosterGear;

	public static int gameInterstitialUnlockLv;

	public static float gameInterstitialUnlockTime;

	public static float gameInterstitialIntervalTime;

	public static int interstitialUnlockLv;

	public static float interstitialIntervalTime;

	public static float nextInterstitialIntervalTime;

	public static int attOpenLevelID;

	public static bool watchRewardedVideoNotOutNextInterstitial;

	public static int unlimitedEnergyLv;

	public static int buyPropGiftShowNum;

	public static int unlockNoAdsLv;

	public static int unlockRateUsLv;

	public static int giftStallsMin;

	public static int giftStallsMax;

	public static int giftStallsDefault;

	public static string giftStallsUpCondition;

	public static string giftStallsDownTime;

	public static int showGiftButtonNum;

	public static int changeLvDifficultyRuleUndueLv;

	public static int startRecordCondition;

	public static string modelFuseUsedMinTimeByLv;

	public static float modelFuseUsedMinTime;

	public static float modelFuseUsedMaxTime;

	public static string failedModelFuseUsedTimeRandomIncrease;

	public static string undueLvTime;

	public static string lvDefaultTime;

	public static string activityRankingStarShowTime;

	public static int[] activityRankingStarUpDownCondition;

	public static int activityRankingStarRefreshOnceTime;

	public static int magnetAdNum;

	public static int magnetNeedGold;

	public static int magnetBuyOnceNum;

	public static int lvBuyProp;

	public static int customAdsVideoTimeMax;

	public static int unfocusTime;

	public static int unfocusTimeCD;

	public static bool isOpenCustomAdsVideo;

	public static int customAdsVidelUnlockLv;

	public static int[,] victoryStarAdsMul;

	public static int rankingStarRefreshTimeInterval;

	public static string giftStallsUpDirectly;

	public static int costOfCreateTeam;

	public static int teamMaxMembers;

	public static int teamUnlockLv;

	public static int parkEnterUnlockLv;

	public static int[] parkOneUnlockLv;

	public static int parkPlayDayCount;

	public static string victoriesActivityDate;

	public static bool victoriesActivityBannerJumpHomeLookAd;

	public static int victoriesActivityUnlockLv;

	public static float victoriesActivityRewardEnergyTime;

	public static bool isOpenMouseCheck;

	public static int treasureHuntActivityUnlockLv;

	public static int treasureHuntActivityUnitScore;

	public static bool canRepeatPopGift;

	public static int monthRankingActivityLimit;

	public static string autoEjectDialogOrder;

	public static int autoEjectDialogMaxNum;

	public static int royalPassActivityUnlockLv;

	public static int royalPassActivityUnitScore;

	public static bool isOpenMonthRankingActivity;

	public static int signBoardUnlockLv;

	public static int showGameParkBannerLv;

	public static int storeMultiple;

	public static string storeMultiplePeriod;

	public static int storeMultipleBuyTimes;

	public static bool storeMultipleSwitch;

	public static int getCardWeight;

	public static string randomCardBoxWeight;

	public static int[,] shopCardBoxQuality;

	public static int[] cardBoxRandomSeeds;

	public static bool cardBoxIsRandom;

	public static bool cardSystemClose;

	public static int cardSystemUnlockLevel;

	public static int noAdBannerAndGiftBannerWeight;

	public static int giftBannerUnlockLv;

	public static int activityBakingMasterUnlockLv;

	public static int activityBakingMasterReviveGold;

	public static string activityBakingMasterDate;

	public static string activityBakingMasterReward;

	public static string[] RechargeActivityDate;

	public static int RechargeActivityUsedRewardGroup;

	public static int RechargeActivityUnlockLv;

	public static bool IsForceCloseRechargeActivity;

	public static string[] RoadToRichesOpenTime;

	public static int RoadToRichesUnlockLv;

	public static int LuckyPrizeUnlockLv;

	public static int LvTimeMin;

	public static int[] RapidlyLevelCondition;

	public static int[] PeriodicityControlDifficultyLevelCondition;

	public static int PeriodicityControlDifficultyModelChangeNumber;

	public static int PeriodicityControlDifficultyTimeChangeNumber;

	public static bool IsOpenPeriodicityControlDifficulty;

	public static int PeriodicityControlDifficultyUseModels;

	public static int BuyNoAdsAddModelsMaxNum;

	public static int BuyNoAdsAddModelsMinNum;

	public static int PayingUsersAfterLevelIncrease;

	public static int[,] PayingUsersIncreaseValue;

	public static int OpenDynamicDifficultyLevel;

	public static float[,] DynamicDifficultyIncreasedPointsByVictoryRemainingTimeThreshold;

	public static float[,] DynamicDifficultyReducedPointsByTimeoutFailResidualModelThreshold;

	public static float[,] DynamicDifficultyReducedPointsByFullFailResidualModelThreshold;

	public static int DynamicDifficultyDownshiftNeedLevelNumber;

	public static float DynamicDifficultyDownshiftThreshold;

	public static int DynamicDifficultyDefaultPoints;

	public static int DynamicDifficultyMinPoints;

	public static int DynamicDifficultyMaxPoints;

	public static float WavePercentage;

	public static int[,] ElementGuide;

	public static int[] RegenerateLevelCondition;

	public static int ElementWholeGuideLevel;

	public static bool IsOpenLevelRecord;

	public static int ShortBattlePassUnlockLevel;

	public static bool IsOpenShortBattlePass;

	public static int ShortBattlePassUnitScore;

	public static int DailyLoginGuidePopTimes;

	public static float AdBreakTime;

	public static bool IsOpenVictoryReward;

	public static int[] VictoryRewardRewardGold;

	public static int IsVictoryBackHome;

	public static int VictoryBackHomeLevel;

	public static int ShowAdMultiStarLevel;

	public static string HardLevelIds;

	public static string HardLevelNumberCondition;

	public static string VeryHardLevelIds;

	public static string VeryHardLevelNumberCondition;

	public static int[,] NoviceProps;

	public static void Init()
	{
		Type typeFromHandle = typeof(cf_const);
		Dictionary<string, object> dictionary = LoadMgr.LoadJson<Dictionary<string, object>>("Json/cf_const");
		FieldInfo[] fields = typeFromHandle.GetFields();
		for (int i = 0; i < fields.Length; i++)
		{
			FieldInfo fieldInfo = fields[i];
			if (!dictionary.ContainsKey(fieldInfo.Name))
			{
				continue;
			}
			if (dictionary[fieldInfo.Name] is JArray || dictionary[fieldInfo.Name] is JObject)
			{
				fieldInfo.SetValue(typeFromHandle, JsonConvert.DeserializeObject(dictionary[fieldInfo.Name].ToString(), fieldInfo.FieldType));
			}
			else if (fieldInfo.FieldType.Name == "Int32")
			{
				fieldInfo.SetValue(typeFromHandle, Convert.ToInt32(dictionary[fieldInfo.Name]));
			}
			else if (fieldInfo.FieldType.Name == "Single")
			{
				fieldInfo.SetValue(typeFromHandle, Convert.ToSingle(dictionary[fieldInfo.Name]));
			}
			else
			{
				fieldInfo.SetValue(typeFromHandle, dictionary[fieldInfo.Name]);
			}
		}
	}
}
