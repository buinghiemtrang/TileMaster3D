using System;
using System.Collections.Generic;
using DG.Tweening;
using GameTeam;
using UnityEngine;
using UnityEngine.UI;

public class TeamBoard : MonoBehaviour
{
	[SerializeField]
	private TeamBoardScrollView pScroll;

	[SerializeField]
	private Image TeamIcon;

	[SerializeField]
	private Text TeamName;

	[SerializeField]
	private Text OnlineMemberCount;

	[SerializeField]
	private RequestEnergyBtn requestEnergyBtn;

	[SerializeField]
	private InputField chatInput;

	[SerializeField]
	private MButton chatBtn;

	private Sequence loopGetBoardsCallSeq;

	private bool isDirectCallGetBoards;

	private void Awake()
	{
		EventOperation(false);
		requestEnergyBtn.onAskEnergySuccess = delegate
		{
			AddOneAskEnergyData();
			pScroll.RefreshItems();
		};
		chatInput.onEndEdit.AddListener(_SendChatContent);
		chatBtn.onClick.AddListener(_ClickChat);
		loopGetBoardsCallSeq = DOTween.Sequence().AppendInterval(UnityEngine.Random.Range(15, 21)).AppendCallback(delegate
		{
			_GetBoardsInterface(false);
		}).SetLoops(-1)
			.SetId("TeamBoard_Delay");
		loopGetBoardsCallSeq.Pause();
	}

	public void Show(bool isWait)
	{
		RefreshTeamUI();
		requestEnergyBtn.Init();
		chatBtn.interactable = false;
		isDirectCallGetBoards = true;
		_GetBoardsInterface(isWait);
	}

	public void RefreshTeamUI()
	{
		if (TeamDataMgr.I.MyTeamInfo != null)
		{
			TeamName.text = TeamDataMgr.I.MyTeamInfo.name;
			TeamIcon.sprite = TeamDataMgr.I.GetTeamBadge();
			OnlineMemberCount.SetLBy(TeamDataMgr.I.MyTeamInfo.members, cf_const.teamMaxMembers);
		}
	}

	private void _GetBoardsInterface(bool isWait)
	{
		if (isWait)
		{
			TeamMaskCtl.I.Show();
		}
		TeamHttpMgr.I.GetBoards();
	}

	private void _OnGetBoards(ServerDataListRoot<BoardsInfo> root)
	{
		if (isDirectCallGetBoards)
		{
			TeamMaskCtl.I.Hide();
		}
		if (root.code == 1)
		{
			if (isDirectCallGetBoards)
			{
				isDirectCallGetBoards = false;
				loopGetBoardsCallSeq.Restart();
			}
			if (!chatBtn.interactable)
			{
				chatBtn.interactable = true;
			}
			if (root.data != null && root.data.Count != 0 && root.data[0].Boards != null)
			{
				TeamDataMgr.I.MyBoardsInfos = root.data;
				pScroll.RefreshItems();
			}
		}
		else if (isDirectCallGetBoards)
		{
			Global.ShowRetryDialog("Language55", delegate(bool isRetry)
			{
				if (isRetry)
				{
					_GetBoardsInterface(true);
				}
				else
				{
					loopGetBoardsCallSeq.Restart();
				}
			});
		}
	}

	private void AddOneAskEnergyData()
	{
		BoardsInfo.BoardInfo boardInfo = new BoardsInfo.BoardInfo();
		boardInfo._t = "RequestingLife";
		boardInfo.GUID = DB.Account.Userid;
		boardInfo.Type = "RequestingLife";
		boardInfo.UserId = DB.Account.Userid;
		boardInfo.UserName = DB.Account.GetName();
		boardInfo.Created = DateTime.Now;
		boardInfo.LifeReceived = 0;
		boardInfo.LifeMax = 5;
		boardInfo.LifeSenders = new string[0];
		boardInfo.EndTime = DateTime.Now.AddHours(4.0);
		boardInfo.Medal = DB.User.Medal;
		if (TeamDataMgr.I.MyBoardsInfos[0] == null)
		{
			BoardsInfo boardsInfo = new BoardsInfo();
			boardsInfo.Boards = new Dictionary<string, BoardsInfo.BoardInfo>();
			boardsInfo.Boards.Add(boardInfo.GUID, boardInfo);
			TeamDataMgr.I.MyBoardsInfos[0] = boardsInfo;
		}
		else
		{
			if (TeamDataMgr.I.MyBoardsInfos[0].Boards == null)
			{
				TeamDataMgr.I.MyBoardsInfos[0].Boards = new Dictionary<string, BoardsInfo.BoardInfo>();
			}
			TeamDataMgr.I.MyBoardsInfos[0].Boards.Add(boardInfo.GUID, boardInfo);
		}
	}

	private void _ClickChat(MButton btn)
	{
		chatInput.ActivateInputField();
	}

	private void _SendChatContent(string va)
	{
		chatInput.text = "";
		va = va.Trim();
		if (!string.IsNullOrEmpty(va))
		{
			TeamMaskCtl.I.Show();
			TeamHttpMgr.I.Chat(va);
		}
	}

	private void _OnChat(ServerDataRootBase root)
	{
		TeamMaskCtl.I.Hide();
		if (root.code == 1)
		{
			_GetBoardsInterface(false);
		}
		else
		{
			Global.ShowMsgTip("Language55");
		}
	}

	public void EventOperation(bool isOff)
	{
		if (isOff)
		{
			DispatcherMgr.Off<ServerDataListRoot<BoardsInfo>>("TeamHttp_GetBoards", _OnGetBoards);
			DispatcherMgr.Off<ServerDataRootBase>("TeamHttp_Chat", _OnChat);
		}
		else
		{
			DispatcherMgr.On<ServerDataListRoot<BoardsInfo>>("TeamHttp_GetBoards", _OnGetBoards);
			DispatcherMgr.On<ServerDataRootBase>("TeamHttp_Chat", _OnChat);
		}
	}

	private void OnDestroy()
	{
		if (loopGetBoardsCallSeq != null)
		{
			loopGetBoardsCallSeq.Kill();
			loopGetBoardsCallSeq = null;
		}
		EventOperation(true);
	}
}
