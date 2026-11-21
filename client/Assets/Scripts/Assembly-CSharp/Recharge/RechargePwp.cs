using System;
using Com.TheFallenGames.OSA.CustomParams;

namespace Recharge
{
	[Serializable]
	public class RechargePwp : BaseParamsWithPrefab
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
