using System;
using GameTeam;
using UnityEngine;
using UnityEngine.UI;

public class RequestItemInBoard : ItemInBoard
{
	[SerializeField]
	private Text nameTex;

	[SerializeField]
	private MButton helpBtn;

	[SerializeField]
	private HeartItem cellHeartItem;

	[SerializeField]
	private Transform tfHeartRoot;

	[SerializeField]
	private Transform tfTop;

	[SerializeField]
	private Image imageBg;

	[SerializeField]
	private Text tipsTex;

	[SerializeField]
	private Image imageMedal;

	private void Awake()
	{
		helpBtn.onClick.AddListener(_ClickHelp);
	}

	public override void Init(BoardsInfo.BoardInfo info)
	{
		if (info == null)
		{
			return;
		}
		base.Init(info);
		UpdateHeartItem(info.LifeReceived, info.LifeMax);
		bool flag = info.UserId.Equals(DB.Account.Userid);
		if (flag)
		{
			helpBtn.gameObject.SetActive(value: false);
		}
		else if (info.LifeReceived == info.LifeMax)
		{
			helpBtn.interactable = false;
		}
		else
		{
			helpBtn.gameObject.SetActive(value: true);
			helpBtn.interactable = _IsSend(info);
		}
		Vector3 localScale = imageBg.transform.localScale;
		localScale.x = (flag ? (-1f) : 1f);
		imageBg.transform.localScale = localScale;
		Rect rect = tfTop.Rect();
		tfTop.RT().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, flag ? 10f : 30f, rect.width);
		ContentSizeFitter component = nameTex.GetComponent<ContentSizeFitter>();
		float num = tipsTex.preferredWidth;
		component.enabled = true;
		nameTex.text = info.UserName;
		num = 450f - num;
		float preferredWidth = nameTex.preferredWidth;
		component.enabled = preferredWidth < num;
		nameTex.resizeTextForBestFit = num <= preferredWidth;
		if (num <= preferredWidth)
		{
			nameTex.transform.SetWidth(num);
		}
		imageMedal.gameObject.SetActive(info.Medal.HasValue);
		if (info.Medal.HasValue)
		{
			cfg_medal cfgById = MedalMgr.I.GetCfgById(info.Medal.Value);
			if (cfgById != null)
			{
				imageMedal.sprite = LoadMgr.LoadSprite("MedalIcon/" + cfgById.Icon);
			}
		}
	}

	private void _ClickHelp(MButton btn)
	{
		TeamMaskCtl.I.Show(1f, isNeedMask: true);
		DispatcherMgr.On<ServerDataRootBase>("TeamHttp_EnergyHelp", _OnEnergyHelp);
		TeamHttpMgr.I.EnergyHelp(boardInfo.GUID);
	}

	private bool _IsSend(BoardsInfo.BoardInfo boardInfo)
	{
		string[] lifeSenders = base.boardInfo.LifeSenders;
		bool flag = true;
		for (int i = 0; i < lifeSenders.Length; i++)
		{
			flag = !lifeSenders[i].Equals(DB.Account.Userid) && flag;
		}
		if (!flag)
		{
			return false;
		}
		return boardInfo.LifeReceived != boardInfo.LifeMax;
	}

	private void UpdateHeartItem(int receivedEnergy, int maxEnergy)
	{
		for (int num = tfHeartRoot.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(tfHeartRoot.GetChild(num).gameObject);
		}
		for (int i = 1; i <= maxEnergy; i++)
		{
			HeartItem heartItem = UnityEngine.Object.Instantiate(cellHeartItem, tfHeartRoot);
			heartItem.gameObject.SetActive(value: true);
			heartItem.SetShow(i <= receivedEnergy);
		}
	}

	private void _OnEnergyHelp(ServerDataRootBase root)
	{
		TeamMaskCtl.I.Hide();
		DispatcherMgr.Off<ServerDataRootBase>("TeamHttp_EnergyHelp", _OnEnergyHelp);
		if (root.code == 1)
		{
			boardInfo.LifeReceived++;
			int num = boardInfo.LifeSenders.Length;
			string[] array = new string[num + 1];
			Array.Copy(boardInfo.LifeSenders, array, num);
			array[num] = DB.Account.Userid;
			boardInfo.LifeSenders = array;
			Init(boardInfo);
			TeamUtils.ShowFlayItemAnim(base.transform, SceneMain.ins.goldIcon, PropID.Money, delegate
			{
			});
			if (TeamDataMgr.I.MyTeamInfo != null)
			{
				MemberInfo myMemberInfo = TeamDataMgr.I.GetMyMemberInfo();
				if (myMemberInfo != null)
				{
					LogEvent.I.TeamLifeHelp(TeamDataMgr.I.MyTeamInfo.id, myMemberInfo.projection.Team.Helps);
				}
			}
		}
		else
		{
			Global.ShowMsgTip("Language55");
		}
	}
}
