using System;
using Com.TheFallenGames.OSA.CustomParams;

namespace Ranking
{
	[Serializable]
	public class MyParamsWithPrefab : BaseParamsWithPrefab
	{
		public float DefaultItemHeight
		{
			set
			{
				base.DefaultItemSize = value;
			}
		}
	}
}
