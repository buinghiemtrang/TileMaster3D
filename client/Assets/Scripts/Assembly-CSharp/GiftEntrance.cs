using FunctionEject;

public class GiftEntrance : EjectItem
{
	public override void Init()
	{
	}

	public override bool Eject()
	{
		bool flag = GiftDataMgr.ins.TryPopGift();
		if (flag)
		{
			if ((bool)GiftPop.ins)
			{
				GiftPop.ins.func_close = delegate(bool isNoBuy)
				{
					if ((bool)EjectMgr.I)
					{
						EjectMgr.I.Next(isNoBuy ? EjectType.Continue : EjectType.Break);
					}
				};
			}
			if ((bool)GiftPropDialog.ins)
			{
				GiftPropDialog.ins.func_close = delegate(bool isNoBuy)
				{
					if ((bool)EjectMgr.I)
					{
						EjectMgr.I.Next(isNoBuy ? EjectType.Continue : EjectType.Break);
					}
				};
			}
		}
		return flag;
	}
}
