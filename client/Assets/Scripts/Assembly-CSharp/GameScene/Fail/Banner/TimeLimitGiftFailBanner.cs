namespace GameScene.Fail.Banner
{
	public class TimeLimitGiftFailBanner : FailBanner
	{
		internal GiftData Data;

		internal cfbt_Stores StoreConfig;

		internal int Order = 1;

		internal override FailBannerType BannerType => FailBannerType.TimeLimitGift;
	}
}
