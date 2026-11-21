using System;
using Authentication;

public class GuestLoginManager : Singleton<GuestLoginManager>
{
	private Action<GuestLoginState> _onLoginCallback;

	public GuestLoginManager()
	{
		EventOperation(false);
	}

	public void Login(Action<GuestLoginState> callback)
	{
		_onLoginCallback = callback;
		if (DataManager.I.bLogined)
		{
			if (_onLoginCallback != null)
			{
				_onLoginCallback(GuestLoginState.Success);
			}
		}
		else
		{
			OwnServerLogin.Instance.Login(PlatformType.Guest, DataCenterLoginMgr.I.LoginFinish);
		}
	}

	private void _Login(GuestLoginState state)
	{
		if (_onLoginCallback != null)
		{
			_onLoginCallback(state);
		}
	}

	private void EventOperation(bool isOff)
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
