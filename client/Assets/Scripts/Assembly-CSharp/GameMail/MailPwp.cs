using System;
using Com.TheFallenGames.OSA.CustomParams;

namespace GameMail
{
	[Serializable]
	public class MailPwp : BaseParamsWithPrefab
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
