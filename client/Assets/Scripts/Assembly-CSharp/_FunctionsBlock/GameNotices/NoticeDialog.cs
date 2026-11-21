using UnityEngine;
using UnityEngine.UI;

namespace _FunctionsBlock.GameNotices
{
	public class NoticeDialog : DialogShow
	{
		[SerializeField]
		private Text _text;

		public static NoticeDialog Create()
		{
			NoticeInfo pNoticeInfo = NoticeHttpMgr.I.pNoticeInfo;
			if (pNoticeInfo == null || pNoticeInfo.isUsed)
			{
				return null;
			}
			NoticeDialog noticeDialog = SceneMgr.AddTopNode<NoticeDialog>("NoticeDialog");
			noticeDialog._text.text = pNoticeInfo.content;
			pNoticeInfo.isUsed = true;
			return noticeDialog;
		}
	}
}
