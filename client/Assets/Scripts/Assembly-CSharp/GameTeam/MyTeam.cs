using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace GameTeam
{
	public class MyTeam : MonoBehaviour
	{
		[SerializeField]
		private GameObject teamObj;

		[SerializeField]
		private Image teamIcon;

		[SerializeField]
		private Text teamName;

		[SerializeField]
		private Text memberCount;

		[SerializeField]
		private MButton BtnInfo;

		[SerializeField]
		private MyTeamScrollView pScroll;

		[SerializeField]
		private MyTeamMemberItem pOwnItem;

		[SerializeField]
		private Transform tfBottom;

		private Sequence delaySeq;

		private bool isDirectCall;

		public Action onLevelCallback;

		private void Awake()
		{
			BtnInfo.onClick.AddListener(OnClickBtnInfo);
			EventOperation(false);
			delaySeq = DOTween.Sequence().AppendInterval(UnityEngine.Random.Range(10, 16)).AppendCallback(() =>
			{
				isDirectCall = false;
				_GetTeamMembersInterface(false);
			}).SetLoops(-1).SetId("MyTeam_Delay").Pause();
		}

		public void Show()
		{
			RefreshTeamUI();
			if (TeamDataMgr.I.MyTeamInfo != null && TeamDataMgr.I.MyTeamMemberInfos != null)
			{
				if (delaySeq != null)
				{
					delaySeq.Restart();
				}
			}
			else
			{
				_MyTeamInterface();
			}
		}

		private void _MyTeamInterface()
		{
			TeamMaskCtl.I.Show(1f, true);
			TeamHttpMgr.I.GetMyTeam("MyTeam_MyTeam");
		}

		private void _GetTeamMembersInterface(bool isWait)
		{
			if (TeamDataMgr.I.MyTeamInfo != null)
			{
				if (isWait)
				{
					TeamMaskCtl.I.Show(1f, true);
				}
				TeamHttpMgr.I.GetTeamMembers(TeamDataMgr.I.MyTeamInfo.id, "MyTeam_MyTeamMembers", -1, 50);
			}
		}

		public void RefreshTeamUI()
		{
			if (TeamDataMgr.I.MyTeamInfo != null)
			{
				teamObj.SetActive(true);
				teamName.text = TeamDataMgr.I.MyTeamInfo.name;
				teamIcon.sprite = TeamDataMgr.I.GetTeamBadge();
				memberCount.text = string.Format("{0}/{1}", TeamDataMgr.I.MyTeamInfo.members, cf_const.teamMaxMembers);
				RefreshOwnInfo();
			}
			else
			{
				teamObj.SetActive(false);
				tfBottom.gameObject.SetActive(false);
			}
		}

		private void _RefreshMembersUI()
		{
			if (TeamDataMgr.I.MyTeamInfo != null && TeamDataMgr.I.MyTeamMemberInfos != null)
			{
				RefreshOwnInfo();
				pScroll.RefreshItems();
			}
		}

		private void RefreshOwnInfo()
		{
			int index;
			MemberInfo ownMemberInfo = GetOwnMemberInfo(out index);
			if (tfBottom != null)
			{
				tfBottom.gameObject.SetActive(ownMemberInfo != null);
			}
			if (ownMemberInfo != null)
			{
				pOwnItem.InitData(index, ownMemberInfo);
			}
		}

		private MemberInfo GetOwnMemberInfo(out int index)
		{
			index = 0;
			if (TeamDataMgr.I.MyTeamMemberInfos != null)
			{
				for (int i = 0; i < TeamDataMgr.I.MyTeamMemberInfos.Count; i++)
				{
					MemberInfo memberInfo = TeamDataMgr.I.MyTeamMemberInfos[i];
					if (memberInfo.userId.Equals(DB.Account.Userid))
					{
						index = i;
						return memberInfo;
					}
				}
			}
			return null;
		}

		private void OnClickBtnInfo(MButton btn)
		{
			MyTeamInfoDialog myTeamInfoDialog = SceneMgr.AddTopNode<MyTeamInfoDialog>("Prefabs/GameTeam/MyTeamInfoDialog");
			myTeamInfoDialog.onUpdateCallback = RefreshTeamUI;
			myTeamInfoDialog.onLevelCallback = onLevelCallback;
			myTeamInfoDialog.Show();
		}

		private void _OnKick()
		{
			if (delaySeq != null)
			{
				delaySeq.Pause();
			}
			_MyTeamInterface();
		}

		private void _OnMyTeam(ServerDataRoot<TeamInfo> root)
		{
			if (root.code == 1)
			{
				TeamDataMgr.I.MyTeamInfo = root.data;
				if (root.data != null)
				{
					RefreshTeamUI();
					isDirectCall = true;
					_GetTeamMembersInterface(false);
				}
				else
				{
					TeamMaskCtl.I.Hide(true);
				}
			}
			else
			{
				TeamMaskCtl.I.Hide(true);
				Global.ShowRetryDialog("Language55", (isSuccess) =>
				{
					if (isSuccess)
					{
						_MyTeamInterface();
					}
				});
			}
		}

		private void _OnGetTeamMembers(ServerDataListRoot<MemberInfo> root)
		{
			TeamMaskCtl.I.Hide(true);
			if (root.code == 1)
			{
				isDirectCall = false;
				if (delaySeq != null)
				{
					delaySeq.Restart();
				}
				TeamDataMgr.I.MyTeamMemberInfos = root.data;
				_RefreshMembersUI();
			}
			else if (isDirectCall)
			{
				Global.ShowRetryDialog("Language55", (isSuccess) =>
				{
					if (isSuccess)
					{
						_GetTeamMembersInterface(true);
					}
				});
			}
		}

		public void EventOperation(bool isOff)
		{
			if (isOff)
			{
				DispatcherMgr.Off("MyTeam_Kick", _OnKick);
				DispatcherMgr.Off<ServerDataListRoot<MemberInfo>>("MyTeam_MyTeamMembers", _OnGetTeamMembers);
				DispatcherMgr.Off<ServerDataRoot<TeamInfo>>("MyTeam_MyTeam", _OnMyTeam);
			}
			else
			{
				DispatcherMgr.On("MyTeam_Kick", _OnKick);
				DispatcherMgr.On<ServerDataListRoot<MemberInfo>>("MyTeam_MyTeamMembers", _OnGetTeamMembers);
				DispatcherMgr.On<ServerDataRoot<TeamInfo>>("MyTeam_MyTeam", _OnMyTeam);
			}
		}

		private void OnDestroy()
		{
			if (delaySeq != null)
			{
				delaySeq.Kill();
				delaySeq = null;
			}
			EventOperation(true);
		}
	}
}
