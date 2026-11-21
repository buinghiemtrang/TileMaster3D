using System;
using System.Collections.Generic;
using UnityEngine;

public class GTimer : GSingleton<GTimer>
{
	public delegate void tickAction();

	private Dictionary<tickAction, UnityEngine.Object> _conditionObjDic;

	private List<tickAction> _updateList;

	private List<tickAction> _delayList;

	private List<float> _delayTimeList;

	private List<tickAction> _tickList;

	private List<float> _tickTimeList;

	private List<float> _tickTimeDeltaList;

	private List<int> _tickCountList;

	public Action<int> onPauseResume;

	public override void Setup()
	{
		GUtils.GetDontDestroyObjectWithMono<GTimerMono>();
		_conditionObjDic = new Dictionary<tickAction, UnityEngine.Object>();
		_updateList = new List<tickAction>();
		_delayList = new List<tickAction>();
		_delayTimeList = new List<float>();
		_tickList = new List<tickAction>();
		_tickTimeList = new List<float>();
		_tickTimeDeltaList = new List<float>();
		_tickCountList = new List<int>();
	}

	public void clear()
	{
		_updateList.Clear();
		_delayList.Clear();
		_delayTimeList.Clear();
		_tickList.Clear();
		_tickTimeList.Clear();
		_tickTimeDeltaList.Clear();
		_tickCountList.Clear();
	}

	public void update()
	{
		for (int num = _updateList.Count - 1; num >= 0; num--)
		{
			tickAction tickAction = _updateList[num];
			if (canExecute(tickAction))
			{
				tickAction();
			}
			else
			{
				removeUpdateByIndex(num, tickAction);
			}
		}
		float deltaTime = Time.deltaTime;
		for (int num2 = _delayTimeList.Count - 1; num2 >= 0; num2--)
		{
			_delayTimeList[num2] -= deltaTime;
			if (_delayTimeList[num2] <= 0f)
			{
				tickAction tickAction2 = _delayList[num2];
				if (canExecute(tickAction2))
				{
					tickAction2();
				}
				removeDeleyByIndex(num2, tickAction2);
			}
		}
		for (int num3 = _tickTimeList.Count - 1; num3 >= 0; num3--)
		{
			_tickTimeList[num3] -= deltaTime;
			if (!(_tickTimeList[num3] <= 0f))
			{
				continue;
			}
			tickAction tickAction3 = _tickList[num3];
			if (_tickCountList[num3] == -1)
			{
				_tickTimeList[num3] = _tickTimeDeltaList[num3];
				if (!canExecute(tickAction3))
				{
					removeTickByIndex(num3, tickAction3);
					continue;
				}
			}
			else
			{
				_tickCountList[num3] -= 1;
				if (_tickCountList[num3] > 0)
				{
					_tickTimeList[num3] = _tickTimeDeltaList[num3];
					if (!canExecute(tickAction3))
					{
						removeTickByIndex(num3, tickAction3);
						continue;
					}
				}
				else
				{
					removeTickByIndex(num3, tickAction3);
					if (!canExecute(tickAction3))
					{
						continue;
					}
				}
			}
			tickAction3();
		}
	}

	private bool canExecute(tickAction willExecuteTick)
	{
		bool flag = true;
		if (willExecuteTick.Target != null && willExecuteTick.Target.ToString().Equals("null"))
		{
			Debug.LogError("GTimer: willExecuteTick is 'null', please check logic, is need function (addDelayForceExecute)?");
			flag = false;
		}
		return flag && !isNullConditionObj(willExecuteTick);
	}

	private bool isNullConditionObj(tickAction willExecuteTick)
	{
		if (_conditionObjDic.ContainsKey(willExecuteTick))
		{
			return _conditionObjDic[willExecuteTick] == null;
		}
		return false;
	}

	private void removeUpdateByIndex(int index, tickAction willExecuteTick)
	{
		if (willExecuteTick == null)
		{
			return;
		}
		_updateList.RemoveAt(index);
		removeConditionObj(willExecuteTick);
	}

	private void removeDeleyByIndex(int index, tickAction willExecuteTick)
	{
		if (willExecuteTick == null)
		{
			return;
		}
		_delayList.RemoveAt(index);
		_delayTimeList.RemoveAt(index);
		removeConditionObj(willExecuteTick);
	}

	private void removeTickByIndex(int index, tickAction willExecuteTick)
	{
		if (willExecuteTick == null)
		{
			return;
		}
		_tickList.RemoveAt(index);
		_tickTimeList.RemoveAt(index);
		_tickTimeDeltaList.RemoveAt(index);
		_tickCountList.RemoveAt(index);
		removeConditionObj(willExecuteTick);
	}

	private void removeConditionObj(tickAction willExecuteTick)
	{
		if (willExecuteTick != null && _conditionObjDic.ContainsKey(willExecuteTick))
		{
			_conditionObjDic.Remove(willExecuteTick);
		}
	}

	public void addUpdate(tickAction tick, UnityEngine.Object conditionObj = null)
	{
		if (tick != null)
		{
			if (_updateList.IndexOf(tick) == -1)
			{
				_updateList.Insert(0, tick);
			}
			if (conditionObj != null)
			{
				_conditionObjDic[tick] = conditionObj;
			}
		}
	}

	public void removeUpdate(tickAction tick)
	{
		if (tick != null)
		{
			int num = _updateList.IndexOf(tick);
			if (num != -1)
			{
				removeUpdateByIndex(num, tick);
			}
		}
	}

	public void addDelayForceExecute(tickAction tick, float tickTime)
	{
		tickAction tick2 = delegate
		{
			if (tick != null)
			{
				tick();
			}
		};
		addDelay(tick2, tickTime);
	}

	public void addDelay(tickAction tick, float tickTime, UnityEngine.Object conditionObj = null)
	{
		if (tick != null && tickTime > 0f)
		{
			int num = _delayList.IndexOf(tick);
			if (num == -1)
			{
				_delayList.Insert(0, tick);
				_delayTimeList.Insert(0, tickTime);
			}
			else
			{
				_delayTimeList[num] = tickTime;
			}
			if (conditionObj != null)
			{
				_conditionObjDic[tick] = conditionObj;
			}
		}
	}

	public void removeDelay(tickAction tick)
	{
		if (tick != null)
		{
			int num = _delayList.IndexOf(tick);
			if (num != -1)
			{
				removeDeleyByIndex(num, tick);
			}
		}
	}

	public void addTick(tickAction tick, float tickTime, int count = -1, UnityEngine.Object conditionObj = null)
	{
		if (tick != null && tickTime > 0f)
		{
			int num = _tickList.IndexOf(tick);
			if (num == -1)
			{
				_tickList.Insert(0, tick);
				_tickTimeList.Insert(0, tickTime);
				_tickTimeDeltaList.Insert(0, tickTime);
				_tickCountList.Insert(0, count);
			}
			else
			{
				_tickTimeList[num] = tickTime;
				_tickTimeDeltaList[num] = tickTime;
				_tickCountList[num] = count;
			}
			if (conditionObj != null)
			{
				_conditionObjDic[tick] = conditionObj;
			}
		}
	}

	public void removeTick(tickAction tick)
	{
		if (tick != null)
		{
			int num = _tickList.IndexOf(tick);
			if (num != -1)
			{
				removeTickByIndex(num, tick);
			}
		}
	}
}
