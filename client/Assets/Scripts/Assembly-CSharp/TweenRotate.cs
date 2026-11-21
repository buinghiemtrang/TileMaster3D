using DG.Tweening;
using UnityEngine;

public class TweenRotate : MonoBehaviour
{
	private Tween tween;

	public Vector3 vRotate = Utils.Vec3(0f, 0f, -360f);

	public float fDelay = 3f;

	private void OnEnable()
	{
		if (this.tween != null)
		{
			this.tween.Kill(false);
		}
		this.tween = this.transform.DOLocalRotate(this.vRotate, this.fDelay, RotateMode.FastBeyond360).SetLoops(-1).SetEase(Ease.Linear);
	}

	private void OnDisable()
	{
		this.tween.Kill(false);
	}
}
