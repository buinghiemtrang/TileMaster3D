using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

public class GTimeCycle : GSingleton<GTimeCycle>
{
	private class Energy
	{
		public int nextTime;

		public int delta;
	}

	private class GTimeCycleData
	{
		public int daysNewTime = DateTime.Now.DayOfYear;

		public Dictionary<string, bool> stateOnce = new Dictionary<string, bool>();

		public Dictionary<string, bool> stateEveryday = new Dictionary<string, bool>();

		public Dictionary<string, int> timerOnce = new Dictionary<string, int>();

		public Dictionary<string, Energy> timerEnergy = new Dictionary<string, Energy>();
	}

	private static readonly string KEY = "KEY_GTimeCycle";

	public Action onNewDay;

	public int secondsToNextDay;

	public bool isNewDay;

	private GTimeCycleData data;

	private StringBuilder formatTimeSB;

	private DateTime newDayTime;

	public override void Setup()
	{
		read();
		formatTimeSB = new StringBuilder();
		checkNewDay();
		timerRefresh();
		nextDayCountDown();
		GSingleton<GTimer>.ins.addTick(update, 1f);
	}

	public bool getEverydayState(string key)
	{
		key = formatKey(key);
		if (!data.stateEveryday.ContainsKey(key))
		{
			data.stateEveryday[key] = true;
			save();
			return true;
		}
		return data.stateEveryday[key];
	}

	public void setEverydayState(string key)
	{
		key = formatKey(key);
		data.stateEveryday[key] = false;
		save();
	}

	public bool getOnceState(string key)
	{
		key = formatKey(key);
		if (!data.stateOnce.ContainsKey(key))
		{
			data.stateOnce[key] = true;
			save();
			return true;
		}
		return data.stateOnce[key];
	}

	public void setOnceState(string key, bool value = false)
	{
		key = formatKey(key);
		data.stateOnce[key] = value;
		save();
	}

	public int getTimer(string key, int maxValue)
	{
		key = formatKey(key);
		int deviceRunTime = SuperTimer.GetDeviceRunTime();
		int num = 0;
		if (data.timerOnce.ContainsKey(key))
		{
			int num2 = data.timerOnce[key];
			if (num2 > deviceRunTime)
			{
				num = num2 - deviceRunTime;
			}
			else
			{
				data.timerOnce.Remove(key);
				save();
			}
		}
		if (maxValue <= num)
		{
			num = maxValue;
		}
		return num;
	}

	public int getTimerEnergy(string key, out int round)
	{
		key = formatKey(key);
		int deviceRunTime = SuperTimer.GetDeviceRunTime();
		round = 0;
		int num = 0;
		if (data.timerEnergy.ContainsKey(key))
		{
			Energy energy = data.timerEnergy[key];
			num = energy.nextTime - deviceRunTime;
			if (num <= 0)
			{
				int num2 = deviceRunTime - energy.nextTime;
				round = num2 / energy.delta + 1;
				num = energy.delta + (num2 / energy.delta * energy.delta - num2);
				energy.nextTime = num + deviceRunTime;
				save();
			}
		}
		Logs.Common.Info(string.Concat(key, " secondsTimer: ", num.ToString(), " round: ", round.ToString()));
		return num;
	}

	public int setTimer(string key, int seconds)
	{
		key = formatKey(key);
		int num = SuperTimer.GetDeviceRunTime() + seconds;
		int num2 = (data.timerOnce.ContainsKey(key) ? data.timerOnce[key] : int.MinValue);
		if (num2 < num)
		{
			data.timerOnce[key] = num;
			save();
		}
		return num;
	}

	public int setTimerEnergy(string key, int seconds)
	{
		key = formatKey(key);
		int num = SuperTimer.GetDeviceRunTime() + seconds;
		Energy energy = (data.timerEnergy.ContainsKey(key) ? data.timerEnergy[key] : new Energy());
		energy.nextTime = num;
		energy.delta = seconds;
		data.timerEnergy[key] = energy;
		save();
		return num;
	}

	public void clearTimer(string key)
	{
		key = formatKey(key);
		if (data.timerOnce.ContainsKey(key))
		{
			data.timerOnce.Remove(key);
			save();
		}
	}

	public void clearTimerEnergy(string key)
	{
		key = formatKey(key);
		if (data.timerEnergy.ContainsKey(key))
		{
			data.timerEnergy.Remove(key);
			save();
		}
	}

	public string GetFormatTime(int second, bool showHour0 = true)
	{
		formatTimeSB.Clear();
		Action<int, bool> append = delegate(int val, bool dotflag)
		{
			if (val <= 0)
			{
				formatTimeSB.Append("00");
			}
			else if (val < 10)
			{
				formatTimeSB.Append("0");
				formatTimeSB.Append(val);
			}
			else
			{
				formatTimeSB.Append(val);
			}
			if (dotflag)
			{
				// UNVERIFIED: separator literal is SL_____45; rank-based reading of the literal name gives "." (same literal as in FQDev.Logger.Logger)
				formatTimeSB.Append(".");
			}
		};
		if (second > 3599 || showHour0)
		{
			append(second / 3600, true);
		}
		append(second % 3600 / 60, true);
		append(second % 3600 % 60, false);
		return formatTimeSB.ToString();
	}

	private void update()
	{
		secondsToNextDay = Convert.ToInt32((newDayTime - DateTime.Now).TotalSeconds);
		if (data.daysNewTime != DateTime.Now.DayOfYear)
		{
			checkNewDay();
			nextDayCountDown();
			onNewDay?.Invoke();
		}
	}

	private void checkNewDay()
	{
		isNewDay = false;
		if (data.daysNewTime != DateTime.Now.DayOfYear)
		{
			isNewDay = true;
			data.daysNewTime = DateTime.Now.DayOfYear;
			data.stateEveryday.Clear();
			save();
		}
	}

	private void nextDayCountDown()
	{
		newDayTime = DateTime.Today.AddDays(1.0);
		secondsToNextDay = Convert.ToInt32((newDayTime - DateTime.Now).TotalSeconds);
	}

	private void timerRefresh()
	{
		int deviceRunTime = SuperTimer.GetDeviceRunTime();
		List<string> list = data.timerOnce.Keys.ToList();
		bool flag = false;
		for (int num = list.Count - 1; num >= 0; num--)
		{
			string key = list[num];
			if (data.timerOnce[key] <= deviceRunTime)
			{
				data.timerOnce.Remove(key);
				flag = true;
			}
		}
		if (flag)
		{
			save();
		}
	}

	private string formatKey(string key)
	{
		return "d" + key;
	}

	private void read()
	{
		string @string = PlayerPrefs.GetString(KEY);
		if (!string.IsNullOrEmpty(@string))
		{
			data = JsonConvert.DeserializeObject<GTimeCycleData>(@string);
		}
		if (data == null)
		{
			data = new GTimeCycleData();
			save();
		}
	}

	private void save()
	{
		PlayerPrefs.SetString(KEY, JsonConvert.SerializeObject(data));
	}
}
