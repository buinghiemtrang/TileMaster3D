using System;
using DataCenter;

public class TurntableManager : Singleton<TurntableManager>
{
	public DateTime EndTime;

	public bool IsOpen => cf_ads_const.IsOpenVideoWheel == 1 && DB.User.Maxlv + 1 >= cf_ads_const.OpenLevelVideoWheelLevel;

	public int LeftCount => cf_ads_const.SpinsPerVideoLimit - DB.Turntable.WatchAdTimesOneDay;

	public int RewardCount => DB.Turntable.CurrentGetRewardIds.Count;

	public void InitData()
	{
	}

	public void RefreshData()
	{
		CheckSameDay();
	}

	private void CheckSameDay()
	{
		if (DB.Turntable.DailyInitTime > 0)
		{
			DateTime dateTimeBySeconds = MDateTime.GetDateTimeBySeconds(DB.Turntable.DailyInitTime);
			bool flag = DateTime.Now.Date == dateTimeBySeconds.Date;
			EndTime = DateTime.Today.AddSeconds(cf_ads_const.VideoWheelTime);
			if (flag)
			{
				return;
			}
		}
		DB.Turntable.HasDailyShowPop = false;
		DB.Turntable.WatchAdTimesOneDay = 0;
		DB.Turntable.DailyInitTime = DateTime.Now.TotalSeconds();
		ResetTurntable();
	}

	public void AddWatchCount()
	{
		DB.Turntable.WatchAdTimesOneDay++;
	}

	public void ResetTurntable()
	{
		DB.Turntable.ShowTimes = 0;
		DB.Turntable.CurrentGetRewardIds.Clear();
	}

	public TurntableDialog ShowTurntable(Action<bool> close)
	{
		TurntableDialog turntableDialog = SceneMgr.AddNode<TurntableDialog>("TurntableDialog");
		turntableDialog.OnCloseCallback = close;
		LogEvent.I.AdLuckySpin_Show();
		return turntableDialog;
	}
}
