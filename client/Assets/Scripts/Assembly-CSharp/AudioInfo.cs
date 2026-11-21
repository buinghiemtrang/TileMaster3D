using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class AudioInfo
{
	public AudioClip clip;

	public float interval;

	[Range(0f, 1f)]
	public float volume = 1f;

	private bool _bReady = true;

	public bool isReady
	{
		get
		{
			return _bReady;
		}
		private set
		{
			_bReady = value;
		}
	}

	private void _waiting()
	{
		_bReady = true;
	}

	public void Waiting()
	{
		if (interval != 0f)
		{
			_bReady = false;
			TweenMgr.DelayCall(interval, _waiting);
		}
	}
}
