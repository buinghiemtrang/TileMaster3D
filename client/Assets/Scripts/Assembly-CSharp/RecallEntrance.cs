using FunctionEject;
using SignBoard;

public class RecallEntrance : EjectItem
{
	public override void Init()
	{
		RecallMgr.I.Init();
	}

	public override bool Eject()
	{
		if (RecallMgr.I.JudgeAutoEject())
		{
			SignBoardMgr.I.ResetData();
			RecallDialog recallDialog = SceneMgr.AddTopNode<RecallDialog>("Prefabs/Activity/Recall/RecallDialog", false);
			recallDialog.closeCallback = delegate
			{
				if ((bool)EjectMgr.I)
				{
					EjectMgr.I.Next(EjectType.Continue);
				}
			};
			recallDialog.Show();
			return true;
		}
		return false;
	}
}
