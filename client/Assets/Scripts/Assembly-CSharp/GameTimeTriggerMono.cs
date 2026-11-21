using System;
using System.Collections.Generic;
using UnityEngine;

public class GameTimeTriggerMono : MonoBehaviour
{
	private static GameTimeTriggerMono _instance;

	private static Dictionary<string, TimeTriggerOne> _stream = new Dictionary<string, TimeTriggerOne>();

	private int _sign;

	public static GameTimeTriggerMono I
	{
		get
		{
			if (_instance == null)
			{
				_instance = new GameObject("TimeTriggerMono").AddComponent<GameTimeTriggerMono>();
			}
			return _instance;
		}
	}

	private void Awake()
	{
		DispatcherMgr.OnG<bool, string>("GamePause", OnSetGamePause);
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	public TimeTriggerOne Create(string key = "")
	{
		if (string.IsNullOrEmpty(key))
		{
			_sign++;
			key = _sign.ToString();
		}
		TimeTriggerOne value;
		if (!_stream.ContainsKey(key))
		{
			value = new TimeTriggerOne();
			value.key = key;
			_stream.Add(key, value);
		}
		else
		{
			value = _stream[key];
		}
		value.Init();
		return value;
	}

	public void Clear(string key = "")
	{
		if (string.IsNullOrEmpty(key))
		{
			key = _sign.ToString();
		}
		if (_stream.ContainsKey(key))
		{
			_stream.Remove(key);
		}
	}

	public void Clear(LinearTriggerOne one)
	{
		Clear(one.key);
	}

	public int GetTime(string key = "")
	{
		if (string.IsNullOrEmpty(key))
		{
			key = _sign.ToString();
		}
		if (!_stream.ContainsKey(key))
		{
			return 0;
		}
		return _stream[key].nTime;
	}

	public int EndTime(string key = "")
	{
		if (string.IsNullOrEmpty(key))
		{
			key = _sign.ToString();
		}
		if (!_stream.ContainsKey(key))
		{
			return 0;
		}
		TimeTriggerOne timeTriggerOne = _stream[key];
		timeTriggerOne.Update(DateTime.Now);
		timeTriggerOne.bEnd = true;
		return timeTriggerOne.nTime;
	}

	public void UpdateGameTime()
	{
		DateTime now = DateTime.Now;
		foreach (KeyValuePair<string, TimeTriggerOne> item in _stream)
		{
			item.Value.Update(now);
		}
	}

	public void StartGameTime()
	{
		DateTime now = DateTime.Now;
		foreach (KeyValuePair<string, TimeTriggerOne> item in _stream)
		{
			item.Value.dateTime = now;
		}
	}

	private void OnSetGamePause(bool bPause, string param)
	{
		if (!bPause)
		{
			StartGameTime();
		}
		else
		{
			UpdateGameTime();
		}
	}

	private void OnApplicationFocus(bool focusStatus)
	{
		if (!focusStatus)
		{
			UpdateGameTime();
		}
		else
		{
			StartGameTime();
		}
	}
}
