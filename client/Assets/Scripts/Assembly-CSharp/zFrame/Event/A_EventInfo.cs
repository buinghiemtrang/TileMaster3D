using System;
using System.Collections.Generic;
using UnityEngine;

namespace zFrame.Event
{
	public class A_EventInfo
	{
		public Animator animator;

		public AnimationClip animationClip;

		public int totalFrames;

		public Dictionary<int, Action<AnimationEvent>> frameCallBackPairs = new Dictionary<int, Action<AnimationEvent>>();

		public Dictionary<int, AnimationEvent> frameEventPairs = new Dictionary<int, AnimationEvent>();

		public A_EventInfo(Animator anim, AnimationClip clip)
		{
			animator = anim;
			animationClip = clip;
			totalFrames = Mathf.RoundToInt(animationClip.frameRate * animationClip.length);
		}

		public void Clear()
		{
			animationClip.events = null;
			frameCallBackPairs = new Dictionary<int, Action<AnimationEvent>>();
			frameEventPairs = new Dictionary<int, AnimationEvent>();
			animationClip = null;
			animator = null;
		}
	}
}
