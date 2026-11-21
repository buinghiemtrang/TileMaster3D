using System.Collections.Generic;

public static class DataCfg
{
	public static Dictionary<int, CardSystemCardBox> CardSystemCardBoxs = new Dictionary<int, CardSystemCardBox>();

	public static Dictionary<int, CardSystemCardBoxGroups> CardSystemCardBoxGroupss = new Dictionary<int, CardSystemCardBoxGroups>();

	public static Dictionary<int, CardSystemCards> CardSystemCardss = new Dictionary<int, CardSystemCards>();

	public static Dictionary<int, CardSystemGroup> CardSystemGroups = new Dictionary<int, CardSystemGroup>();

	public static List<CfgValueOnceTrack> CfgValueOnceTracks = new List<CfgValueOnceTrack>();

	public static List<CfgValueRepeatTrack> CfgValueRepeatTracks = new List<CfgValueRepeatTrack>();

	public static List<ConfigHyperBidChannelUpload> ConfigHyperBidChannelUploads = new List<ConfigHyperBidChannelUpload>();

	public static List<DirectPurchase> DirectPurchases = new List<DirectPurchase>();

	public static List<DirectPurchaseStall> DirectPurchaseStalls = new List<DirectPurchaseStall>();

	public static List<FacebookAdValue> FacebookAdValues = new List<FacebookAdValue>();

	public static List<VictoriesActivityReward> VictoriesActivityRewards = new List<VictoriesActivityReward>();

	public static List<cfbt_Gift> cfbt_Gifts = new List<cfbt_Gift>();

	public static List<cfbt_GiftStalls> cfbt_GiftStallss = new List<cfbt_GiftStalls>();

	public static List<cfbt_Stores> cfbt_Storess = new List<cfbt_Stores>();

	public static List<cfgAccumlatedAdReward> cfgAccumlatedAdRewards = new List<cfgAccumlatedAdReward>();

	public static List<cfgActivityEntranceInformation> cfgActivityEntranceInformations = new List<cfgActivityEntranceInformation>();

	public static Dictionary<int, List<cfgBakingMasterLevel>> cfgBakingMasterLevels = new Dictionary<int, List<cfgBakingMasterLevel>>();

	public static List<cfgFestival> cfgFestivals = new List<cfgFestival>();

	public static List<cfgFestivalPointInformation> cfgFestivalPointInformations = new List<cfgFestivalPointInformation>();

	public static List<cfgLevelChest> cfgLevelChests = new List<cfgLevelChest>();

	public static List<cfgLevelDifficulty> cfgLevelDifficultys = new List<cfgLevelDifficulty>();

	public static Dictionary<int, cfgLevelFixed> cfgLevelFixeds = new Dictionary<int, cfgLevelFixed>();

	public static List<cfgLevelRandom> cfgLevelRandoms = new List<cfgLevelRandom>();

	public static List<cfgLevelRewardVideo> cfgLevelRewardVideos = new List<cfgLevelRewardVideo>();

	public static List<cfgLevelUnlock> cfgLevelUnlocks = new List<cfgLevelUnlock>();

	public static List<cfgLuckyPrize> cfgLuckyPrizes = new List<cfgLuckyPrize>();

	public static List<cfgMail> cfgMails = new List<cfgMail>();

	public static List<cfgModelChest> cfgModelChests = new List<cfgModelChest>();

	public static List<cfgMonthRankingActivityReward> cfgMonthRankingActivityRewards = new List<cfgMonthRankingActivityReward>();

	public static List<cfgPropIceBreakingGift> cfgPropIceBreakingGifts = new List<cfgPropIceBreakingGift>();

	public static List<cfgRecallReward> cfgRecallRewards = new List<cfgRecallReward>();

	public static List<cfgRechargeActivityReward> cfgRechargeActivityRewards = new List<cfgRechargeActivityReward>();

	public static List<cfgRegionRankRegions> cfgRegionRankRegionss = new List<cfgRegionRankRegions>();

	public static List<cfgRegionRankReward> cfgRegionRankRewards = new List<cfgRegionRankReward>();

	public static Dictionary<string, List<cfgRoadToRichesReward>> cfgRoadToRichesRewards = new Dictionary<string, List<cfgRoadToRichesReward>>();

	public static List<cfgRoyalPassActivityReward> cfgRoyalPassActivityRewards = new List<cfgRoyalPassActivityReward>();

	public static List<cfgShortBattlePass> cfgShortBattlePasss = new List<cfgShortBattlePass>();

	public static Dictionary<int, List<cfgShortBattlePassReward>> cfgShortBattlePassRewards = new Dictionary<int, List<cfgShortBattlePassReward>>();

	public static List<cfgSignBoardReward> cfgSignBoardRewards = new List<cfgSignBoardReward>();

	public static List<cfgSpecialBlockReward> cfgSpecialBlockRewards = new List<cfgSpecialBlockReward>();

	public static Dictionary<int, cfgSpecialModelFuntion> cfgSpecialModelFuntions = new Dictionary<int, cfgSpecialModelFuntion>();

	public static Dictionary<string, List<cfgStarRankingActivityReward>> cfgStarRankingActivityRewards = new Dictionary<string, List<cfgStarRankingActivityReward>>();

	public static Dictionary<int, cfgSynthetic> cfgSynthetics = new Dictionary<int, cfgSynthetic>();

	public static List<cfgTreasureHuntActivityRankingReward> cfgTreasureHuntActivityRankingRewards = new List<cfgTreasureHuntActivityRankingReward>();

	public static List<cfgTreasureHuntActivityReward> cfgTreasureHuntActivityRewards = new List<cfgTreasureHuntActivityReward>();

	public static List<cfgUnlockNewItem> cfgUnlockNewItems = new List<cfgUnlockNewItem>();

	public static Dictionary<int, cfgUserLevel> cfgUserLevels = new Dictionary<int, cfgUserLevel>();

	public static List<cfgVideoWheelReward> cfgVideoWheelRewards = new List<cfgVideoWheelReward>();

	public static List<cfg_TeamBadge> cfg_TeamBadges = new List<cfg_TeamBadge>();

	public static List<cfg_achive> cfg_achives = new List<cfg_achive>();

	public static List<cfg_achiveScore> cfg_achiveScores = new List<cfg_achiveScore>();

	public static List<cfg_medal> cfg_medals = new List<cfg_medal>();

	public static Dictionary<int, cfgModel> cfgModels = new Dictionary<int, cfgModel>();

	public static Dictionary<int, List<cfgMonthRankingActivityReward>> cfgMonthRankingActivityRewardGroups = new Dictionary<int, List<cfgMonthRankingActivityReward>>();

	public static void Init()
	{
		Clear();
		CardSystemCardBoxs = LoadMgr.LoadJson<Dictionary<int, CardSystemCardBox>>("Json/CardSystemCardBox");
		CardSystemCardBoxGroupss = LoadMgr.LoadJson<Dictionary<int, CardSystemCardBoxGroups>>("Json/CardSystemCardBoxGroups");
		CardSystemCardss = LoadMgr.LoadJson<Dictionary<int, CardSystemCards>>("Json/CardSystemCards");
		CardSystemGroups = LoadMgr.LoadJson<Dictionary<int, CardSystemGroup>>("Json/CardSystemGroup");
		CfgValueOnceTracks = LoadMgr.LoadJson<List<CfgValueOnceTrack>>("Json/CfgValueOnceTrack");
		CfgValueRepeatTracks = LoadMgr.LoadJson<List<CfgValueRepeatTrack>>("Json/CfgValueRepeatTrack");
		ConfigHyperBidChannelUploads = LoadMgr.LoadJson<List<ConfigHyperBidChannelUpload>>("Json/ConfigHyperBidChannelUpload");
		DirectPurchases = LoadMgr.LoadJson<List<DirectPurchase>>("Json/DirectPurchase");
		DirectPurchaseStalls = LoadMgr.LoadJson<List<DirectPurchaseStall>>("Json/DirectPurchaseStall");
		FacebookAdValues = LoadMgr.LoadJson<List<FacebookAdValue>>("Json/FacebookAdValue");
		VictoriesActivityRewards = LoadMgr.LoadJson<List<VictoriesActivityReward>>("Json/VictoriesActivityReward");
		cf_accumulatedAd_const.Init();
		cf_ads_const.Init();
		cf_banner_const.Init();
		cf_const.Init();
		cf_facebookAdValue2Iap_const.Init();
		cf_fail_dialog_const.Init();
		cf_festival_const.Init();
		cf_firebase_const.Init();
		cf_gift_no_ads_const.Init();
		cf_propIceBreakingGift_const.Init();
		cf_regionRank_const.Init();
		cfbt_Gifts = LoadMgr.LoadJson<List<cfbt_Gift>>("Json/cfbt_Gift");
		cfbt_GiftStallss = LoadMgr.LoadJson<List<cfbt_GiftStalls>>("Json/cfbt_GiftStalls");
		cfbt_Storess = LoadMgr.LoadJson<List<cfbt_Stores>>("Json/cfbt_Stores");
		cfgAccumlatedAdRewards = LoadMgr.LoadJson<List<cfgAccumlatedAdReward>>("Json/cfgAccumlatedAdReward");
		cfgActivityEntranceInformations = LoadMgr.LoadJson<List<cfgActivityEntranceInformation>>("Json/cfgActivityEntranceInformation");
		cfgBakingMasterLevels = LoadMgr.LoadJson<Dictionary<int, List<cfgBakingMasterLevel>>>("Json/cfgBakingMasterLevel");
		cfgFestivals = LoadMgr.LoadJson<List<cfgFestival>>("Json/cfgFestival");
		cfgFestivalPointInformations = LoadMgr.LoadJson<List<cfgFestivalPointInformation>>("Json/cfgFestivalPointInformation");
		cfgLevelChests = LoadMgr.LoadJson<List<cfgLevelChest>>("Json/cfgLevelChest");
		cfgLevelDifficultys = LoadMgr.LoadJson<List<cfgLevelDifficulty>>("Json/cfgLevelDifficulty");
		cfgLevelFixeds = LoadMgr.LoadJson<Dictionary<int, cfgLevelFixed>>("Json/cfgLevelFixed");
		cfgLevelRandoms = LoadMgr.LoadJson<List<cfgLevelRandom>>("Json/cfgLevelRandom");
		cfgLevelRewardVideos = LoadMgr.LoadJson<List<cfgLevelRewardVideo>>("Json/cfgLevelRewardVideo");
		cfgLevelUnlocks = LoadMgr.LoadJson<List<cfgLevelUnlock>>("Json/cfgLevelUnlock");
		cfgLuckyPrizes = LoadMgr.LoadJson<List<cfgLuckyPrize>>("Json/cfgLuckyPrize");
		cfgMails = LoadMgr.LoadJson<List<cfgMail>>("Json/cfgMail");
		cfgModelChests = LoadMgr.LoadJson<List<cfgModelChest>>("Json/cfgModelChest");
		cfgMonthRankingActivityRewards = LoadMgr.LoadJson<List<cfgMonthRankingActivityReward>>("Json/cfgMonthRankingActivityReward");
		cfgPropIceBreakingGifts = LoadMgr.LoadJson<List<cfgPropIceBreakingGift>>("Json/cfgPropIceBreakingGift");
		cfgRecallRewards = LoadMgr.LoadJson<List<cfgRecallReward>>("Json/cfgRecallReward");
		cfgRechargeActivityRewards = LoadMgr.LoadJson<List<cfgRechargeActivityReward>>("Json/cfgRechargeActivityReward");
		cfgRegionRankRegionss = LoadMgr.LoadJson<List<cfgRegionRankRegions>>("Json/cfgRegionRankRegions");
		cfgRegionRankRewards = LoadMgr.LoadJson<List<cfgRegionRankReward>>("Json/cfgRegionRankReward");
		cfgRoadToRichesRewards = LoadMgr.LoadJson<Dictionary<string, List<cfgRoadToRichesReward>>>("Json/cfgRoadToRichesReward");
		cfgRoyalPassActivityRewards = LoadMgr.LoadJson<List<cfgRoyalPassActivityReward>>("Json/cfgRoyalPassActivityReward");
		cfgShortBattlePasss = LoadMgr.LoadJson<List<cfgShortBattlePass>>("Json/cfgShortBattlePass");
		cfgShortBattlePassRewards = LoadMgr.LoadJson<Dictionary<int, List<cfgShortBattlePassReward>>>("Json/cfgShortBattlePassReward");
		cfgSignBoardRewards = LoadMgr.LoadJson<List<cfgSignBoardReward>>("Json/cfgSignBoardReward");
		cfgSpecialBlockRewards = LoadMgr.LoadJson<List<cfgSpecialBlockReward>>("Json/cfgSpecialBlockReward");
		cfgSpecialBlockReward_const.Init();
		cfgSpecialModelFuntions = LoadMgr.LoadJson<Dictionary<int, cfgSpecialModelFuntion>>("Json/cfgSpecialModelFuntion");
		cfgStarRankingActivityRewards = LoadMgr.LoadJson<Dictionary<string, List<cfgStarRankingActivityReward>>>("Json/cfgStarRankingActivityReward");
		cfgSynthetics = LoadMgr.LoadJson<Dictionary<int, cfgSynthetic>>("Json/cfgSynthetic");
		cfgTreasureHuntActivityRankingRewards = LoadMgr.LoadJson<List<cfgTreasureHuntActivityRankingReward>>("Json/cfgTreasureHuntActivityRankingReward");
		cfgTreasureHuntActivityRewards = LoadMgr.LoadJson<List<cfgTreasureHuntActivityReward>>("Json/cfgTreasureHuntActivityReward");
		cfgUnlockNewItems = LoadMgr.LoadJson<List<cfgUnlockNewItem>>("Json/cfgUnlockNewItem");
		cfgUserLevels = LoadMgr.LoadJson<Dictionary<int, cfgUserLevel>>("Json/cfgUserLevel");
		cfgUserLevel_const.Init();
		cfgVideoWheelRewards = LoadMgr.LoadJson<List<cfgVideoWheelReward>>("Json/cfgVideoWheelReward");
		cfg_TeamBadges = LoadMgr.LoadJson<List<cfg_TeamBadge>>("Json/cfg_TeamBadge");
		cfg_achives = LoadMgr.LoadJson<List<cfg_achive>>("Json/cfg_achive");
		cfg_achiveScores = LoadMgr.LoadJson<List<cfg_achiveScore>>("Json/cfg_achiveScore");
		cfg_medals = LoadMgr.LoadJson<List<cfg_medal>>("Json/cfg_medal");
		network_inspection_const.Init();
		pig_const.Init();
		_init_ex();
	}

	public static void Clear()
	{
		CardSystemCardBoxs.Clear();
		CardSystemCardBoxGroupss.Clear();
		CardSystemCardss.Clear();
		CardSystemGroups.Clear();
		CfgValueOnceTracks.Clear();
		CfgValueRepeatTracks.Clear();
		ConfigHyperBidChannelUploads.Clear();
		DirectPurchases.Clear();
		DirectPurchaseStalls.Clear();
		FacebookAdValues.Clear();
		VictoriesActivityRewards.Clear();
		cfbt_Gifts.Clear();
		cfbt_GiftStallss.Clear();
		cfbt_Storess.Clear();
		cfgAccumlatedAdRewards.Clear();
		cfgActivityEntranceInformations.Clear();
		cfgBakingMasterLevels.Clear();
		cfgFestivals.Clear();
		cfgFestivalPointInformations.Clear();
		cfgLevelChests.Clear();
		cfgLevelDifficultys.Clear();
		cfgLevelFixeds.Clear();
		cfgLevelRandoms.Clear();
		cfgLevelRewardVideos.Clear();
		cfgLevelUnlocks.Clear();
		cfgLuckyPrizes.Clear();
		cfgMails.Clear();
		cfgModelChests.Clear();
		cfgMonthRankingActivityRewards.Clear();
		cfgPropIceBreakingGifts.Clear();
		cfgRecallRewards.Clear();
		cfgRechargeActivityRewards.Clear();
		cfgRegionRankRegionss.Clear();
		cfgRegionRankRewards.Clear();
		cfgRoadToRichesRewards.Clear();
		cfgRoyalPassActivityRewards.Clear();
		cfgShortBattlePasss.Clear();
		cfgShortBattlePassRewards.Clear();
		cfgSignBoardRewards.Clear();
		cfgSpecialBlockRewards.Clear();
		cfgSpecialModelFuntions.Clear();
		cfgStarRankingActivityRewards.Clear();
		cfgSynthetics.Clear();
		cfgTreasureHuntActivityRankingRewards.Clear();
		cfgTreasureHuntActivityRewards.Clear();
		cfgUnlockNewItems.Clear();
		cfgUserLevels.Clear();
		cfgVideoWheelRewards.Clear();
		cfg_TeamBadges.Clear();
		cfg_achives.Clear();
		cfg_achiveScores.Clear();
		cfg_medals.Clear();
		_clear_ex();
	}

	public static void _init_ex()
	{
		cfgModels = LoadMgr.LoadJson<Dictionary<int, cfgModel>>("Json/cfgModel");
		foreach (KeyValuePair<int, cfgModel> item in cfgModels)
		{
			item.Value.id = item.Key;
		}
		cfgMonthRankingActivityRewardGroups.Clear();
		foreach (cfgMonthRankingActivityReward item in cfgMonthRankingActivityRewards)
		{
			if (cfgMonthRankingActivityRewardGroups.ContainsKey(item.group))
			{
				cfgMonthRankingActivityRewardGroups[item.group].Add(item);
				continue;
			}
			List<cfgMonthRankingActivityReward> list = new List<cfgMonthRankingActivityReward>();
			list.Add(item);
			cfgMonthRankingActivityRewardGroups.Add(item.group, list);
		}
	}

	public static void _clear_ex()
	{
		cfgModels.Clear();
		cfgMonthRankingActivityRewardGroups.Clear();
	}
}
