using System;
using Com.TheFallenGames.OSA.CustomParams;

namespace GameTeam
{
	[Serializable]
	public class SelectBadgeHuntPwp : BaseParamsWithPrefab
	{
		public int cellInclusionNum;

		public float DefaultItemHeight
		{
			set
			{
				DefaultItemSize = value;
			}
		}
	}
}
