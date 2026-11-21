using System;
using System.Collections.Generic;
using Authentication;
using BestHTTP;
using DataCenter;
using UnityEngine;

public class DataCenterLoginMgr
{
	private static DataCenterLoginMgr _instance;

	public Data pAuth;

	public bool bInitEnd;

	public Action onLoginAndGetDataSuccess;

	private HTTPRequest signReq;

	public static DataCenterLoginMgr I => _instance;

	static DataCenterLoginMgr()
	{
		_instance = new DataCenterLoginMgr();
	}

	public void Init()
	{
		signReq = null;
		DataManager.I.LoadLocalData();
		if (!string.IsNullOrEmpty(DB.Account.Token))
		{
			DataManager.I.Logined(DB.Account.Token, DB.Account.Userid);
		}
		if (DB.User.Maxlv != 0)
		{
			FuncEnd();
			return;
		}
		if (DataManager.I.bLogined)
		{
			DataManager.I.HttpGet(delegate(Dictionary<string, object> data, string msg)
			{
				if (data != null && data.Count > 0)
				{
					DataManager.I.LoadRemoteData(data);
				}
				FuncEnd();
			});
			return;
		}
		FuncEnd();

		void FuncEnd()
		{
			bInitEnd = true;
			if (DataManager.I.bLogined)
			{
				foreach (IAssembleDB value in DataManager.I.DictCls.Values)
				{
					value.DequeueEnabled();
				}
			}
		}
	}

	public void LoginFinish(PlatformType paltform, int code, ServerInfo info)
	{
		if (code == -1)
		{
			pAuth = new Data();
			pAuth.user = new Authentication.User();
			pAuth.token = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjlBNUZBMDQ2OTY1MUZDNkZFN0RBMDNFQkZFM0I1NEU0IiwidHlwIjoiYXQrand0In0.eyJuYmYiOjE2MTk2MDU1OTQsImV4cCI6MTkzNDk2NTU5NCwiaXNzIjoiaHR0cDovL3NlcnZpY2UuZnVxaWFuZ2FtZS5jb20iLCJhdWQiOlsiYWxsIiwiaHR0cDovL3NlcnZpY2UuZnVxaWFuZ2FtZS5jb20vcmVzb3VyY2VzIl0sImNsaWVudF9pZCI6IlRpbGVNYXN0ZXIzRCIsInN1YiI6IjYwODkzODVhNzcyMmM4Y2M2NDg5NWIyMCIsImF1dGhfdGltZSI6MTYxOTYwNTU5NCwiaWRwIjoiZmFjZWJvb2siLCJlbWFpbCI6InpoYW5nenkxMzVAcXEuY29tIiwibmFtZSI6IuW8oOiHtOi_nCIsInBpY3R1cmUiOiJodHRwczovL3BsYXRmb3JtLWxvb2thc2lkZS5mYnNieC5jb20vcGxhdGZvcm0vcHJvZmlsZXBpYy8_YXNpZD0zNTA2NjgxMDI4MzMxNzgmaGVpZ2h0PTUwJndpZHRoPTUwJmV4dD0xNjIyMTk3NTk0Jmhhc2g9QWVUVkxrYlBxdlVhVlk3WlZaTSIsImp0aSI6IjlEMDk0MDhBQzQyNEQyNUFCNEY1QTBCRkI2ODJBRDk5IiwiaWF0IjoxNjE5NjA1NTk0LCJzY29wZSI6WyJhbGwuZnVsbF9hY2Nlc3MiXSwiYW1yIjpbIlRva2VuRXhjaGFuZ2UiXX0.gxLIRKxN8UTtKcUIczq-njkyDFMT3dj5BRbNRLFLcl4QhYm3F_tKtAcnFwofNieSYiY2N07PmHjVEUZ0tYGta4aJvCb1HOWnHOZELX0TZhWZbAbSrCQcYGpaAOTVjXDse8Gx3B4G_yJ9jmcjr2BhSWPQnbjMeBnzzFL54OFf8x_wiwJgPVrb1BrQAD5dFm3b3JaclaFEP6g7p6M6UA_1_MyrVWJasZxzl89Mb0iSU6g-98jlsN8jowBplS_w6PgxgAsuvdr-miV_6B2pAzFSyqiJ3xyuaKmyPXMRbtVUTrHXxy576QoDp2XQuZyoMGri5fR28e4wKx4MI6WCX79h6Q";
			pAuth.user.UserId = "6089385a7722c8cc64895b20";
			pAuth.user.DisplayName = paltform.ToString() + "_Test";
			SignIn(paltform.ToString().ToLower());
		}
		else
		{
			pAuth = null;
			if (code != 1)
			{
				SceneMgr.CloseWaiting();
				if (paltform != PlatformType.None && paltform != PlatformType.Abort)
				{
					if (paltform == PlatformType.Guest)
					{
						DispatcherMgr.Emit("Guest_Login", GuestLoginState.Fail);
					}
					else
					{
						string text = paltform.ToString();
						string errorMessage = (info == null) ? string.Format("code:{0},info is null", code) : string.Format("LoginFinish code:{0}.{1}", info.code, info.msg);
						LogEventLogin.LogInError(text, errorMessage);
						Global.CreateMsgBoxDialog("Language124", null, false, "");
					}
				}
				Logout();
				return;
			}
			pAuth = info.data;
			SignIn(paltform.ToString().ToLower());
		}
	}

	public void AbortSignIn()
	{
		if (signReq != null)
		{
			signReq.Abort();
			signReq.Tag = "AbortRequest";
		}
	}

	public void SignIn(string platform)
	{
		DB.Account.LoginPlatform = platform;
		if (!string.IsNullOrEmpty(pAuth.token))
		{
			DataManager.I.Logined(pAuth.token, pAuth.user.UserId);
			signReq = DataManager.I.HttpGet(delegate(Dictionary<string, object> data, string msg)
			{
				bool flag = signReq == null || (signReq.Tag as string != "AbortRequest");
				signReq = null;
				if (data == null)
				{
					Logout();
					SignOut(false);
					SceneMgr.CloseWaiting();
					if (flag)
					{
						_LoginFail(false);
					}
					LogEventLogin.LogInError(platform, "data is null, " + msg);
				}
				else
				{
					Debug.LogError("SignIn data ");
					foreach (KeyValuePair<string, object> item in data)
					{
						Debug.LogError("key: " + item.Key + " value: " + item.Value);
					}
					DispatcherMgr.Emit("LoginFinishReadData", data);
					if (onLoginAndGetDataSuccess != null)
					{
						onLoginAndGetDataSuccess();
					}
					LogEvent.I.Track("LogIn_Finish").Add("LogInType", platform).CommonAttr(true).Send(false);
				}
			});
			return;
		}
		SceneMgr.CloseWaiting();
		_LoginFail(false);
	}

	private void _LoginFail(bool isJudge = true)
	{
		if (GetPlatformID() == -1)
		{
			DispatcherMgr.Emit("Guest_Login", GuestLoginState.Fail);
		}
		else
		{
			Global.CreateMsgBoxDialog("Language124", null, false, "");
		}
	}

	public void Logout()
	{
		OwnServerLogin.Instance.Logout(GetPlatformID());
	}

	public void SignOut(bool clear = true)
	{
		DB.Account.Token = string.Empty;
		pAuth = null;
		DataManager.I.Logout();
		LogEvent.I.Track("LogIn_Out").Add("LogInType", DB.Account.LoginPlatform).CommonAttr(true).Send(false);
		DB.Account.LoginPlatform = "";
		if (clear)
		{
			DataManager.I.InitLocalDataAll();
			DataManager.I.DelLocalDataAll();
			UpdateDataAll();
		}
	}

	public void UpdateDataAll()
	{
		AudioMgr.I.Init();
		DispatcherMgr.Emit("Update_UserData");
		DispatcherMgr.Emit("Update_Account");
		DispatcherMgr.Emit("Update_User_Name");
	}

	public int GetPlatformID(Account account = null)
	{
		if (account == null)
		{
			account = DB.Account;
		}
		string loginPlatform = account.LoginPlatform;
		if (loginPlatform.Equals("facebook"))
		{
			return 0;
		}
		if (loginPlatform.Equals("google"))
		{
			return 1;
		}
		if (loginPlatform.Equals("apple"))
		{
			return 2;
		}
		return -1;
	}
}
