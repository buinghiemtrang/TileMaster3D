using System;
using GameTeam;
using RoyalPass;
using UnityEngine;
using UnityEngine.UI;

public class RoyalPassActivityItemInBoard : ItemInBoard
{
	[SerializeField]
	private Text nameTex;

	[SerializeField]
	private Text numTex;

	[SerializeField]
	private GameObject receivedObj;

	[SerializeField]
	private GameObject rewardObj;

	[SerializeField]
	private MButton receiveBtn;

	[SerializeField]
	private Transform contentsTran;

	private int coinNum;

	private void Awake()
	{
		receiveBtn.onClick.AddListener(_ClickReceive);
	}

	public override void Init(BoardsInfo.BoardInfo info)
	{
		if (info == null)
		{
			return;
		}
		base.Init(info);
		nameTex.text = info.UserName;
		int[,] paidReward = Singleton<RoyalPassActivityMgr>.I.GetPaidReward();
		if (paidReward != null)
		{
			int length = paidReward.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				if (paidReward[i, 0] == 0)
				{
					coinNum = paidReward[i, 1];
					numTex.text = coinNum.ToString();
					break;
				}
			}
		}
		bool flag = info.UserId.Equals(DB.Account.Userid);
		bool flag2 = JudgeReceived();
		receivedObj.SetActive(flag || flag2);
		rewardObj.SetActive(!flag && !flag2);
		receiveBtn.gameObject.SetActive(!flag && !flag2);
		Vector3 localPosition = contentsTran.localPosition;
		contentsTran.localPosition = new Vector3(localPosition.x, (!flag && !flag2) ? 0f : -17f, localPosition.z);
	}

	private bool JudgeReceived()
	{
		if (boardInfo != null && boardInfo.Receivers != null)
		{
			for (int i = 0; i < boardInfo.Receivers.Length; i++)
			{
				if (boardInfo.Receivers[i].Equals(DB.Account.Userid))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void _ClickReceive(MButton btn)
	{
		TeamMaskCtl.I.Show(1f, true);
		DispatcherMgr.On<ServerDataRootBase>("AddTeamBoardReceiver", _OnAddTeamBoardReceiver);
		TeamHttpMgr.I.AddTeamBoardReceiver(boardInfo.GUID);
	}

	private void _OnAddTeamBoardReceiver(ServerDataRootBase root)
	{
		TeamMaskCtl.I.Hide(true);
		DispatcherMgr.Off<ServerDataRootBase>("AddTeamBoardReceiver", _OnAddTeamBoardReceiver);
		if (root.code == 1)
		{
			int num = boardInfo.Receivers.Length;
			string[] array = new string[num + 1];
			Array.Copy(boardInfo.Receivers, array, num);
			array[num] = DB.Account.Userid;
			boardInfo.Receivers = array;
			Init(boardInfo);
			DB.Bag.Modify(PropID.Money, coinNum, true, "GameTeam");
			TeamUtils.ShowFlayItemAnim(base.transform, SceneMain.ins.goldIcon, PropID.Money, delegate
			{
			});
		}
		else
		{
			Global.ShowMsgTip("Language55");
		}
	}
}
