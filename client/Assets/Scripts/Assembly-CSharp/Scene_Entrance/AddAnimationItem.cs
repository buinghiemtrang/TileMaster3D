using System;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace Scene_Entrance
{
	public class AddAnimationItem : MonoBehaviour
	{
		[SerializeField]
		private Image _iconImg;

		[SerializeField]
		private Animator _addAnimator;

		[SerializeField]
		private Text _addNumberTex;

		[SerializeField]
		private SkeletonGraphic _lightSpine;

		private GameObject _root;

		private bool _isDestroyed;

		private void OnDestroy()
		{
			if (_root)
			{
				_root.transform.DOKill();
			}
		}

		public void Play(Vector3 position, int number, Sprite icon, Action completed)
		{
			_isDestroyed = false;
			if (!_root)
			{
				_root = gameObject;
			}
			_root.SetActive(true);
			_root.transform.position = position;
			_addNumberTex.text = string.Format("+{0}", number);
			_iconImg.sprite = icon;
			_addAnimator.Play("add", 0, 0f);
			_lightSpine.AnimationState.SetAnimation(0, "idle", true);
			_root.transform.DelayCall(0.8f, delegate
			{
				_lightSpine.AnimationState.SetAnimation(0, "collect", false);
				_lightSpine.AnimationState.AddAnimation(0, "idle", true, 0f);
			});
			_root.transform.DelayCall(1.5f, delegate
			{
				if (completed != null)
				{
					completed();
				}
				Destroy();
			});
		}

		public void Destroy()
		{
			if (_isDestroyed)
			{
				return;
			}
			_isDestroyed = true;
			UnityEngine.Object.Destroy(_root);
		}
	}
}
