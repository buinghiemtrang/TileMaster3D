using FunctionEject;

public class StorePromotionsEntrance : EjectItem
{
	public override void Init()
	{
	}

	public override bool Eject()
	{
		if (Global.pStores.storeMultipleCtrl.ActivityIsOpen() && !DB.StoreMultipleActivity.IsActivityPushed)
		{
			StoreMultipleDialog storeMultipleDialog = SceneMgr.AddTopNode<StoreMultipleDialog>("Prefabs/Activity/StorePromotions/StoreMultipleDialog");
			storeMultipleDialog.closeCallback = delegate
			{
				End(EjectType.Continue);
			};
			storeMultipleDialog.Show();
			DB.StoreMultipleActivity.IsActivityPushed = true;
			return true;
		}
		return false;
	}
}
