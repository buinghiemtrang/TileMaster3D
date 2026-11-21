using System;
using BestHTTP;
using DG.Tweening;
using OwnHttp;
using UnityEngine;

public class ServerDateTimeMgr : MonoBehaviour
{
	private const string RealTime = "GetRealTime";

	private float _startTime;

	private DateTime _serverTimeUtc;

	private DateTime _serverTime;

	private static ServerDateTimeMgr _instance;

	public DateTime NetTime => _serverTime.AddSeconds(Time.realtimeSinceStartup - _startTime);

	public DateTime NetTimeUtc => _serverTimeUtc.AddSeconds(Time.realtimeSinceStartup - _startTime);

	public bool IsServerTimeSynced { get; private set; }

	public static ServerDateTimeMgr I
	{
		get
		{
			if (!_instance)
			{
				GameObject gameObject = new GameObject("ServerDateTimeMgr");
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				_instance = gameObject.AddComponent<ServerDateTimeMgr>();
				_instance._startTime = Time.realtimeSinceStartup;
				_instance._serverTimeUtc = DateTime.UtcNow;
				_instance._serverTime = DateTime.Now;
			}
			return _instance;
		}
	}

	public void RequestRealTime(Action callbackSuccess = null)
	{
		DOTweenModuleExtend.InvokeUntilSuccess(delegate(TweenCallback func)
		{
			CheckRepeatRequestsMgr.I.Creat("GetRealTime", HTTPMethods.Get, "https://service.fuqiangame.com/common/time?format=yyyyMMddHHmmss", delegate(ResultInfo info)
			{
				OwnHttp.ServerDataRoot<string> serverDataRoot = ParsingServerData.SendInfo<string>(info);
				if (serverDataRoot != null && serverDataRoot.code == 1 && serverDataRoot.Data != null)
				{
					_serverTimeUtc = DateTime.UtcNow.ParseFull(serverDataRoot.Data, true);
					_serverTime = _serverTimeUtc.AddSeconds(TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds);
					_startTime = Time.realtimeSinceStartup;
					IsServerTimeSynced = true;
					Global.pStores.storeMultipleCtrl.JudgeIsNewActivity();
					if (callbackSuccess != null)
					{
						callbackSuccess();
					}
				}
				else if (func != null)
				{
					func();
				}
			}).AddHeader("CommonServiceToken", "9b5ba0c0419840ccab872275f41e5cc7").ConnectTimeout(30.0).Send();
		}, 10f, -1);
	}
}
