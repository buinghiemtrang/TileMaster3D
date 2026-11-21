using System.Collections.Generic;
using GameTeam;
using UnityEngine;
using UnityEngine.UI;

public class TeamDataMgr
{
	private static TeamDataMgr _ins = new TeamDataMgr();

	private TeamInfo myTeamInfo;

	private List<MemberInfo> myTeamMemberInfos;

	private List<BoardsInfo> myBoardsInfos = new List<BoardsInfo>();

	private string curKickUserId;

	public static TeamDataMgr I => _ins;

	public TeamInfo MyTeamInfo
	{
		get
		{
			return myTeamInfo;
		}
		set
		{
			myTeamInfo = value;
			if (value == null)
			{
				Singleton<TreasureHunt.TreasureHuntActivityMgr>.I.LevelTeam();
				Singleton<Game.Activity.BakingMaster.ActivityMgr>.I.SignOutTeamClearLocalData();
			}
			else
			{
				Singleton<TreasureHunt.TreasureHuntActivityMgr>.I.JoinTeam(value.id);
			}
		}
	}

	public string CurKickUserId
	{
		get
		{
			return curKickUserId;
		}
		set
		{
			curKickUserId = value;
		}
	}

	public List<MemberInfo> MyTeamMemberInfos
	{
		get
		{
			return myTeamMemberInfos;
		}
		set
		{
			myTeamMemberInfos = value;
		}
	}

	public List<BoardsInfo> MyBoardsInfos
	{
		get
		{
			return myBoardsInfos;
		}
		set
		{
			myBoardsInfos = value;
		}
	}

	public TeamStatusType GetTeamStatusType()
	{
		if (IsLeader())
		{
			return TeamStatusType.Leader;
		}
		return (myTeamInfo != null) ? TeamStatusType.Member : TeamStatusType.NoTeam;
	}

	public MemberInfo GetMyMemberInfo()
	{
		if (myTeamMemberInfos == null)
		{
			return null;
		}
		return myTeamMemberInfos.Find((MemberInfo info) => info.userId.Equals(DB.Account.Userid));
	}

	public bool IsLeader()
	{
		MemberInfo myMemberInfo = GetMyMemberInfo();
		if (myMemberInfo != null)
		{
			return myMemberInfo.projection.Team.IsLeader;
		}
		return false;
	}

	public bool RemoveKickMember()
	{
		if (myTeamMemberInfos == null)
		{
			return false;
		}
		foreach (MemberInfo myTeamMemberInfo in myTeamMemberInfos)
		{
			if (myTeamMemberInfo.userId.Equals(curKickUserId))
			{
				myTeamMemberInfos.Remove(myTeamMemberInfo);
				return true;
			}
		}
		return false;
	}

	public Sprite GetTeamBadge()
	{
		Sprite sprite = null;
		if (myTeamInfo != null)
		{
			sprite = LoadMgr.LoadRes<Sprite>("TeamIcon/" + myTeamInfo.badge);
		}
		if ((bool)sprite)
		{
			return sprite;
		}
		Sprite sprite2 = Resources.Load<Sprite>("TeamIcon/team-icon-1");
		Sprite sprite3 = null;
		if ((bool)sprite2)
		{
			sprite3 = Object.Instantiate(sprite2);
			sprite3.name = sprite2.name;
		}
		return sprite3;
	}

	public void SetTeamBadge(TeamInfo info, Image image)
	{
		GetTeamBadgeName(info, image);
	}

	public string SetTeamBadge(string badgeName, Image image)
	{
		Sprite sprite = LoadMgr.LoadRes<Sprite>("TeamIcon/" + badgeName);
		if (!sprite)
		{
			sprite = LoadMgr.LoadRes<Sprite>("TeamIcon/team-icon-1");
			badgeName = "team-icon-1";
		}
		if ((bool)image)
		{
			image.sprite = sprite;
		}
		return badgeName;
	}

	public string GetTeamBadgeName(TeamInfo info, Image image = null)
	{
		return SetTeamBadge((info != null) ? info.badge : "team-icon-1", image);
	}
}
