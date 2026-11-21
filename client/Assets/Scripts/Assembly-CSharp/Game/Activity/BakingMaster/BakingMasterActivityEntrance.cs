using FunctionEject;

namespace Game.Activity.BakingMaster
{
	public class BakingMasterActivityEntrance : EjectItem
	{
		private void Awake()
		{
			DB.BakingMasterActivity.Info.RefreshTime();
			Init();
		}

		public override void Init()
		{
			ActivityHttpMgr.I.RequestIssue();
		}

		public override bool Eject()
		{
			if (base.gameObject.activeSelf && DB.BakingMasterActivity.State == 0)
			{
				BakingMasterActivityPosterDialog.Create(false);
				return true;
			}
			return false;
		}

		public override void ForceEject()
		{
			SceneMgr.AddTopNode<BakingMasterActivityRootDialog>("Prefabs/Activity/BakingMaster/ActivityRootDialog");
		}
	}
}
