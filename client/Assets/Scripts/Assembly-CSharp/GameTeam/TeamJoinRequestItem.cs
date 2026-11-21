using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameTeam
{
	public class TeamJoinRequestItem : MonoBehaviour
	{
		[SerializeField]
		private Text TextName;

		[SerializeField]
		private MButton BtnYes;

		[SerializeField]
		private MButton BtnNo;

		private ApplyJoinTeamInfo applyJoinTeamInfo;

		public Action<bool, string, string> cback;

		private void Awake()
		{
			if (BtnYes != null)
			{
				BtnYes.onClick.AddListener(OnClickBtnYes);
			}
			if (BtnNo != null)
			{
				BtnNo.onClick.AddListener(OnClickBtnNo);
			}
		}

		public void InitData(ApplyJoinTeamInfo info)
		{
			applyJoinTeamInfo = info;
			TextName.text = info.JoinTeamApply.UserName;
		}

		private void OnClickBtnYes(MButton button)
		{
			if (applyJoinTeamInfo != null && cback != null)
			{
				cback(true, applyJoinTeamInfo.JoinTeamApply.GUID, applyJoinTeamInfo.JoinTeamApply.UserId);
			}
		}

		private void OnClickBtnNo(MButton button)
		{
			if (applyJoinTeamInfo != null && cback != null)
			{
				cback(false, applyJoinTeamInfo.JoinTeamApply.GUID, applyJoinTeamInfo.JoinTeamApply.UserId);
			}
		}
	}
}
