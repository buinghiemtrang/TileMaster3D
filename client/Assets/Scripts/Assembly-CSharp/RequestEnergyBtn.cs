using System;
using DG.Tweening;
using GameMail;
using GameTeam;
using UnityEngine;
using UnityEngine.UI;

public class RequestEnergyBtn : MonoBehaviour
{
	[SerializeField]
	private MButton askBtn;

	[SerializeField]
	private GameObject canAskObj;

	[SerializeField]
	private GameObject noCanAskObj;

	[SerializeField]
	private Text timeTex;

	public Action onAskEnergySuccess;

	private Tween requestingTw;

	private Tween energyTw;

	private Tween countdownTw;

	private bool isJumpReceiveEnergyPage;

	private void Awake()
	{
		askBtn.onClick.AddListener(_ClickAsk);
		DispatcherMgr.On<ServerDataRoot<ApplyEnergyInfo>>("TeamHttp_GetLastRequestingEnergy", _OnGetLastRequestingEnergy);
		DispatcherMgr.On<ServerDataListRoot<EnergysInfo>>("TeamHttp_GetEnergy", _OnGetEnergy);
		DispatcherMgr.On<ServerDataRootBase>("TeamHttp_AskEnergy", _OnAskEnergy);
		DispatcherMgr.On("MailsDialog_Close", Init);
	}

	public void Init()
	{
		_ShowBtn(isInit: true, isCanClick: false);
		isJumpReceiveEnergyPage = false;
		TeamHttpMgr.I.GetLastRequestingEnergy();
	}

	private void _ClickAsk(MButton btn)
	{
		if (!isJumpReceiveEnergyPage)
		{
			TeamMaskCtl.I.Show(1f, isNeedMask: true);
			TeamHttpMgr.I.AskEnergy();
		}
		else
		{
			SceneMgr.AddTopNode<MailsDialog>("Prefabs/Mail/MailsDialog").Show("Click");
		}
	}

	private void _OnGetLastRequestingEnergy(ServerDataRoot<ApplyEnergyInfo> root)
	{
		if (root.code == 1)
		{
			if (root.data != null)
			{
				double totalSeconds = new TimeSpan(root.data.endTimeSpan).TotalSeconds;
				if (0.0 <= totalSeconds)
				{
					int num = Convert.ToInt32(totalSeconds);
					if (num != 0)
					{
						_ShowBtn(isInit: false, isCanClick: false);
						_ShowCountdown(num);
						return;
					}
				}
			}
			TeamHttpMgr.I.GetEnergy("TeamHttp_GetEnergy");
		}
		else
		{
			requestingTw = TweenMgr.DelayCall(2f, delegate
			{
				TeamHttpMgr.I.GetLastRequestingEnergy();
			}).SetId("RequestEnergyBtn_Requesting");
		}
	}

	private void _OnGetEnergy(ServerDataListRoot<EnergysInfo> root)
	{
		if (root.code == 1)
		{
			_ShowBtn(isInit: false, isCanClick: true);
			if (root.data != null && root.data.Count != 0 && root.data[0].Lives != null)
			{
				isJumpReceiveEnergyPage = 0 < root.data[0].Lives.Count;
				MailMgr.I.EnergyMailMgr.AnalysisEnergy(root.data[0].Lives);
			}
		}
		else
		{
			energyTw = TweenMgr.DelayCall(2f, delegate
			{
				TeamHttpMgr.I.GetEnergy("TeamHttp_GetEnergy");
			}).SetId("RequestEnergyBtn_Energy");
		}
	}

	private void _OnAskEnergy(ServerDataRootBase root)
	{
		TeamMaskCtl.I.Hide();
		if (root.code == 1)
		{
			LogEvent.I.TeamLifeRequest(TeamDataMgr.I.MyTeamInfo.id);
			_ShowBtn(isInit: true, isCanClick: false);
			TeamHttpMgr.I.GetLastRequestingEnergy();
			if (onAskEnergySuccess != null)
			{
				onAskEnergySuccess();
			}
		}
		else
		{
			Global.ShowMsgTip((root.code == -1) ? "Language55" : "Language243");
		}
	}

	private void _ShowCountdown(int second)
	{
		TimeSpan ts;
		countdownTw = TweenMgr.NumberTo(second, second, 0, delegate(int va)
		{
			if ((bool)timeTex)
			{
				ts = new TimeSpan(0, 0, va);
				timeTex.text = string.Format("{0:D2}:{1:D2}:{2:D2}", ts.Hours + ts.Days * 24, ts.Minutes, ts.Seconds);
			}
		}).SetEase(Ease.Linear).OnComplete(delegate
		{
			TeamHttpMgr.I.GetLastRequestingEnergy();
		});
	}

	private void _ShowBtn(bool isInit, bool isCanClick)
	{
		askBtn.interactable = isCanClick;
		canAskObj.SetActive(isInit || isCanClick);
		noCanAskObj.SetActive(!(isInit || isCanClick));
	}

	private void OnDestroy()
	{
		if (requestingTw != null)
		{
			requestingTw.Kill();
			requestingTw = null;
		}
		if (energyTw != null)
		{
			energyTw.Kill();
			energyTw = null;
		}
		if (countdownTw != null)
		{
			countdownTw.Kill();
			countdownTw = null;
		}
		DispatcherMgr.Off<ServerDataRoot<ApplyEnergyInfo>>("TeamHttp_GetLastRequestingEnergy", _OnGetLastRequestingEnergy);
		DispatcherMgr.Off<ServerDataListRoot<EnergysInfo>>("TeamHttp_GetEnergy", _OnGetEnergy);
		DispatcherMgr.Off<ServerDataRootBase>("TeamHttp_AskEnergy", _OnAskEnergy);
		DispatcherMgr.Off("MailsDialog_Close", Init);
	}
}
