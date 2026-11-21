using DG.Tweening;
using UnityEngine;

public class TweenFade : MonoBehaviour
{
	public float fDelay = 0.3f;

	public float fAlphaStart;

	public float fAlphaEnd = 1f;

	private Tween tween;

	private void Start()
	{
		CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
		if (canvasGroup == null)
		{
			canvasGroup = gameObject.AddComponent<CanvasGroup>();
		}
		canvasGroup.alpha = fAlphaStart;
		tween = canvasGroup.DOFade(fAlphaEnd, fDelay);
	}

	private void OnDisable()
	{
		if (tween != null)
		{
			tween.Kill(false);
			tween = null;
		}
	}
}
