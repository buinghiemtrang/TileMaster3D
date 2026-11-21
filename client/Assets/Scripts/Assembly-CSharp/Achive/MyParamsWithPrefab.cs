using System;
using Com.TheFallenGames.OSA.CustomParams;

namespace Achive
{
	[Serializable]
	public class MyParamsWithPrefab : BaseParamsWithPrefab
	{
		public float DefaultItemHeight
		{
			set
			{
				DefaultItemSize = value;
			}
		}
	}
}
