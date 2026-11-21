using System;
using System.Collections.Generic;
using DG.Tweening;
using Fishtail.PlayTheBall.Vibration;
using GameScene_Element;
using UnityEngine;

namespace GameScene
{
	public class FusePoint
	{
		public Action<Vector3, ElementType> PlayFuseEffect;

		public Action FuseCompleteCallback;

		private List<Element> _fuseElements;

		private Sequence _leftSequence;

		private Sequence _rightSequence;

		public bool IsFree { get; private set; }

		public void Init()
		{
			// Pseudo-C: Init nhảy thẳng vào thân Clear
			Clear();
		}

		public void Clear()
		{
			_fuseElements = null;
			IsFree = true;
			_leftSequence?.Kill();
			_rightSequence?.Kill();
		}

		public void Fuse(List<Element> elements)
		{
			ElementType elementType;
			Vector3 effectPosition;
			DispatcherMgr.Emit("GameScene_Platform_StartFuse");
			LevelInterAd.TryShow();
			_fuseElements = elements;
			IsFree = false;
			elementType = elements[0].Type;
			Vector3 localPosition = elements[1].Self.localPosition;
			effectPosition = elements[1].Self.position;
			_leftSequence = DOTween.Sequence();
			_rightSequence = DOTween.Sequence();
			for (int i = 0; i < _fuseElements.Count; i++)
			{
				Element element = _fuseElements[i];
				if (i == 0)
				{
					Vector3 leftPosition = localPosition;
					leftPosition.x = localPosition.x - 1f;
					_leftSequence.Append(element.MoveToPosition(leftPosition, 0.15f, true).SetEase(Ease.OutCirc));
					_leftSequence.Append(element.MoveToPosition(localPosition, 0.1f, true).SetEase(Ease.InCubic));
				}
				else if (i == 2)
				{
					Vector3 rightPosition = localPosition;
					rightPosition.x = localPosition.x + 1f;
					_rightSequence.Append(element.MoveToPosition(rightPosition, 0.15f, true).SetEase(Ease.OutCirc));
					_rightSequence.Append(element.MoveToPosition(localPosition, 0.1f, true).SetEase(Ease.InCubic));
					_leftSequence.OnComplete(delegate
					{
						float z = effectPosition.z;
						effectPosition.y = 5f;
						effectPosition.z = z - 1.6f;
						Vector3 vector = new Vector3(effectPosition.x, 5f, z + 0.4f);
						RemoveFuseElements(vector);
						AudioMgr.I.PlaySound("fusion");
						VibrationController.I.ImpactM();
						GameMgr.I.comboControl.Combo();
						GameMgr.I.comboControl.FlyStart(vector, isMoreStart: true);
						PlayFuseEffect?.Invoke(effectPosition, elementType);
						FuseCompleteCallback?.Invoke();
						DispatcherMgr.Emit("GameScene_Platform_Fuse_Completed", elementType);
					});
				}
			}
		}

		private void RemoveFuseElements(Vector3 startPosition)
		{
			_fuseElements[1].ExecuteDestroyEffect();
			ElementsMgr.I.RemoveElements(_fuseElements, startPosition);
			_fuseElements = null;
		}
	}
}
