using System;
using System.Collections.Generic;
using UnityEngine;

namespace zFrame.Event
{
	public class BaseEventConfig
	{
		protected AnimationEvent _ClipEvent;

		protected int _keyFrame;

		protected A_EventInfo a_Event;

		protected Animator _animator;

		public BaseEventConfig(A_EventInfo eventInfo, int frame)
		{
			a_Event = eventInfo;
			_keyFrame = frame;
			_animator = eventInfo.animator;
		}

		public Animator SetParms(string stringParm = null, int intParm = 0, float floatParm = 0f, UnityEngine.Object objectParm = null)
		{
			if (a_Event != null)
			{
				a_Event.frameEventPairs.TryGetValue(_keyFrame, out var value);
				if (value != null)
				{
					value.intParameter = intParm;
					value.floatParameter = floatParm;
					value.stringParameter = stringParm;
					value.objectReferenceParameter = objectParm;
					ResignEvent();
					return a_Event.animator;
				}
			}
			return _animator;
		}

		private void ResignEvent()
		{
			a_Event.animationClip.events = null;
			foreach (AnimationEvent value in a_Event.frameEventPairs.Values)
			{
				a_Event.animationClip.AddEvent(value);
			}
			a_Event.animator.Rebind();
		}

		protected void ConfigProcess(int frame, Action<AnimationEvent> action)
		{
			if (action == null)
			{
				return;
			}
			_keyFrame = frame;
			if (!a_Event.frameCallBackPairs.ContainsKey(frame))
			{
				a_Event.frameCallBackPairs.Add(_keyFrame, action);
			}
			else if (a_Event.frameCallBackPairs[_keyFrame] != null)
			{
				if (Array.IndexOf(a_Event.frameCallBackPairs[_keyFrame].GetInvocationList(), action) != -1)
				{
					Debug.LogWarningFormat("AnimatorEventSystem[一般]：指定AnimationClip【{0}】已经订阅了该事件【{1}】！\n 建议：请勿频繁订阅！", a_Event.animationClip.name, action.Method.Name);
				}
				else
				{
					Dictionary<int, Action<AnimationEvent>> frameCallBackPairs = a_Event.frameCallBackPairs;
					int keyFrame = _keyFrame;
					frameCallBackPairs[keyFrame] = (Action<AnimationEvent>)Delegate.Combine(frameCallBackPairs[keyFrame], action);
				}
			}
			else
			{
				a_Event.frameCallBackPairs[_keyFrame] = action;
			}
			if (!a_Event.frameEventPairs.ContainsKey(_keyFrame))
			{
				A_EventHandler.Handler.GenerAnimationEvent(a_Event, _keyFrame);
			}
		}

		public Animator SetBool(string name, bool value)
		{
			_animator.SetBool(name, value);
			return _animator;
		}

		public Animator SetBool(int id, bool value)
		{
			_animator.SetBool(id, value);
			return _animator;
		}

		public Animator SetFloat(int id, float value)
		{
			_animator.SetFloat(id, value);
			return _animator;
		}

		public Animator SetFloat(string name, float value)
		{
			_animator.SetFloat(name, value);
			return _animator;
		}

		public Animator SetFloat(string name, float value, float dampTime, float deltaTime)
		{
			_animator.SetFloat(name, value, dampTime, deltaTime);
			return _animator;
		}

		public Animator SetFloat(int id, float value, float dampTime, float deltaTime)
		{
			_animator.SetFloat(id, value, dampTime, deltaTime);
			return _animator;
		}

		public Animator SetTrigger(int id)
		{
			_animator.SetTrigger(id);
			return _animator;
		}

		public Animator SetTrigger(string name)
		{
			_animator.SetTrigger(name);
			return _animator;
		}
	}
}
