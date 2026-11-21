using System;
using GameTeam;
using UnityEngine;
using UnityEngine.UI;

public class TeamLimitInfoUI : MonoBehaviour
{
	[SerializeField]
	private Text teamTypeTex;

	[SerializeField]
	private MButton typeLiftBtn;

	[SerializeField]
	private MButton typeRightBtn;

	[SerializeField]
	private Text requiredLvTex;

	[SerializeField]
	private MButton requiredLvLiftBtn;

	[SerializeField]
	private MButton requiredLvRightBtn;

	private bool isOpen = true;

	private string[] teamTypes = new string[2] { "Open", "Close" };

	private int lvMultiple;

	[HideInInspector]
	public string teamType;

	[HideInInspector]
	public int requiredLv;

	private void Awake()
	{
		typeLiftBtn.onClick.AddListener(_ClickTeamType);
		typeRightBtn.onClick.AddListener(_ClickTeamType);
		requiredLvLiftBtn.onClick.AddListener(_ClickRequiredLv);
		requiredLvRightBtn.onClick.AddListener(_ClickRequiredLv);
	}

	public void Init(TeamInfo info)
	{
		if (info != null)
		{
			teamType = info.type;
			isOpen = teamType.Equals(teamTypes[0]);
			teamTypeTex.text = teamType;
			requiredLv = info.requiredLevel;
			requiredLvTex.text = requiredLv.ToString();
		}
		else
		{
			_SetTeamType();
			requiredLv = 0;
			requiredLvTex.text = requiredLv.ToString();
		}
	}

	private void _ClickTeamType(MButton btn)
	{
		isOpen = !isOpen;
		_SetTeamType();
	}

	private void _ClickRequiredLv(MButton btn)
	{
		int count = (btn.sMsg.Equals("Left") ? (-1) : 1);
		_SetRequiredLv(count);
	}

	private void _SetRequiredLv(int count = 0)
	{
		int num;
		if (count == 0)
		{
			num = 0;
		}
		else
		{
			int num2 = DB.User.Maxlv / 50;
			lvMultiple = Math.Max(0, lvMultiple + count);
			if (lvMultiple > num2)
			{
				Global.ShowMsgTip("Language220");
			}
			lvMultiple = Math.Min(num2, lvMultiple);
			num = lvMultiple * 50;
		}
		requiredLv = num;
		requiredLvTex.text = requiredLv.ToString();
	}

	private void _SetTeamType()
	{
		teamType = teamTypes[isOpen ? 0 : 1];
		teamTypeTex.text = teamType;
	}
}
