using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameScene_Element;
using UnityEngine;

namespace GameScene
{
	public class MagnetMgr : MonoBehaviour
	{
		public static MagnetMgr I;

		[SerializeField]
		private Transform pointNode;

		[SerializeField]
		private Transform parentNode;

		[SerializeField]
		private ParticleSystem effect;

		[SerializeField]
		private Animator anim;

		private Action _completedCallback;

		private List<Element> _elements;

		private void Awake()
		{
			I = this;
			effect.Stop(true);
			_elements = new List<Element>();
		}

		public void Init()
		{
			_elements.Clear();
		}

		public void Execute(Action completed)
		{
					_completedCallback = completed;
			_elements.AddRange(GameMgr.I.DestroyControl.JudgeCanUse(UnityEngine.Random.Range(3, 6)));
			if (_elements.Count == 0)
			{
				_completedCallback.InvokeSafe();
				return; // [fix] khớp pseudo-C
			}
			anim.Play("Show", 0, 0f);
		}

		public void ShowAnimEnd()
		{
			effect.Play(true);
			StartCoroutine("IEMagnet");
		}

		public void HideAnimEnd()
		{
			ElementsMgr.I.RemoveElements(_elements, Vector3.zero);
			_elements.Clear();
			_completedCallback.InvokeSafe();
		}

		private IEnumerator IEMagnet()
		{
			int number;
			int count = _elements.Count;
			for (int i = 0; i < count; i++)
			{
				Element element = _elements[i];
				element.ModifyLockStatus(true);
				element.ModifyParent(parentNode, true);
				element.Freeze(RigidbodyConstraints.FreezeAll);
			}
			number = count;
			TweenCallback onComplete = delegate
			{
				number--;
				if (number <= 0)
				{
					effect.Stop(true);
					anim.Play("Hide", 0, 0f);
				}
			};
			for (int j = 0; j < count; j++)
			{
							_elements[j].MoveToPosition(pointNode.localPosition, 0.2f, true).SetEase(Ease.Linear).OnComplete(onComplete);
				yield return new WaitForSeconds(0.05f);
			}
		}
	}
}
