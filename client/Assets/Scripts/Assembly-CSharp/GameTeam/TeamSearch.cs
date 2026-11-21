using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameTeam
{
	public class TeamSearch : MonoBehaviour
	{
		[SerializeField]
		private Text textTip;

		[SerializeField]
		private InputField inputName;

		[SerializeField]
		private MButton searchBtn;

		[SerializeField]
		private TeamSearchScrollView pScroll;

		public Action onJoinSuccess;

		private void Awake()
		{
			searchBtn.onClick.AddListener(_OnClickSearch);
			pScroll.onJoinSuccess = () =>
			{
				if (onJoinSuccess != null)
				{
					onJoinSuccess();
				}
			};
			EventOperation(false);
		}

		public void Show()
		{
			inputName.text = "";
		}

		private void _OnSearch(ServerDataListRoot<TeamInfo> root)
		{
			TeamMaskCtl.I.Hide(true);
			if (root.code == 1)
			{
				textTip.gameObject.SetActive(root.data.Count < 1);
				pScroll.gameObject.SetActive(root.data.Count > 0);
				if (root.data.Count > 0)
				{
					pScroll.RefreshItems(TeamDataMgr.I.MyTeamInfo != null, root.data);
				}
				else
				{
					textTip.SetLKey("Language241");
				}
			}
			else
			{
				Global.ShowMsgTip("Language55");
			}
		}

		private void _OnClickSearch(MButton btn)
		{
			string text = inputName.text.Trim();
			if (!string.IsNullOrEmpty(text))
			{
				TeamMaskCtl.I.Show(1f, true);
				TeamHttpMgr.I.VagueSearch(text);
			}
		}

		public void EventOperation(bool isOff)
		{
			if (isOff)
			{
				DispatcherMgr.Off<ServerDataListRoot<TeamInfo>>("TeamHttp_Search", _OnSearch);
			}
			else
			{
				DispatcherMgr.On<ServerDataListRoot<TeamInfo>>("TeamHttp_Search", _OnSearch);
			}
		}

		private void OnDestroy()
		{
			EventOperation(true);
		}
	}
}
