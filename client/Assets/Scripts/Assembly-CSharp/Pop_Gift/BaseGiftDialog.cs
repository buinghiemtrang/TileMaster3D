using System;

namespace Pop_Gift
{
	public abstract class BaseGiftDialog : DialogBase
	{
		public abstract void Init(bool isAutoPop, GiftData data, Action<bool> onCloseCallback);
	}
}
