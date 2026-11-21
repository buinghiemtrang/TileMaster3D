using System;
using DG.Tweening;
using UnityEngine;

namespace _FrameWork
{
	[RequireComponent(typeof(ParticleSystem))]
	public class ParticleSystemInfo : MonoBehaviour
	{
		public ParticleSystemStatus status = ParticleSystemStatus.Stop;

		private Transform _self;

		private ParticleSystem _particle;

		private Action _playCompletedCallback;

		private void Awake()
		{
			_self = base.transform;
			_particle = GetComponent<ParticleSystem>();
			if (status == ParticleSystemStatus.Pause)
			{
				Pause();
			}
			else if (status == ParticleSystemStatus.Stop)
			{
				Stop();
			}
		}

		public void Play(bool withChildren = true)
		{
			status = ParticleSystemStatus.Playing;
			_particle.Play(withChildren);
		}

		public void PlayOnce(bool isDestroy = false, float offset = 0f, bool withChildren = true)
		{
			_self.DOKill();
			Play(withChildren);
			float particleDuration = _self.gameObject.GetParticleDuration(includeChildren: true, includeInactive: false, allowLoop: false);
			AddDuration(particleDuration + offset, isDestroy, withChildren);
		}

		public void AddDuration(float duration, bool isDestroy = false, bool withChildren = true)
		{
			_self.DOKill();
			_self.DelayCall(duration, delegate
			{
				if (status == ParticleSystemStatus.Playing)
				{
					Stop(isDestroy, withChildren);
				}
			});
		}

		public void AddPlayCompletedCallback(Action callback)
		{
			_playCompletedCallback = callback;
		}

		public void SetPosition(Vector3 position, bool isLocal = true)
		{
			if (isLocal)
			{
				_self.localPosition = position;
			}
			else
			{
				_self.position = position;
			}
		}

		public void SetAngle(Vector3 angle)
		{
			_self.localRotation = Quaternion.Euler(angle);
		}

		public void SetParent(Transform node, bool worldPositionStays = true)
		{
			_self.SetParent(node, worldPositionStays);
		}

		public void Stop(bool isDestroy = false, bool withChildren = true)
		{
			_particle.Stop(withChildren);
			if (isDestroy)
			{
				_self.DelayCall(2f, delegate
				{
					UnityEngine.Object.Destroy(base.gameObject);
					_playCompletedCallback.InvokeSafe();
					_playCompletedCallback = null;
				});
			}
			else
			{
				status = ParticleSystemStatus.Stop;
				_playCompletedCallback.InvokeSafe();
				_playCompletedCallback = null;
			}
		}

		public void Pause(bool withChildren = true)
		{
			status = ParticleSystemStatus.Pause;
			_particle.Pause(withChildren);
		}

		private void OnDestroy()
		{
			DOTween.Kill(_self);
		}
	}
}
