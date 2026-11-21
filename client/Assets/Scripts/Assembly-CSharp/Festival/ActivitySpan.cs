using System;

namespace Festival
{
	public class ActivitySpan
	{
		private readonly DateTime startTime;

		private readonly DateTime endTime;

		public ActivitySpan(string start, string end)
		{
			if (!DateTime.TryParse(start, out startTime))
			{
				startTime = DateTime.MaxValue;
			}
			if (!DateTime.TryParse(end, out endTime))
			{
				endTime = DateTime.MinValue;
			}
		}

		public bool Contain(DateTime dateTime)
		{
			return dateTime >= startTime && dateTime <= endTime;
		}
	}
}
