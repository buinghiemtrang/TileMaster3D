using System;

namespace FQDev.Time
{
	public class ActionEvent
	{
		public int ID;

		public float TimeStamp;

		public Action Handler;

		public void Invoke()
		{
			Handler?.Invoke();
		}
	}
}
