using System;
using Newtonsoft.Json;

namespace Game.Activity.BakingMaster
{
	[JsonObject(MemberSerialization.OptIn)]
	public class RequestIssueData
	{
		[JsonProperty]
		public string issue;

		[JsonProperty]
		public string startTime;

		[JsonProperty]
		public string endTime;

		[JsonProperty]
		public string currentTime;

		[JsonProperty]
		public long endTimeSpan;

		[JsonProperty]
		public bool available;

		[JsonProperty]
		public MyTeam myTeam;

		public DateTime StartTime;

		public DateTime EndTime;

		public string Issue => "BakingMaster_" + issue;

		public void RefreshTime()
		{
			StartTime = MDateTime.Parse(startTime);
			EndTime = MDateTime.Parse(endTime);
		}
	}
}
