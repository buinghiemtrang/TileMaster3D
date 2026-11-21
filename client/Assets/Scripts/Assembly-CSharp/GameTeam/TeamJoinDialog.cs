using System;
using UnityEngine;

namespace GameTeam
{
	public class TeamJoinDialog : DialogBase
	{
		[SerializeField]
		private TeamBaseInfoUI baseInfoUI;

		[SerializeField]
		private TeamDataInfoUI dataInfoUI;

		[SerializeField]
		private TeamJoinDialogScrollView pScroll;

		[SerializeField]
		private MButton joinBtn;

		[SerializeField]
		private MButton closeBtn;

		[SerializeField]
		private MaskCtl maskCtl;

		private TeamInfo teamInfo;

		public Action onJoinSuccess;

		protected override void Awake()
		{
			base.Awake();
			joinBtn.onClick.AddListener(_ClickJoin);
			closeBtn.onClick.AddListener(_ClickClose);
			DispatcherMgr.On<ServerDataListRoot<MemberInfo>>("TeamHttp_GetTeamMembers", _OnGetTeamMembers);
			DispatcherMgr.On<ServerDataRootBase>("TeamHttp_JoinTeam", _OnJoinTeam);
		}

		public void Show(TeamInfo info, bool isShowJoinBtn)
		{
			teamInfo = info;
			baseInfoUI.Init(info);
			joinBtn.interactable = isShowJoinBtn;
			_Open();
			maskCtl.Show(1f, true);
			TeamHttpMgr.I.GetTeamMembers(info.id);
		}

		private void _OnGetTeamMembers(ServerDataListRoot<MemberInfo> root)
		{
			maskCtl.Hide(true);
			if (root.code == 1)
			{
				pScroll.RefreshItems(root.data);
				dataInfoUI.Init(root.data, teamInfo);
			}
		}

		private void _OnJoinTeam(ServerDataRootBase root)
		{
			maskCtl.Hide(true);
			if (root.code == 1)
			{
				if (teamInfo.type == "Open")
				{
					if (onJoinSuccess != null)
					{
						onJoinSuccess();
					}
				}
				else
				{
					Global.ShowMsgTip("Language242");
				}
				_Close();
				LogEvent.I.Track("Team_In").Add("Team_ID", teamInfo.id).Add("Type", teamInfo.type).CommonAttr(true).Send();
			}
			else
			{
				if (root.code == -1)
				{
					Global.ShowMsgTip("Language55");
				}
				else if (root.msg == "RequiredLevel")
				{
					Global.ShowMsgTip("Language248");
				}
				else if (root.msg == "Full")
				{
					Global.ShowMsgTip("Language219");
				}
				else
				{
					Global.ShowMsgTip("Language247");
				}
			}
		}

		private void _ClickJoin(MButton btn)
		{
			maskCtl.Show(1f, true);
			TeamHttpMgr.I.JoinTeam(teamInfo.id);
		}

		private void _ClickClose(MButton btn)
		{
			_Close();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			DispatcherMgr.Off<ServerDataListRoot<MemberInfo>>("TeamHttp_GetTeamMembers", _OnGetTeamMembers);
			DispatcherMgr.Off<ServerDataRootBase>("TeamHttp_JoinTeam", _OnJoinTeam);
		}
	}
}
