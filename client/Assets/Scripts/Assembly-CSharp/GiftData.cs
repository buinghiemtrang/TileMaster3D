using System;

[Serializable]
public class GiftData
{
	public int id;

	public int day;

	public bool isBuy;

	public int nPop;

	public DateTime timer;

	public void Init(cfbt_Gift cfg)
	{
		id = cfg.id;
		day = DateTime.Now.DayOfYear;
		isBuy = false;
		nPop = cfg.popCycle;
		if (cfg.continueTime != -1)
		{
			timer = DateTime.Now.AddHours((double)cfg.continueTime);
		}
	}
}
