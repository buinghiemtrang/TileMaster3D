using System;
using GameTeam;
using UnityEngine;

public class ModifyTeamDialog : DialogBase
{
	[SerializeField]
	private TeamBaseInfoUI baseInfoUI;

	[SerializeField]
	private TeamLimitInfoUI limitInfoUI;

	[SerializeField]
	private MButton saveBtn;

	[SerializeField]
	private MButton closeBtn;

	[SerializeField]
	private MaskCtl maskCtl;

	private Action onSaveCallback;

	protected override void Awake()
	{
		base.Awake();
		closeBtn.onClick.AddListener(delegate
		{
			_Close();
		});
		saveBtn.onClick.AddListener(_ClickSave);
		DispatcherMgr.On<ServerDataRoot<TeamInfo>>("TeamHttp_EditTeam", _OnEditorTeam);
	}

	public void Show(Action onSaveCallback)
	{
		this.onSaveCallback = onSaveCallback;
		baseInfoUI.Init(TeamDataMgr.I.MyTeamInfo);
		limitInfoUI.Init(TeamDataMgr.I.MyTeamInfo);
		_Open();
	}

	private void _OnEditorTeam(ServerDataRoot<TeamInfo> root)
	{
		maskCtl.Hide();
		if (root.code == 1)
		{
			TeamDataMgr.I.MyTeamInfo = root.data;
			if (onSaveCallback != null)
			{
				onSaveCallback();
			}
			_Close();
		}
		else
		{
			Global.ShowMsgTip((root.code == -1) ? "Language55" : "Language246");
		}
	}

	public void _ClickSave(MButton btn)
	{
		maskCtl.Show();
		Debug.Log(string.Format("ModifyTeam badge:{0}、desc:{1}、type:{2}、lv:{3}", baseInfoUI.teamIconName, baseInfoUI.teamDesc, limitInfoUI.teamType, limitInfoUI.requiredLv));
		TeamHttpMgr.I.EditTeam(baseInfoUI.teamIconName, baseInfoUI.teamDesc, limitInfoUI.teamType, limitInfoUI.requiredLv);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		DispatcherMgr.Off<ServerDataRoot<TeamInfo>>("TeamHttp_EditTeam", _OnEditorTeam);
	}
}
