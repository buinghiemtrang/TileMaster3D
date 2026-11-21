using UnityEngine;

public class ParticleSystemStop : MonoBehaviour
{
	[HideInInspector]
	public bool isStop;

	private Transform self;

	private ParticleSystem particle;

	private float stopTime;

	public void Play(Vector3 pos)
	{
		if (!self)
		{
			self = base.transform;
			particle = GetComponent<ParticleSystem>();
			stopTime = base.gameObject.GetParticleDuration() + 0.5f;
		}
		isStop = false;
		self.position = pos;
		_hide(false);
		particle.Play(true);
		Invoke("_stop", stopTime);
	}

	private void _stop()
	{
		isStop = true;
		base.gameObject.SetActive(false);
	}

	private void _hide(bool isHide)
	{
		base.gameObject.SetActive(!isHide);
	}
}
