using UnityEngine;

namespace _Game.SceneMain.Scripts.ToolBarVertical
{
	public class MailBtnInit : MonoBehaviour
	{
		[SerializeField]
		private GameObject redObj;

		[SerializeField]
		private MButton entranceBtn;

		private void Awake()
		{
			DispatcherMgr.On("MailsDialog_Close", _ClosedDialog);
			DispatcherMgr.On("GameMail_Get_Success", _RefreshUI);
			entranceBtn.onClick.AddListener(_ClickEntrance);
		}

		private void Start()
		{
			_RefreshUI();
		}

		private void _ClickEntrance(MButton btn)
		{
			_EnterMailsDialog("Click");
			LogEvent.I.HomeIconClick("Mail");
		}

		private void _EnterMailsDialog(string openType)
		{
			SceneMgr.AddTopNode<GameMail.MailsDialog>("Prefabs/Mail/MailsDialog").Show(openType);
		}

		private void _RefreshUI()
		{
			bool flag = GameMail.MailMgr.I.JudgeRed();
			redObj.SetActive(flag);
			if (flag)
			{
				DispatcherMgr.Emit("SceneMainToolBarVertical_EventSwitchTopBtn", base.transform);
			}
		}

		private void _ClosedDialog()
		{
			_RefreshUI();
		}
	}
}
