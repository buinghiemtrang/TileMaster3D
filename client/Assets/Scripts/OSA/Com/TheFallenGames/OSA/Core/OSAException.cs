using System;

namespace Com.TheFallenGames.OSA.Core
{
	[Serializable]
	public class OSAException : SystemException
	{
		public OSAException()
		{
		}

		public OSAException(string message)
			: base(message)
		{
		}

		public OSAException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
