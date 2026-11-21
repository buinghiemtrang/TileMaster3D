using System;

public class LoginMgr : Singleton<LoginMgr>
{
	public void Cost()
	{
		if (SceneMgr.sCurSceneName.Equals("GameScene"))
		{
			DB.Login.IsCostInGameScene = true;
		}
		else if (SceneMgr.sCurSceneName.Equals("MainScene"))
		{
			ShowLoginGuide();
		}
	}

	public void SetNotRemindAgainLoginState(bool isNotRemindAgain)
	{
		DB.Login.IsNotRemindAgainLogin = isNotRemindAgain;
	}

	public bool ShowLoginGuide()
	{
		DB.Login.IsCostInGameScene = false;
		if (DB.Login.IsNotRemindAgainLogin || !_JudgeCanGuideLogin())
		{
			return false;
		}
		int platformID = DataCenterLoginMgr.I.GetPlatformID();
		if (DataManager.I.bLogined && platformID != -1)
		{
			return false;
		}
		DB.Login.ShowNumber++;
		SceneMgr.AddTopNode<DialogShow>("Prefabs/Login/LoginDialog");
		return true;
	}

	private bool _JudgeCanGuideLogin()
	{
		DateTime today = DateTime.Today;
		string lastRefreshTime = DB.Login.LastRefreshTime;
		if (!string.IsNullOrEmpty(lastRefreshTime))
		{
			DateTime d = today.ParseFull(lastRefreshTime);
			if ((today - d).Days <= 0)
			{
				return DB.Login.ShowNumber < cf_const.DailyLoginGuidePopTimes;
			}
		}
		DB.Login.LastRefreshTime = today.ToStringFull();
		DB.Login.ShowNumber = 0;
		return true;
	}
}
