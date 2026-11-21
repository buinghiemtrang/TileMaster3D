using UnityEngine;

public static class ParticleSystemExtend
{
	public static float GetDuration(this ParticleSystem particle, bool allowLoop = false)
	{
		if (!particle.emission.enabled)
		{
			return 0f;
		}
		if (particle.main.loop && !allowLoop)
		{
			return -1f;
		}
		float num;
		float num2;
		if (particle.emission.rateOverTime.constantMax <= 0f)
		{
			num = particle.main.startDelay.constantMax;
			num2 = particle.main.startLifetime.constantMax;
		}
		else
		{
			num = particle.main.startDelay.constantMax;
			num2 = particle.main.duration;
			float constantMax = particle.main.startLifetime.constantMax;
			if (num2 <= constantMax)
			{
				num2 = constantMax;
			}
		}
		return num + num2;
	}

	public static float GetParticleDuration(this GameObject gameObject, bool includeChildren = true, bool includeInactive = false, bool allowLoop = false)
	{
		if (includeChildren)
		{
			ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>(includeInactive);
			float num = -1f;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				float duration = componentsInChildren[i].GetDuration(allowLoop);
				if (duration <= num)
				{
					duration = num;
				}
				num = duration;
			}
			return num;
		}
		ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
		if (component != null)
		{
			return component.GetDuration(allowLoop);
		}
		return -1f;
	}
}
