using FunctionEject;

namespace SignBoard
{
	public class SignBoardEntrance : EjectItem
	{
		public override void Init()
		{
			SignBoardMgr.I.Init();
		}

		public override bool Eject()
		{
			if (SignBoardMgr.I.JudgeAutoEject())
			{
				SignBoardDialog signBoardDialog = SceneMgr.AddTopNode<SignBoardDialog>("Prefabs/Activity/SignBoard/SignBoardDialog");
				signBoardDialog.CloseCallback = delegate
				{
					if ((bool)EjectMgr.I)
					{
						EjectMgr.I.Next(EjectType.Continue);
					}
				};
				signBoardDialog.Show();
				return true;
			}
			return false;
		}
	}
}
