using FunctionEject;

namespace _FunctionsBlock.GameNotices
{
	public class NoticesEntrance : EjectItem
	{
		public override void Init()
		{
		}

		public override bool Eject()
		{
			NoticeDialog noticeDialog = NoticeDialog.Create();
			if ((bool)noticeDialog)
			{
				noticeDialog.func_callback_close = delegate
				{
					if ((bool)EjectMgr.I)
					{
						EjectMgr.I.Next(EjectType.Continue);
					}
				};
			}
			return noticeDialog;
		}
	}
}
