using System;

public abstract class ActivityManagerBase<T> : GSingleton<T> where T : class, ISetup, new()
{
	public Action OnNewActivityStart;

	public Action OnActivityStateEnding;

	public override void Setup()
	{
	}

	public virtual void Refresh()
	{
		RefreshData();
	}

	public virtual ActivityStateBase GetActivityState()
	{
		return GetData().State;
	}

	public virtual ActivityStateBase GetActivityStateWithRefresh()
	{
		return RefreshData();
	}

	protected virtual void ActivityOpenPrepare(int issue)
	{
		ActivityDataBase data = GetData();
		data.Issue = issue;
		data.State = ActivityStateBase.StartPrepare;
		SaveData();
		SetActivityOpen();
	}

	public virtual void SetActivityOpen()
	{
		NewActivityStart(GetData().Issue);
		InitAIData();
		RefreshAIScore();
		GSingleton<GTimer>.ins.addDelay(delegate
		{
			OnNewActivityStart?.Invoke();
		}, 0.05f);
	}

	public virtual bool TryPop(bool isForcePop = false, Action<bool> endCall = null)
	{
		ActivityStateBase activityStateBase = GetActivityStateWithRefresh();
		if (activityStateBase == ActivityStateBase.StartPrepare || activityStateBase == ActivityStateBase.During)
		{
			return PrepareOrDuringPop(isForcePop, endCall);
		}
		if (activityStateBase != ActivityStateBase.Ending)
		{
			return false;
		}
		return EndingPop(isForcePop, endCall);
	}

	public virtual long GetTimeStamp()
	{
		return (DateTime.Now.Ticks - 621355968000000000L) / 10000000;
	}

	public virtual int GetActivityEndSeconds()
	{
		if (GetActivityState() == ActivityStateBase.During)
		{
			long endTime = GetData().EndTime;
			if (endTime != 0)
			{
				long num = endTime - GetTimeStamp();
				if (num >= 0)
				{
					int num2 = Convert.ToInt32(num);
					if (num2 < 1)
					{
						return num2;
					}
					return num2 + 1;
				}
			}
		}
		return 0;
	}

	protected virtual ActivityStateBase RefreshData()
	{
		ActivityDataBase data = GetData();
		if (data.State == ActivityStateBase.Ending)
		{
			return ActivityStateBase.Ending;
		}
		if (data.State == ActivityStateBase.During)
		{
			if (!IsActivityOpenControl())
			{
				GetData().State = ActivityStateBase.NoStart;
			}
			else
			{
				RefreshAIScore();
				if (data.EndTime <= GetTimeStamp() && JudgeEndExtra())
				{
					data.State = ActivityStateBase.Ending;
					SaveData();
					OnActivityStateEnding?.Invoke();
					return ActivityStateBase.Ending;
				}
				if (JudgeDuring())
				{
					return data.State;
				}
				data.State = ActivityStateBase.Ending;
			}
			SaveData();
		}
		else
		{
			int num = JudgeNewActivity();
			if (num == -1)
			{
				ResetNoStart();
			}
			else
			{
				ActivityOpenPrepare(num);
			}
		}
		return data.State;
	}

	protected void ResetNoStart()
	{
		ActivityDataBase data = GetData();
		if (data.State != ActivityStateBase.NoStart)
		{
			data.StartTime = 0L;
			data.EndTime = 0L;
			data.State = ActivityStateBase.NoStart;
			data.StartPop = false;
			SaveData();
		}
	}

	public virtual void RefreshAIScore(bool isVictory = false)
	{
		long endTime = GetData().EndTime;
		long timeStamp = GetTimeStamp();
		ActivityDataBase data = GetData();
		if (endTime <= timeStamp)
		{
			timeStamp = endTime;
		}
		long num = timeStamp - data.AiLastRefreshTime;
		if (num > 59 && !RefreshAIScore((int)(num / 60), 0))
		{
			return;
		}
		GetData().AiLastRefreshTime = GetTimeStamp();
		SaveData();
	}

	protected abstract bool IsActivityOpenControl();

	protected abstract bool PrepareOrDuringPop(bool isForcePop, Action<bool> endCall);

	protected abstract bool EndingPop(bool isForcePop, Action<bool> endCall);

	protected abstract ActivityDataBase GetData();

	protected abstract void SaveData();

	protected abstract void NewActivityStart(int issue);

	protected abstract bool JudgeDuring();

	protected abstract bool JudgeEndExtra();

	protected abstract int JudgeNewActivity();

	protected virtual void InitAIData()
	{
	}

	public virtual bool RefreshAIScore(int minutes, int playerAddScore)
	{
		return false;
	}
}
