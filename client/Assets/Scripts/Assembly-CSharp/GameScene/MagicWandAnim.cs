using System;
using UnityEngine;

namespace GameScene
{
	public class MagicWandAnim : MonoBehaviour
	{
		[SerializeField]
		private Animator anim;

		[SerializeField]
		private Animator maskAnim;

		public Action ClearUpCallback;

		public Action CloseCallback;

		private void Awake()
		{
			base.gameObject.SetActive(false);
			maskAnim.gameObject.SetActive(false);
		}

		public void Play()
		{
			_Hide(false);
			anim.Play("Show", 0, 0f);
			maskAnim.Play("Show", 0, 0f);
		}

		public void ClearUp()
		{
			ClearUpCallback?.Invoke();
		}

		public void Close()
		{
			_Hide(true);
			CloseCallback?.Invoke();
		}

		private void _Hide(bool isHide)
		{
			base.gameObject.SetActive(!isHide);
			maskAnim.gameObject.SetActive(!isHide);
		}
	}
}
