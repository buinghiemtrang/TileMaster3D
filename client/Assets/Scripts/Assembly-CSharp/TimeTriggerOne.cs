using System;

public class TimeTriggerOne
{
	public string key = "";

	public int nTime;

	public DateTime dateTime;

	public bool bEnd;

	public void Init()
	{
		bEnd = false;
		nTime = 0;
		dateTime = DateTime.Now;
	}

	public void Update(DateTime now)
	{
		if (!bEnd)
		{
			int num = nTime;
			TimeSpan timeSpan = now - dateTime;
			dateTime = now;
			nTime = timeSpan.Seconds + num;
		}
	}
}
