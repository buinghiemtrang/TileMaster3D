using System;
using UnityEngine;

namespace GameTeam
{
	public class TeamList : MonoBehaviour
	{
		[SerializeField]
		private TeamListScrollView pScroll;

		public Action onJoinSuccess;

		private void Awake()
		{
			pScroll.onJoinSuccess = () =>
			{
				if (onJoinSuccess != null)
				{
					onJoinSuccess();
				}
			};
			EventOperation(false);
		}

		public void Show(bool isWait)
		{
			if (isWait)
			{
				TeamMaskCtl.I.Show(1f, true);
			}
			TeamHttpMgr.I.GetTeams();
		}

		private void _OnGetTeams(ServerDataListRoot<TeamInfo> root)
		{
			TeamMaskCtl.I.Hide(true);
			if (root.code == 1)
			{
				pScroll.RefreshItems(false, root.data);
			}
			else
			{
				Global.ShowRetryDialog("Language55", (isRetry) =>
				{
					if (isRetry)
					{
						Show(true);
					}
				});
			}
		}

		public void EventOperation(bool isOff)
		{
			if (isOff)
			{
				DispatcherMgr.Off<ServerDataListRoot<TeamInfo>>("TeamHttp_GetTeams", _OnGetTeams);
			}
			else
			{
				DispatcherMgr.On<ServerDataListRoot<TeamInfo>>("TeamHttp_GetTeams", _OnGetTeams);
			}
		}

		private void OnDestroy()
		{
			EventOperation(true);
		}
	}
}
