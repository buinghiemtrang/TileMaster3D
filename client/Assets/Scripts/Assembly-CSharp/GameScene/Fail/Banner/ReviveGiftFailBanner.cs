namespace GameScene.Fail.Banner
{
	public class ReviveGiftFailBanner : FailBanner
	{
		internal cfbt_Stores Config;

		internal bool IsTimeOut;

		internal int ReviveAfterAddTime;

		internal override FailBannerType BannerType => FailBannerType.Revive;
	}
}
