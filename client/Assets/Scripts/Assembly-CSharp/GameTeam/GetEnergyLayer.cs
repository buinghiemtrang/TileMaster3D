using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameTeam
{
	public class GetEnergyLayer : MonoBehaviour
	{
		[SerializeField]
		private GetEnergyScrollView pScroll;

		[SerializeField]
		private GameObject objTips;

		[SerializeField]
		private MButton BtnJoin;

		[SerializeField]
		private Text textTips;

		[SerializeField]
		private Text textBtnWord;

		[SerializeField]
		private GuestLoginControl loginControl;

		private bool isCheckGetEnergyInterface;

		private void Awake()
		{
			_EventOperation(false);
			DispatcherMgr.On("Change_Module_Event", _ClearAllEvent);
			objTips.SetActive(false);
			BtnJoin.onClick.AddListener(ClickBtnJoin);
			pScroll.onReceiveSuccess = () =>
			{
				isCheckGetEnergyInterface = false;
				_GetEnergyInterface(false);
			};
			loginControl.onLogin = (type) =>
			{
				if (type == GuestLoginState.Start)
				{
					GetEnergyMaskCtl.I.Show(1f, true);
				}
				else if (type == GuestLoginState.Success)
				{
					_MyTeamInterface();
				}
				else
				{
					GetEnergyMaskCtl.I.Hide(true);
					GetEnergyNetworkTip.I.Show();
				}
			};
		}

		private void Start()
		{
			if (DB.User.Maxlv < cf_const.teamUnlockLv)
			{
				objTips.SetActive(true);
				_RefreshUI(true, false);
			}
			else
			{
				loginControl.Login();
			}
		}

		private void _MyTeamInterface()
		{
			GetEnergyMaskCtl.I.Show(1f, true);
			EnergyHttpMgr.I.GetMyTeam("GetEnergyLayer_MyTeam");
		}

		private void _GetEnergyInterface(bool isWait)
		{
			if (isWait)
			{
				GetEnergyMaskCtl.I.Show(1f, true);
			}
			EnergyHttpMgr.I.GetEnergy("GetEnergyLayer_GetEnergy");
		}

		private void _OnGetMyTeam(ServerDataRoot<TeamInfo> root)
		{
			if (root.code == 1)
			{
				TeamDataMgr.I.MyTeamInfo = root.data;
				if (root.data != null)
				{
					isCheckGetEnergyInterface = true;
					_GetEnergyInterface(false);
				}
				else
				{
					GetEnergyMaskCtl.I.Hide(true);
					_RefreshUI(true, false);
				}
			}
			else
			{
				GetEnergyMaskCtl.I.Hide(true);
				GetEnergyNetworkTip.I.Show();
			}
		}

		private void OnGetEnergy(ServerDataListRoot<EnergysInfo> root)
		{
			GetEnergyMaskCtl.I.Hide(true);
			if (root.code == 1)
			{
				if (root.data[0] != null)
				{
					if (root.data[0].Lives == null)
					{
						_RefreshUI(true, true);
						return;
					}
					List<EnergysInfo.EnergyInfo> list = new List<EnergysInfo.EnergyInfo>();
					foreach (KeyValuePair<string, EnergysInfo.EnergyInfo> item in root.data[0].Lives)
					{
						list.Add(item.Value);
					}
					if (list.Count == 0)
					{
						_RefreshUI(true, true);
					}
					else
					{
						pScroll.SetInfos(list);
					}
				}
			}
			else
			{
				Global.ShowRetryDialog("Language55", (isSuccess) =>
				{
					if (isSuccess)
					{
						isCheckGetEnergyInterface = true;
						_GetEnergyInterface(true);
					}
					else
					{
						DispatcherMgr.Emit("ActiveHomeBtn", Array.Empty<object>());
					}
				});
			}
		}

		private void _RefreshUI(bool bIsNoTeam = false, bool bIsNoEnergyInfos = false)
		{
			bool flag = bIsNoTeam || bIsNoEnergyInfos;
			objTips.SetActive(flag);
			if (flag)
			{
				SetTipsText(bIsNoEnergyInfos);
			}
		}

		public void ClickBtnJoin(MButton btn)
		{
			SceneMainBottom.instance.ClickBtnTeam(null);
		}

		private void SetTipsText(bool bIsNoEnergyInfos = true)
		{
			textTips.SetLBaseKey(bIsNoEnergyInfos ? "Language250" : "Language249");
			textTips.SetLBy(Array.Empty<object>());
			textBtnWord.SetLBaseKey(bIsNoEnergyInfos ? "Language210" : "Language163");
			textBtnWord.SetLBy(Array.Empty<object>());
		}

		private void _ClearAllEvent()
		{
			if (Authentication.OwnServerLogin.Instance != null)
			{
				Authentication.OwnServerLogin.Instance.AbortGuestSignIn();
			}
			_EventOperation(true);
			if (loginControl != null)
			{
				loginControl.EventOperation(true);
			}
		}

		private void _EventOperation(bool isOff)
		{
			if (isOff)
			{
				DispatcherMgr.Off<ServerDataRoot<TeamInfo>>("GetEnergyLayer_MyTeam", _OnGetMyTeam);
				DispatcherMgr.Off<ServerDataListRoot<EnergysInfo>>("GetEnergyLayer_GetEnergy", OnGetEnergy);
			}
			else
			{
				DispatcherMgr.On<ServerDataRoot<TeamInfo>>("GetEnergyLayer_MyTeam", _OnGetMyTeam);
				DispatcherMgr.On<ServerDataListRoot<EnergysInfo>>("GetEnergyLayer_GetEnergy", OnGetEnergy);
			}
		}

		private void OnDestroy()
		{
			_EventOperation(true);
			DispatcherMgr.Off("Change_Module_Event", _ClearAllEvent);
		}
	}
}
