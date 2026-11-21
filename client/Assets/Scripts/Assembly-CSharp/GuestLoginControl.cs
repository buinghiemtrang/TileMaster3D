using System;
using Authentication;
using UnityEngine;

public class GuestLoginControl : MonoBehaviour
{
	public Action<GuestLoginState> onLogin;

	private void Awake()
	{
		EventOperation(false);
	}

	public void Login()
	{
		if (DataManager.I.bLogined)
		{
			if (onLogin != null)
			{
				onLogin(GuestLoginState.Success);
			}
		}
		else
		{
			OwnServerLogin.Instance.Login(PlatformType.Guest, DataCenterLoginMgr.I.LoginFinish);
		}
	}

	private void _Login(GuestLoginState state)
	{
		if (onLogin != null)
		{
			onLogin(state);
		}
	}

	public void EventOperation(bool isOff)
	{
		if (isOff)
		{
			DispatcherMgr.Off<GuestLoginState>("Guest_Login", _Login);
		}
		else
		{
			DispatcherMgr.On<GuestLoginState>("Guest_Login", _Login);
		}
	}

	private void OnDestroy()
	{
		EventOperation(true);
	}
}
