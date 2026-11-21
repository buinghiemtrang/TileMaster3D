using System;
using Spine;
using Spine.Unity;
using UnityEngine;

[RequireComponent(typeof(SkeletonGraphic))]
[DisallowMultipleComponent]
public class SpineUIControl : MonoBehaviour
{
	private SkeletonGraphic _spine;

	private Action<string> _funcCompleteOnce;

	private Action<string> _funcComplete;

	public bool bDestroy;

	public string PlayName { get; private set; }

	public string SkinName => _spine.Skeleton.Skin.Name;

	private void Awake()
	{
		_spine = base.transform.GetComponent<SkeletonGraphic>();
		_spine.AnimationState.Complete += OnEventComplete;
		_spine.AnimationState.Start += OnEventStart;
	}

	public SpineUIControl Play(string sName, Action<string> onceCallBack = null)
	{
		base.gameObject.SetActive(value: true);
		_spine.AnimationState.SetAnimation(0, sName, loop: false);
		_spine.timeScale = 1f;
		_funcCompleteOnce = onceCallBack;
		return this;
	}

	public SpineUIControl Play(string sName, bool loop, Action<string> onceCallBack = null)
	{
		base.gameObject.SetActive(value: true);
		_spine.AnimationState.SetAnimation(0, sName, loop);
		_spine.timeScale = 1f;
		_funcCompleteOnce = onceCallBack;
		return this;
	}

	public void PlayFrame(string sName, bool loop, float frames, int trackIndex = 0)
	{
		_spine.AnimationState.SetAnimation(trackIndex, sName, loop).TrackTime = frames / 30f;
	}

	public void ClearFunc()
	{
		_funcCompleteOnce = null;
		_funcComplete = null;
	}

	public void SetEventComplete(Action<string> callback)
	{
		_funcComplete = callback;
	}

	public SpineUIControl Add(string sName, bool loop = false, float delay = 0f)
	{
		_spine.AnimationState.AddAnimation(0, sName, loop, delay);
		return this;
	}

	public void Skin(string sName)
	{
		_spine.Skeleton.SetSkin(sName);
	}

	public void Mix(string beforeName, string afterName, float time)
	{
		_spine.AnimationState.Data.SetMix(beforeName, afterName, time);
	}

	public void FlipX(bool flip)
	{
		_spine.Skeleton.ScaleX = (flip ? (-1f) : 1f);
	}

	public void FlipY(bool flip)
	{
		_spine.Skeleton.ScaleY = (flip ? (-1f) : 1f);
	}

	public void Speed(float speed)
	{
		_spine.timeScale = speed;
	}

	public void FrameStart(int trackIndex = 0)
	{
		_spine.timeScale = 0f;
		_spine.AnimationState.GetCurrent(trackIndex).TrackTime = 0f;
	}

	public void SetFrame(float percent, int trackIndex = 0)
	{
		TrackEntry current = _spine.AnimationState.GetCurrent(trackIndex);
		current.TrackTime = current.Animation.Duration * percent;
	}

	private void OnEventComplete(TrackEntry trackEntry)
	{
		if (_funcCompleteOnce != null)
		{
			_funcCompleteOnce(trackEntry.Animation.Name);
			_funcCompleteOnce = null;
		}
		if (_funcComplete != null)
		{
			_funcComplete(trackEntry.Animation.Name);
		}
		if (bDestroy)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnEventStart(TrackEntry trackEntry)
	{
		PlayName = trackEntry.Animation.Name;
	}
}
