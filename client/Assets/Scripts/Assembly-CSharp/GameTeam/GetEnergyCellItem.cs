using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameTeam
{
	public class GetEnergyCellItem : MonoBehaviour
	{
		[SerializeField]
		private Image ImageIcon;

		[SerializeField]
		private Text TextName;

		[SerializeField]
		private MButton BtnGet;

		public Action<string> onReceiveSuccess;

		private string guid;

		private void Awake()
		{
			BtnGet.onClick.AddListener(ClickGet);
		}

		public void InitData(EnergysInfo.EnergyInfo info)
		{
			guid = info.GUID;
			TextName.text = info.SendUserName;
		}

		private void ClickGet(MButton btn)
		{
			if (!DB.Bag.JudgeUnlimitedEnergy() && DB.Bag.GetProp(PropID.Energy) < DB.Bag.GetMaxEnergyRecover())
			{
				GetEnergyMaskCtl.I.Show(1f, true);
				DispatcherMgr.On<ServerDataRootBase>("TeamHttp_ReceiveEnergy", _OnRiceiveEnergy);
				EnergyHttpMgr.I.ReceiveEnergy(guid);
			}
			else
			{
				Global.ShowMsgTip("Language238");
			}
		}

		private void _OnRiceiveEnergy(ServerDataRootBase root)
		{
			DispatcherMgr.Off<ServerDataRootBase>("TeamHttp_ReceiveEnergy", _OnRiceiveEnergy);
			if (root.code == 1)
			{
				LogEvent.I.TeamLifeReceive(TeamDataMgr.I.MyTeamInfo.id);
				int oldNum = DB.Bag.ModifyEnergy(1, false, "Team");
				TeamUtils.ShowFlayItemAnim(base.transform, SceneMain.ins.energyIcon, PropID.Energy, delegate
				{
					DispatcherMgr.Emit("Update_" + PropID.Energy, oldNum, (oldNum + 1 <= cf_const.maxEnergyUpperLimit) ? (oldNum + 1) : cf_const.maxEnergyUpperLimit);
				});
				if (onReceiveSuccess != null)
				{
					onReceiveSuccess(guid);
				}
			}
			else
			{
				GetEnergyMaskCtl.I.Hide(true);
				Global.ShowMsgTip(root.msg);
			}
		}
	}
}
