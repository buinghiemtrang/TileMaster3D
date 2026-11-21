using System;

public class AdBreakBox : DialogBase
{
	private int _timeLeft1;

	private int _timeLeft2;

	public AdBreakBox()
	{
		fMaskAlpha = 0.8f;
	}

	public void Show(Action action)
	{
		_timeLeft1 = FQDev.Time.TimeManager.SetupInvoke(cf_const.AdBreakTime, action.InvokeSafe);
		_timeLeft2 = FQDev.Time.TimeManager.SetupInvoke(cf_const.AdBreakTime + 2.5f, delegate
		{
			_Close();
		});
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		FQDev.Time.TimeManager.CancelInvoke(_timeLeft1);
		FQDev.Time.TimeManager.CancelInvoke(_timeLeft2);
	}
}
