using System;
using Com.TheFallenGames.OSA.CustomParams;

namespace GameTeam
{
	[Serializable]
	public class TeamJoinRequestPWP : BaseParamsWithPrefab
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
