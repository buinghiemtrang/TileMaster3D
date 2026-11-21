using Com.TheFallenGames.OSA.Core;
using frame8.Logic.Misc.Other.Extensions;

namespace GameMail
{
	public class MailIvh : BaseItemViewsHolder
	{
		public MailItem PItem;

		public override void CollectViews()
		{
			base.CollectViews();
			PItem = root.GetComponentAtPath<MailItem>("Root");
		}
	}
}
