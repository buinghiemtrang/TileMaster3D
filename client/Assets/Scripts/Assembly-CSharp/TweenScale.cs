using DG.Tweening;
using UnityEngine;

public class TweenScale : MonoBehaviour
{
	public float fDelay = 0.3f;

	public float fWait = 0.3f;

	public float fStart;

	public float fEnd = 1f;

	public bool bLoop;

	private Sequence sequence;

	private void OnEnable()
	{
		base.transform.SetScale(fStart);
		sequence = DOTween.Sequence().SetId("TweenScale_Start");
		sequence.Append(base.transform.DOScale(Utils.Vec3(fEnd + 0.1f, fEnd + 0.1f, 1f), fDelay));
		sequence.AppendInterval(0.1f);
		sequence.Append(base.transform.DOScale(Utils.Vec3(fEnd, fEnd, 1f), fDelay));
		if (bLoop)
		{
			sequence.AppendInterval(fWait);
			sequence.SetLoops(-1);
		}
	}

	private void OnDisable()
	{
		sequence.Kill(false);
	}
}
