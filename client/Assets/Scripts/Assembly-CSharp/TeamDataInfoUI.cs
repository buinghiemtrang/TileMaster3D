using System.Collections.Generic;
using GameTeam;
using UnityEngine;
using UnityEngine.UI;

public class TeamDataInfoUI : MonoBehaviour
{
	[SerializeField]
	private Text lvTex;

	[SerializeField]
	private Text helpsTex;

	[SerializeField]
	private Text requiredLvTex;

	[SerializeField]
	private Text teamTypeTex;

	public void Init(List<MemberInfo> members, TeamInfo info)
	{
		if (info != null)
		{
			requiredLvTex.text = info.requiredLevel.ToString();
			teamTypeTex.text = info.type;
		}
		if (members == null)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (MemberInfo member in members)
		{
			num += member.projection.User.maxlv;
			num2 += member.projection.Team.Helps;
		}
		lvTex.text = num.ToString();
		helpsTex.text = num2.ToString();
	}
}
