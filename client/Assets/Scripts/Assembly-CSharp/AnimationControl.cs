using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animation))]
[DisallowMultipleComponent]
public class AnimationControl : MonoBehaviour
{
	public bool bDestroy;

	private Animation _animation;

	private Action<string> funcFinishOnce;

	private Action<string> funcFinish;

	private Action<string> funcStartOnce;

	private Action<string> funcStart;

	private Action<string> funcFrameOnce;

	private Action<string> funcFrame;

	private Action<string> funcCompleteOnce;

	private List<string> _ayAddNames;

	private string _sCurPlayName;

	public string PlayName => _sCurPlayName;

	private void Awake()
	{
		_ayAddNames = new List<string>();
		_animation = base.transform.GetComponent<Animation>();
		foreach (AnimationState item in _animation)
		{
			item.clip.events = Array.FindAll(item.clip.events, (AnimationEvent a) => a.functionName != "OnEventFinish" && a.functionName != "OnEventStart");
			AnimationEvent animationEvent = new AnimationEvent();
			animationEvent.functionName = "OnEventFinish";
			animationEvent.stringParameter = item.clip.name;
			animationEvent.time = item.clip.length;
			item.clip.AddEvent(animationEvent);
			animationEvent = new AnimationEvent();
			animationEvent.functionName = "OnEventStart";
			animationEvent.stringParameter = item.clip.name;
			animationEvent.time = 0f;
			item.clip.AddEvent(animationEvent);
		}
	}

	private void OnEventStart(string sName)
	{
		if (funcStartOnce != null)
		{
			funcStartOnce(sName);
			funcStartOnce = null;
		}
		if (funcStart != null)
		{
			funcStart(sName);
		}
	}

	private void OnEventFinish(string sName)
	{
		if (funcFinishOnce != null)
		{
			funcFinishOnce(sName);
			funcFinishOnce = null;
		}
		if (funcFinish != null)
		{
			funcFinish(sName);
		}
		if (_ayAddNames.Count < 1)
		{
			if (funcCompleteOnce != null)
			{
				funcCompleteOnce(sName);
				funcCompleteOnce = null;
			}
			if (bDestroy)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		else
		{
			_Play(_ayAddNames.Pop(0), WrapMode.ClampForever, null);
		}
	}

	private void OnEventFrame(string param)
	{
		if (funcFrameOnce != null)
		{
			funcFrameOnce(base.name);
			funcFrameOnce = null;
		}
		if (funcFrame != null)
		{
			funcFrame(param);
		}
	}

	public void SetEventFrame(Action<string> callback)
	{
		funcFrame = callback;
	}

	public void SetEventFrameOnce(Action<string> callback)
	{
		funcFrameOnce = callback;
	}

	public void SetEventFinish(Action<string> callback)
	{
		funcFinish = callback;
	}

	public void SetEventFinishOnce(Action<string> callback)
	{
		funcFinishOnce = callback;
	}

	public void SetEventStart(Action<string> callback)
	{
		funcStart = callback;
	}

	public void SetEventStartOnce(Action<string> callback)
	{
		funcStartOnce = callback;
	}

	public void SetCompleteOnce(Action<string> callback)
	{
		funcCompleteOnce = callback;
	}

	private AnimationControl _Play(string sName, WrapMode wrap, Action<string> callback)
	{
		AnimationState state = _animation[sName];
		state.wrapMode = wrap;
		state.speed = 1f;
		_animation.Play(sName);
		funcFinishOnce = callback;
		_sCurPlayName = sName;
		return this;
	}

	public void Rewind(string sName, Action<string> callback = null)
	{
		AnimationState state = _animation[sName];
		state.wrapMode = WrapMode.Once;
		state.speed = 1f;
		_animation.Rewind(sName);
		funcStartOnce = callback;
		_sCurPlayName = sName;
	}

	public AnimationControl Play(string sName, Action<string> callback = null)
	{
		return _Play(sName, WrapMode.ClampForever, callback);
	}

	public AnimationControl PlayOnce(string sName, Action<string> callback = null)
	{
		return _Play(sName, WrapMode.Once, callback);
	}

	public AnimationControl PlayLoop(string sName, Action<string> callback = null)
	{
		return _Play(sName, WrapMode.Loop, callback);
	}

	public AnimationControl Add(string sName)
	{
		_ayAddNames.Add(sName);
		return this;
	}

	public void StopToStart(string sName)
	{
		AnimationState animationState = _animation[sName];
		animationState.speed = 0f;
		animationState.time = 0f;
		_animation.Rewind(sName);
	}

	public void StopToFinish(string sName)
	{
		AnimationState animationState = _animation[sName];
		animationState.speed = 0f;
		animationState.time = animationState.clip.length;
		_animation.Rewind(sName);
	}

	public void SetTime(string sName, float time)
	{
		_animation[sName].time = time;
	}

	public void SetTime(float time)
	{
		_animation[_sCurPlayName].time = time;
	}

	public void SetSpeed(string sName, float speed)
	{
		_animation[sName].speed = speed;
	}

	public void SetSpeed(float speed)
	{
		_animation[_sCurPlayName].speed = speed;
	}
}
