using UnityEngine;
using zFrame.Event;

public class AnimatorDestroy : MonoBehaviour
{
	public Animator animator;

	private void Awake()
	{
		if (animator == null)
		{
			animator = base.transform.GetComponent<Animator>();
		}
	}

	private void OnEventFinish(AnimationEvent ev)
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void Play(string name)
	{
		animator.SetTarget(name).OnCompleted(OnEventFinish);
		animator.Play(Animator.StringToHash(name));
	}
}
