using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MyToggle : Toggle
{
	private CanvasGroup cg;

	protected override void Awake()
	{
		base.Awake();
		if (graphic != null)
		{
			cg = graphic.GetComponent<CanvasGroup>();
			if (cg == null)
			{
				cg = graphic.gameObject.AddComponent<CanvasGroup>();
			}
		}
		onValueChanged.AddListener(cgFade);
		cgFade(base.isOn);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		onValueChanged.RemoveListener(cgFade);
		if (cg != null)
		{
			DOTween.Kill(cg);
		}
	}

	private void cgFade(bool _isOn)
	{
		if (cg != null)
		{
			float endValue = (_isOn ? 1f : 0f);
			float duration = ((toggleTransition != 0) ? 0.1f : 0f);
			cg.DOFade(endValue, duration);
		}
		if (_isOn && AudioMgr.I != null)
		{
			AudioMgr.I.PlaySound("button");
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		base.OnPointerClick(eventData);
	}
}
