using System;

namespace GameScene.Fail.Banner
{
	public abstract class FailBanner
	{
		internal Action ClickBuyCallback;

		internal Action AdCountUsedUpCallback;

		internal abstract FailBannerType BannerType { get; }
	}
}
