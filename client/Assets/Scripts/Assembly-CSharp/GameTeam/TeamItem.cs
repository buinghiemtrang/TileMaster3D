using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameTeam
{
	public class TeamItem : MonoBehaviour
	{
		[SerializeField]
		private Image iconImg;

		[SerializeField]
		private Text nameTex;

		[SerializeField]
		private Text membersTex;

		[SerializeField]
		private MButton joinBtn;

		[SerializeField]
		private Text textJoinBtn;

		public Action onJoinSuccess;

		private TeamInfo teamInfo;

		private bool isShowJoinBtn;

		private void Awake()
		{
			joinBtn.onClick.AddListener(_OnClickJoin);
		}

		public void Init(bool isJoined, TeamInfo info)
		{
			teamInfo = info;
			if (info != null && nameTex != null)
			{
				nameTex.text = info.name;
				if (membersTex != null)
				{
					membersTex.text = info.members.ToString() + "/" + cf_const.teamMaxMembers.ToString();
				}
				TeamDataMgr.I.SetTeamBadge(info, iconImg);
				isShowJoinBtn = info.members < cf_const.teamMaxMembers && !isJoined;
				if (joinBtn != null)
				{
					joinBtn.interactable = info.members < cf_const.teamMaxMembers;
					textJoinBtn.SetLBaseKey((info.members < cf_const.teamMaxMembers) ? "Language371" : "Language52");
				}
			}
		}

		private void _OnClickJoin(MButton button)
		{
			TeamJoinDialog teamJoinDialog = SceneMgr.AddTopNode<TeamJoinDialog>("Prefabs/GameTeam/TeamJoinDialog");
			teamJoinDialog.onJoinSuccess = delegate
			{
				if (onJoinSuccess != null)
				{
					onJoinSuccess();
				}
			};
			teamJoinDialog.Show(teamInfo, isShowJoinBtn);
		}
	}
}
