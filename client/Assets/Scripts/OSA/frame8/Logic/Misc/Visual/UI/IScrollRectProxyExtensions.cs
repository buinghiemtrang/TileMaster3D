namespace frame8.Logic.Misc.Visual.UI
{
	public static class IScrollRectProxyExtensions
	{
		public static double GetScrollableArea(this IScrollRectProxy proxy)
		{
			return proxy.GetContentSize() - proxy.GetViewportSize();
		}

		public static double GetContentSizeToViewportRatio(this IScrollRectProxy proxy)
		{
			return proxy.GetContentSize() / proxy.GetViewportSize();
		}
	}
}
