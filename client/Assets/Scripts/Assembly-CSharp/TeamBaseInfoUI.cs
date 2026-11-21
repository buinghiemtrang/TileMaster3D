using FQDev.WordBlocker;
using GameTeam;
using UnityEngine;
using UnityEngine.UI;

public class TeamBaseInfoUI : MonoBehaviour
{
	[SerializeField]
	private Image iconImg;

	[SerializeField]
	private Text memberTex;

	[SerializeField]
	private Text nameTex;

	[SerializeField]
	private FilteredText descTex;

	[SerializeField]
	private InputField nameField;

	[SerializeField]
	private InputField descField;

	[SerializeField]
	private MButton SelectedBtn;

	[HideInInspector]
	public string teamName;

	[HideInInspector]
	public string teamDesc;

	[HideInInspector]
	public string teamIconName;

	private void Awake()
	{
		if ((bool)SelectedBtn)
		{
			SelectedBtn.onClick.AddListener(_ClickSelected);
		}
		if ((bool)nameField)
		{
			nameField.onEndEdit.AddListener(_NameEndEdit);
		}
		if ((bool)descField)
		{
			descField.onEndEdit.AddListener(_DescEndEdit);
		}
		DispatcherMgr.On<string>("TeamSelectedBadge", _SelectedBadge);
	}

	public void Init(TeamInfo info)
	{
		teamIconName = TeamDataMgr.I.GetTeamBadgeName(info, iconImg);
		if (info == null)
		{
			return;
		}
		if ((bool)memberTex)
		{
			memberTex.text = info.members.ToString() + "/" + cf_const.teamMaxMembers.ToString();
		}
		teamName = info.name;
		if ((bool)nameTex)
		{
			nameTex.text = teamName;
		}
		if ((bool)nameField)
		{
			nameField.text = teamName;
		}
		teamDesc = info.description;
		if ((bool)descTex)
		{
			descTex.Text = teamDesc;
		}
		if ((bool)descField)
		{
			descField.text = teamDesc;
		}
	}

	public void OnTeamNameValidation(bool valid)
	{
		if (!valid && (bool)nameField)
		{
			Global.ShowMsgTip("Language153");
			nameField.text = "";
		}
	}

	private void _NameEndEdit(string va)
	{
		teamName = va.Trim();
	}

	private void _DescEndEdit(string va)
	{
		teamDesc = va.Trim();
	}

	private void _ClickSelected(MButton btn)
	{
		SceneMgr.AddTopNode<TeamSelectBadge>("Prefabs/GameTeam/TeamSelectBadge").Show();
	}

	private void _SelectedBadge(string badgeName)
	{
		teamIconName = badgeName;
		if ((bool)iconImg)
		{
			iconImg.sprite = LoadMgr.LoadSprite("TeamIcon/" + badgeName);
		}
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off<string>("TeamSelectedBadge", _SelectedBadge);
	}
}
