using DataCenter;

public static class DB
{
	public static Account Account;

	public static Achieve Achieve;

	public static Bag Bag;

	public static BakingMasterActivity BakingMasterActivity;

	public static Booster Booster;

	public static CardSystem CardSystem;

	public static Chest Chest;

	public static DataCenter.FacebookAdValue FacebookAdValue;

	public static FirebaseAdValue FirebaseAdValue;

	public static GiftNoAds GiftNoAds;

	public static GiftStalls GiftStalls;

	public static House3DTask House3DTask;

	public static Login Login;

	public static Mail Mail;

	public static MonthRankingActivity MonthRankingActivity;

	public static Other Other;

	public static PropIceBreakingGift PropIceBreakingGift;

	public static Recall Recall;

	public static RechargeActivity RechargeActivity;

	public static RegionRankActivity RegionRankActivity;

	public static DataCenter.RewardAdView RewardAdView;

	public static RewardVideo RewardVideo;

	public static RoyalPassActivity RoyalPassActivity;

	public static DataCenter.ShortBattlePass ShortBattlePass;

	public static DataCenter.SignBoard SignBoard;

	public static SpecialBlockData SpecialBlockData;

	public static StarRankingActivity StarRankingActivity;

	public static StoreMultipleActivity StoreMultipleActivity;

	public static Synthetic Synthetic;

	public static Team Team;

	public static Tmp Tmp;

	public static Track Track;

	public static TreasureHuntActivity TreasureHuntActivity;

	public static DataCenter.Turntable Turntable;

	public static UnityAdValue UnityAdValue;

	public static User User;

	public static UserLevelReward UserLevelReward;

	public static ValueOnceTrack ValueOnceTrack;

	public static ValueRepeatTrack ValueRepeatTrack;

	public static VictoriesActivity VictoriesActivity;

	public static void Init()
	{
		Account = DataManager.I.Create<Account>("");
		Achieve = DataManager.I.Create<Achieve>("");
		Bag = DataManager.I.Create<Bag>("");
		BakingMasterActivity = DataManager.I.Create<BakingMasterActivity>("");
		Booster = DataManager.I.Create<Booster>("");
		CardSystem = DataManager.I.Create<CardSystem>("");
		Chest = DataManager.I.Create<Chest>("");
		FacebookAdValue = DataManager.I.Create<DataCenter.FacebookAdValue>("");
		FirebaseAdValue = DataManager.I.Create<FirebaseAdValue>("");
		GiftNoAds = DataManager.I.Create<GiftNoAds>("");
		GiftStalls = DataManager.I.Create<GiftStalls>("");
		House3DTask = DataManager.I.Create<House3DTask>("");
		Login = DataManager.I.Create<Login>("");
		Mail = DataManager.I.Create<Mail>("");
		MonthRankingActivity = DataManager.I.Create<MonthRankingActivity>("");
		Other = DataManager.I.Create<Other>("");
		PropIceBreakingGift = DataManager.I.Create<PropIceBreakingGift>("");
		Recall = DataManager.I.Create<Recall>("");
		RechargeActivity = DataManager.I.Create<RechargeActivity>("");
		RegionRankActivity = DataManager.I.Create<RegionRankActivity>("");
		RewardAdView = DataManager.I.Create<DataCenter.RewardAdView>("");
		RewardVideo = DataManager.I.Create<RewardVideo>("");
		RoyalPassActivity = DataManager.I.Create<RoyalPassActivity>("");
		ShortBattlePass = DataManager.I.Create<DataCenter.ShortBattlePass>("");
		SignBoard = DataManager.I.Create<DataCenter.SignBoard>("");
		SpecialBlockData = DataManager.I.Create<SpecialBlockData>("");
		StarRankingActivity = DataManager.I.Create<StarRankingActivity>("");
		StoreMultipleActivity = DataManager.I.Create<StoreMultipleActivity>("");
		Synthetic = DataManager.I.Create<Synthetic>("");
		Team = DataManager.I.Create<Team>("");
		Tmp = DataManager.I.Create<Tmp>("");
		Track = DataManager.I.Create<Track>("");
		TreasureHuntActivity = DataManager.I.Create<TreasureHuntActivity>("");
		Turntable = DataManager.I.Create<DataCenter.Turntable>("");
		UnityAdValue = DataManager.I.Create<UnityAdValue>("");
		User = DataManager.I.Create<User>("");
		UserLevelReward = DataManager.I.Create<UserLevelReward>("");
		ValueOnceTrack = DataManager.I.Create<ValueOnceTrack>("");
		ValueRepeatTrack = DataManager.I.Create<ValueRepeatTrack>("");
		VictoriesActivity = DataManager.I.Create<VictoriesActivity>("");
	}
}
