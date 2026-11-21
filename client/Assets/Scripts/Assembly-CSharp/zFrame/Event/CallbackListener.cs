using UnityEngine;

namespace zFrame.Event
{
	public class CallbackListener : MonoBehaviour
	{
		private Animator animator;

		private A_EventHandler eventHandler;

		private void Start()
		{
			eventHandler = A_EventHandler.Handler;
			animator = GetComponent<Animator>();
		}

		private void AnimatorEventCallBack(AnimationEvent ae)
		{
			AnimationClip clip = ae.animatorClipInfo.clip;
			int frame = Mathf.RoundToInt(ae.animatorClipInfo.clip.frameRate * ae.time);
			eventHandler.GetAction(animator, clip, frame)?.Invoke(ae);
		}
	}
}
