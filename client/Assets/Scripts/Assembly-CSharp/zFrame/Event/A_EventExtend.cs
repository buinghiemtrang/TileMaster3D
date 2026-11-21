using UnityEngine;

namespace zFrame.Event
{
	public static class A_EventExtend
	{
		public static A_EventConfig_A SetTarget(this Animator animator, string clipName)
		{
			A_EventInfo a_EventInfo = A_EventHandler.Handler.GenerAnimationInfo(animator, clipName);
			if (a_EventInfo != null && null == animator.GetComponent<CallbackListener>())
			{
				animator.gameObject.AddComponent<CallbackListener>();
			}
			return new A_EventConfig_A(a_EventInfo);
		}

		public static A_EventConfig_B SetTarget(this Animator animator, string clipName, int frame)
		{
			A_EventInfo a_EventInfo = A_EventHandler.Handler.GenerAnimationInfo(animator, clipName);
			if (a_EventInfo != null && null == animator.GetComponent<CallbackListener>())
			{
				animator.gameObject.AddComponent<CallbackListener>();
			}
			return new A_EventConfig_B(a_EventInfo, frame);
		}
	}
}
