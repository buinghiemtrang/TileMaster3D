using System;
using UnityEngine;

public class NetworkInspectionManager : Singleton<NetworkInspectionManager>
{
	private bool _isOpenInspection;

	private bool _isDisableGames;

	private int _unlockLevel;

	private long _lastTime;

	public void InitData()
	{
		_isOpenInspection = network_inspection_const.NetworkInspectionSwitch == 1;
		_unlockLevel = network_inspection_const.NetworkInspectionUnlockLv;
		_isDisableGames = network_inspection_const.DisableGameSwitch == 1;
	}

	public bool CheckNetwork(Action haveNetCallBack)
	{
		Debug.Log("CheckNetwork ");
		if (!_isOpenInspection)
		{
			Debug.Log(string.Format("CheckNetwork is not open _isOpenInspection:{0}, _unlockLevel:{1}, level:{2}", false, _unlockLevel, DB.User.Maxlv + 1));
		}
		else
		{
			if (DB.User.Maxlv < _unlockLevel)
			{
				Debug.Log(string.Format("CheckNetwork is not open _isOpenInspection:{0}, _unlockLevel:{1}, level:{2}", _isOpenInspection, _unlockLevel, DB.User.Maxlv + 1));
				haveNetCallBack?.Invoke();
				return true;
			}
			if (!_isDisableGames)
			{
				if (DB.Other.PopNetworkBoxCount >= network_inspection_const.DailyFrequency)
				{
					Debug.Log(string.Format("CheckNetwork PopNetworkBoxCount >= DailyFrequency,{0}:{1}", DB.Other.PopNetworkBoxCount, network_inspection_const.DailyFrequency));
					haveNetCallBack?.Invoke();
					return true;
				}
			}
			long num = DateTime.Now.TotalSeconds();
			if (_isDisableGames || num - _lastTime >= network_inspection_const.PopupIntervalTime)
			{
				if (!IsHaveNetwork())
				{
					_lastTime = num;
					DB.Other.PopNetworkBoxCount++;
					NetWorkTipBox.Create(_isDisableGames, delegate
					{
						haveNetCallBack?.Invoke();
						_lastTime = DateTime.Now.TotalSeconds();
					});
					return false;
				}
			}
			else
			{
				Debug.Log(string.Format("CheckNetwork nowTime - _lastTime < PopupIntervalTime,{0}:{1}", num - _lastTime, network_inspection_const.PopupIntervalTime));
			}
		}
		haveNetCallBack?.Invoke();
		return true;
	}

	public bool IsHaveNetwork()
	{
		return Application.internetReachability != NetworkReachability.NotReachable;
	}
}
