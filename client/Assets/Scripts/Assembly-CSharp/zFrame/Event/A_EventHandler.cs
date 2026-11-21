using System;
using System.Collections.Generic;
using UnityEngine;

namespace zFrame.Event
{
	public class A_EventHandler
	{
		private static A_EventHandler _instance;

		private List<A_EventInfo> eventContainer = new List<A_EventInfo>();

		private const string func = "AnimatorEventCallBack";

		public static A_EventHandler Handler
		{
			get
			{
				if (_instance == null)
				{
					_instance = new A_EventHandler();
				}
				return _instance;
			}
		}

		public A_EventInfo GenerAnimationInfo(Animator animator, string clipName)
		{
			AnimationClip animationClip = GetAnimationClip(animator, clipName);
			if (null == animationClip)
			{
				return null;
			}
			return GetEventInfo(animator, animationClip);
		}

		public void GenerAnimationEvent(A_EventInfo eventInfo, int frame)
		{
			if (frame >= 0 && frame <= eventInfo.totalFrames)
			{
				float _time = (float)frame / eventInfo.animationClip.frameRate;
				AnimationEvent animationEvent = Array.Find(eventInfo.animationClip.events, (AnimationEvent v) => v.time == _time);
				if (animationEvent != null)
				{
					if (!(animationEvent.functionName != "AnimatorEventCallBack"))
					{
						Debug.LogWarningFormat("AnimatorEventSystem[一般]：【{0}】所在的动画机【{1}】片段【{2}】帧已存在回调方法，无需重复添加！", eventInfo.animator.name, eventInfo.animationClip.name, frame);
						if (!eventInfo.frameEventPairs.ContainsKey(frame))
						{
							eventInfo.frameEventPairs.Add(frame, animationEvent);
						}
						return;
					}
					Debug.LogWarningFormat("AnimatorEventSystem[一般]：【{0}】所在的动画机【{1}】片段【{2}】帧已存在回调方法【{3}】，将自动覆盖！", eventInfo.animator.name, eventInfo.animationClip.name, frame, animationEvent.functionName);
				}
				animationEvent = new AnimationEvent();
				animationEvent.functionName = "AnimatorEventCallBack";
				animationEvent.time = _time;
				animationEvent.messageOptions = SendMessageOptions.DontRequireReceiver;
				eventInfo.animationClip.AddEvent(animationEvent);
				eventInfo.frameEventPairs.Add(frame, animationEvent);
				eventInfo.animator.Rebind();
			}
			else
			{
				Debug.LogErrorFormat("AnimatorEventSystem[紧急]：【{0}】所在的动画机【{1}】片段帧数设置错误【{2}】！", eventInfo.animator.name, eventInfo.animationClip.name, frame);
			}
		}

		public void Clear()
		{
			foreach (A_EventInfo item in eventContainer)
			{
				item.Clear();
			}
			eventContainer = new List<A_EventInfo>();
		}

		private A_EventInfo GetEventInfo(Animator animator, AnimationClip clip)
		{
			A_EventInfo a_EventInfo = eventContainer.Find((A_EventInfo v) => v.animator == animator && v.animationClip == clip);
			if (a_EventInfo == null)
			{
				a_EventInfo = new A_EventInfo(animator, clip);
				eventContainer.Add(a_EventInfo);
			}
			return a_EventInfo;
		}

		public AnimationClip GetAnimationClip(Animator animator, string name)
		{
			if (null == animator)
			{
				Debug.LogError("AnimatorEventSystem[紧急]：指定Animator不得为空！");
				return null;
			}
			RuntimeAnimatorController runtimeAnimatorController = animator.runtimeAnimatorController;
			if (null == runtimeAnimatorController)
			{
				Debug.LogError("AnimatorEventSystem[紧急]：指定【" + animator.name + "】Animator未挂载Controller！");
				return null;
			}
			AnimationClip[] array = Array.FindAll(runtimeAnimatorController.animationClips, (AnimationClip v) => v.name == name);
			if (array != null && array.Length != 0)
			{
				if (array.Length > 1)
				{
					Debug.LogWarningFormat("AnimatorEventSystem[一般]：指定【{0}】Animator存在【{1}】个名为【{2}】的动画片段！\n 建议：若非复用导致的重名，请务必修正！否则，事件将绑定在找的第一个Clip上。", animator.name, array.Length, name);
				}
				return array[0];
			}
			Debug.LogError("AnimatorEventSystem[紧急]：指定【" + animator.name + "】Animator不存在名为【" + name + "】的动画片段！");
			return null;
		}

		public Action<AnimationEvent> GetAction(Animator animator, AnimationClip clip, int frame)
		{
			A_EventInfo a_EventInfo = eventContainer.Find((A_EventInfo v) => v.animator == animator && v.animationClip == clip);
			Action<AnimationEvent> value = null;
			a_EventInfo?.frameCallBackPairs.TryGetValue(frame, out value);
			return value;
		}
	}
}
