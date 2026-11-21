using System;
using GameTeam;
using UnityEngine;

public class TeamCreate : MonoBehaviour
{
	[SerializeField]
	private TeamBaseInfoUI baseInfoUI;

	[SerializeField]
	private TeamLimitInfoUI limitInfoUI;

	[SerializeField]
	private MButton createBtn;

	public Action onCreateSucc;

	private void Awake()
	{
		createBtn.onClick.AddListener(_ClickCreate);
		EventOperation(false);
	}

	public void Show()
	{
		baseInfoUI.Init(null);
		limitInfoUI.Init(null);
	}

	private void _ClickCreate(MButton btn)
	{
		string msg;
		if (string.IsNullOrEmpty(baseInfoUI.teamName))
		{
			msg = "Language240";
		}
		else
		{
			if (DB.Bag.GetProp(PropID.Money) >= cf_const.costOfCreateTeam)
			{
				TeamMaskCtl.I.Show();
				TeamHttpMgr.I.CreateTeam(baseInfoUI.teamName, baseInfoUI.teamIconName, baseInfoUI.teamDesc, limitInfoUI.teamType, limitInfoUI.requiredLv);
				return;
			}
			msg = "Language53";
		}
		Global.ShowMsgTip(msg);
	}

	private void _OnCreateTeam(ServerDataRoot<TeamInfo> root)
	{
		TeamMaskCtl.I.Hide();
		if (root.code == 1)
		{
			TeamDataMgr.I.MyTeamInfo = root.data;
			if (root.data != null)
			{
				LogEvent.I.TeamCreate(root.data.id);
			}
			DB.Bag.Modify(PropID.Money, -cf_const.costOfCreateTeam, "CreateTeam");
			if (onCreateSucc != null)
			{
				onCreateSucc();
			}
		}
		else if (root.code == -1)
		{
			Global.ShowMsgTip("Language55");
		}
		else
		{
			Global.ShowMsgTip("Language245");
		}
	}

	public void EventOperation(bool isOff)
	{
		if (isOff)
		{
			DispatcherMgr.Off<ServerDataRoot<TeamInfo>>("TeamHttp_CreateTeam", _OnCreateTeam);
		}
		else
		{
			DispatcherMgr.On<ServerDataRoot<TeamInfo>>("TeamHttp_CreateTeam", _OnCreateTeam);
		}
	}

	private void OnDestroy()
	{
		EventOperation(true);
	}
}
