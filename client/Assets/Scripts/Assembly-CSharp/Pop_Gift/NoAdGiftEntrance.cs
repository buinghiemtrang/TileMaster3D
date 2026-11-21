using FunctionEject;

namespace Pop_Gift
{
	public class NoAdGiftEntrance : EjectItem
	{
		public override void Init()
		{
		}

		public override bool Eject()
		{
			return GiftNoAdsManager.Instance.TryAutoPopGift(GiftTriggerPosition.ReturnMainScene, delegate(bool isBought, bool _)
			{
				if ((bool)EjectMgr.I)
				{
					EjectMgr.I.Next(isBought ? EjectType.Break : EjectType.Continue);
				}
			}, false);
		}
	}
}
