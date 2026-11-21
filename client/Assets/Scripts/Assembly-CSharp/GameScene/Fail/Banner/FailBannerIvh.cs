using Com.TheFallenGames.OSA.Core;

namespace GameScene.Fail.Banner
{
	public class FailBannerIvh : BaseItemViewsHolder
	{
		private FailBannerItem _item;

		private FailBanner _banner;

		public override void CollectViews()
		{
			base.CollectViews();
			_item = root.GetComponent<FailBannerItem>();
		}

		internal void UpdateViewsHolder(FailBanner banner)
		{
			_item.Init(banner);
			_banner = banner;
		}

		internal bool CanRecyclable(FailBanner banner)
		{
			if (_banner.BannerType != banner.BannerType)
			{
				return false;
			}
			if (banner.BannerType == FailBannerType.TimeLimitGift)
			{
				return ((TimeLimitGiftFailBanner)_banner).Order == ((TimeLimitGiftFailBanner)banner).Order;
			}
			return true;
		}
	}
}
