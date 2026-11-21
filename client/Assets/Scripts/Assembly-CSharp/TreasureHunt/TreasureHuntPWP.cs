using System;
using Com.TheFallenGames.OSA.CustomParams;

namespace TreasureHunt
{
	[Serializable]
	public class TreasureHuntPWP : BaseParamsWithPrefab
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
