using System;
using Com.TheFallenGames.OSA.CustomParams;

namespace GameTeam
{
	[Serializable]
	public class TeamJoinDialogPWP : BaseParamsWithPrefab
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
