using System;
using UnityEngine;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Track : ITrack
	{
		public DateTime fristLoginTime;

		public void SetFristLoginTime()
		{
			if (FristloginTimeTicks == 0)
			{
				FristloginTimeTicks = DateTime.Now.Ticks;
			}
			fristLoginTime = new DateTime(FristloginTimeTicks);
			Debug.Log("fristLoginTime " + fristLoginTime.ToString());
		}

		public bool IsIn24Hour()
		{
			double totalSeconds = (DateTime.Now - fristLoginTime).TotalSeconds;
			Debug.Log("IsIn24Hour passTime " + totalSeconds.ToString());
			bool flag = totalSeconds <= 86400.0;
			Debug.Log("IsIn24Hour " + flag.ToString());
			return totalSeconds <= 86400.0;
		}
	}
}
