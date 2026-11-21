using System;
using GameTeam;
using ShortBattlePass;
using UnityEngine;
using UnityEngine.UI;

public class ShortBattlePassItemInBoard : ItemInBoard
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

	private int _coinNum;

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
		int[,] costReward = Singleton<ShortBattlePassMgr>.I.GetCostReward();
		int length = costReward.GetLength(0);
		for (int i = 0; i < length; i++)
		{
			if (costReward[i, 0] == 0)
			{
				_coinNum = costReward[i, 1];
				numTex.text = _coinNum.ToString();
				break;
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
		string[] receivers = boardInfo.Receivers;
		foreach (string text in receivers)
		{
			if (text.Equals(DB.Account.Userid))
			{
				return true;
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
			DB.Bag.Modify(PropID.Money, _coinNum, true, "GameTeam");
			TeamUtils.ShowFlayItemAnim(base.transform, SceneMain.ins.goldIcon, PropID.Money, delegate
			{
				int prop = DB.Bag.GetProp(PropID.Money);
				DispatcherMgr.Emit("Update_" + PropID.Money, prop, prop);
			});
		}
		else
		{
			Global.ShowMsgTip("Language55");
		}
	}
}
