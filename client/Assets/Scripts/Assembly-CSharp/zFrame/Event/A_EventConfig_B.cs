using System;
using UnityEngine;

namespace zFrame.Event
{
	public class A_EventConfig_B : BaseEventConfig
	{
		public A_EventConfig_B(A_EventInfo eventInfo, int frame)
			: base(eventInfo, frame)
		{
		}

		public A_EventConfig_B OnProcess(Action<AnimationEvent> onProcess)
		{
			if (a_Event == null)
			{
				return null;
			}
			ConfigProcess(_keyFrame, onProcess);
			return this;
		}
	}
}
