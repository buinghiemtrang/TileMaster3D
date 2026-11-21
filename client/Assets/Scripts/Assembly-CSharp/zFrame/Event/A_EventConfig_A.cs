using System;
using UnityEngine;

namespace zFrame.Event
{
	public class A_EventConfig_A : BaseEventConfig
	{
		public A_EventConfig_A(A_EventInfo eventInfo, int frame = -1)
			: base(eventInfo, frame)
		{
		}

		public A_EventConfig_A OnStart(Action<AnimationEvent> onStart)
		{
			if (a_Event == null)
			{
				return null;
			}
			ConfigProcess(0, onStart);
			return this;
		}

		public A_EventConfig_A OnCompleted(Action<AnimationEvent> onCompleted)
		{
			if (a_Event == null)
			{
				return null;
			}
			ConfigProcess(a_Event.totalFrames, onCompleted);
			return this;
		}
	}
}
