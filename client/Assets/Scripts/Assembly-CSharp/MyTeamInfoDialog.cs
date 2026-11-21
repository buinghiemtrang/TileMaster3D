using System;
using GameTeam;
using UnityEngine;

public class MyTeamInfoDialog : DialogBase
{
	[SerializeField]
	private TeamBaseInfoUI baseInfoUI;

	[SerializeField]
	private TeamDataInfoUI dataInfoUI;

	[SerializeField]
	private MButton leaveBtn;

	[SerializeField]
	private MButton updateBtn;

	[SerializeField]
	private MButton closeBtn;

	[SerializeField]
	private MaskCtl maskCtl;

	public Action onUpdateCallback;

	public Action onLevelCallback;

	public MyTeamInfoDialog()
	{
		fMaskAlpha = 0.8f;
	}

	protected override void Awake()
	{
		base.Awake();
		leaveBtn.onClick.AddListener(_ClickLeave);
		updateBtn.onClick.AddListener(_ClickUpdate);
		closeBtn.onClick.AddListener(_ClickClose);
		DispatcherMgr.On<ServerDataRootBase>("TeamHttp_LeaveTeam", _OnLeaveTeam);
	}

	public void Show()
	{
		baseInfoUI.Init(TeamDataMgr.I.MyTeamInfo);
		dataInfoUI.Init(TeamDataMgr.I.MyTeamMemberInfos, TeamDataMgr.I.MyTeamInfo);
		updateBtn.gameObject.SetActive(TeamDataMgr.I.IsLeader());
		_Open();
	}

	private void _ClickLeave(MButton button)
	{
		maskCtl.Show();
		TeamHttpMgr.I.LeaveTeam();
	}

	private void _ClickUpdate(MButton button)
	{
		SceneMgr.AddTopNode<ModifyTeamDialog>("Prefabs/GameTeam/ModifyTeamDialog").Show(onUpdateCallback);
		_Close();
	}

	private void _ClickClose(MButton button)
	{
		_Close();
	}

	private void _OnLeaveTeam(ServerDataRootBase root)
	{
		maskCtl.Hide();
		if (root.code == 1)
		{
			TeamDataMgr.I.MyTeamInfo = null;
			TeamDataMgr.I.MyTeamMemberInfos = null;
			if (onLevelCallback != null)
			{
				onLevelCallback();
			}
			_Close();
		}
		else if (root.code == -1)
		{
			Global.ShowMsgTip("Language55");
		}
		else
		{
			Global.ShowMsgTip("Language246");
		}
	}

	protected override void OnDestroy()
	{
		DispatcherMgr.Off<ServerDataRootBase>("TeamHttp_LeaveTeam", _OnLeaveTeam);
	}
}
