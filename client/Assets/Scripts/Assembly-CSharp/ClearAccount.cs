using System;
using BestHTTP;
using Newtonsoft.Json;
using UnityEngine;

public class ClearAccount : HttpMgrBase
{
	[SerializeField]
	private MButton _btnClearAccount;

	[SerializeField]
	private DialogShow _pDialog;

	[SerializeField]
	private GameObject smallButtonsObj;

	[SerializeField]
	private GameObject contactUsButtonObj;

	private const string DELETE_DATA = "ClearAccountData_DELETE";

	private void Start()
	{
		_btnClearAccount.onClick.AddListener(ClickClearAccount);
		smallButtonsObj.SetActive(true);
		contactUsButtonObj.SetActive(false);
	}

	private void ClickClearAccount(MButton button)
	{
		LogEvent.I.Track("AccountDate_Clear")
			.Add("Platform", DB.Account.LoginPlatform)
			.Add("Steps", 1)
			.Level(true)
			.Send(false);
		AccountCancellationAgreementDialog.Create(delegate(bool agree)
		{
			if (agree)
			{
				MakeSureBox makeSureBox = SceneMgr.AddTopNode<MakeSureBox>("Prefabs/MakeSureBox");
				LogEvent.I.Track("AccountDate_Clear")
					.Add("Platform", DB.Account.LoginPlatform)
					.Add("Steps", 2)
					.Level(true)
					.Send(false);
				makeSureBox.InitData("Language1034", delegate
				{
					makeSureBox._Close();
				}, delegate
				{
					makeSureBox._Close();
					DoClearAccount();
				}, "Language1033");
			}
		});
	}

	private void DoClearAccount()
	{
		MakeSureBox makeSureBox = SceneMgr.AddTopNode<MakeSureBox>("Prefabs/MakeSureBox").GetComponent<MakeSureBox>();
		makeSureBox.InitData("Language433", delegate
		{
			makeSureBox._Close();
		}, delegate
		{
			LogEvent.I.Track("AccountDate_Clear")
				.Add("Platform", DB.Account.LoginPlatform)
				.Add("Steps", 3)
				.Level(true)
				.Send(false);
			if (string.IsNullOrEmpty(DB.Account.Token))
			{
				makeSureBox._Close();
				ClearAccountData();
			}
			else
			{
				HttpRequest(delegate
				{
					makeSureBox._Close();
				});
			}
		});
	}

	private void HttpRequest(Action callBack)
	{
		SceneMgr.OpenWaiting(10f, -1f);
		headers.Clear();
		headers.Add("Authorization", "Bearer " + DB.Account.Token);
		headers.Add("Content-Type", "application/json");
		string text = "";
		if (DataCenterLoginMgr.I.GetPlatformID() != -1)
		{
			text = SystemInfo.deviceUniqueIdentifier;
		}
		string body = JsonConvert.SerializeObject(new
		{
			GuestDeviceid = text
		});
		OwnHttpMgr.I.SendDelete("https://service.fuqiangame.com/data", headers, body, delegate(HTTPResponse resp)
		{
			Debug.LogError(" ClearAccountData resp :" + resp);
			ServerDataRootBase serverDataRootBase = SendInfo("ClearAccountData_DELETE", resp);
			SceneMgr.CloseWaiting();
			if (serverDataRootBase.code == 1)
			{
				callBack?.Invoke();
				ClearAccountData();
			}
			else
			{
				Debug.LogError(" ClearAccountData Error :" + serverDataRootBase.msg);
				Global.CreateMsgBoxDialog("Language55", null, false, "");
			}
		}, "ClearAccountData_DELETE");
	}

	private void ClearAccountData()
	{
		DataCenterLoginMgr.I.Logout();
		DB.Account.Token = string.Empty;
		DataCenterLoginMgr.I.pAuth = null;
		DataManager.I.Logout();
		DB.Account.LoginPlatform = "";
		DataManager.I.InitLocalDataAll();
		DataManager.I.WriteLocalDataAll();
		GiftDataMgr.ins.CleanCache();
		TweenMgr.DelayCall(0.5f, delegate
		{
			_pDialog._Close();
			SceneMgr.LoadScene("LoadingScene");
			SceneMgr.SetData("ClearAccount");
		});
	}
}
