using UnityEngine;
using UnityEngine.UI;

namespace GameTeam
{
	public class MyTeamMemberItem : MonoBehaviour
	{
		[SerializeField]
		private Sprite[] itemBgs;

		[SerializeField]
		private Sprite[] indexIcons;

		[SerializeField]
		private Sprite[] textBgs;

		[SerializeField]
		private Color[] textLeaderColors;

		[SerializeField]
		private Text textIndex;

		[SerializeField]
		private Text textName;

		[SerializeField]
		private Text textLeader;

		[SerializeField]
		private Text textHelp;

		[SerializeField]
		private Text textHelpCnt;

		[SerializeField]
		private Text textLevel;

		[SerializeField]
		private Text textLevelCnt;

		[SerializeField]
		private Image itemBg;

		[SerializeField]
		private Image helpBg;

		[SerializeField]
		private Image levelBg;

		[SerializeField]
		private Image imageIndex;

		[SerializeField]
		private Image imageLeader;

		[SerializeField]
		private MButton btnClick;

		private int index;

		private MemberInfo memberInfo;

		public void InitData(int index, MemberInfo info)
		{
			this.index = index;
			this.memberInfo = info;
			UpdateInfo();
		}

		private void UpdateInfo()
		{
			if (memberInfo == null)
			{
				return;
			}
			imageIndex.gameObject.SetActive(index < 3);
			textIndex.gameObject.SetActive(index > 2);
			if (index < 3)
			{
				imageIndex.sprite = indexIcons[index];
			}
			bool isLeader = memberInfo.projection.Team.IsLeader;
			textLeader.gameObject.SetActive(isLeader);
			imageLeader.gameObject.SetActive(isLeader);
			Vector3 localPosition = textName.transform.localPosition;
			localPosition.y = (isLeader ? 20f : 0f);
			textName.transform.localPosition = localPosition;
			bool isMe = memberInfo.userId.Equals(DB.Account.Userid);
			int num = (isMe ? 1 : 0);
			itemBg.sprite = itemBgs[num];
			helpBg.sprite = textBgs[num];
			levelBg.sprite = textBgs[num];
			textLeader.color = textLeaderColors[num];
			textIndex.text = (index + 1).ToString();
			textHelpCnt.text = memberInfo.projection.Team.Helps.ToString();
			textLevelCnt.text = memberInfo.projection.User.maxlv.ToString();
			if (memberInfo.projection != null && memberInfo.projection.Account != null && !string.IsNullOrEmpty(memberInfo.projection.Account.name))
			{
				textName.text = memberInfo.projection.Account.name;
			}
			else
			{
				textName.text = "Player" + memberInfo.userId.Substring(0, 8);
			}
		}

		public void OnClick(MButton button)
		{
			if (memberInfo != null && TeamDataMgr.I.IsLeader() && !memberInfo.projection.Team.IsLeader)
			{
				TeamMemberKick teamMemberKick = SceneMgr.AddTopNode<TeamMemberKick>("Prefabs/GameTeam/TeamMemberKick");
				teamMemberKick.Show(memberInfo.userId, memberInfo.projection.Account.name, base.transform);
			}
		}
	}
}
