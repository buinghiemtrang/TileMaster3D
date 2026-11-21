using FunctionEject;

namespace GameMail
{
	public class MailEntrance : EjectItem
	{
		private void Awake()
		{
			DispatcherMgr.On("MailsDialog_Close", _ClosedDialog);
		}

		private void OnDestroy()
		{
			DispatcherMgr.Off("MailsDialog_Close", _ClosedDialog);
		}

		public override void Init()
		{
			MailMgr.I.RequestMailsFormServer();
		}

		public override bool Eject()
		{
			bool flag = MailMgr.I.JudgeNewSysTemMail();
			if (flag)
			{
				_EnterMailsDialog("Initiative");
			}
			return flag;
		}

		private void _EnterMailsDialog(string openType)
		{
			SceneMgr.AddTopNode<MailsDialog>("Prefabs/Mail/MailsDialog").Show(openType);
		}

		private void _ClosedDialog()
		{
			End(EjectType.Continue);
		}
	}
}
