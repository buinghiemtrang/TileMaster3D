using DG.Tweening;
using UnityEngine;

namespace GameTeam
{
	public class TeamLayer : MonoBehaviour
	{
		private enum functionType
		{
			Team = 0,
			CreateTeam = 1,
			search = 2,
			MyTeam = 3,
			JoinRequest = 4,
			Board = 5
		}

		[SerializeField]
		private Transform tfLayerRoot;

		[SerializeField]
		private GameObject unlockTipObj;

		[SerializeField]
		private GuestLoginControl loginControl;

		[SerializeField]
		private MButton teamsBtn;

		[SerializeField]
		private MButton createTeamBtn;

		[SerializeField]
		private MButton searchBtn;

		[SerializeField]
		private MButton myTeamBtn;

		[SerializeField]
		private MButton joinRequestBtn;

		[SerializeField]
		private MButton boardBtn;

		private GameObject lastPage;

		private MButton lastBtn;

		private Sequence delaySeq;

		private TeamList teamList;

		private TeamCreate teamCreate;

		private TeamSearch teamSearch;

		private MyTeam myTeam;

		private TeamJoinRequest teamJoinRequest;

		private TeamBoard teamBoard;

		private void Awake()
		{
			_EventOperation(false);
			DispatcherMgr.On("Change_Module_Event", _ClearAllEvent);
			delaySeq = DOTween.Sequence();
			delaySeq.AppendInterval(10f);
			delaySeq.AppendCallback(() =>
			{
				TeamHttpMgr.I.GetMyTeam("TeamLayer_Loop_MyTeam");
			});
			delaySeq.SetLoops(-1);
			delaySeq.SetId("TeamLayer_Delay");
			delaySeq.Pause();
			loginControl.onLogin = (type) =>
			{
				if (type == GuestLoginState.Start)
				{
					TeamMaskCtl.I.Show(1f, true);
				}
				else if (type == GuestLoginState.Success)
				{
					if (DB.Account.IsEditorName())
					{
						_MyTeamInterface();
					}
					else
					{
						RankNameDialog.Show(() =>
						{
							DB.Account.Name = DB.Account.GetName();
							_MyTeamInterface();
						}, _MyTeamInterface);
					}
				}
				else
				{
					TeamMaskCtl.I.Hide(true);
					TeamNetworkTip.I.Show();
				}
			};
			teamsBtn.onClick.AddListener((btn) => _EnterTeamsPage(true, btn));
			createTeamBtn.onClick.AddListener(_EnterCreateTeamPage);
			searchBtn.onClick.AddListener(_EnterSearchPage);
			myTeamBtn.onClick.AddListener(_EnterMyTeamPage);
			joinRequestBtn.onClick.AddListener(_EnterJoinRequestPage);
			boardBtn.onClick.AddListener((btn) => _EnterBoardPage(true, btn));
		}

		private void Start()
		{
			unlockTipObj.SetActive(DB.User.Maxlv < cf_const.teamUnlockLv);
			if (DB.User.Maxlv >= cf_const.teamUnlockLv)
			{
				loginControl.Login();
			}
		}

		private void _MyTeamInterface()
		{
			TeamMaskCtl.I.Show(1f, true);
			TeamHttpMgr.I.GetMyTeam("TeamLayer_MyTeam");
		}

		private void _GetTeamMembersInterface(bool isWait, string teamId = "")
		{
			if (string.IsNullOrEmpty(teamId))
			{
				if (TeamDataMgr.I.MyTeamInfo == null)
				{
					return;
				}
				teamId = TeamDataMgr.I.MyTeamInfo.id;
			}
			if (isWait)
			{
				TeamMaskCtl.I.Show(1f, true);
			}
			TeamHttpMgr.I.GetTeamMembers(teamId, "TeamLayer_MyTeamMembers", -1, 50);
		}

		private void _ImmediatelyExecuteLoopMyTeamInterface()
		{
			if (delaySeq.IsPlaying())
			{
				delaySeq.Restart();
			}
			TeamHttpMgr.I.GetMyTeam("TeamLayer_Loop_MyTeam");
		}

		private GameObject _Execute(MButton btn)
		{
			if (btn == null || btn == lastBtn)
			{
				return null;
			}
			if (btn != teamsBtn)
			{
				teamList = null;
			}
			if (btn != createTeamBtn)
			{
				teamCreate = null;
			}
			if (btn != searchBtn)
			{
				teamSearch = null;
			}
			if (btn != myTeamBtn)
			{
				myTeam = null;
			}
			if (btn != joinRequestBtn)
			{
				teamJoinRequest = null;
			}
			if (btn != boardBtn)
			{
				teamBoard = null;
			}
			if (lastPage != null)
			{
				Object.Destroy(lastPage);
			}
			if (lastBtn != null)
			{
				lastBtn.interactable = true;
			}
			btn.interactable = false;
			lastBtn = btn;
			GameObject gameObject = LoadMgr.LoadRes<GameObject>(btn.sMsg);
			gameObject.transform.SetParent(tfLayerRoot, false);
			lastPage = gameObject;
			return gameObject;
		}

		private void _ShowBtns(bool isJoined, bool isLeader)
		{
			teamsBtn.gameObject.SetActive(!isJoined);
			createTeamBtn.gameObject.SetActive(!isJoined);
			searchBtn.gameObject.SetActive(!isLeader);
			myTeamBtn.gameObject.SetActive(isJoined);
			joinRequestBtn.gameObject.SetActive(isJoined && isLeader);
			boardBtn.gameObject.SetActive(isJoined);
		}

		private void _EnterTeamsPage(bool isWait, MButton btn)
		{
			GameObject gameObject = _Execute(btn);
			if (gameObject != null)
			{
				teamList = gameObject.GetComponent<TeamList>();
				teamList.onJoinSuccess = () =>
				{
					_ShowBtns(true, false);
					_EnterBoardPage(true, boardBtn);
					_ImmediatelyExecuteLoopMyTeamInterface();
				};
				teamList.Show(isWait);
			}
		}

		private void _EnterCreateTeamPage(MButton btn)
		{
			GameObject gameObject = _Execute(btn);
			if (gameObject != null)
			{
				teamCreate = gameObject.GetComponent<TeamCreate>();
				teamCreate.onCreateSucc = () =>
				{
					if (TeamDataMgr.I.MyTeamInfo == null)
					{
						return;
					}
					_ShowBtns(true, true);
					_EnterBoardPage(true, boardBtn);
					_ImmediatelyExecuteLoopMyTeamInterface();
				};
				teamCreate.Show();
			}
		}

		private void _EnterSearchPage(MButton btn)
		{
			GameObject gameObject = _Execute(btn);
			if (gameObject != null)
			{
				teamSearch = gameObject.GetComponent<TeamSearch>();
				teamSearch.onJoinSuccess = () =>
				{
					_ShowBtns(true, false);
					_EnterBoardPage(true, boardBtn);
					_ImmediatelyExecuteLoopMyTeamInterface();
				};
				teamSearch.Show();
			}
		}

		private void _EnterBoardPage(bool isWait, MButton btn)
		{
			GameObject gameObject = _Execute(btn);
			if (gameObject != null)
			{
				teamBoard = gameObject.GetComponent<TeamBoard>();
				teamBoard.Show(isWait);
			}
		}

		private void _EnterMyTeamPage(MButton btn)
		{
			GameObject gameObject = _Execute(btn);
			if (gameObject != null)
			{
				myTeam = gameObject.GetComponent<MyTeam>();
				myTeam.onLevelCallback = () =>
				{
					_ShowBtns(false, false);
					_EnterTeamsPage(true, teamsBtn);
				};
				myTeam.Show();
			}
		}

		private void _EnterJoinRequestPage(MButton btn)
		{
			GameObject gameObject = _Execute(btn);
			if (gameObject != null)
			{
				teamJoinRequest = gameObject.GetComponent<TeamJoinRequest>();
				teamJoinRequest.Show();
			}
		}

		private void _OnMyTeamOwn(ServerDataRoot<TeamInfo> root)
		{
			if (root.code == 1)
			{
				if (root.data != null)
				{
					if (TeamDataMgr.I.MyTeamInfo == null)
					{
						_GetTeamMembersInterface(false, root.data.id);
					}
					TeamDataMgr.I.MyTeamInfo = root.data;
					if (myTeam != null)
					{
						myTeam.RefreshTeamUI();
					}
					if (teamBoard != null)
					{
						teamBoard.RefreshTeamUI();
					}
				}
				else if (TeamDataMgr.I.MyTeamInfo != null)
				{
					TeamDataMgr.I.MyTeamInfo = null;
					TeamDataMgr.I.MyTeamMemberInfos = null;
					_ShowBtns(false, false);
					_EnterTeamsPage(true, teamsBtn);
					LogEvent.I.SetTeamPosition(LogEvent.TeamPosition.Player);
				}
			}
		}

		private void _OnMyTeam(ServerDataRoot<TeamInfo> root)
		{
			if (root.code == 1)
			{
				TeamDataMgr.I.MyTeamInfo = root.data;
				if (root.data != null)
				{
					_GetTeamMembersInterface(false, root.data.id);
					return;
				}
				if (delaySeq != null)
				{
					delaySeq.Restart();
				}
				_ShowBtns(false, false);
				_EnterTeamsPage(false, teamsBtn);
				LogEvent.I.SetTeamPosition(LogEvent.TeamPosition.Player);
			}
			else
			{
				TeamMaskCtl.I.Hide(true);
				TeamNetworkTip.I.Show();
			}
		}

		private void _OnMyTeamMembers(ServerDataListRoot<MemberInfo> root)
		{
			if (root.code == 1)
			{
				if (delaySeq != null)
				{
					delaySeq.Restart();
				}
				TeamDataMgr.I.MyTeamMemberInfos = root.data;
				bool isLeader = TeamDataMgr.I.IsLeader();
				_ShowBtns(true, isLeader);
				_EnterBoardPage(false, boardBtn);
				LogEvent.I.SetTeamPosition(isLeader ? LogEvent.TeamPosition.Leader : LogEvent.TeamPosition.Member);
			}
			else
			{
				TeamMaskCtl.I.Hide(true);
				Global.ShowRetryDialog("Language55", (isRetry) =>
				{
					if (isRetry)
					{
						_GetTeamMembersInterface(true, "");
					}
				});
			}
		}

		private void _ClearAllEvent()
		{
			_EventOperation(true);
			if (loginControl != null)
			{
				loginControl.EventOperation(true);
			}
			if (teamList != null)
			{
				teamList.EventOperation(true);
			}
			if (teamCreate != null)
			{
				teamCreate.EventOperation(true);
			}
			if (teamSearch != null)
			{
				teamSearch.EventOperation(true);
			}
			if (myTeam != null)
			{
				myTeam.EventOperation(true);
			}
			if (teamJoinRequest != null)
			{
				teamJoinRequest.EventOperation(true);
			}
			if (teamBoard != null)
			{
				teamBoard.EventOperation(true);
			}
			if (Authentication.OwnServerLogin.Instance != null)
			{
				Authentication.OwnServerLogin.Instance.AbortGuestSignIn();
			}
		}

		private void _EventOperation(bool isOff)
		{
			if (isOff)
			{
				DispatcherMgr.Off<ServerDataRoot<TeamInfo>>("TeamLayer_Loop_MyTeam", _OnMyTeamOwn);
				DispatcherMgr.Off<ServerDataRoot<TeamInfo>>("TeamLayer_MyTeam", _OnMyTeam);
				DispatcherMgr.Off<ServerDataListRoot<MemberInfo>>("TeamLayer_MyTeamMembers", _OnMyTeamMembers);
			}
			else
			{
				DispatcherMgr.On<ServerDataRoot<TeamInfo>>("TeamLayer_Loop_MyTeam", _OnMyTeamOwn);
				DispatcherMgr.On<ServerDataRoot<TeamInfo>>("TeamLayer_MyTeam", _OnMyTeam);
				DispatcherMgr.On<ServerDataListRoot<MemberInfo>>("TeamLayer_MyTeamMembers", _OnMyTeamMembers);
			}
		}

		private void OnDestroy()
		{
			DispatcherMgr.Off("Change_Module_Event", _ClearAllEvent);
			if (delaySeq != null)
			{
				delaySeq.Kill();
				delaySeq = null;
			}
			_EventOperation(true);
		}
	}
}
