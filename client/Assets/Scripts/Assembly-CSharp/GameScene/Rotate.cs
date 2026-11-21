using DG.Tweening;
using UnityEngine;

namespace GameScene
{
	public class Rotate : MonoBehaviour
	{
		private Transform _self;

		private void Awake()
		{
			_self = base.transform;
			_self.DORotate(Vector3.up * 360f, 9f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
		}

		private void OnDestroy()
		{
			if (_self != null)
			{
				_self.DOKill();
			}
		}
	}
}
