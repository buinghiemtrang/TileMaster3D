using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class AnimatorControl : MonoBehaviour
{
	public bool bDestroy;

	private Animator _animator;

	private Action<string> funcFinishOnce;

	private Action<string> funcFinish;

	private Action<string> funcFrame;

	private string _sCurPlayName;

	public string PlayName => _sCurPlayName;

	private void Awake()
	{
		_animator = base.transform.GetComponent<Animator>();
		AnimationClip[] animationClips = _animator.runtimeAnimatorController.animationClips;
		foreach (AnimationClip animationClip in animationClips)
		{
			animationClip.events = Array.FindAll(animationClip.events, (AnimationEvent a) => a.functionName != "OnEventFinish" && a.functionName != "OnEventStart");
			AnimationEvent animationEvent = new AnimationEvent();
			animationEvent.functionName = "OnEventFinish";
			animationEvent.stringParameter = animationClip.name;
			animationEvent.time = animationClip.length;
			animationClip.AddEvent(animationEvent);
			animationEvent = new AnimationEvent();
			animationEvent.functionName = "OnEventStart";
			animationEvent.stringParameter = animationClip.name;
			animationEvent.time = 0f;
			animationClip.AddEvent(animationEvent);
		}
	}

	private void OnEventStart(string name)
	{
		_sCurPlayName = name;
	}

	private void OnEventFinish(string name)
	{
		if (funcFinishOnce != null)
		{
			funcFinishOnce(name);
			funcFinishOnce = null;
		}
		if (funcFinish != null)
		{
			funcFinish(name);
		}
		if (bDestroy)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnEventFrame(string param)
	{
		if (funcFrame != null)
		{
			funcFrame(base.name);
		}
	}

	public void SetEventFrame(Action<string> callback)
	{
		funcFrame = callback;
	}

	public void SetEventFinish(Action<string> callback)
	{
		funcFinish = callback;
	}

	public void SetEventFinishOnce(Action<string> callback)
	{
		funcFinishOnce = callback;
	}

	public void Play(string name, Action<string> callback = null)
	{
		_animator.Play(name);
		funcFinishOnce = callback;
	}
}
