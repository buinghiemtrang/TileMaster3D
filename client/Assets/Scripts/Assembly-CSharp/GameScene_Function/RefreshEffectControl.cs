using UnityEngine;
using _FrameWork;

namespace GameScene_Function
{
	public class RefreshEffectControl : MonoBehaviour
	{
		public static RefreshEffectControl I;

		[SerializeField]
		private ParticleSystemInfo effect;

		private void Awake()
		{
			I = this;
		}

		public void Play(float duration)
		{
			effect.Play();
			effect.AddDuration(duration);
		}
	}
}
