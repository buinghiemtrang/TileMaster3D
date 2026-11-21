using DG.Tweening;
using UnityEngine;

namespace GameTeam
{
	public class TeamJoinRequest : MonoBehaviour
	{
		[SerializeField]
		private TeamJoinRequestScrollView pScroll;

		private Sequence delaySeq;

		private bool isDealApply;

		private string userId;

		private void Awake()
		{
			isDealApply = false;
			EventOperation(false);
			int num = Random.Range(10, 16);
			delaySeq = DOTween.Sequence();
			delaySeq.AppendInterval(num);
			delaySeq.AppendCallback(delegate
			{
				TeamHttpMgr.I.GetJoinTeamApplies();
			});
			delaySeq.SetLoops(-1);
			delaySeq.SetId("TeamJoinRequest_Delay");
			delaySeq.Pause();
		}

		private void Start()
		{
			TeamHttpMgr.I.GetJoinTeamApplies();
			pScroll.cback = delegate(bool isAgree, string guid, string userId)
			{
				this.userId = userId;
				_Pause();
				TeamMaskCtl.I.Show(1f, true);
				if (isAgree)
				{
					TeamHttpMgr.I.AgreeJoinTeamApply(guid);
				}
				else
				{
					TeamHttpMgr.I.RejectJoinTeamApply(guid);
				}
			};
		}

		private void _OnGetApplyJoinTeamInfos(ServerDataListRoot<ApplyJoinTeamInfo> root)
		{
			_ReStart();
			if (root.code == 1)
			{
				pScroll.SetInfos(root.data);
			}
		}

		private void _OnAgreeApply(ServerDataRootBase root)
		{
			if (root.code == 1)
			{
				TeamHttpMgr.I.GetJoinTeamApplies();
			}
			else
			{
				_ReStart();
				Global.ShowMsgTip((root.code == -1) ? "Language55" : "Language244");
			}
		}

		private void _OnRefuseApply(ServerDataRootBase root)
		{
			if (root.code == 1)
			{
				TeamHttpMgr.I.GetJoinTeamApplies();
			}
			else
			{
				_ReStart();
				Global.ShowMsgTip((root.code == -1) ? "Language55" : "Language244");
			}
		}

		private void _Pause()
		{
			isDealApply = true;
			delaySeq?.Pause();
		}

		private void _ReStart()
		{
			isDealApply = false;
			TeamMaskCtl.I.Hide(true);
			delaySeq?.Restart();
		}

		public void EventOperation(bool isOff)
		{
			if (isOff)
			{
				DispatcherMgr.Off<ServerDataListRoot<ApplyJoinTeamInfo>>("TeamHttp_GetJoinTeamApplies", _OnGetApplyJoinTeamInfos);
				DispatcherMgr.Off<ServerDataRootBase>("TeamHttp_AgreeJoinTeamApply", _OnAgreeApply);
				DispatcherMgr.Off<ServerDataRootBase>("TeamHttp_RejectJoinTeamApply", _OnRefuseApply);
			}
			else
			{
				DispatcherMgr.On<ServerDataListRoot<ApplyJoinTeamInfo>>("TeamHttp_GetJoinTeamApplies", _OnGetApplyJoinTeamInfos);
				DispatcherMgr.On<ServerDataRootBase>("TeamHttp_AgreeJoinTeamApply", _OnAgreeApply);
				DispatcherMgr.On<ServerDataRootBase>("TeamHttp_RejectJoinTeamApply", _OnRefuseApply);
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
